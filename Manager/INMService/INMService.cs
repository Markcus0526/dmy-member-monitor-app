using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using INMServer;

namespace INMService
{
    public partial class INMService : ServiceBase
    {
        #region Fields and Properties

        private const uint SEARCH_TIMER_INTERVAL = 1800;	

        System.Threading.Timer monTimer;

        #endregion

        #region Constructors

        public INMService()
        {
            InitializeComponent();

            monTimer = new System.Threading.Timer(new TimerCallback(MonTimerProc));
        }

        #endregion

        #region Internal Methods
        #endregion

        #region Private Methods


        #endregion

        #region Event Methods

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        private void MonTimerProc(object state)
        {
            ComMisc.commBackServer = new CommBackServer();
            ComMisc.commBackServer.StartServer();

            ComMisc.commBackServer.StopServer();
        }

        #endregion

    }
}
