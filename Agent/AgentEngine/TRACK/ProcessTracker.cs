using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AgentEngine;
using INMC.INMMessage;
using System.Collections;
using System.IO;
using System.Threading;

namespace AgentEngine
{
    public class ProcessTracker : PollingTracker
    {
        // Thread Management
        private object procobj = new object();

        ArrayList prohibitApp = new ArrayList();
        ArrayList curProcMon = new ArrayList();

        public bool bCheckProcess = false;
        public bool bCheckRunningApps = false;

        private const String strAppConFileName = "INMAppControl_";
        private DateTime dtLastAccessApp;

        object procObj = new object();

        public ProcessTracker()
        {
        }

        public ProcessTracker(TrackLog pTrackLog)
        {
#if CONFIG_SERVICE
            bIsAdmin = true;
#endif
            trackLog = pTrackLog;

            ProcessList = new Global.ProcessEntry[Global.MAX_PROCESS_ENTRY];

            nCurrentSID = 0;

            Process CurProcess = Process.GetCurrentProcess();
            nCurrentSID = CurProcess.SessionId;

            dtLastAccessApp = DateTime.MinValue;
            RestoreAppBlockFromFile();
        }

        protected override bool Init()
        {
            // Initialize Process List
            for (int nIndex = 0; nIndex < Global.MAX_PROCESS_ENTRY; nIndex++)
                ProcessList[nIndex].nStatus = Global.ENTRY_STATUS.UNUSED_ENTRY;

	        // Get Initial Process List
	        int j = 0;
	        foreach (Process procInfo in Process.GetProcesses())
	        {
		        if(procInfo.SessionId == 0)
			        continue;	// console session pre Vista

                if (MakeProcessEntry(procInfo, procInfo.Id, (uint)procInfo.SessionId, j) == true)
				    j++;
	        }

	        nCurrentSID = 0;

            Process CurProcess = Process.GetCurrentProcess();
	        nCurrentSID = CurProcess.SessionId;

	        return true;
        }

        // Save the process's information to the list
        private bool MakeProcessEntry(Process hProcess, int dwPid, uint dwSession, int index)
        {
            if (hProcess.ProcessName.IndexOf(AgentEngine.Properties.Resources.INMSAGENT_FILE_NAME) != -1)
                return false;

	        ProcessList[index].dwPID = dwPid;
            ProcessList[index].dwSID = (int)dwSession;
            ProcessList[index].nStatus = Global.ENTRY_STATUS.USED_ENTRY;

            try
            {
                ProcessList[index].strProcName = hProcess.ProcessName;
                ProcessList[index].strProcPath = hProcess.MainModule.FileName;
                ProcessList[index].strStartTime = hProcess.StartTime;
            }
            catch (System.Exception ex)
            {
                ProcessList[index].nStatus = Global.ENTRY_STATUS.UNUSED_ENTRY;
                ComMisc.LogErrors(ex.ToString());
                return false;
            }

	        return true;
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: LogAndCheckKill
        //////////////////////////////////////////////////////////////////////////
        private void LogAndCheckKill(int nIndex)
        {
            Global.ProcessEntry proc = ProcessList[nIndex];
            IntPtr wParam = IntPtr.Zero;
            IntPtr lParam = Marshal.AllocHGlobal(Marshal.SizeOf(proc.dwPID));
            Marshal.StructureToPtr(proc.dwPID, lParam, false);

#if CONFIG_SERVICE
	        if (bIsAdmin)
                trackLog.WriteTrackLog(LOG_TYPE.LOG_APP_EXIT, proc.strProcName, (uint)proc.dwSID);
#endif

	        if (nCurrentSID == proc.dwSID)	/* because message is received only same session */
	        {
                 trackLog.WriteTrackLog(LOG_TYPE.LOG_TASK_END, proc.strProcName, (uint)proc.dwSID);
	        }
        }

        private void LogAndCheckKill(int nIndex, ref DateTime cTime)
        {
            Global.ProcessEntry proc = ProcessList[nIndex];
            IntPtr wParam = IntPtr.Zero;
            IntPtr lParam = Marshal.AllocHGlobal(Marshal.SizeOf(proc.dwPID));
            Marshal.StructureToPtr(proc.dwPID, lParam, false);

#if CONFIG_SERVICE
	        if (bIsAdmin)
                trackLog.WriteTrackLog(LOG_TYPE.LOG_APP_EXIT, proc.strProcName, (uint)proc.dwSID, ref cTime);
#endif

	        if (nCurrentSID == proc.dwSID)	/* because message is received only same session */
	        {
                trackLog.WriteTrackLog(LOG_TYPE.LOG_TASK_END, proc.strProcName, (uint)proc.dwSID, ref cTime);
	        }
        }

        public bool CheckValidProcess(Process proc)
        {
            bool bRet = true;
            
            try
            {
                const int nChars = 256;
                StringBuilder Buff = new StringBuilder(nChars);
   			
                String procName = proc.ProcessName;
                String title = "";//proc.MainWindowTitle;
                String className = "";

                if (Win32.GetWindowText(proc.MainWindowHandle, Buff, nChars) > 0)
                {
                    title = Buff.ToString();
                }

                if (Win32.GetClassName(proc.MainWindowHandle, Buff, nChars) > 0)
                {
                    className = Buff.ToString();
                }

                foreach (M1DisableAppItem info in prohibitApp)
                {
                    if (info.bDisable == false)
                        continue;

                    if (info.strClassName != "")    // Check if ClassName is filled
                    {
                        if (info.bMatchClass)   // Compare matchcase
                        {
                            if (String.Compare(className, info.strClassName, true) == 0)
                            {
                                bRet = false;
                            }
                        }
                        else // Compare submatchcase
                        {
                            if (className.ToLower().IndexOf(info.strClassName.ToLower()) != -1)
                            {
                                bRet = false;
                            }
                        }
                    }

                    if (info.strTitle != "")    // Check if Title is filled
                    {
                        if (info.bMatchTitle)   // Compare matchcase
                        {
                            if (String.Compare(title, info.strTitle, true) == 0)
                            {
                                bRet = false;
                            }
                        }
                        else // Compare submatchcase
                        {
                            if (title.ToLower().IndexOf(info.strTitle.ToLower()) != -1)
                            {
                                bRet = false;
                            }
                        }
                    }

                    if (info.strTitle == "" && info.strClassName == "" && info.strProcName != "")   // if title and class name are blank then check process name
                    {
                        if (info.bMatchProc)   // Compare matchcase
                        {
                            if (String.Compare(procName, info.strProcName, true) == 0)
                            {
                                bRet = false;
                            }
                        }
                        else // Compare submatchcase
                        {
                            if (procName.ToLower().IndexOf(info.strProcName.ToLower()) != -1)
                            {
                                bRet = false;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Cannot Access Process's information
                ComMisc.LogErrors(ex.ToString());
            }

            return bRet;
        }

        protected override bool TrackLog()
        {
	        /* Update Process List */
	        int j = 0;

            Monitor.Enter(procObj);

            Process[] CurProcessList = Process.GetProcesses();
            foreach (Process process in CurProcessList)
            {
                if (CheckValidProcess(process) == false)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (System.Exception ex)
                    {
                    	// Unknown Error occured while killing the process
                        ComMisc.LogErrors(ex.ToString());
                    }
                }
            }

            Process pProcess;
            for (int i = 0; i < CurProcessList.Length; i++)
	        {
                pProcess = CurProcessList[i];

                if (pProcess.SessionId != nCurrentSID)
                    continue;

#if (LOG_ONLY_REMOTE_SESSION)
                if (pProcess.SessionId == 0)
                    continue;	// console session
#endif

                for (j = 0; j < Global.MAX_PROCESS_ENTRY; j++)
			    {
                    if (ProcessList[j].nStatus == Global.ENTRY_STATUS.UNUSED_ENTRY)
                        continue;

                    if (ProcessList[j].dwPID != pProcess.Id)
					    continue;

                    ProcessList[j].nStatus = Global.ENTRY_STATUS.EXIST_ENTRY;
				    break;
			    }

                if (j < Global.MAX_PROCESS_ENTRY && ProcessList[j].nStatus == Global.ENTRY_STATUS.EXIST_ENTRY)
			    {
				    continue;
			    }

			    // Add New Process To Process List
                for (j = 0; j < Global.MAX_PROCESS_ENTRY; j++)
			    {
                    if (ProcessList[j].nStatus == Global.ENTRY_STATUS.UNUSED_ENTRY)
                    {
                        if (MakeProcessEntry(pProcess, pProcess.Id, (uint)pProcess.SessionId, j) == true)
                            ProcessList[j].nStatus = Global.ENTRY_STATUS.CREATED_ENTRY;
                        break;
                    }
                }
	        }

	        /* Process Tracking */
	        bool bExitProcess;
	        DateTime CreationTime;

	        for(j = 0; j < Global.MAX_PROCESS_ENTRY; j++)
	        {
                bExitProcess = true;
		        switch(ProcessList[j].nStatus)
		        {
                    case Global.ENTRY_STATUS.EXIST_ENTRY:
                        ProcessList[j].nStatus = Global.ENTRY_STATUS.USED_ENTRY;
                        continue;
                    case Global.ENTRY_STATUS.CREATED_ENTRY:
                        ProcessList[j].nStatus = Global.ENTRY_STATUS.USED_ENTRY;
                        bExitProcess = false;
                        break;
                    case Global.ENTRY_STATUS.USED_ENTRY:
                        ProcessList[j].nStatus = Global.ENTRY_STATUS.UNUSED_ENTRY;
                        break;
                    case Global.ENTRY_STATUS.UNUSED_ENTRY:
				        continue;
                    default:
                        continue;
		        }

		        // Log only for USED_ENTRY
                if (bExitProcess == true)
		        {
			        // Exit Process Log
			        LogAndCheckKill(j);
		        }
                else
                {
                    try
                    {
                        pProcess = Process.GetProcessById(ProcessList[j].dwPID);
                    }
                    catch (Exception ex)
                    {
                        ComMisc.LogErrors(ex.ToString());
                        continue;
                    }  
                    
                    if (pProcess != null)
                    {
                        try
                        {
                            CreationTime = pProcess.StartTime;
#if CONFIG_SERVICE
                            if (bIsAdmin)
                                trackLog.WriteTrackLog(LOG_TYPE.LOG_APP_START, ProcessList[j].strProcName, (uint)ProcessList[j].dwSID, ref CreationTime);
#endif
                        }
                        catch (Exception ex)
                        {
                            ComMisc.LogErrors(ex.ToString());
                        }
                    }
                }
	        }

            Monitor.Exit(procObj);

	        return true;
        }

        public void InitList(DateTime curAccessTime, System.Collections.ArrayList newList)
        {
            prohibitApp.Clear();
            StopApps(curAccessTime, newList);
        }

        public void StopApps(DateTime curAccessTime, System.Collections.ArrayList newList)
        {
            Monitor.Enter(procObj);
            ArrayList prevList = new ArrayList();

            foreach (M1DisableAppItem item in prohibitApp)
            {
                M1DisableAppItem newitem = new M1DisableAppItem(item);
                prevList.Add(newitem);
            }
            prohibitApp.Clear();

            int iPrev = 0;
            int nCount = newList.Count;
            for (int i = 0; i < nCount; i++)
            {
                M1DisableAppItem item = (M1DisableAppItem)newList[i];

                if (item.nType == 0)    // Add
                {
                    M1DisableAppItem newitem = new M1DisableAppItem(item);
                    prohibitApp.Add(newitem);
                }
                else if (item.nType == 1)   // Modify
                {
                    iPrev = 0;
                    for (iPrev = 0; iPrev < prevList.Count; iPrev++)
                    {
                        M1DisableAppItem previtem = (M1DisableAppItem)prevList[iPrev];
                        if (previtem.bMatchProc == item.bMatchProc && previtem.strProcName == item.strProcName &&
                            previtem.bMatchClass == item.bMatchClass && previtem.strClassName == item.strClassName &&
                            previtem.bMatchTitle == item.bMatchTitle && previtem.strTitle == item.strTitle)
                            previtem.nType = -1;
                    }

                    if (i != nCount - 1)
                    {
                        M1DisableAppItem itemnew = (M1DisableAppItem)newList[i + 1];
                        M1DisableAppItem newitem = new M1DisableAppItem(itemnew);
                        prohibitApp.Add(newitem);
                        i++;
                    }
                }
                else if (item.nType == 2)   // Delete
                {
                    iPrev = 0;
                    for (iPrev = 0; iPrev < prevList.Count; iPrev++)
                    {
                        M1DisableAppItem previtem = (M1DisableAppItem)prevList[iPrev];
                        if (previtem.bMatchProc == item.bMatchProc && previtem.strProcName == item.strProcName &&
                            previtem.bMatchClass == item.bMatchClass && previtem.strClassName == item.strClassName &&
                            previtem.bMatchTitle == item.bMatchTitle && previtem.strTitle == item.strTitle)
                            previtem.nType = -1;
                    }
                }
            }

            for (iPrev = 0; iPrev < prevList.Count; iPrev++)
            {
                M1DisableAppItem previtem = (M1DisableAppItem)prevList[iPrev];
                if (previtem.nType != -1)
                {
                    M1DisableAppItem newitem = new M1DisableAppItem(previtem);
                    prohibitApp.Add(newitem);
                }
            }

            SetAppConDataToFile(curAccessTime, prohibitApp);
            Monitor.Exit(procObj);
        }

        public void GetRunningApps(System.Collections.ArrayList appList)
        {
            if (appList == null)
                return;

            appList.Clear();

            var collection = new List<string>();
            Win32.EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
            {
                StringBuilder strbTitle = new StringBuilder(255);
                int nLength = Win32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                string strTitle = strbTitle.ToString();

                if (Win32.IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
                {
                    collection.Add(strTitle);
                }
                return true;
            };

            if (Win32.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
            {
                foreach (var item in collection)
                {
                    M4AppItem newItem = new M4AppItem();
                    newItem.appName = (String)item;
                    newItem.appStatus = "Running";
                    appList.Add(newItem);
                }
            }
        }

        public void SetCheckProcess(bool bEnable)
        {
            bCheckProcess = bEnable;
        }

        public void SetCheckRunningApps(bool bEnable)
        {
            bCheckRunningApps = bEnable;
        }

        String GetDivideString(long nBytes)
        {
            try
            {
                String strRet = "";

                while (nBytes >= 1000)
                {
                    long nValue = nBytes % 1000;
                    nBytes = nBytes / 1000;

                    if (strRet != "")
                        strRet = nValue + "," + strRet;
                    else
                        strRet = String.Format("{0:D3}", nValue);
                }

                if (strRet != "")
                    strRet = nBytes + "," + strRet;
                else
                    strRet = String.Format("{0:D}", nBytes);

                return strRet + " K";
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
           
        }

        public String GetCpuUsage(String strProcName)
        {
            String strRet = "00";

            try
            {
                foreach (Global.ProcessInfo procInfo in prevProcList)
                {
                    if (procInfo.strProcName == strProcName)
                    {
                        int nValue = (int)procInfo.pfCounter.NextValue();
                        if (nValue > 100)
                            nValue = 100;
                        strRet = String.Format("{0:D2}", nValue / Environment.ProcessorCount);
                    }
                }
            }
            catch
            {
                
            }

            return strRet;
        }

        public void GetCurProcList(ArrayList proclist, bool bRealTime)
        {
            Monitor.Enter(procobj);

            proclist.Clear();

            if (bRealTime)
            {
                foreach (M8ProcItem item in curProcMon)
                {
                    M8ProcItem newitem = new M8ProcItem(item);
                    proclist.Add(newitem);
                }
            }
            else
            {
                foreach (Process procInfo in Process.GetProcesses())
                {
                    M6ProcMonItem procItem = new M6ProcMonItem();
                    try
                    {
                        procItem.strProcName = procInfo.ProcessName;
                        procItem.pId = procInfo.Id;
                        procItem.sessionId = procInfo.SessionId;
                        procItem.strProcPath = procInfo.MainModule.FileName;
                        procItem.startTime = procInfo.StartTime;
                        proclist.Add(procItem);
                    }
                    catch
                    {

                    }
                }
            }

            Monitor.Exit(procobj);
        }

        public void RefCurProcList()
        {
            Monitor.Enter(procObj);

            Process[] CurProcessList = Process.GetProcesses();
            foreach (Process process in CurProcessList)
            {
                if (CheckValidProcess(process) == false)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (System.Exception ex)
                    {
                        // Unknown Error occured while killing the process
                        ComMisc.LogErrors(ex.ToString());
                    }
                }
            }

            if (bCheckProcess == false)
            {
                Monitor.Exit(procObj);
                return;
            }

            curProcMon.Clear();
            foreach (Process procInfo in Process.GetProcesses())
            {
                M8ProcItem procItem = new M8ProcItem();
                try
                {
                    procItem.procName = procInfo.ProcessName;
                    procItem.pId = procInfo.Id;
                    procItem.sessionId = procInfo.SessionId;
                    procItem.procPath = procInfo.MainModule.FileName;
                    procItem.startTime = procInfo.StartTime;
                    procItem.memUsage = GetDivideString(procInfo.WorkingSet64 / 1024);
                    procItem.cpuUsage = GetCpuUsage(procInfo.ProcessName);

                    curProcMon.Add(procItem);
                }
                catch
                {
                	
                }
            }

            prevProcList.Clear();
            foreach (Process procInfo in Process.GetProcesses())
            {
                Global.ProcessInfo procInf = new Global.ProcessInfo();
                try
                {
                    procInf.strProcName = procInfo.ProcessName;
                    procInf.pfCounter = new PerformanceCounter("Process", "% Processor Time", procInfo.ProcessName);
                    procInf.pfCounter.NextValue();

                    prevProcList.Add(procInf);
                }
                catch (System.Exception ex)
                {
                    ComMisc.LogErrors(ex.ToString());
                }
            }

            Monitor.Exit(procObj);
        }

        public void ControlProcesses(M6ProcAction procAct)
        {
            try
            {
                if (procAct.Action == 0)    // Process Start
                {
                    String strProcessName = "";
                    if (procAct.strProcPath != null && procAct.strProcPath != "")
                        strProcessName = procAct.strProcPath + "\\";

                    strProcessName += procAct.strProcName;

                    System.Diagnostics.Process newProcess = new System.Diagnostics.Process();

                    newProcess.StartInfo.FileName = strProcessName;
                    newProcess.Start();
                }
                else    // Process Kill
                {
                    Process newProcess = Process.GetProcessById(procAct.pId);
                    newProcess.Kill();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public DateTime GetAppConSetTime()
        {
            return dtLastAccessApp;
        }

        public void RestoreAppBlockFromFile()
        {
            String strDelimiter = "|";
            prohibitApp.Clear();

            try
            {
                StreamReader writeStream = File.OpenText(System.Environment.SystemDirectory + "\\" + strAppConFileName + nCurrentSID);

                if (writeStream.EndOfStream == false)
                {
                    String strLine = writeStream.ReadLine();
                    dtLastAccessApp = DateTime.Parse(strLine);
                }

                while (writeStream.EndOfStream == false)
                {
                    M1DisableAppItem appItem = new M1DisableAppItem();
                    String strLine = writeStream.ReadLine();

                    int nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Type
                    appItem.nType = int.Parse(strLine.Substring(0, nKeyPos));
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Allow State
                    appItem.bDisable = (strLine.Substring(0, nKeyPos) == "1") ? true : false;
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Match State
                    appItem.bMatchClass = (strLine.Substring(0, nKeyPos) == "1") ? true : false;
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Class Name
                    appItem.strClassName = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Title State
                    appItem.bMatchTitle = (strLine.Substring(0, nKeyPos) == "1") ? true : false;
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Title Name
                    appItem.strTitle = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Process State
                    appItem.bMatchProc = (strLine.Substring(0, nKeyPos) == "1") ? true : false;
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                    {
                        // Get Process Name
                        appItem.strProcName = strLine;
                    }
                    else
                    {
                        // Get Process Name
                        appItem.strProcName = strLine.Substring(0, nKeyPos);
                    }

                    prohibitApp.Add(appItem);
                }
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void SetAppConDataToFile(DateTime lastAccess, ArrayList dataList)
        {
            dtLastAccessApp = lastAccess;

            try
            {
                StreamWriter writeStream = File.CreateText(System.Environment.SystemDirectory + "\\" + strAppConFileName + nCurrentSID);

                String strLine = "";
                strLine = lastAccess.ToString() + "\r\n";
                writeStream.Write(strLine);

                foreach (M1DisableAppItem item in dataList)
                {
                    strLine = "";
                    strLine = item.nType.ToString() + "|" + (item.bDisable ? "1" : "0") + "|" + (item.bMatchClass ? "1" : "0") + "|" + item.strClassName + "|" + (item.bMatchTitle ? "1" : "0") + "|" + item.strTitle + "|" + (item.bMatchProc ? "1" : "0") + "|" + item.strProcName + "\r\n";
                    writeStream.Write(strLine);
                }
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

#if CONFIG_SERVICE
        public bool       bIsAdmin;
#endif
        protected int nCurrentSID;
        private Global.ProcessEntry[] ProcessList;
        ArrayList prevProcList = new ArrayList();
    }
}
