using System;
using System.Collections.Generic;
using System.Windows.Forms;
using INMServer;
using System.Diagnostics;

namespace INMServer
{
    static class INMSMain
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (CheckRunEnable() == false)
                return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (ComMisc.AutoLogonCheck() == false)
            {
                FrmLogon frmLogon = new FrmLogon();
                DialogResult bRet = frmLogon.ShowDialog();
                if (bRet == DialogResult.Cancel)
                {
                    Application.Exit();
                    return;
                }
            }

            //if (ComMisc.CheckValidLicense() == false)
            //{
            //    FrmRegister frmRegister = new FrmRegister();
            //    DialogResult bRet = frmRegister.ShowDialog();
            //    if (bRet == DialogResult.Cancel)
            //    {
            //        Application.Exit();
            //        return;
            //    }
            //}         
            //////////////////////////////////////////////////////////////////////////
            ComMisc.frmMain = new FrmMain();

            Application.Run(ComMisc.frmMain);

        }

        private static bool CheckRunEnable()
        {
            try
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
                    if (szModuleName == szBuffer)
                        iSame++;
                }

                if (iSame > 1)
                    return false;

                return true;
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                return false;
            }
        }      

    }
}