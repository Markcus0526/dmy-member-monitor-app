using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using AgentEngine;
using System.Collections;
using INMC.INMMessage;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Management;

namespace AgentEngine
{
    public class FileOpTracker
    {
        public FileOpTracker()
        {
        }

        public FileOpTracker(TrackLog pTrackLog)
        {
#if TRACK_LOGFILE
            trackLog = pTrackLog;
#endif

            strRecArr = new String[MAX_RECORD_FILES];
            logType = 0;
            logCount = 0;
            timeStamp = 0;

            // If not correct, use the Notification's function.
            strRecent = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Recent);

            nCurSId = 0;

            GetCurSharedList();

            Process CurProcess = Process.GetCurrentProcess();
            nCurSId = CurProcess.SessionId;

            RestoreFileHistoryFromFile();
        }

        public void RefreshShareList()
        {
            Monitor.Enter(fileobj);
            ArrayList prevShareList = new ArrayList();

            foreach (M5FileItem item in curshareList)
            {
                M5FileItem previtem = new M5FileItem(item);
                prevShareList.Add(previtem);
            }

            GetCurSharedList();

            int nCount = prevShareList.Count;
            foreach (M5FileItem item in curshareList)
            {
                bool bExist = false;

                for (int i = 0; i < nCount; i++)
                {
                    M5FileItem previtem = (M5FileItem)prevShareList[i];
                    if (item.FileName == previtem.FileName &&
                        item.Path == previtem.Path)
                    {
                        previtem.FileType = "";
                        bExist = true;
                        break;
                    }
                }

                if (bExist == false)
                {
                    M5FileItem newitem = new M5FileItem(item);
                    newitem.Action = "Share Started";
                    fileHisList.Add(newitem);
                    AddFileHistoryToFile(newitem);
                }
            }

            foreach (M5FileItem item in prevShareList)
            {
                if (item.FileType != "")
                {
                    M5FileItem newitem = new M5FileItem(item);
                    newitem.ActionTime = DateTime.Now;
                    newitem.Action = "Share Stopped";
                    fileHisList.Add(newitem);
                    AddFileHistoryToFile(newitem);
                }
            }

            Monitor.Exit(fileobj);
        }

        private void GetCurSharedList()
        {
            curshareList.Clear();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Share");
                ManagementObjectCollection collection = searcher.Get();
                System.Management.ManagementObjectCollection.ManagementObjectEnumerator enumerator = collection.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    ManagementObject obj = (ManagementObject)enumerator.Current;

                    M5FileItem newitem = new M5FileItem();
                    newitem.Action = "Shared";

                    String strDate = (String)obj["InstallDate"];
                    if (strDate == null || strDate == "")
                        newitem.ActionTime = DateTime.Now;
                    else
                        newitem.ActionTime = DateTime.Parse(strDate);
                    newitem.FileName = (string)obj["Name"];
                    newitem.Path = (string)obj["Path"];

                    uint nType = (uint)obj["Type"];
                    newitem.FileType = "Dir";

                    curshareList.Add(newitem);
                }
            }
            catch (SystemException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // Check for valid string
        private bool CheckExceptions(ref String szData)
        {
            int nLength = szData.Length;
            int nFindPos = szData.IndexOf(strRecent);

            /* Check wheter is blank string */
            if (szData == "")
                return false;

            /* Check whether is in Recent */
            if (szData.EndsWith(".lnk") && nFindPos == 0)
                return false;

            /* Check whether is in Recycle bin */
            nFindPos = szData.IndexOf(@":\RECYCLE.BIN");
            if (nFindPos == 1)
                return false;

            nFindPos = szData.IndexOf(@":\RECYCLER");
            if (nFindPos == 1)
                return false;

            return true;
        }

        // Record the user's file operation
        public void Record(NotifyInfos info)
        {
            Monitor.Enter(fileobj);

            LOG_TYPE lType;
	        long timeTick = 0;
	        string strBuffer;

            if (info.Item1 == "")
            {
                Monitor.Exit(fileobj);
                return;
            }

            M5FileItem item = new M5FileItem(); 

            switch (info.Notification)
	        {
		        case ShellNotifications.SHCNE.SHCNE_CREATE:
                case ShellNotifications.SHCNE.SHCNE_MKDIR:
                    lType = LOG_TYPE.LOG_FILE_CREATE;
                    strBuffer = info.Item1;
                    item.Action = "Create";

                    if (info.Notification == ShellNotifications.SHCNE.SHCNE_CREATE)
                        item.FileType = "File";
                    else
                        item.FileType = "Dir";
			        break;
                case ShellNotifications.SHCNE.SHCNE_DELETE:
                case ShellNotifications.SHCNE.SHCNE_RMDIR:
                    lType = LOG_TYPE.LOG_FILE_DELETE;
                    strBuffer = info.Item1;
                    item.Action = "Delete";

                    if (info.Notification == ShellNotifications.SHCNE.SHCNE_DELETE)
                        item.FileType = "File";
                    else
                        item.FileType = "Dir";
			        break;
                case ShellNotifications.SHCNE.SHCNE_RENAMEITEM:
                case ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER:
                    lType = LOG_TYPE.LOG_FILE_RENAME;
                    strBuffer = String.Format("{0:s} -> {1:s}", info.Item1, info.Item2);
                    item.Action = "Rename";

                    if (info.Item2 == "")
                    {
                        Monitor.Exit(fileobj);
                        return;
                    }

                    if (info.Notification == ShellNotifications.SHCNE.SHCNE_RENAMEITEM)
                        item.FileType = "File";
                    else
                        item.FileType = "Dir";
			        break;
		        default :
                    Monitor.Exit(fileobj);
			        return;
	        }

            if (!CheckExceptions(ref strBuffer))
            {
                Monitor.Exit(fileobj);
                return;
            }

            timeTick = System.Environment.TickCount;

            if (lType != logType || (timeTick - timeStamp) > RECORD_TIME_THRESHOLD)
	        {
		        /* New command */
                logType = lType;
		        logCount = 0;
                timeStamp = timeTick;
	        }
	        else
	        {
                timeStamp = timeTick;

		        /* Find to matching item to prevent duplicate log */
		        for(int i = 0; i < logCount; i++)
		        {
                    if (strRecArr[i] == info.Item1)
                    {
                        Monitor.Exit(fileobj);
                        return;
                    }
		        }
	        }

            strRecArr[logCount] = info.Item1;
	        logCount++;

	        if (logCount >= MAX_RECORD_FILES)
		        logCount = MAX_RECORD_FILES - 1;

            item.ActionTime = DateTime.Now;

            int nSlashIdx = info.Item1.LastIndexOf('\\');
            if (nSlashIdx != -1)
            {
                item.Path = info.Item1.Substring(0, nSlashIdx);
                item.FileName = info.Item1.Substring(nSlashIdx + 1, info.Item1.Length - nSlashIdx - 1);
            }
            else
            {
                item.Path = "";
                item.FileName = info.Item1;
            }

            if (item.Action == "Rename")
            {
                String fChangedName = "";
                nSlashIdx = info.Item2.LastIndexOf('\\');
                if (nSlashIdx != -1)
                    fChangedName = info.Item2.Substring(nSlashIdx + 1, info.Item2.Length - nSlashIdx - 1);
                else
                    fChangedName = info.Item2;

                item.FileName += " -> " + fChangedName;
            }

            if (IsItemInArrayList(item, fileHisList) == false)
            {
                fileHisList.Add(item);
                AddFileHistoryToFile(item);
            }

#if TRACK_LOGFILE
            trackLog.WriteTrackLog(lType, strBuffer);
#endif

            Monitor.Exit(fileobj);
        }

        public void GetFileHistory(ArrayList hisList)
        {
            Monitor.Enter(fileobj);

            hisList.Clear();

            foreach (M5FileItem item in fileHisList)
            {
                M5FileItem newitem = new M5FileItem(item);
                hisList.Add(newitem);
            }

            fileHisList.Clear();
            ClearFileHistory();

            Monitor.Exit(fileobj);
        }

        public void RestoreFileHistoryFromFile()
        {
            String strDelimiter = "|";
            fileHisList.Clear();

            try
            {
                StreamReader writeStream = File.OpenText(System.Environment.SystemDirectory + "\\" + strHisFileName + nCurSId);

                while (writeStream.EndOfStream == false)
                {
                    M5FileItem fileItem = new M5FileItem();
                    String strLine = writeStream.ReadLine();

                    int nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Action Name
                    fileItem.Action = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get ActionTime
                    fileItem.ActionTime = DateTime.Parse(strLine.Substring(0, nKeyPos));
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get FileName
                    fileItem.FileName = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get FileType
                    fileItem.FileType = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                    {
                        // Get Path
                        fileItem.Path = strLine;
                    }
                    else
                    {
                        // Get Path
                        fileItem.Path = strLine.Substring(0, nKeyPos);
                    }

                    fileHisList.Add(fileItem);
                }
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void AddFileHistoryToFile(M5FileItem fileItem)
        {
            try
            {
                StreamWriter writeStream = File.AppendText(System.Environment.SystemDirectory + "\\" + strHisFileName + nCurSId);

                String strLine = "";
                strLine = fileItem.Action + "|" + fileItem.ActionTime + "|" + fileItem.FileName + "|" + fileItem.FileType + "|" + fileItem.Path + "\r\n";
                writeStream.Write(strLine);
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void ClearFileHistory()
        {
            fileHisList.Clear();

            try
            {
                StreamWriter writeStream = File.CreateText(System.Environment.SystemDirectory + "\\" + strHisFileName + nCurSId);
                writeStream.Write("");
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public bool IsItemInArrayList(M5FileItem item, ArrayList itemList)
        {
            bool bExist = false;

            foreach (M5FileItem listitem in itemList)
            {
                if (listitem.Action == item.Action &&
                    listitem.ActionTime == item.ActionTime &&
                    listitem.FileName == item.FileName &&
                    listitem.FileType == item.FileType &&
                    listitem.Path == item.Path)
                {
                    bExist = true;
                    break;
                }
            }

            return bExist;
        }

        // Record File History
        private const int RECORD_TIME_THRESHOLD = 2000;			// 2s
        private const int MAX_RECORD_FILES = 100;               /* Max record number for multiple remove */

        LOG_TYPE logType;
        int logCount;

        long timeStamp;

        String[] strRecArr;
        String strRecent;

#if TRACK_LOGFILE
        // TrackLog for Writing Log File
        TrackLog trackLog;
#endif

        // List for Saving File History
        private int nCurSId = 0;
        ArrayList fileHisList = new ArrayList();
        private const String strHisFileName = "INMFileHis_";

        ArrayList curshareList = new ArrayList();

        // Thread Management
        object fileobj = new object();
    }
}
