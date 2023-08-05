namespace AgentService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

       # region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TrackerProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.TrackerInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // TrackerProcessInstaller
            // 
            this.TrackerProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.TrackerProcessInstaller.Password = null;
            this.TrackerProcessInstaller.Username = null;
            // 
            // TrackerInstaller
            // 
            this.TrackerInstaller.Description = "Monitor INMAgent";
            this.TrackerInstaller.DisplayName = "INMService";
            this.TrackerInstaller.ServiceName = "INMService";
            this.TrackerInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.TrackerInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.TrackerInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.TrackerProcessInstaller,
            this.TrackerInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller TrackerProcessInstaller;
        private System.ServiceProcess.ServiceInstaller TrackerInstaller;
    }
}