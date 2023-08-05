using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Management;
using AgentEngine;

namespace AgentService
{
    // Administrative Tools Item
    struct AdminItem
    {
        public string ItemName;
        public string ProcFullName;
        public string CmdLine;
    }

    /************************************************************************/
    // Administrative Process Class
    /************************************************************************/
    struct AdminProcess
    {
        public Global.ProcessEntry ProcessEntry;
        public AdminItem AdminItem;
    }

    /************************************************************************/
    // CAdminToolsTracker
    /************************************************************************/
    public class AdminToolsTracker
    {
        public AdminToolsTracker()
        {
            adminItemList = new AdminItem[Global.MAX_ADMIN_ITEM_LIST_SIZE];
            adminProcessList = new List<AdminProcess>();
            adminItemCount = 0;

            /* fetch key from registry "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" */
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(SHELLFOLDERS_REG_KEY);
            if (rk == null) return;

            string strPath = (string)rk.GetValue(ADMINTOOLS_REG_KEY, ""); 

            string strSystemPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.System);

            rk.Close();
            /* search system path and file of registry key
            etc. service.msc, compmgmt.msc ... */
            Global.FileDirectoryEnumerable dirEnum = new Global.FileDirectoryEnumerable();
            dirEnum.SearchPath = strPath;
            dirEnum.SearchPattern = "*.*";
            dirEnum.SearchForFile = true;
            dirEnum.SearchForDirectory = false;
            dirEnum.ReturnstringType = false;

            foreach (System.IO.FileInfo file in dirEnum)
			{
                if (file.Name == "." || file.Name == "..") continue;

                adminItemList[adminItemCount].ItemName = file.Name;

                int nIndex = adminItemList[adminItemCount].ItemName.IndexOf(".lnk");
                if (nIndex >= 0)
                    adminItemList[adminItemCount].ItemName = adminItemList[adminItemCount].ItemName.Substring(0, nIndex);
                
                string strCommand = Global.ResolveShortcut(file.FullName);

                if (strCommand == "") continue;

                if (strCommand.LastIndexOf(".msc") > 0) // Microsoft Management Console application
                {
                    adminItemList[adminItemCount] = adminItemList[adminItemCount];
                    adminItemList[adminItemCount].ProcFullName = strSystemPath + @"\" + "mmc.exe";
                    adminItemList[adminItemCount].CmdLine = strCommand;
                }
                else
                {
                    adminItemList[adminItemCount].ProcFullName = strCommand;
                    adminItemList[adminItemCount].CmdLine = "";
                }

                if(++adminItemCount == Global.MAX_ADMIN_ITEM_LIST_SIZE)
			        break;

			}
            dirEnum.Close();
        }

        public bool Init()
        {
            adminProcessList.Clear();
            return true;
        }

        /* if parameter "pProcess" is admin tools component, remove it from processlist
         and write the contents to tracklog.txt with catagory = [ADMIN] */
        public bool CheckAdminToolsProcess(bool bExit, ref Global.ProcessEntry pProcess)
        {
            if (bExit)
            {
                foreach (AdminProcess proc in adminProcessList)
                {
                    if (pProcess.dwPID != proc.ProcessEntry.dwPID)
                        continue;

                    AgentServices.trackLog.WriteTrackLog(LOG_TYPE.LOG_ADMIN_EXIT, proc.AdminItem.ItemName, (uint)proc.ProcessEntry.dwSID);

                    adminProcessList.Remove(proc);	// delete
                    return true;
                }

                return false;
            }

            // Start admin process
            try
            {
                string szCmdLine = "";
                string wmiQuery = "select CommandLine from Win32_Process where ProcessId=" + pProcess.dwPID.ToString();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
                ManagementObjectCollection retObjectCollection = searcher.Get();
                foreach (ManagementObject retObject in retObjectCollection)
                {
                    szCmdLine = retObject["CommandLine"].ToString();
                }
                //

                //Process pProc = Process.GetProcessById(pProcess.dwPID);
                //ProcessStartInfo ps = new ProcessStartInfo();
                //string szCmdLine = Global.GetProcessCmdline(pProcess.dwPID);//pProc.MainModule.FileName;
                AdminProcess adminProcess = new AdminProcess();

                for (uint i = 0; i < adminItemCount; i++)
                {
                    if (adminItemList[i].ProcFullName == null)
                        continue;

                    if (adminItemList[i].ProcFullName.IndexOf(pProcess.strProcName) < 0)	// Process Name
                        continue;

                    if (adminItemList[i].CmdLine == "" || szCmdLine.IndexOf(adminItemList[i].CmdLine, StringComparison.OrdinalIgnoreCase) >= 0)				// Command Line if exist
                    {
                        adminProcess.ProcessEntry.dwPID = pProcess.dwPID;
                        adminProcess.ProcessEntry.dwSID = pProcess.dwSID;

                        adminProcess.AdminItem = adminItemList[i];

                        adminProcessList.Add(adminProcess);

                        AgentServices.trackLog.WriteTrackLog(LOG_TYPE.LOG_ADMIN_START, adminItemList[i].ItemName, (uint)pProcess.dwSID);

                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
            
            return false;
        }

        // const
        const string SHELLFOLDERS_REG_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
        const string ADMINTOOLS_REG_KEY = @"Common Administrative Tools";
        private int     adminItemCount;

        // Tracking control Process in Administrative tools
        private List<AdminProcess>      adminProcessList;
        private AdminItem[]             adminItemList;
    }
}
