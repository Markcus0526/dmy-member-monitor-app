using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using AgentEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Drawing2D;
using INMC.Communication.Inmc.Client;
using INMC.Communication.Inmc.Communication.EndPoints.Tcp;
using INMC.Communication.Inmc.Communication.Messages;
using System.Runtime.Serialization;
using INMC.INMMessage;
using System.Management;


namespace INMAgent
{
    public partial class INMSAgent : Form
    {
        public INMSAgent()
        {
            InitializeComponent();
            MainEngine = new EngineMain();

            mouseHook = new MouseHook();
            keyboardHook = new KeyboardHook();
        }

        public void OnStart()
        {
            MainEngine.Start();

            // Start Polling Track Log for all sessions
            initTimer.Enabled = true;
            historyTimer.Enabled = true;
            realhisTimer.Enabled = true;
            processTimer.Enabled = true;
            stateTimer.Enabled = true;
            screenTimer.Enabled = true;

#if CONFIG_LICENSE
            LicenseTimer.Enabled = true;
#endif
        }

        public void OnStop()
        {
            MainEngine.Stop();

            initTimer.Stop();
            processTimer.Stop();
            stateTimer.Stop();
            screenTimer.Stop();
            historyTimer.Stop();
            realhisTimer.Stop();
            //folderTimer.Stop();
        }

        public void OnExit()
        {
            OnStop();
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case ShellNotifications.WM_SHNOTIFY:
                    MainEngine.ChangeNotify(m.WParam, m.LParam);
                    break;
                case Global.WM_ENDSESSION:
                    OnClose(m.WParam, m.LParam);
                    break;
            }
            base.WndProc(ref m);
        }

        public bool Init()
        {
            InitTimers();
            MainEngine.Init(this.Handle);
            OnStart();

#if CONFIG_LICENSE
            DateTime dt = DateTime.Now;

            // Start License Timer
            long dwInterval = dt.Ticks % MAX_LICENSE_TIMER_INTERVAL;

            if (dwInterval < MIN_LICENSE_TIMER_INTERVAL)
                dwInterval += (dt.Ticks % MIN_LICENSE_TIMER_INTERVAL);

            LicenseTimer.Interval = dwInterval;

            if (Global.CheckValidLicense() == false)
            {
                trackLog.WriteTrackLog(LOG_TYPE.LOG_APP_EXIT, "INMSAgent Cannot Start. Allowable period has been expired.");	// Record Start Log Message
                return false;
            }
#endif  // CONFIG_LICENSE

            return true;
        }

        private void OnClose(IntPtr wParam, IntPtr lParam)
        {
            OnStop();
        }

        private void StartTracking()
        {
            initTimer.Stop();

            MainEngine.StartTracking();

            processTimer.Start();
            stateTimer.Start();
            historyTimer.Start();
            realhisTimer.Start();
            screenTimer.Start();
            //folderTimer.Start();
        }

        private void InitTimers()
        {
            initTimer = new System.Timers.Timer();
            processTimer = new System.Timers.Timer();
            stateTimer = new System.Timers.Timer();
            screenTimer = new System.Timers.Timer();
            historyTimer = new System.Timers.Timer();
            realhisTimer = new System.Timers.Timer();
            //folderTimer = new System.Timers.Timer();
#if CONFIG_LICENSE
            LicenseTimer = new System.Timers.Timer();
#endif

            // Set The Init Background Timer
            initTimer.Elapsed += new ElapsedEventHandler(OnInitTimerEvent);
            initTimer.Interval = INIT_TIMER_INTERVAL;

            // Set The Process Background Timer
            processTimer.Elapsed += new ElapsedEventHandler(OnProcessTimerEvent);
            processTimer.Interval = PROCESS_TIMER_INTERVAL;

            stateTimer.Elapsed += new ElapsedEventHandler(OnStateTimerEvent);
            stateTimer.Interval = STATE_TIMER_INTERVAL;

            // Set The History Background Timer
            historyTimer.Elapsed += new ElapsedEventHandler(OnHistoryTimerEvent);
            historyTimer.Interval = HISTORY_TIMER_INTERVAL;

            // Set The ProcessCheck Background Timer
            realhisTimer.Elapsed += new ElapsedEventHandler(OnRealHisTimerEvent);
            realhisTimer.Interval = REALHIS_TIMER_INTERVAL;

            // Set the NetManage Timer
            screenTimer.Elapsed += new ElapsedEventHandler(OnScreenTimerEvent);
            screenTimer.Interval = SCREEN_CAPTURE_TIMER_INTERVAL;

            // Set The folder options Background Timer
            //folderTimer.Elapsed += new ElapsedEventHandler(OnFolderTimerEvent);
            //folderTimer.Interval = FOLDER_OPT_TIMER_INTERVAL;

#if CONFIG_LICENSE
            // Set The License Timer
            LicenseTimer.Elapsed += new ElapsedEventHandler(OnLicenseTimerEvent);
            LicenseTimer.Interval = MIN_LICENSE_TIMER_INTERVAL;
#endif
        }

        private void INMSAgent_Load(object sender, EventArgs e)
        {
            bool blRet = Init();

            if (!blRet)
                OnExit();
        }

        protected override void SetVisibleCore(bool value)
        {
#if     UI_VISIBLE_MODE
            base.SetVisibleCore(true);
#else
            base.SetVisibleCore(false);
            bool blRet = Init();

            //mouseHook.MouseMove += new MouseEventHandler(mouseHook_MouseMove);
            mouseHook.MouseDown += new MouseEventHandler(mouseHook_MouseDown);
            mouseHook.MouseUp += new MouseEventHandler(mouseHook_MouseUp);
            mouseHook.MouseWheel += new MouseEventHandler(mouseHook_MouseWheel);

            keyboardHook.KeyDown += new KeyEventHandler(keyboardHook_KeyDown);
            keyboardHook.KeyUp += new KeyEventHandler(keyboardHook_KeyUp);
            keyboardHook.KeyPress += new KeyPressEventHandler(keyboardHook_KeyPress);

            if (!blRet)
                OnExit();
#endif
        }

        public static bool IsFirstProcess()
        {
            String szBuffer;
            Process pCur = Process.GetCurrentProcess();
            szBuffer = pCur.ProcessName;

            int iSame = 0;
            foreach (Process curProcess in Process.GetProcesses())
            {
                if (iSame >= 2)
                    break;

                String szModuleName = curProcess.ProcessName;
                if (szModuleName == szBuffer &&
                    curProcess.SessionId == pCur.SessionId)
                    iSame++;
            }

            return (iSame < 2);
        }

        // Init Timer Event Function
        public void OnInitTimerEvent(object sender, ElapsedEventArgs e)
        {
            // Init Timer
            StartTracking();
        }

        // Process Timer Event Function
        public void OnProcessTimerEvent(object sender, ElapsedEventArgs e)
        {
            // Process Timer
            MainEngine.UpdateProcessState();
        }

        // State Timer Event Function
        public void OnStateTimerEvent(object sender, ElapsedEventArgs e)
        {
            // State get,set Functions
            MainEngine.UpdateNetworkState();
            //MainEngine.netManage.GetNetworkConnection();

            if (MainEngine.netManage.bWatchNetwork == true)
            {
                M1BandMon bandMon = new M1BandMon();
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                MainEngine.netManage.GetNetworkState(bandMon);

                stream.Position = 0;
                formatter.Serialize(stream, bandMon);

                ComMisc.commClient.clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }

            if (MainEngine.processTracker.bCheckRunningApps == true)
            {
                M4RealAppMon realappMon = new M4RealAppMon();
                ComMisc.mainAgent.MainEngine.processTracker.GetRunningApps(realappMon.appList);

                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, realappMon);

                if (MainEngine.processTracker.bCheckRunningApps == true)
                    ComMisc.commClient.clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }
        }

        // Streaming Timer Event Function
        public void OnScreenTimerEvent(object sender, ElapsedEventArgs e)
        {
            MainEngine.UpdateScreenTimer();

            if (MainEngine.screenCapture.captureEnable == true)
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, MainEngine.screenCapture.scrImage);

                if (MainEngine.screenCapture.captureEnable == true)
                    ComMisc.commClient.clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }
        }

        // RealHis Timer Event Function
        public void OnRealHisTimerEvent(object sender, ElapsedEventArgs e)
        {
            MainEngine.UpdateCurrentProcess();

            if (MainEngine.processTracker.bCheckProcess)
            {
                M8ProcMon curProcMon = new M8ProcMon();
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                MainEngine.processTracker.GetCurProcList(curProcMon.procList, true);
                stream.Position = 0;
                formatter.Serialize(stream, curProcMon);

                if (MainEngine.processTracker.bCheckProcess == true)
                    ComMisc.commClient.clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }

            iPrintHis++;

            if (iPrintHis >= PRINT_HIS_INTERVAL)
            {
                iPrintHis = 0;
                MainEngine.printTracker.RefreshPrinterState();
            }
        }

        // History Timer Event Function
        public void OnHistoryTimerEvent(object sender, ElapsedEventArgs e)
        {
            iNetHistory++;
            iNetRefresh++;
            iInstall++;
            iUrlHis++;
            iDevice++;
            iShareHis++;

            if (iNetRefresh >= NETAPP_REFRESH_INTERVAL)
            {
                iNetRefresh = 0;
                MainEngine.UpdateConnectionHistory();
            }

            if (iNetHistory >= NETAPP_HIS_INTERVAL)
            {
                iNetHistory = 0;
                ComMisc.commClient.SendNetAppHistory();
            }

            if (iUrlHis >= WEBURL_HIS_INTERVAL)
            {
                iUrlHis = 0;
                MainEngine.netManage.GetUrlHistory();
                ComMisc.commClient.SendWebUrlHistory();
            }

            if (iShareHis >= SHARE_HIS_INTERVAL)
            {
                iShareHis = 0;
                MainEngine.cFileOp.RefreshShareList();
            }

            if (iDevice >= DEVICE_CHK_INTERVAL)
            {
                iDevice = 0;
                MainEngine.devManage.GetHardwareInfo();
            }

            if (iInstall >= INSTALL_CHK_INTERVAL)
            {
                iInstall = 0;
                MainEngine.UpdateInstallState();

                if (MainEngine.installTracker.bCheckInstall)
                {
                    M8InstAppHis instAppHis = new M8InstAppHis();
                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                    MainEngine.installTracker.getInstallList(instAppHis.installList);

                    stream.Position = 0;
                    formatter.Serialize(stream, instAppHis);

                    if (MainEngine.installTracker.bCheckInstall == true)
                        ComMisc.commClient.clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
        }

        #region COM_LOCK_FUNCS
        private delegate void ComLockInvoke(bool bMouse, bool bKeybd);

        private void ComLockFunc(bool bMouse, bool bKeybd)
        {
            try
            {
                if (_bLockMouse)
                    mouseHook.Start();
                else
                    mouseHook.Stop();

                if (_bLockKeybd)
                    keyboardHook.Start();
                else
                    keyboardHook.Stop();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        public void LockComputer(bool bMouse, bool bKeybd)
        {
            _bLockMouse = bMouse;
            _bLockKeybd = bKeybd;

            this.Invoke(new ComLockInvoke(ComLockFunc), new object[] { false, false });
        }

        public void Shutdown(bool bShutdown)
        {
            ManagementBaseObject mboShutdown = null;
            ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();          // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");
            
            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            mboShutdownParams["Flags"] = (bShutdown) ? "1" : "2";
            mboShutdownParams["Reserved"] = "0";
            
            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
            }
        } 

        public void ResetComputer(bool bShutdown, bool bRestart)
        {
            if (bRestart)
            {
                //Shutdown(false);
                System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                try
                {
                    netshProcess.StartInfo.FileName = "shutdown";
                    netshProcess.StartInfo.UseShellExecute = false;
                    netshProcess.StartInfo.CreateNoWindow = true;

                    netshProcess.StartInfo.Arguments = "/r /t 0";
                    netshProcess.Start();
                }
                catch (System.Exception ex)
                {
                    // Error occured
                    ComMisc.LogErrors(ex.ToString());
                }
            }
            else if (bShutdown)
            {
                //Shutdown(true);
                System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                try
                {
                    netshProcess.StartInfo.FileName = "shutdown";
                    netshProcess.StartInfo.UseShellExecute = false;
                    netshProcess.StartInfo.CreateNoWindow = true;

                    netshProcess.StartInfo.Arguments = "/s /t 0";
                    netshProcess.Start();
                }
                catch (System.Exception ex)
                {
                    // Error occured
                    ComMisc.LogErrors(ex.ToString());
                }
            }
        }

        void mouseHook_MouseMove(object sender, MouseEventArgs e)
        {
        }

        void mouseHook_MouseDown(object sender, MouseEventArgs e)
        {
        }

        void mouseHook_MouseUp(object sender, MouseEventArgs e)
        {
        }

        void mouseHook_MouseWheel(object sender, MouseEventArgs e)
        {
        }

        void keyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
        }

        void keyboardHook_KeyUp(object sender, KeyEventArgs e)
        {
        }

        void keyboardHook_KeyPress(object sender, KeyPressEventArgs e)
        {
        }
        #endregion

        #region MESSAGE_SHOW_FUNC
        public void ShowMessage(String strMessage)
        {
            new Thread(new ThreadStart(delegate
            {
                MessageBox.Show(strMessage, "Server Message", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            })).Start();    
        }
        #endregion

        public void OnLicenseTimerEvent(object sender, ElapsedEventArgs e)
        {
#if CONFIG_LICENSE
            bool blRet = Global.CheckValidLicense();
            if (blRet)
            {
                DateTime dt = DateTime.Now;
                long dwInterval = dt.Ticks % MAX_LICENSE_TIMER_INTERVAL;

                if (dwInterval < MIN_LICENSE_TIMER_INTERVAL)
                    dwInterval += (dt.Ticks % MIN_LICENSE_TIMER_INTERVAL);

                LicenseTimer.Interval = dwInterval;
            }
            else
            {
                trackLog.WriteTrackLog(LOG_TYPE.LOG_APP_EXIT, "INMSAgent Cannot Start. Allowable period has been expired.");	// Record Start Log Message
                OnExit();
            }
#endif
        }

        #region CONSTANT_DEFINE
        /************************************************************************/
        // Define Constants
        /************************************************************************/
        // Do not hurt this Part of the Interval Setting.

        // Init Timer Interval
        private const int INIT_TIMER_INTERVAL = 500;				// 0.5s

        // TrackMon Timer Interval
        private const int PROCESS_TIMER_INTERVAL = 2300;			// 2.3s
        private const int STATE_TIMER_INTERVAL = 1000;			    // 1s

        // History Timer Interval
        private const int HISTORY_TIMER_INTERVAL = 1000;			// 1s
        private const int REALHIS_TIMER_INTERVAL = 1000;            // 1s
        
        // License Timer Interval
        private const int MAX_LICENSE_TIMER_INTERVAL = 7200000;		// 2h[ms]
        private const int MIN_LICENSE_TIMER_INTERVAL = 3600000;		// 1h[ms]

        // Streaming Timer Interval
        private const int SCREEN_CAPTURE_TIMER_INTERVAL = 500; // 500ms

        // Not used
        //private const uint FOLDER_OPT_TIMER_INTERVAL = 3000;	    // 3s

        public int WEBURL_HIS_INTERVAL = 80;   // 1min
        public int NETAPP_HIS_INTERVAL = 100;   // 1min
        public int NETAPP_REFRESH_INTERVAL = 5; // 5s
        public int SHARE_HIS_INTERVAL = 10; // 10s
        public int NETAPP_MON_INTERVAL = 3;     // 3s
        public int INSTALL_CHK_INTERVAL = 180; // 3min
        public int DEVICE_CHK_INTERVAL = 200; // 3min
        public int PRINT_HIS_INTERVAL = 2; // 5s
        #endregion

        System.Timers.Timer initTimer = null;
        System.Timers.Timer processTimer = null;
        System.Timers.Timer screenTimer = null;
        System.Timers.Timer stateTimer = null;

        // History Timer
        System.Timers.Timer historyTimer = null;
        System.Timers.Timer realhisTimer = null;

        //System.Timers.Timer folderTimer = null;
#if CONFIG_LICENSE
        System.Timers.Timer LicenseTimer = null;
#endif

        static int iNetHistory = 0;
        static int iNetRefresh = 0;
        static int iInstall = 0;
        static int iUrlHis = 0;
        static int iDevice = 0;
        static int iShareHis = 0;
        static int iPrintHis = 0;

        public EngineMain MainEngine;

        // Used for Lock Computer
        MouseHook mouseHook;
        KeyboardHook keyboardHook;

        private bool _bLockMouse { get; set; }
        private bool _bLockKeybd { get; set; }
    }
}