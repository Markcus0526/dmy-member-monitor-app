using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace INMAgent
{
    class AgentMain
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!INMSAgent.IsFirstProcess())
            {
                return;
            }

            ComMisc.InitLogFile();
            ComMisc.ReadServerAddress();

            ComMisc.mainAgent = new INMSAgent();
            
            ComMisc.commClient = new CommClient();
            ComMisc.commClient.StartClient();

            Application.Run(ComMisc.mainAgent);

            ComMisc.commClient.StopClient();
        }
    }
}
