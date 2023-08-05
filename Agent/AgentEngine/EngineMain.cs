using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AgentEngine
{
    public class EngineMain
    {
        #region Member_Define
        
        public FileOpTracker cFileOp;				// File Operation Tracker
        public ProcessTracker processTracker;		// Process Tracker
        public FolderOptionTracker folderTracker;
        public TrackLog trackLog;
        public NetworkManage netManage;
        public DeviceManage devManage;
        public ShellNotifications ShellNotify;
        public ScreenCapture screenCapture;
        public InstallTracker installTracker;		// Install Tracker
        public UserManage userManage;
        public PrintTracker printTracker;
        //public MailTracker mailTracker;
 
        private bool bTrackRunning;
        private String strFile = "";						// status file path
        private IntPtr MainHandle;

        #endregion

        public EngineMain()
        {
            ComMisc.IndicateOS();

            trackLog = new TrackLog();
            ShellNotify = new ShellNotifications();

            cFileOp = new FileOpTracker(trackLog);
            processTracker = new ProcessTracker(trackLog);
            folderTracker = new FolderOptionTracker(trackLog);
            netManage = new NetworkManage();
            devManage = new DeviceManage();
            screenCapture = new ScreenCapture();
            installTracker = new InstallTracker();
            userManage = new UserManage();
            printTracker = new PrintTracker();
        }

        public void Init(IntPtr MainHandle)
        {
            this.MainHandle = MainHandle;

            RegisterFileNotify();
        }

        public void Start()
        {
            if (bTrackRunning)
                return;

            String szBuffer;

            szBuffer = System.Environment.SystemDirectory;
            strFile = szBuffer + @"\";
            strFile += AgentEngine.Properties.Resources.STATUS_FILE_NAME;

            trackLog.strFile = String.Format(@"{0}\{1}", szBuffer, AgentEngine.Properties.Resources.LOG_FILE_NAME);
            trackLog.strIdFile = String.Format(@"{0}\{1}", szBuffer, AgentEngine.Properties.Resources.LOG_FILE_ID);

            trackLog.StartTrackLog();						// Start Track Log

            bTrackRunning = true;
        }

        public void Stop()
        {
            if (bTrackRunning)
            {
                processTracker.Stop();
                folderTracker.Stop();
                installTracker.Stop();

                processTracker.OnTracking();
                ShellNotify.UnregisterChangeNotify();
                bTrackRunning = false;
            }
        }

        public void InitState()
        {
            processTracker.bCheckRunningApps = false;
            processTracker.bCheckProcess = false;
            netManage.bWatchNetwork = false;
            screenCapture.captureEnable = false;
            installTracker.bCheckInstall = false;
        }

        public void ChangeNotify(IntPtr wParam, IntPtr lParam)
        {
            if (!trackLog.IsStartTrackLog())
                return;

            if (ShellNotify.NotificationReceipt(wParam, lParam) == false)
                return;

            cFileOp.Record((NotifyInfos)ShellNotify.NotificationsReceived[ShellNotify.NotificationsReceived.Count - 1]);
        }

        private bool RegisterFileNotify()
        {
            bool ret = false;
                
            if (ShellNotify.RegisterChangeNotify(
                MainHandle,
                //ShellNotifications.CSIDL.CSIDL_DESKTOP,
                ShellNotifications.CSIDL.CSIDL_DRIVES,
                true) > 0)
            {
                ret = true;
            }
            else
            {
                ComMisc.LogErrors("File operation failed.");
            }

            return ret;
        }

        public void StartTracking()
        {
            processTracker.Start();
            folderTracker.Start();
            installTracker.Start();
        }

        // Timer Event

        // Network Timer Event
        public void UpdateNetworkState()
        {
            netManage.UpdateNetworkState();
        }

        public void UpdateConnectionHistory()
        {
            netManage.UpdateConnectionHistory();
        }
        
        // Process Timer Event
        public void UpdateProcessState()
        {
            processTracker.OnTracking();
        }

        public void UpdateCurrentProcess()
        {
            processTracker.RefCurProcList();
        }

        // FolderTracker Timer Event
        public void UpdateFolderState()
        {
            // Not used
            //folderTracker.OnTracking();
        }

        // Streaming Timer Event
        public void UpdateScreenTimer()
        {
            screenCapture.CaptureHandler();
        }

        // Install Timer Event
        public void UpdateInstallState()
        {
            installTracker.OnTracking();
        }
    }
}
