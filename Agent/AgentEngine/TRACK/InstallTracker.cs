using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Collections;
using INMC.INMMessage;
using System.Management;
using System.Globalization;
using System.Threading;

namespace AgentEngine
{
    /************************************************************************/
    // InstallTracker class  derived from PollingTracker
    // function : monitor install/uninstall action which user manipulate "Add/Remove programs" of control panel.
    /************************************************************************/
    public class InstallTracker : PollingTracker
    {
        #region Fields and Properties
        const string INSTALL_REG_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        const string INSTALLWOW64_REG_KEY = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        const string DISPLAY_NAME = "DisplayName";
        const string DISPLAY_VERSION = "DisplayVersion";
        const string INSTALL_PATH = "InstallLocation";        
        const string INSTALL_DATE = "InstallDate";

        const uint MAX_INSTALL_LIST = 512;
        const uint INSTALL_MAX_ONCE_ADD = 10;   	/* Assume max 10 at period of INSTALL_TIMER_INTERVAL */

        //private bool IsDuplicate(string szData);
        //private bool GetUninstallPath(HKEY hKey);

        private InstallInfoEntry[] installList;
        public bool bCheckInstall = false;

        // Thread Management
        private object installobj = new object();
        #endregion

        protected override bool Init()
        {
            #region Old WMI mode
            /*
            installList = new InstallInfoEntry[MAX_INSTALL_LIST];

            int i = 0, j = 0;

	        // Initialize Install List
	        for(i = 0; i < MAX_INSTALL_LIST; i++)
		        installList[i].nStatus = ENTRY_STATUS.UNUSED_ENTRY;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT " + DISPLAY_NAME_KEY + "," + INSTALL_PATH + "," + INSTALL_DATE + " FROM Win32_Product");
            ManagementObjectCollection collection = searcher.Get();
            System.Management.ManagementObjectCollection.ManagementObjectEnumerator enumerator = collection.GetEnumerator();

            i = 0;
            while (enumerator.MoveNext())
            {
                ManagementObject obj = (ManagementObject)enumerator.Current;
                installList[i].strDisplayName = (string)obj[DISPLAY_NAME_KEY];
                installList[i].strInstallPath = (string)obj[INSTALL_PATH];
                installList[i].strInstallDate = (string)obj[INSTALL_DATE];

                installList[i].nStatus = ENTRY_STATUS.USED_ENTRY;
                i++;
            }

            for (i = 0; i < MAX_INSTALL_LIST - 1; i++)
            {
                if (installList[i].nStatus == ENTRY_STATUS.UNUSED_ENTRY)
                    continue;

                for (j = i + 1; j < MAX_INSTALL_LIST; j++)
                {
                    if (installList[j].nStatus == ENTRY_STATUS.UNUSED_ENTRY)
                        continue;

                    if (installList[i].strDisplayName == installList[j].strDisplayName)
                        installList[j].nStatus = ENTRY_STATUS.UNUSED_ENTRY;
                }
            }*/
            #endregion
            
            #region Registry mode
            try
            {
                int i, j;

                installList = new InstallInfoEntry[MAX_INSTALL_LIST];

                // Initialize Install List
                for (i = 0; i < MAX_INSTALL_LIST; i++)
                    installList[i].nStatus = ENTRY_STATUS.UNUSED_ENTRY;

                i = 0;
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(INSTALL_REG_KEY))
                {
                    //Let's go through the registry keys and get the info we need:
                    foreach (string skName in rk.GetSubKeyNames())
                    {
                        using (RegistryKey sk = rk.OpenSubKey(skName))
                        {
                            //If the key has value, continue, if not, skip it:
                            if (sk.GetValue(DISPLAY_NAME) != null)
                            {
                                installList[i].strDisplayName = sk.GetValue(DISPLAY_NAME).ToString();

                                if (sk.GetValue(DISPLAY_VERSION) != null)
                                    installList[i].strDisplayVersion = sk.GetValue(DISPLAY_VERSION).ToString();

                                if (sk.GetValue(INSTALL_PATH) != null)
                                    installList[i].strInstallPath = sk.GetValue(INSTALL_PATH).ToString();

                                if (sk.GetValue(INSTALL_DATE) != null)
                                    installList[i].strInstallDate = sk.GetValue(INSTALL_DATE).ToString();

                                installList[i].nStatus = ENTRY_STATUS.USED_ENTRY;

                                i++;
                            }
                        }
                    }                    
                }

                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(INSTALLWOW64_REG_KEY))
                {
                    if (rk != null)
                    {
                        //Let's go through the registry keys and get the info we need:
                        foreach (string skName in rk.GetSubKeyNames())
                        {
                            using (RegistryKey sk = rk.OpenSubKey(skName))
                            {
                                //If the key has value, continue, if not, skip it:
                                if (sk.GetValue(DISPLAY_NAME) != null)
                                {
                                    installList[i].strDisplayName = sk.GetValue(DISPLAY_NAME).ToString();

                                    if (sk.GetValue(DISPLAY_VERSION) != null)
                                        installList[i].strDisplayVersion = sk.GetValue(DISPLAY_VERSION).ToString();

                                    if (sk.GetValue(INSTALL_PATH) != null)
                                        installList[i].strInstallPath = sk.GetValue(INSTALL_PATH).ToString();

                                    if (sk.GetValue(INSTALL_DATE) != null)
                                        installList[i].strInstallDate = sk.GetValue(INSTALL_DATE).ToString();

                                    installList[i].nStatus = ENTRY_STATUS.USED_ENTRY;

                                    i++;
                                }
                            }
                        }
                    }                    
                }

                for (i = 0; i < MAX_INSTALL_LIST - 1; i++)
                {
                    if (installList[i].nStatus == ENTRY_STATUS.UNUSED_ENTRY)
                        continue;

                    for (j = i + 1; j < MAX_INSTALL_LIST; j++)
                    {
                        if (installList[j].nStatus == ENTRY_STATUS.UNUSED_ENTRY)
                            continue;

                        if (installList[i].strDisplayName == installList[j].strDisplayName)
                            installList[j].nStatus = ENTRY_STATUS.UNUSED_ENTRY;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
            
            #endregion

            return true;
        }

        protected override bool TrackLog()
        {
            #region WMI mode
            /*int i = 0, j = 0;

            Monitor.Enter(installobj);

            // Initialize Install List
            for (i = 0; i < MAX_INSTALL_LIST; i++)
                installList[i].nStatus = ENTRY_STATUS.UNUSED_ENTRY;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product");
            ManagementObjectCollection collection = searcher.Get();
            System.Management.ManagementObjectCollection.ManagementObjectEnumerator enumerator = collection.GetEnumerator();
            int nCount = collection.Count;

            for (i = 0; i < nCount; i++)
            {
                if (enumerator.MoveNext() == false)
                    break;

                ManagementBaseObject obj = enumerator.Current;
                installList[i].strDisplayName = (string)obj[DISPLAY_NAME_KEY];
                installList[i].strInstallPath = (string)obj[INSTALL_PATH];
                installList[i].strInstallDate = (string)obj[INSTALL_DATE];

                installList[i].nStatus = ENTRY_STATUS.USED_ENTRY;
            }

            for (i = 0; i < MAX_INSTALL_LIST - 1; i++)
            {
                if (installList[i].nStatus == ENTRY_STATUS.UNUSED_ENTRY)
                    continue;

                for (j = i + 1; j < MAX_INSTALL_LIST; j++)
                {
                    if (installList[j].nStatus == ENTRY_STATUS.UNUSED_ENTRY)
                        continue;

                    if (installList[i].strDisplayName == installList[j].strDisplayName)
                        installList[j].nStatus = ENTRY_STATUS.UNUSED_ENTRY;
                }
            }

            Monitor.Exit(installobj);*/
            #endregion

            #region Registry mode
            try
            {
                int i, j;

                installList = new InstallInfoEntry[MAX_INSTALL_LIST];

                // Initialize Install List
                for (i = 0; i < MAX_INSTALL_LIST; i++)
                    installList[i].nStatus = ENTRY_STATUS.UNUSED_ENTRY;

                i = 0;
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(INSTALL_REG_KEY))
                {
                    //Let's go through the registry keys and get the info we need:
                    foreach (string skName in rk.GetSubKeyNames())
                    {
                        using (RegistryKey sk = rk.OpenSubKey(skName))
                        {
                            //If the key has value, continue, if not, skip it:
                            if (sk.GetValue(DISPLAY_NAME) != null)
                            {
                                installList[i].strDisplayName = sk.GetValue(DISPLAY_NAME).ToString();

                                if (sk.GetValue(DISPLAY_VERSION) != null)
                                    installList[i].strDisplayVersion = sk.GetValue(DISPLAY_VERSION).ToString();

                                if (sk.GetValue(INSTALL_PATH) != null)
                                    installList[i].strInstallPath = sk.GetValue(INSTALL_PATH).ToString();

                                if (sk.GetValue(INSTALL_DATE) != null)
                                    installList[i].strInstallDate = sk.GetValue(INSTALL_DATE).ToString();

                                installList[i].nStatus = ENTRY_STATUS.USED_ENTRY;

                                i++;
                            }
                        }
                    }
                }

                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(INSTALLWOW64_REG_KEY))
                {
                    if (rk != null)
                    {
                        //Let's go through the registry keys and get the info we need:
                        foreach (string skName in rk.GetSubKeyNames())
                        {
                            using (RegistryKey sk = rk.OpenSubKey(skName))
                            {
                                //If the key has value, continue, if not, skip it:
                                if (sk.GetValue(DISPLAY_NAME) != null)
                                {
                                    installList[i].strDisplayName = sk.GetValue(DISPLAY_NAME).ToString();

                                    if (sk.GetValue(DISPLAY_VERSION) != null)
                                        installList[i].strDisplayVersion = sk.GetValue(DISPLAY_VERSION).ToString();

                                    if (sk.GetValue(INSTALL_PATH) != null)
                                        installList[i].strInstallPath = sk.GetValue(INSTALL_PATH).ToString();

                                    if (sk.GetValue(INSTALL_DATE) != null)
                                        installList[i].strInstallDate = sk.GetValue(INSTALL_DATE).ToString();

                                    installList[i].nStatus = ENTRY_STATUS.USED_ENTRY;

                                    i++;
                                }
                            }
                        }
                    }                    
                }

                for (i = 0; i < MAX_INSTALL_LIST - 1; i++)
                {
                    if (installList[i].nStatus == ENTRY_STATUS.UNUSED_ENTRY)
                        continue;

                    for (j = i + 1; j < MAX_INSTALL_LIST; j++)
                    {
                        if (installList[j].nStatus == ENTRY_STATUS.UNUSED_ENTRY)
                            continue;

                        if (installList[i].strDisplayName == installList[j].strDisplayName)
                            installList[j].nStatus = ENTRY_STATUS.UNUSED_ENTRY;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }

            #endregion

            return true;
        }

        public void getInstallList(ArrayList instAppHisList)
        {
            int k = 0;
            try
            {
                Monitor.Enter(installobj);

                instAppHisList.Clear();

                CultureInfo culture = new CultureInfo("en-US", true);

                for (int i = 0; i < MAX_INSTALL_LIST; i++)
                {
                    if (installList[i].nStatus != ENTRY_STATUS.UNUSED_ENTRY)
                    {
                        k = i;
                        M8InstallItem instItem = new M8InstallItem();
                        instItem.strProgram = installList[i].strDisplayName;
                        instItem.strVersion = installList[i].strDisplayVersion;
                        instItem.strPath = installList[i].strInstallPath;

                        if (string.IsNullOrEmpty(installList[i].strInstallDate) == false)
                        {
                            DateTime instDate;

                            if (DateTime.TryParseExact(installList[i].strInstallDate, "yyyyMMdd", culture, DateTimeStyles.None, out instDate) == true)
                                instItem.installTime = instDate;
                        }                            
                        else
                            instItem.installTime = DateTime.MinValue;

                        instAppHisList.Add(instItem);
                    }
                }

                Monitor.Exit(installobj);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }            
        }

        public void SetEnableCheck(bool bEnable)
        {
            bCheckInstall = bEnable;
        }
        
        // const
        //////////////////////////////////////////////////////////////////////////
        // EntryStatus
        //////////////////////////////////////////////////////////////////////////
        enum ENTRY_STATUS
        {
            UNUSED_ENTRY,
            USED_ENTRY,
            EXIST_ENTRY
        }

        //////////////////////////////////////////////////////////////////////////
        // Define Structures
        //////////////////////////////////////////////////////////////////////////
        struct InstallInfoEntry
        {
            public ENTRY_STATUS nStatus;			// Entry Status
            public string strDisplayName;			// Display Name
            public string strDisplayVersion;		// Display Name
            public string strInstallPath;           // Install Path
            public string strInstallDate;           // Install Date
        }       
        
    }
}
