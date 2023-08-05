using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;

namespace AgentService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void TrackerInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}