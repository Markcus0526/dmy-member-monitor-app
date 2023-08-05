using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;
using System.Data;

namespace AgentEngine
{
    #region LOGTYPE_DEFINE
    //////////////////////////////////////////////////////////////////////////
    // LogType
    //////////////////////////////////////////////////////////////////////////
    public enum LOG_TYPE
    {
        LOG_GENERAL_START,
        LOG_GENERAL_STOP,
        LOG_GENERAL_EXIT,

        LOG_REMOTE_LOGON,
        LOG_REMOTE_LOGOFF,
        LOG_REMOTE_RECONNECT,
        LOG_REMOTE_DISCONNECT,

        LOG_FILE_CREATE,
        LOG_FILE_DELETE,
        LOG_FILE_RENAME,

        LOG_APP_START,
        LOG_APP_EXIT,

        LOG_ADMIN_START,
        LOG_ADMIN_EXIT,

        LOG_COMP_INSTALL,
        LOG_COMP_UNINSTALL,

        LOG_SERVICE_START,
        LOG_SERVICE_STOP,
        LOG_SERVICE_PAUSED,

        LOG_NET_ADD,
        LOG_NET_DELETE,
        LOG_NET_MODIFY,

        LOG_DEV_CHANGE,
        LOG_DEV_JOIN,
        LOG_DEV_REMOVE,

        LOG_TASK_END,

        LOG_ACCOUNT_CHANGE,
        LOG_ACCOUNT_RESET,
        LOG_ACCOUNT_LOCK,
        LOG_ACCOUNT_UNLOCK,

        LOG_FOLDER_OPT_CHANGE,

        LOG_PWD_CHANGE,
        LOG_PWD_RESET,

        LOG_EVENT,

        LOG_END
    }
    #endregion

    #region TRACKLOGQUEUE_DEFINE
    //////////////////////////////////////////////////////////////////////////
    // TrackLogQueue Class Define
    //////////////////////////////////////////////////////////////////////////
    class TrackLogQueue
    {
        public TrackLogQueue()
        {
            TrackLogBuffer = new String[LOG_QUEUE_LEN];

            nTail = 0;
            nWrited = 0;

            bWaitFull = false;
        }

        public bool AddTail(ref String szData)
        {
            if (bWaitFull)
                return false;

            TrackLogBuffer[nTail++] = szData;
            nTail %= LOG_QUEUE_LEN;

            // Distance is equal to Queue length
            if (nTail == nWrited)
                bWaitFull = true;

            return true;
        }

        public bool GetWriteItem(ref String strData)
        {
            if (nWrited == nTail)
            {
                if (bWaitFull)
                    bWaitFull = false;
                else	/* All queue data writted */
                {
                    nTail = nWrited = 0;
                    return false;
                }
            }

            strData = TrackLogBuffer[nWrited];
            TrackLogBuffer[nWrited] = "";

            nWrited++;
            nWrited %= LOG_QUEUE_LEN;

            return true;
        }

        public int GetWaitSize()
        {
            int n = nTail - nWrited;

            return ((n >= 0) ? n : (n + LOG_QUEUE_LEN));
        }

        private const int LOG_QUEUE_LEN = 100;

        private int nTail, nWrited;
        private bool bWaitFull;
        private String[] TrackLogBuffer;
    }
    #endregion

    #region TRACKLOGDATA_DEFINE
    //////////////////////////////////////////////////////////////////////////
    // TrackLogData Class Define
    //////////////////////////////////////////////////////////////////////////
    class TrackLogData
    {
        public TrackLogData()
        {
            Empty();
        }

        public TrackLogData(ref String strData, uint nSID, ref DateTime TimeStamp)
        {
            nSession = nSID;

            strTrackData = strData;
            this.TimeStamp = TimeStamp;
        }

        public void Empty()
        {
            TimeStamp = DateTime.Now;
            strTrackData = "";
        }

        public uint nSession;
        public String strTrackData;
        public DateTime TimeStamp;
    }
    #endregion
    //////////////////////////////////////////////////////////////////////////
    // TrackLog Class Define
    //////////////////////////////////////////////////////////////////////////
    public class TrackLog
    {
        // Construction
        public TrackLog()
        {
            bStart = false;
            nCurrentSID = 0;

            trackingItem = new TrackLogData();
            trackingQueue = new TrackLogQueue();

            Process currentProcess = Process.GetCurrentProcess();
            nCurrentSID = (uint)currentProcess.SessionId;

#if USE_SQL_DATABASE
            bDBConnected = false;
#endif
        }

        public TrackLog(string logFile, string logidFile)
        {
            bStart = false;

            strFile = logFile;
            strIdFile = logidFile;

            trackingQueue = new TrackLogQueue();
            trackingItem = new TrackLogData();

#if USE_SQL_DATABASE            
            bDBConnected = false;
#endif

            Process P = Process.GetCurrentProcess();
            nCurrentSID = (uint)P.SessionId;
        }

        public bool WriteTrackLog(LOG_TYPE nType, String strData)
        {
            trackingItem.strTrackData = strData;
            trackingItem.nSession = nCurrentSID;
            trackingItem.TimeStamp = DateTime.Now;

	        return RecordLog(nType);
        }

        public bool WriteTrackLog(LOG_TYPE nType, String strData, uint nSID)
        {
            trackingItem.strTrackData = strData;
            trackingItem.nSession = nSID;
            trackingItem.TimeStamp = DateTime.Now;

	        return RecordLog(nType);
        }

        public bool WriteTrackLog(LOG_TYPE nType, String strData, uint nSID, ref DateTime cTimeStamp)
        {
            trackingItem.strTrackData = strData;
            trackingItem.TimeStamp = cTimeStamp;

            if (nType == LOG_TYPE.LOG_EVENT || nType == LOG_TYPE.LOG_COMP_INSTALL ||
                nType == LOG_TYPE.LOG_COMP_UNINSTALL)
                trackingItem.nSession = nCurrentSID;
            else
                trackingItem.nSession = nSID;

            return RecordLog(nType);
        }

        public void StartTrackLog()
        {
            bStart = true;
#if USE_SQL_DATABASE
            DBConnection = new SqlConnection();
            DBConnection.ConnectionString =
                    "Server=" + Global.GetSqlServerName() + ";database=WindowsLogs;uid=sa;pwd=sa;";
            CreateDBThread();
#endif
        }

        public void StopTrackLog()
        {
            bStart = false;
#if USE_SQL_DATABASE
            TerminateDBThread();
#endif
        }
        
        public bool IsStartTrackLog() { return bStart; }

        private bool RecordLog(LOG_TYPE nType)
        {
	        // Log Stop Status?
	        if(!bStart)
		        return false;
	
	        if (!CheckRemoteSession(nType))
		        return false;

            FileStream LogFile = null;

            // Is Exist Log File ?
            Monitor.Enter(syncObj);

            // Add Tail in TrackLog Queue
            string strRet = MakeTrackLog(nType);
            if (!trackingQueue.AddTail(ref strRet))
            {
                if (!msg_overflow)
                {	/* called only once */
                    ComMisc.LogErrors("Log Queue Over Flow");
                    msg_overflow = true;
                }
            }

            try
            {
                bool blFileExist = File.Exists(strFile);
                if (!blFileExist)
                {
                    LogFile = new FileStream(strFile, FileMode.Create, FileAccess.ReadWrite);
                    // Log File Open Failure, Only Add Tail in TrackLog Queue, Return
                    if (LogFile == null)
                    {
                        Monitor.Exit(syncObj);
                        return false;
                    }
                    else
                        LogFile.Close();
                }
                else
                {
                    LogFile = new FileStream(strFile, FileMode.Open, FileAccess.ReadWrite);
                    if (!LogFile.CanWrite)
                    {
                        LogFile.Close();
                        return false;
                    }
                    LogFile.Close();
                }                

                StreamWriter sw = new StreamWriter(strFile, true);

                // Get Head in TrackLog Queue
                String strTrackLog = "";

                while (trackingQueue.GetWriteItem(ref strTrackLog))
                {
                    //WriteStringBytes(strTrackLog, LogFile);
                    sw.Write(strTrackLog + "\r\n");

#if USE_SQL_DATABASE
                    // write strLog to database
                    if (bDBConnected)
                    {
                        try
                        {
                            DBConnection.Open();

                            String qryData;
                            char[] delimiter = { '\t' };
                            string[] strItem = strTrackLog.Split(delimiter);

                            // [0]: number, [1]: category, [2]: description
                            // [3]: logtime, [4]: user, [5]: domain
                            // [6]: clientIP, [7]: client Hostname
                            qryData = " INSERT INTO dbo.logdata (domainname, userid, logtime, logtext, keylogger) VALUES(" +
                                "'" + strItem[5] + "', " +
                                "'" + strItem[4] + "', " +
                                "'" + strItem[3] + "', " +
                                "'" + strItem[2] + "', " +
                                "'" + " " + "');";

                            SqlCommand cmd = new SqlCommand(qryData, DBConnection);
                            SqlDataReader rd = cmd.ExecuteReader();
                            rd.Close();
                        }
                        catch (Exception ex)
                        {
                            ComMisc.LogErrors(ex.ToString());
                        }
                    }
#endif
                    /* Usually write one item */
                    if (trackingQueue.GetWaitSize() < QUEUE_EXCESS_THRESHOLD)
                        break;

                    /* If Exit or Stop, write all queue items to file else write one item */
                    if (nType != LOG_TYPE.LOG_GENERAL_EXIT && nType != LOG_TYPE.LOG_GENERAL_STOP)
                        break;
                }

                sw.Flush();
                sw.Close();                
            }
            catch (System.Exception e)
            {
                ComMisc.LogErrors(e.ToString());
            }
            finally
            {
#if USE_SQL_DATABASE
                try
                {
                    DBConnection.Close();
                }
                catch (Exception ex)
                {
                    ComMisc.LogErrors(ex.ToString());
                }
#endif
            }

            Monitor.Exit(syncObj);

	        return true;
        }

        private bool CheckRemoteSession(LOG_TYPE nType)
        {
        #if LOG_ONLY_REMOTE_SESSION
            LOG_TYPE lType = nType;
	        if(trackingItem.nSession == 0)
	        {
		        switch (lType)
		        {
			        /* Record also for console session */
                    case LOG_TYPE.LOG_GENERAL_START:
                    case LOG_TYPE.LOG_GENERAL_STOP:
                    case LOG_TYPE.LOG_GENERAL_EXIT:
                    case LOG_TYPE.LOG_NET_ADD:
                    case LOG_TYPE.LOG_NET_DELETE:
                    case LOG_TYPE.LOG_NET_MODIFY:
                    case LOG_TYPE.LOG_DEV_JOIN:
                    case LOG_TYPE.LOG_DEV_CHANGE:
                    case LOG_TYPE.LOG_DEV_REMOVE:
                    case LOG_TYPE.LOG_FOLDER_OPT_CHANGE:
                    case LOG_TYPE.LOG_PWD_CHANGE:
                    case LOG_TYPE.LOG_PWD_RESET:
                    case LOG_TYPE.LOG_EVENT:
                    case LOG_TYPE.LOG_SERVICE_PAUSED:
                    case LOG_TYPE.LOG_SERVICE_START:
                    case LOG_TYPE.LOG_SERVICE_STOP:
                    case LOG_TYPE.LOG_COMP_INSTALL:
                    case LOG_TYPE.LOG_COMP_UNINSTALL:
				        return true;
                }
		        /* Record only for remote session */
		        return false;
	        }
#endif	// LOG_ONLY_REMOTE_SESSION
            return true;
        }

        public static byte[] WriteStringBytes(string str, FileStream fs)
        {
            str += "\r\n";
            byte[] info = new UTF8Encoding().GetBytes(str);
            fs.Write(info, 0, info.Length);
            return info;
        }

        private String MakeTrackLog(LOG_TYPE nType)
        {            
	        String strLogMark, strTimeStamp;
            String strLogData = trackingItem.strTrackData;

	        // Make Log Data
	        switch(nType)
	        {
		    case LOG_TYPE.LOG_GENERAL_START :
                strLogMark = "[GENERAL]";
                strLogData = "--- Tracking started";
			    break;
            case LOG_TYPE.LOG_GENERAL_STOP:
                strLogMark = "[GENERAL]";
                strLogData = "--- Tracking stopped";
			    break;
            case LOG_TYPE.LOG_GENERAL_EXIT:
                strLogMark = "[GENERAL]";
                strLogData = "--- Exit Tracking";
			    break;
            case LOG_TYPE.LOG_REMOTE_LOGON:
            case LOG_TYPE.LOG_REMOTE_LOGOFF:
            case LOG_TYPE.LOG_REMOTE_RECONNECT:
            case LOG_TYPE.LOG_REMOTE_DISCONNECT:
                strLogMark = "[SESSION]";
			    break;
            case LOG_TYPE.LOG_FILE_CREATE:
                strLogMark = "[FILE]";
                strLogData += " Created";
			    break;
            case LOG_TYPE.LOG_FILE_DELETE:
                strLogMark = "[FILE]";
                strLogData += " Deleted";
			    break;
            case LOG_TYPE.LOG_FILE_RENAME:
                strLogMark = "[FILE]";
                strLogData += " Renamed";
			    break;
            case LOG_TYPE.LOG_APP_START:
                strLogMark = "[APP]";
                strLogData += " Started";
			    break;
            case LOG_TYPE.LOG_APP_EXIT:
                strLogMark = "[APP]";
                strLogData += " Exited";
			    break;
            case LOG_TYPE.LOG_COMP_INSTALL:
                strLogMark = "[INSTALL]";
                strLogData += " Installed";
			    break;
            case LOG_TYPE.LOG_COMP_UNINSTALL:
                strLogMark = "[INSTALL]";
                strLogData += " Uninstalled";
			    break;
            case LOG_TYPE.LOG_ADMIN_START:
                strLogMark = "[ADMIN]";
                strLogData += " Started";
			    break;
            case LOG_TYPE.LOG_ADMIN_EXIT:
                strLogMark = "[ADMIN]";
                strLogData += " Exited";
			    break;
            case LOG_TYPE.LOG_SERVICE_PAUSED:
                strLogMark = "[SERVICE]";
                strLogData += " Paused";
			    break;
            case LOG_TYPE.LOG_SERVICE_START:
                strLogMark = "[SERVICE]";
                strLogData += " Started";
			    break;
            case LOG_TYPE.LOG_SERVICE_STOP:
                strLogMark = "[SERVICE]";
                strLogData += " Stopped";
			    break;
            case LOG_TYPE.LOG_NET_ADD:
            case LOG_TYPE.LOG_NET_DELETE:
            case LOG_TYPE.LOG_NET_MODIFY:
                strLogMark = "[NETWORK]";
			    break;
            case LOG_TYPE.LOG_DEV_JOIN:
            case LOG_TYPE.LOG_DEV_CHANGE:
            case LOG_TYPE.LOG_DEV_REMOVE:
                strLogMark = "[DEVICE]";
			    break;
            case LOG_TYPE.LOG_FOLDER_OPT_CHANGE:
                strLogMark = "[FOLDER]";
			    break;
            case LOG_TYPE.LOG_PWD_CHANGE:
            case LOG_TYPE.LOG_PWD_RESET:
                strLogMark = "[PASSWD]";
			    break;
            case LOG_TYPE.LOG_EVENT:
                strLogMark = "[EVENT]";
			    break;
            case LOG_TYPE.LOG_TASK_END:
                strLogMark = "[TASK]";
                strLogData += " End";
			    break;
		    default :
                strLogMark = "[Unknown]";
                strLogData = "Unknown";
                break;
	        }

            //szTimeStamp = trackingItem.TimeStamp.ToLongDateString();
            strTimeStamp = String.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}",
                trackingItem.TimeStamp.Year, trackingItem.TimeStamp.Month, trackingItem.TimeStamp.Day,
                trackingItem.TimeStamp.Hour, trackingItem.TimeStamp.Minute, trackingItem.TimeStamp.Second);

            String strUser = "", strDomain = "", strIP = "", strHost = "", strLog = "";

            bool ret = GetSessionInfo(trackingItem.nSession, ref strUser, ref strDomain, ref strIP, ref strHost);

#if REC_LOG_NUMBER
            uint nIndex = Global.GetWriteIndex(strIdFile, strFile);
            if (ret)
		        strLog = string.Format(
                    "{0:d}", nIndex) + "\t" +
                    strLogMark + "\t" +
                    strLogData + "\t" +
                    strTimeStamp + "\t" +
                    strUser + ("\t") +
                    strDomain + "\t" +
                    strIP + "\t" +
                    strHost;
	        else
                strLog = string.Format(
                    "{0:d}", nIndex) + "\t" +
                    strLogMark + "\t" +
                    strLogData + "\t" +
                    strTimeStamp;
#else
            if (ret)
	        {
                if (strHost.Length == 0)	/* console session */
                    strLog = String.Format("{0}\t{1}\t{2}\t{3}@{4}\r\n",
                    strLogMark, strLogData, strTimeStamp, strUser, strDomain);
		        else
                    strLog = String.Format("{0}\t{1}\t{2}\t{3}@{4}\t{5}({6})\r\n",
                    strLogMark, strLogData, strTimeStamp, strUser, strDomain, strIP, strHost);
	        }
	        else
	        {
                strLog = String.Format("{0}\t{1}\t{2}\t\r\n", strLogMark, strLogData, strTimeStamp);
	        }
        #endif

            return strLog;
        }

        public bool GetSessionInfo(uint nSid, ref String strUser, ref String strDom, ref String strIP, ref String strHost)
        {
	        uint dwBytesReturned = 0;
            IntPtr pBuf = IntPtr.Zero;

	        /* User Name */
            if (!Win32.WTSQuerySessionInformation(Win32.WTS_CURRENT_SERVER_HANDLE, (int)nSid, Win32.WTS_INFO_CLASS.WTSUserName, out pBuf, out dwBytesReturned))
		        return false;

            strUser = Marshal.PtrToStringUni(pBuf);
            Win32.WTSFreeMemory(pBuf);

	        /* Domain Name */
	        dwBytesReturned  = 0;
            pBuf = IntPtr.Zero;

            if (!Win32.WTSQuerySessionInformation(Win32.WTS_CURRENT_SERVER_HANDLE, (int)nSid, Win32.WTS_INFO_CLASS.WTSDomainName, out pBuf, out dwBytesReturned))
                return false;

	        strDom = Marshal.PtrToStringUni(pBuf);
            Win32.WTSFreeMemory(pBuf);

	        /* Client Name */
	        dwBytesReturned = 0;
            pBuf = IntPtr.Zero;

            if (!Win32.WTSQuerySessionInformation(Win32.WTS_CURRENT_SERVER_HANDLE, (int)nSid, Win32.WTS_INFO_CLASS.WTSClientName, out pBuf, out dwBytesReturned))
                return false;

            strHost = Marshal.PtrToStringUni(pBuf);
            Win32.WTSFreeMemory(pBuf);

	        if (strHost.Length == 0)
                return true;

	        /* Client Address */
            dwBytesReturned = 0;
            pBuf = IntPtr.Zero;
            Win32.WTS_CLIENT_ADDRESS buffer;

            if (!Win32.WTSQuerySessionInformation(Win32.WTS_CURRENT_SERVER_HANDLE, (int)nSid, Win32.WTS_INFO_CLASS.WTSClientAddress, out pBuf, out dwBytesReturned))
                return false;

            buffer = (Win32.WTS_CLIENT_ADDRESS)Marshal.PtrToStructure(pBuf, typeof(Win32.WTS_CLIENT_ADDRESS));
            strIP = string.Format("{0:d}.{1:d}.{2:d}.{3:d}", buffer.Address[2], buffer.Address[3], buffer.Address[4], buffer.Address[5]);
            Win32.WTSFreeMemory(pBuf);
	
	        return true;
        }

#if USE_SQL_DATABASE
        //////////////////////////////////////////////////////////////////////////
        // Function Name   : CreateDBThread
        //////////////////////////////////////////////////////////////////////////
        private bool CreateDBThread()
        {
            watchStop = false;
            dbThread = new Thread(new ThreadStart(DBConnectEventHandler));
            dbThread.Start();

            return (dbThread == null ? false : true);
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name   : TerminateRDPThread
        //////////////////////////////////////////////////////////////////////////
        private void TerminateDBThread()
        {
            Thread.Sleep(1);
            RequestStop();
        }

        public void RequestStop()
        {
            watchStop = true;
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: DBConnectEventHandler
        //////////////////////////////////////////////////////////////////////////
        private void DBConnectEventHandler()
        {
            while (!watchStop)
            {
                bDBConnected = false;

                try
                {
                    if (DBConnection.State == ConnectionState.Closed)
                    {
                        DBConnection.Open();
                        bDBConnected = true;
                        DBConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                    bDBConnected = false;
                    ComMisc.LogErrors(ex.ToString());
                }
                finally
                {
                    Thread.Sleep(5000);
                }

            }
        }
#endif

        const uint QUEUE_EXCESS_THRESHOLD = 10;

        static bool msg_overflow = false;
        static object syncObj = new object();

        private bool bStart;
        private uint nCurrentSID;

        private TrackLogQueue trackingQueue = null;
        private TrackLogData trackingItem = null;

        // variable
        public String strFile;		/* log file path */
        public String strIdFile;    /* log id file path */
#if USE_SQL_DATABASE
        SqlConnection DBConnection;
        bool bDBConnected;
        Thread dbThread;
        private volatile bool watchStop;
#endif
    }
}
