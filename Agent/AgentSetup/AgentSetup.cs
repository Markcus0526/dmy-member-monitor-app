using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Collections;
using AgentSetup.Properties;
using System.IO;

namespace AgentSetup
{
    static class AgentSetup
    {
        /// <summary>
        /// The main entry point for the application.
        /// 
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string parameter = string.Concat(args);

            parameter = "--install";
            switch (parameter)
            {
                case "--install":
                    InstallService(true);
                    break;
                case "--uninstall":
                    InstallService(false);
                    break;
                case "--start":
                    StartService();
                    break;
                case "--stop":
                    StopService();
                    break;
            }
        }

        private static void InstallService(bool pInstall)
        {
            string strPath = Directory.GetCurrentDirectory();
            IDictionary ServiceState = new Hashtable();

            if (pInstall) // install
            {
                try
                {
                    ServiceController serviceController = new ServiceController(Resources.SERVICENAME);
                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                        serviceController.Refresh();
                    }
                }
                catch (System.Exception ex)
                {
                }
                finally
                {
                }

                try
                {
                    AssemblyInstaller TrackAssemblyInstaller = new AssemblyInstaller(strPath + @"\" + Resources.SERVICE_FILE_NAME, null);
                    TrackAssemblyInstaller.UseNewContext = true;
                    TrackAssemblyInstaller.Install(ServiceState);
                    TrackAssemblyInstaller.Commit(ServiceState);

                    StartService();
                }
                catch (System.Exception ex)
                {
                    	
                }
                
            }
            else // uninstall
            {
                ServiceController serviceController = new ServiceController(Resources.SERVICENAME);
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                    serviceController.Refresh();
                }

                // unregister service from system
                AssemblyInstaller TrackAssemblyInstaller = new AssemblyInstaller(strPath + @"\" + Resources.SERVICE_FILE_NAME, null);
                TrackAssemblyInstaller.UseNewContext = true;
                TrackAssemblyInstaller.Uninstall(ServiceState);
            }
        }

        private static void StartService()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                ServiceController serviceController = new ServiceController(Resources.SERVICENAME);

                // if service is stopped, start it.
                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    serviceController.Start();

                    serviceController.WaitForStatus(ServiceControllerStatus.Running);
                    serviceController.Refresh();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                Cursor.Current = Cursors.Arrow;
            }
        }

        private static void StopService()
        {
            try
            {
                ServiceController serviceController = new ServiceController(Resources.SERVICENAME);

                // if service is running, stop it
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                    serviceController.Refresh();
                }
            }
            catch (Exception ex)
            {
            }
        }

        //private static bool DoesServiceExist(string serviceName, string machineName)
        //{
        //    ServiceController[] services = ServiceController.GetServices(machineName); 
        //    var service = services..FirstOrDefault(s => s.ServiceName == serviceName);
        //    return service != null;
        //} 
    }
}
