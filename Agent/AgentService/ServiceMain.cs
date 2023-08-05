using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace AgentService
{
    public class ServiceMain
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new AgentServices() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
