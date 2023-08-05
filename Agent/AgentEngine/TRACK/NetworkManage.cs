using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Timers;
using System.Management;
using Microsoft.Win32;
using NetFwTypeLib;
using INMC.INMMessage;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;

namespace AgentEngine
{
    public class NetworkManage
    {
        // Thread Management
        object netObj = new object();

        // Web Url List
        GoogleChrome chromeHis = new GoogleChrome();
        FireFox firefoxHis = new FireFox();
        SogouExplorer sogouHis = new SogouExplorer();
        ArrayList urlHistoryList = new ArrayList();

        // Network Interface
        NetworkInterface[] nicArr;

        // BandWidth, RealTime History
        public bool bWatchNetwork = false;
        double upBand = 100;
        double downBand = 100;
        M1BandMon bandMon = new M1BandMon();

        // Internet Setting
        bool bNetEnable = false;
        bool bStartEnable = false;
        DateTime startTime = DateTime.Now;
        bool bEndEnable = false;
        DateTime endTime = DateTime.Now;

        // Network Process List
        ArrayList ipAppList = new ArrayList();
        ArrayList prevConList = new ArrayList();

        // Network Process History
        ArrayList netAppHisList = new ArrayList();

        // Url History Interval
        private static int nCheckLimit = 10;
        private static int nDnsCheckIinterval = 180;
        private static int nHistoryUrl_limitday = 60;
        private int nCurrentCheck = 0;
        private int nCurrentDnsCheck = 0;

        // \SYSTEM\CurrentControlSet\Control\Terminal Server\fDenyTSConnections Key is used to change Terminal Connection state.
        // fDenyTSConnections: 0, Terminal Connection Enable
        // fDenyTSConnections: 1, Terminal Connection Disable
        private static String subTerminalKey = @"SYSTEM\CurrentControlSet\Control\Terminal Server";
        private string SHELLFOLDERS_REG_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
        private string HISTORY_REG_KEY = @"History";
        private string APPDATA_REG_KEY = @"AppData";
        private string LOCALAPPDATA_REG_KEY = @"Local AppData";

        // Web Url Block
        private static String strGroupName = "INMS_IPSET";

        // History File Name
        private const String strWebFileName = "INMWebUrlHis_";
        private const String strNetFileName = "INMNetAppHis_";

        private const String strNetAppFileName = "INMNetAppSet_";

        private DateTime dtLastAccessNet;

        private int nCurSId = 0;
        private int nCurRuleId = 0;
        DateTime urlLastCheck;

        M3PortRule policy = null;
        bool bPolicyValid = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public NetworkManage()
        {
            InitializeNetworkInterface();
            EnableRemoteSession();

            if (ComMisc.vistaOverOS == true)
            {
                EnableServerPort("10180");
                EnableServerPort("10181");
            }

            ipAppList.Clear();

            ArrayList newList = new ArrayList();
            GetNetworkConnection(newList);

            prevConList.Clear();
            foreach (M2NetAppItem netItem in newList)
            {
                M2NetAppItem newitem = new M2NetAppItem(netItem);
                prevConList.Add(newitem);
            }

            nCurSId = 0;

            Process CurProcess = Process.GetCurrentProcess();
            nCurSId = CurProcess.SessionId;

            urlLastCheck = DateTime.MinValue;

            RestoreNetHistoryFromFile();
            RestoreWebHistoryFromFile();

            dtLastAccessNet = DateTime.MinValue;

            RestoreNetAppFromFile();
        }

        /// <summary>
        /// Initialize all network interfaces on this computer
        /// </summary>
        private void InitializeNetworkInterface()
        {
            // Grab all local interfaces to this computer
            nicArr = NetworkInterface.GetAllNetworkInterfaces();

            for (int i = 0; i < nicArr.Length; i++)
            {
                M1NetworkInfo netinf = new M1NetworkInfo();

                NetworkInterface nic = nicArr[i];

                // Grab the stats for that interface
                IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();

                netinf.strNetType = nic.Name;
                netinf.nUtilization = 0;
                netinf.nSpeed = nic.Speed;
                netinf.strInterfaceType = nic.NetworkInterfaceType.ToString();
                netinf.nBytesReceive = interfaceStats.BytesReceived;
                netinf.nBytesSent = interfaceStats.BytesSent;
                netinf.nUploadSpeed = 0;
                netinf.nDownloadSpeed = 0;

                bandMon.netInfoList.Add(netinf);
            }

            System.Collections.ArrayList newlist = new System.Collections.ArrayList();
            if (ComMisc.vistaOverOS == true)
                GetFirewallRules2(newlist);
            else
                GetFirewallRules(newlist);
        }

        public void EnableServerPort(String strPort)
        {
            M3PortRule portRule = new M3PortRule();

            portRule.Direction = "Inbound";
            portRule.Enabled = "Yes";
            portRule.RemoteAddr = "*";
            portRule.Protocol = "TCP";
            portRule.RemotePorts = strPort;
            portRule.Name = "INMS_ServerPort";

            if (ComMisc.vistaOverOS == true)
            {
                DelRuleToFireWall(portRule);

                portRule.Direction = "Outbound";
                DelRuleToFireWall(portRule);

                portRule.Direction = "Inbound";
                AddRuleToFirewall(portRule);

                portRule.Direction = "Outbound";
                AddRuleToFirewall(portRule);
            }
            else
            {
                AddRuleToFirewall(portRule);
            }
        }

        public void EnableRemoteSession()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(subTerminalKey, true);

            key.SetValue("fDenyTSConnections", 0);

            // Close Registry Key
            key.Close();
        }

        public void ChangeNetworkState(bool bEnable)
        {
            SelectQuery wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL");
            ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
            foreach (ManagementObject item in searchProcedure.Get())
            {
                String connectid = (string)item["NetConnectionId"];
                if (connectid == "Local Area Connection")
                {
                    item.InvokeMethod((bEnable) ? "Enable" : "Disable", null);
                }
            } 
        }

        public void ChangeFirewallSettings(bool bDomain, bool bPrivate, bool bPublic)
        {
            System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

            try
            {
                netshProcess.StartInfo.FileName = "netsh";
                netshProcess.StartInfo.UseShellExecute = false;
                netshProcess.StartInfo.CreateNoWindow = true;

                // Change Domain Firewall Setting
                netshProcess.StartInfo.Arguments = "advfirewall set domainprofile state " + ((bDomain) ? "on" : "off");
                netshProcess.Start();

                // Change Private Firewall Setting
                netshProcess.StartInfo.Arguments = "advfirewall set publicprofile state " + ((bPublic) ? "on" : "off");
                netshProcess.Start();

                // Change Public Firewall Setting
                netshProcess.StartInfo.Arguments = "advfirewall set privateprofile state " + ((bPrivate) ? "on" : "off");
                netshProcess.Start();
            }
            catch (System.Exception ex)
            {
                // Error occured
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void ChangeInternetState(bool bEnable)   // 0: enable, 1: disable
        {
            System.Diagnostics.Process webInet = new System.Diagnostics.Process();

            try
            {
                webInet.StartInfo.FileName = "AgentWebInet";
                webInet.StartInfo.Arguments = (bEnable) ? "no" : "yes";
                webInet.StartInfo.UseShellExecute = false;
                webInet.StartInfo.CreateNoWindow = true;
                webInet.Start();
            }
            catch (System.Exception ex)
            {
                // Error occured
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void SetBandWidth(double pUp, double pDown)
        {
            try
            {
                upBand = pUp;
                downBand = pDown;

                string localIP = string.Empty;

                ManagementClass MC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = MC.GetInstances();
                string MACAddress = string.Empty;
                foreach (ManagementObject mo in moc)
                {
                    if (MACAddress == String.Empty) // only return MAC Address from first card
                    {
                        if ((bool)mo["IPEnabled"] == true)
                        {
                            string[] s = (string[])mo["IPAddress"];
                            localIP = s[0];
                            break;
                        }
                    }
                    mo.Dispose();
                }

                Process bandProc = new Process();
                bandProc.EnableRaisingEvents = false;
                bandProc.StartInfo.FileName = Directory.GetCurrentDirectory() + @"\BandSet.exe";
                bandProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                bandProc.StartInfo.Arguments =
                    @"-c " + localIP;
                bandProc.Start();

                bandProc.StartInfo.Arguments =
                    @"-4 -p tcp -i " + pUp.ToString("00.0000") + @" " + pDown.ToString("00.0000") + @" " + localIP;
                bandProc.Start();
            }
            catch (System.Exception ex)
            {
            	ComMisc.LogErrors(ex.ToString());
            }            
        }

        private bool CheckIPAddress(String strAddress)
        {
            IPAddress address;
            return IPAddress.TryParse(strAddress, out address);
        }

        public void InitInternetInfo(DateTime curAccessTime, M2NetAllowSet allowset)
        {
            ipAppList.Clear();
            SetInternetInfo(curAccessTime, allowset);
        }

        public void SetInternetInfo(DateTime curAccessTime, M2NetAllowSet allowset)
        {
            Monitor.Enter(netObj);
            bNetEnable = !allowset.bNetEnable;

            bStartEnable = allowset.bValidStartTime;
            bEndEnable = allowset.bValidEndTime;
            startTime = allowset.startTime;
            endTime = allowset.endTime;

            if (allowset.ipList != null)
            {
                ArrayList urlPrevList = new ArrayList();
                foreach (NetAllowIPItem item in ipAppList)
                {
                    NetAllowIPItem newitem = new NetAllowIPItem(item);
                    urlPrevList.Add(newitem);
                }

                ipAppList.Clear();
                int iCnt = allowset.ipList.Count;
                for (int i = 0; i < iCnt; i++)
                {
                    M2NetAllowAppItem item = (M2NetAllowAppItem)allowset.ipList[i];

                    if (item.strIp == "")
                        continue;

                    if (CheckIPAddress(item.strIp)) //IP
                    {
                        if (item.nType == 0)
                        {
                            AllowIPAddr(item.strIp, "");

                            NetAllowIPItem additem = new NetAllowIPItem();
                            additem.bAddBefore = false;
                            additem.bUrl = false;
                            additem.nType = item.nType;
                            additem.strIp = item.strIp;
                            additem.startTime = item.startTime;
                            additem.endTime = item.endTime;
                            ipAppList.Add(additem);
                        }
                        else if (item.nType == 1)
                        {
                            int iPrev = 0;
                            for (iPrev = 0; iPrev < urlPrevList.Count; iPrev++)
                            {
                                NetAllowIPItem previtem = (NetAllowIPItem)urlPrevList[iPrev];
                                if (previtem.strIp == item.strIp)
                                    previtem.strIp = "";
                            }

                            AllowIPAddr(item.strIp, "");
                            if (i < iCnt - 1)
                            {
                                item = (M2NetAllowAppItem)allowset.ipList[i + 1];
                                AllowIPAddr(item.strIp, "");

                                if (item.strIp != "")
                                {
                                    NetAllowIPItem additem = new NetAllowIPItem();
                                    additem.bAddBefore = false;
                                    additem.bUrl = false;
                                    additem.nType = item.nType;
                                    additem.strIp = item.strIp;
                                    additem.startTime = item.startTime;
                                    additem.endTime = item.endTime;
                                    ipAppList.Add(additem);
                                }

                                i++;
                            }
                        }
                        else if (item.nType == 2)
                        {
                            int iPrev = 0;
                            for (iPrev = 0; iPrev < urlPrevList.Count; iPrev++)
                            {
                                NetAllowIPItem previtem = (NetAllowIPItem)urlPrevList[iPrev];
                                if (previtem.strIp == item.strIp)
                                    previtem.strIp = "";
                            }

                            AllowIPAddr(item.strIp, "");
                        }
                    }
                    else
                    {
                        if (item.nType == 0)
                        {
                            NetAllowIPItem additem = new NetAllowIPItem();
                            additem.bAddBefore = false;
                            additem.bUrl = true;
                            additem.nType = item.nType;
                            additem.strIp = item.strIp;

                            IPAddress[] addresslist = Dns.GetHostAddresses(additem.strIp);
                            if (addresslist.Length > 0)
                                additem.strDnsIP = GetIPAddresses(addresslist);

                            additem.startTime = item.startTime;
                            additem.endTime = item.endTime;

                            if (additem.strDnsIP != "")
                                ipAppList.Add(additem);

                            AllowIPAddr(additem.strDnsIP, "");
                        }
                        else if (item.nType == 1)
                        {
                            int iPrev = 0;
                            for (iPrev = 0; iPrev < urlPrevList.Count; iPrev++)
                            {
                                NetAllowIPItem previtem = (NetAllowIPItem)urlPrevList[iPrev];
                                if (previtem.strIp == item.strIp)
                                    previtem.strIp = "";
                            }

                            IPAddress[] addresslist = Dns.GetHostAddresses(item.strIp);
                            if (addresslist.Length > 0)
                            {
                                String strDnsIP = GetIPAddresses(addresslist);
                                AllowIPAddr(strDnsIP, "");
                            }

                            if (i < iCnt - 1)
                            {
                                item = (M2NetAllowAppItem)allowset.ipList[i + 1];

                                NetAllowIPItem additem = new NetAllowIPItem();
                                additem.bAddBefore = false;
                                additem.bUrl = true;
                                additem.nType = item.nType;
                                additem.strIp = item.strIp;

                                addresslist = Dns.GetHostAddresses(additem.strIp);
                                if (addresslist.Length > 0)
                                    additem.strDnsIP = GetIPAddresses(addresslist);

                                additem.startTime = item.startTime;
                                additem.endTime = item.endTime;

                                if (additem.strIp != "" && additem.strDnsIP != "")
                                    ipAppList.Add(additem);
                                i++;

                                AllowIPAddr(additem.strDnsIP, "");
                            }
                        }
                        else if (item.nType == 2)
                        {
                            int iPrev = 0;
                            for (iPrev = 0; iPrev < urlPrevList.Count; iPrev++)
                            {
                                NetAllowIPItem previtem = (NetAllowIPItem)urlPrevList[iPrev];
                                if (previtem.strIp == item.strIp)
                                    previtem.strIp = "";
                            }

                            IPAddress[] addresslist = Dns.GetHostAddresses(item.strIp);
                            if (addresslist.Length > 0)
                            {
                                String strDnsIP = GetIPAddresses(addresslist);
                                AllowIPAddr(strDnsIP, "");
                            }
                        }
                    }
                }

                foreach (NetAllowIPItem item in urlPrevList)
                {
                    if (item.strIp != "")
                    {
                        NetAllowIPItem newitem = new NetAllowIPItem(item);
                        ipAppList.Add(newitem);
                    }
                }
            }

            SetNetAppDataToFile(curAccessTime, ipAppList);

            UpdateInternetState(true);
            Monitor.Exit(netObj);
        }

        public void GetLatestHistory(ArrayList urllist)
        {
            Monitor.Enter(netObj);
            urllist.Clear();

            foreach (M7WebHisItem item in urlHistoryList)
            {
                M7WebHisItem newitem = new M7WebHisItem(item);
                urllist.Add(newitem);
            }
            Monitor.Exit(netObj);
        }

        public void GetUrlHistory()
        {
            Monitor.Enter(netObj);
            urlHistoryList.Clear();
            urlHistoryList.TrimToSize();
            // Get IE Browse History
            try
            {
                string strIEPattern = GetHistoryPath() + "\\History.ie5\\index.dat";
                string strPatternName = Environment.SystemDirectory + "\\" + 
                    Properties.Resources.IEHIS_FILE_NAME + "_" + nCurSId;

                File.Copy(strIEPattern, strPatternName, true);
                FileStream streader = File.OpenRead(strPatternName);

                byte[] strReadText = new byte[streader.Length];
                streader.Read(strReadText, 0, (int)streader.Length);
                streader.Close();

                int nReadSize = 0;
                int nLength = strReadText.Length;
                int nType = 0;

                while (nReadSize < nLength)
                {
                    nType = Global.bMatchPattern(strReadText, nReadSize);
                    if (nType > 0)
                    {
                        Win32.history item = Global.getURL(strReadText, nType, ref nReadSize);

                        if (item != null && !item.pURL.StartsWith("file:"))
                        {
                            DateTime nowTime = DateTime.Now;
                            TimeSpan spendtime = nowTime - item.st;

                            if (spendtime.Days < nHistoryUrl_limitday &&
                                item.st >= urlLastCheck)
                            {
                                if (IsHisItemInArrayList(item.pURL, item.pTitle, item.st, urlHistoryList) == false)
                                {
                                    M7WebHisItem hisItem = new M7WebHisItem();
                                    hisItem.strUrl = item.pURL;
                                    hisItem.strTitle = item.pTitle;
                                    hisItem.strAccessTime = item.st;

                                    urlHistoryList.Add(hisItem);
                                }
                            }
                        }
                    }

                    nReadSize++;
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
         
            // Get Google Browse History
            try
            {
                IEnumerable<URL> chromeUrl = chromeHis.GetHistory(GetLocalAppDataPath(), urlLastCheck, nCurSId);

                if (chromeUrl != null)
                {
                    foreach (URL urlInfo in chromeHis.URLs)
                    {
                        DateTime accessTime = DateTime.Parse(urlInfo.VisitTime);
                        if (IsHisItemInArrayList(urlInfo.Url, urlInfo.Title, accessTime, urlHistoryList) == false)
                        {
                            M7WebHisItem hisItem = new M7WebHisItem();
                            hisItem.strUrl = urlInfo.Url;
                            hisItem.strTitle = urlInfo.Title;
                            hisItem.strAccessTime = accessTime;
                            urlHistoryList.Add(hisItem);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }

            // Get Firefox Browse History
            try
            {
                IEnumerable<URL> ffUrl = firefoxHis.GetHistory(GetAppDataPath(), urlLastCheck);

                if (ffUrl != null)
                {
                    foreach (URL urlInfo in firefoxHis.URLs)
                    {
                        DateTime accessTime = DateTime.Parse(urlInfo.VisitTime.ToString());

                        if (IsHisItemInArrayList(urlInfo.Url, urlInfo.Title, accessTime, urlHistoryList) == false)
                        {
                            M7WebHisItem hisItem = new M7WebHisItem();
                            hisItem.strUrl = urlInfo.Url;
                            hisItem.strTitle = urlInfo.Title;
                            hisItem.strAccessTime = accessTime;
                            urlHistoryList.Add(hisItem);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }

            // Get Sogou Browse History
            try
            {
                IEnumerable<URL> sogouUrl = sogouHis.GetHistory(GetAppDataPath(), urlLastCheck);

                if (sogouUrl != null)
                {
                    foreach (URL urlInfo in sogouHis.URLs)
                    {
                        DateTime accessTime = DateTime.Parse(urlInfo.VisitTime);

                        if (IsHisItemInArrayList(urlInfo.Url, urlInfo.Title, accessTime, urlHistoryList) == false)
                        {
                            M7WebHisItem hisItem = new M7WebHisItem();
                            hisItem.strUrl = urlInfo.Url;
                            hisItem.strTitle = urlInfo.Title;
                            hisItem.strAccessTime = accessTime;
                            urlHistoryList.Add(hisItem);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }

            if (urlHistoryList.Count != 0)
                urlHistoryList.Sort(SortFileTimeAscendingHelper.SortFileTimeAscending());

            urlLastCheck = DateTime.Now;
            AddWebHistoryToFile();

            Monitor.Exit(netObj);
        }

        /// <summary>
        /// GetAppDataPath
        /// </summary>
        /// <returns></returns>
        private string GetHistoryPath()
        {
            string strPath = "";

            RegistryKey rk = Registry.CurrentUser.OpenSubKey(SHELLFOLDERS_REG_KEY);
            if (rk == null) return strPath;

            strPath = (string)rk.GetValue(HISTORY_REG_KEY, "");
            rk.Close();

            return strPath;
        }

        /// <summary>
        /// GetAppDataPath
        /// </summary>
        /// <returns></returns>
        private string GetAppDataPath()
        {
            string strPath = "";

            RegistryKey rk = Registry.CurrentUser.OpenSubKey(SHELLFOLDERS_REG_KEY);
            if (rk == null) return strPath;

            strPath = (string)rk.GetValue(APPDATA_REG_KEY, ""); 
            rk.Close();

            return strPath;
        }

        /// <summary>
        /// GetLocalAppDataPath
        /// </summary>
        /// <returns></returns>
        private string GetLocalAppDataPath()
        {
            string strPath = "";

            RegistryKey rk = Registry.CurrentUser.OpenSubKey(SHELLFOLDERS_REG_KEY);
            if (rk == null) return strPath;

            strPath = (string)rk.GetValue(LOCALAPPDATA_REG_KEY, "");
            rk.Close();

            return strPath;
        }

        /// <summary>
        /// Update GUI components for the network interfaces
        /// </summary>
        private void UpdateNetworkInterface()
        {
            for (int id = 0; id < nicArr.Length; id++)
            {
                // Grab NetworkInterface object that describes the current interface
                NetworkInterface nic = nicArr[id];

                // Grab the stats for that interface
                IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();

                M1NetworkInfo netItem = (M1NetworkInfo)bandMon.netInfoList[id];
                if (netItem.nBytesSent == 0)
                    netItem.nBytesSent = interfaceStats.BytesSent;

                if (netItem.nBytesReceive == 0)
                    netItem.nBytesReceive = interfaceStats.BytesReceived;

                int bytesSentSpeed = (int)(interfaceStats.BytesSent - (double)netItem.nBytesSent);
                int bytesReceivedSpeed = (int)(interfaceStats.BytesReceived - (double)netItem.nBytesReceive);
                netItem.nUtilization = (int)((double)(bytesSentSpeed + bytesReceivedSpeed) / (nic.Speed / 8.0) * 100);

                if (netItem.nUtilization < 0) netItem.nUtilization = 0;
                else if (netItem.nUtilization > 100) netItem.nUtilization = 100;

                // Update the labels
                netItem.nSpeed = nic.Speed;
                netItem.strInterfaceType = nic.NetworkInterfaceType.ToString();
                netItem.nBytesReceive = interfaceStats.BytesReceived;
                netItem.nBytesSent = interfaceStats.BytesSent;
                netItem.nUploadSpeed = bytesSentSpeed;
                netItem.nDownloadSpeed = bytesReceivedSpeed;
            }
        }

        public void UpdateInternetState(bool bOldState)
        {
            nCurrentDnsCheck++;

            //ChangeFirewallSettings(true, true, true);

            bool bCondition = true;

            if (bStartEnable)
            {
                if (DateTime.Now < startTime)
                    bCondition = false;
            }

            if (bEndEnable)
            {
                if (DateTime.Now > endTime)
                    bCondition = false;
            }

            bool bEnableInternet = bNetEnable;
            if (bCondition == false)
                bEnableInternet = !bEnableInternet;

            ChangeInternetState(bEnableInternet);

            bool bStateChanged = bOldState;

            // Check IP List to enable or disable
            foreach (NetAllowIPItem appItem in ipAppList)
            {
                bCondition = true;

                if (appItem.startSet)
                {
                    if (DateTime.Now < appItem.startTime)
                        bCondition = false;
                }

                if (appItem.endSet)
                {
                    if (DateTime.Now > appItem.endTime)
                        bCondition = false;
                }

                String strRuleIP = appItem.strIp;
                if (appItem.bUrl)
                {
                    if (nCurrentDnsCheck >= nDnsCheckIinterval)
                    {
                        IPAddress[] addresslist = Dns.GetHostAddresses(appItem.strIp);
                        if (addresslist.Length > 0)
                        {
                            String strDnsIP = GetIPAddresses(addresslist);
                            if (strDnsIP != appItem.strDnsIP)
                            {
                                AllowIPAddr(appItem.strDnsIP, "");
                                appItem.strDnsIP = strDnsIP;
                                appItem.bAddBefore = false;
                                bStateChanged = true;
                            }
                        }
                    }

                    strRuleIP = appItem.strDnsIP;
                }

                if (bCondition)
                {
                    if (appItem.bAddBefore == false)
                    {
                        DisAllowIPAddr(strRuleIP, "");
                        appItem.bAddBefore = true;
                    }
                }
                else
                {
                    if (appItem.bAddBefore == true)
                    {
                        AllowIPAddr(strRuleIP, "");
                        appItem.bAddBefore = false;
                        bStateChanged = true;
                    }
                }
            }

            if (bStateChanged == true && ComMisc.vistaOverOS == false)
            {
                AllowIPAddr("*", "");
                foreach (NetAllowIPItem appItem in ipAppList)
                {
                    String strRuleIP = appItem.strIp;
                    if (appItem.bUrl)
                        strRuleIP = appItem.strDnsIP;

                    if (appItem.bAddBefore == true)
                        DisAllowIPAddr(strRuleIP, "");
                }
            }

            if (nCurrentDnsCheck >= nDnsCheckIinterval)
                nCurrentDnsCheck = 0;
        }

        /// <summary>
        /// The Timer event for each Tick (second) to update the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateNetworkState()
        {
            Monitor.Enter(netObj);
            if (bWatchNetwork)
                UpdateNetworkInterface();

            nCurrentCheck++;
            if (nCurrentCheck >= nCheckLimit)
            {
                nCurrentCheck = 0;
                UpdateInternetState(false);
            }
            Monitor.Exit(netObj);
        }

        public void WatchNetwork(bool bEnable)
        {
            bWatchNetwork = bEnable;
        }

        public void GetNetworkState(M1BandMon bandMonInf)
        {
            if (bandMonInf == null)
                return;

            Monitor.Enter(netObj);
            bandMonInf.netInfoList.Clear();

            foreach (M1NetworkInfo netinfo in bandMon.netInfoList)
            {
                M1NetworkInfo newinfo = new M1NetworkInfo(netinfo);
                bandMonInf.netInfoList.Add(newinfo);
            }
            Monitor.Exit(netObj);
        }

        public void SelRuleToFireWall(M3PortRule firerule, M3PortRule firerulenew)
        {
            if (ComMisc.vistaOverOS == true)
            {
                String netshCommand = "";

                netshCommand = "advfirewall firewall set rule";
                if (firerule.Name != null && firerule.Name != "")
                    netshCommand += " name=\"" + firerule.Name + "\"";

                if (firerule.Direction != null && firerule.Direction == "Inbound")
                    netshCommand += " dir=in";
                else
                    netshCommand += " dir=out";

                if (firerule.Profiles != null && firerule.Profiles != "")
                    netshCommand += " profile=" + firerule.Profiles;

                if (firerule.LocalAddr != null && firerule.LocalAddr != "" && firerule.LocalAddr != "*")
                    netshCommand += " localip=" + firerule.LocalAddr;

                if (firerule.LocalPorts != null && firerule.LocalPorts != "" && firerule.LocalPorts != "*")
                    netshCommand += " localport=" + firerule.LocalPorts;

                if (firerule.RemoteAddr != null && firerule.RemoteAddr != "" && firerule.RemoteAddr != "*")
                    netshCommand += " remoteip=" + firerule.RemoteAddr;

                if (firerule.RemotePorts != null && firerule.RemotePorts != "" && firerule.RemotePorts != "*")
                    netshCommand += " remoteport=" + firerule.RemotePorts;

                if (firerule.Protocol != null && firerule.Protocol != "")
                    netshCommand += " protocol=" + firerule.Protocol;

                netshCommand += " new";
                if (firerulenew.Name != null && firerulenew.Name != "")
                    netshCommand += " name=\"" + firerulenew.Name + "\"";
                else
                    netshCommand += " name=\"DefaultName\"";

                if (firerulenew.Direction == "Inbound")
                    netshCommand += " dir=in";
                else
                    netshCommand += " dir=out";

                if (firerulenew.Action != null && firerulenew.Action != "")
                    netshCommand += " action=" + firerulenew.Action;
                else
                    netshCommand += " action=allow";

                if (firerulenew.Profiles != null && firerulenew.Profiles != "")
                    netshCommand += " profile=" + firerulenew.Profiles;
                else
                    netshCommand += " profile=any";

                if (firerulenew.Description != null && firerulenew.Description != "")
                    netshCommand += " description=\"" + firerulenew.Description + "\"";

                if (firerulenew.AppPath != null && firerulenew.AppPath != "")
                    netshCommand += " program=\"" + firerulenew.AppPath + "\"";

                if (firerulenew.ServiceName != null && firerulenew.ServiceName != "")
                    netshCommand += " service=\"" + firerulenew.ServiceName + "\"";

                if (firerulenew.Enabled != null && firerulenew.Enabled != "")
                    netshCommand += " enable=" + firerulenew.Enabled;

                if (firerulenew.LocalAddr != null && firerulenew.LocalAddr != "" && firerulenew.LocalAddr != "*")
                    netshCommand += " localip=" + firerulenew.LocalAddr;

                if (firerulenew.LocalPorts != null && firerulenew.LocalPorts != "" && firerulenew.LocalPorts != "*")
                    netshCommand += " localport=" + firerulenew.LocalPorts;

                if (firerulenew.RemoteAddr != null && firerulenew.RemoteAddr != "" && firerulenew.RemoteAddr != "*")
                    netshCommand += " remoteip=" + firerulenew.RemoteAddr;

                if (firerulenew.RemotePorts != null && firerulenew.RemotePorts != "" && firerulenew.RemotePorts != "*")
                    netshCommand += " remoteport=" + firerulenew.RemotePorts;

                if (firerulenew.Protocol != null && firerulenew.Protocol != "")
                    netshCommand += " protocol=" + firerulenew.Protocol;

                if (firerulenew.InterfaceTypes != null && firerulenew.InterfaceTypes != "")
                    netshCommand += " interfacetype=" + firerulenew.InterfaceTypes;

                if (firerulenew.edge_traversal != null && firerulenew.edge_traversal != "")
                    netshCommand += " edge=" + firerulenew.edge_traversal;

                System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                try
                {
                    netshProcess.StartInfo.FileName = "netsh";
                    netshProcess.StartInfo.Arguments = netshCommand;
                    netshProcess.StartInfo.UseShellExecute = false;
                    netshProcess.StartInfo.CreateNoWindow = true;
                    netshProcess.Start();
                }
                catch (System.Exception ex)
                {
                    // Error occured
                    ComMisc.LogErrors(ex.ToString());
                }
            }
            else
            {
                DelRuleToFireWall(firerule);
                AddRuleToFirewall(firerulenew);
            }
        }

        public void DelRuleToFireWall(M3PortRule firerule)
        {
            if (ComMisc.vistaOverOS == true)
            {
                String netshCommand = "";

                netshCommand = "advfirewall firewall delete rule";
                if (firerule.Name != null && firerule.Name != "")
                    netshCommand += " name=\"" + firerule.Name + "\"";
                else
                    netshCommand += " name=all";

                if (firerule.Direction != null && firerule.Direction == "Inbound")
                    netshCommand += " dir=in";
                else
                    netshCommand += " dir=out";

                if (firerule.Profiles != null && firerule.Profiles != "")
                    netshCommand += " profile=" + firerule.Profiles;

                if (firerule.AppPath != null && firerule.AppPath != "")
                    netshCommand += " program=\"" + firerule.AppPath + "\"";

                if (firerule.ServiceName != null && firerule.ServiceName != "")
                    netshCommand += " service=\"" + firerule.ServiceName + "\"";

                if (firerule.LocalAddr != null && firerule.LocalAddr != "" && firerule.LocalAddr != "*")
                    netshCommand += " localip=" + firerule.LocalAddr;

                if (firerule.LocalPorts != null && firerule.LocalPorts != "" && firerule.LocalPorts != "*")
                    netshCommand += " localport=" + firerule.LocalPorts;

                if (firerule.RemoteAddr != null && firerule.RemoteAddr != "" && firerule.RemoteAddr != "*")
                    netshCommand += " remoteip=" + firerule.RemoteAddr;

                if (firerule.RemotePorts != null && firerule.RemotePorts != "" && firerule.RemotePorts != "*")
                    netshCommand += " remoteport=" + firerule.RemotePorts;

                if (firerule.Protocol != null && firerule.Protocol != "")
                    netshCommand += " protocol=" + firerule.Protocol;

                System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                try
                {
                    netshProcess.StartInfo.FileName = "netsh";
                    netshProcess.StartInfo.Arguments = netshCommand;
                    netshProcess.StartInfo.UseShellExecute = false;
                    netshProcess.StartInfo.CreateNoWindow = true;
                    netshProcess.Start();
                }
                catch (System.Exception ex)
                {
                    // Error occured
                    ComMisc.LogErrors(ex.ToString());
                }
            }
            else
            {
                if (firerule.AppPath != null && firerule.AppPath != "") // Add to Default Firewall list
                {
                    String netshCommand = "";

                    netshCommand = "firewall delete allowedprogram";
                    if (firerule.AppPath != null && firerule.AppPath != "")
                        netshCommand += " program = \"" + firerule.AppPath + "\"";

                    System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                    try
                    {
                        netshProcess.StartInfo.FileName = "netsh";
                        netshProcess.StartInfo.Arguments = netshCommand;
                        netshProcess.StartInfo.UseShellExecute = false;
                        netshProcess.StartInfo.CreateNoWindow = true;
                        netshProcess.Start();
                    }
                    catch (System.Exception ex)
                    {
                        // Error occured
                        ComMisc.LogErrors(ex.ToString());
                    }
                }
                else if (firerule.LocalPorts != null && firerule.LocalPorts != "")
                {
                    String netshCommand = "";

                    netshCommand = "firewall delete portopening";
                    if (firerule.Protocol != null && firerule.Protocol != "")
                        netshCommand += " protocol = " + firerule.Protocol;

                    if (firerule.LocalPorts != null && firerule.LocalPorts != "")
                        netshCommand += " port = " + firerule.LocalPorts;

                    System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                    try
                    {
                        netshProcess.StartInfo.FileName = "netsh";
                        netshProcess.StartInfo.Arguments = netshCommand;
                        netshProcess.StartInfo.UseShellExecute = false;
                        netshProcess.StartInfo.CreateNoWindow = true;
                        netshProcess.Start();
                    }
                    catch (System.Exception ex)
                    {
                        // Error occured
                        ComMisc.LogErrors(ex.ToString());
                    }
                }
                else
                {
                    System.ServiceProcess.ServiceController scPAServ = new System.ServiceProcess.ServiceController("PolicyAgent"); //IPSec

                    if (scPAServ.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                    {
                        scPAServ.Start(); //Start If Not Running
                    }
                    string strCommands = @"-w REG -p ""INMS_Firewall""";

//                     if (firerule.Name != null && firerule.Name != "")
//                         strCommands += " -r \"" + firerule.Name + "\"";
// 
//                     strCommands += " -f *+";
//                     if (firerule.RemoteAddr != null && firerule.RemoteAddr != "")
//                         strCommands += firerule.RemoteAddr + ":";
//                     else
//                         strCommands += "*:";
// 
//                     if (firerule.RemotePorts != null && firerule.RemotePorts != "")
//                         strCommands += firerule.RemotePorts + ":";
//                     else
//                         strCommands += "*:";
// 
//                     if (firerule.Protocol != null && firerule.Protocol != "")
//                         strCommands += firerule.Protocol;

                    strCommands += " -y";

                    try
                    {
                        ProcessStartInfo psiStart = new ProcessStartInfo(); //Process To Start

                        psiStart.CreateNoWindow = true; //Invisible

                        psiStart.FileName = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\ipseccmd.exe"; //IPSEC

                        psiStart.Arguments = strCommands; //Break Command Strings Apart

                        psiStart.WindowStyle = ProcessWindowStyle.Hidden; //Invisible

                        Process p = System.Diagnostics.Process.Start(psiStart); //Start Process To Block Internet Connection

                    }
                    catch (System.Exception ex)
                    {
                        ComMisc.LogErrors(ex.ToString());
                    }

                    strCommands = strCommands.Substring(0, strCommands.Length - 1);
                    strCommands += "o";

                    try
                    {
                        ProcessStartInfo psiStart = new ProcessStartInfo(); //Process To Start

                        psiStart.CreateNoWindow = true; //Invisible

                        psiStart.FileName = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\ipseccmd.exe"; //IPSEC

                        psiStart.Arguments = strCommands; //Break Command Strings Apart

                        psiStart.WindowStyle = ProcessWindowStyle.Hidden; //Invisible

                        Process p = System.Diagnostics.Process.Start(psiStart); //Start Process To Block Internet Connection

                    }
                    catch (System.Exception ex)
                    {
                        ComMisc.LogErrors(ex.ToString());
                    }
                }
            }
        }

        public void AddRuleToFirewall(M3PortRule firerule)
        {
            if (ComMisc.vistaOverOS == true)
            {
                String netshCommand = "";

                netshCommand = "advfirewall firewall add rule";
                if (firerule.Name != null && firerule.Name != "")
                    netshCommand += " name=\"" + firerule.Name + "\"";
                else
                    netshCommand += " name=\"DefaultName\"";

                if (firerule.Direction != null && firerule.Direction == "Inbound")
                    netshCommand += " dir=in";
                else
                    netshCommand += " dir=out";

                if (firerule.Action != null && firerule.Action != "")
                    netshCommand += " action=" + firerule.Action;
                else
                    netshCommand += " action=allow";

                if (firerule.Profiles != null && firerule.Profiles != "")
                    netshCommand += " profile=" + firerule.Profiles;
                else
                    netshCommand += " profile=any";

                if (firerule.Description != null && firerule.Description != "")
                    netshCommand += " description=\"" + firerule.Description + "\"";

                if (firerule.AppPath != null && firerule.AppPath != "")
                    netshCommand += " program=\"" + firerule.AppPath + "\"";

                if (firerule.ServiceName != null && firerule.ServiceName != "")
                    netshCommand += " service=\"" + firerule.ServiceName + "\"";

                if (firerule.Enabled != null && firerule.Enabled != "")
                    netshCommand += " enable=" + firerule.Enabled;

                if (firerule.LocalAddr != null && firerule.LocalAddr != "" && firerule.LocalAddr != "*")
                    netshCommand += " localip=" + firerule.LocalAddr;

                if (firerule.LocalPorts != null && firerule.LocalPorts != "" && firerule.LocalPorts != "*")
                    netshCommand += " localport=" + firerule.LocalPorts;

                if (firerule.RemoteAddr != null && firerule.RemoteAddr != "" && firerule.RemoteAddr != "*")
                    netshCommand += " remoteip=" + firerule.RemoteAddr;

                if (firerule.RemotePorts != null && firerule.RemotePorts != "" && firerule.RemotePorts != "*")
                    netshCommand += " remoteport=" + firerule.RemotePorts;

                if (firerule.Protocol != null && firerule.Protocol != "")
                    netshCommand += " protocol=" + firerule.Protocol;

                if (firerule.InterfaceTypes != null && firerule.InterfaceTypes != "")
                    netshCommand += " interfacetype=" + firerule.InterfaceTypes;

                if (firerule.edge_traversal != null && firerule.edge_traversal != "")
                    netshCommand += " edge=" + firerule.edge_traversal;

                System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                try
                {
                    netshProcess.StartInfo.FileName = "netsh";
                    netshProcess.StartInfo.Arguments = netshCommand;
                    netshProcess.StartInfo.UseShellExecute = false;
                    netshProcess.StartInfo.CreateNoWindow = true;
                    netshProcess.Start();
                }
                catch (System.Exception ex)
                {
                    // Error occured
                    ComMisc.LogErrors(ex.ToString());
                }
            }
            else
            {
                if (firerule.AppPath != null && firerule.AppPath != "") // Add to Default Firewall list
                {
                    String netshCommand = "";

                    netshCommand = "firewall add allowedprogram";
                    if (firerule.AppPath != null && firerule.AppPath != "")
                        netshCommand += " program = \"" + firerule.AppPath + "\"";

                    if (firerule.Name != null && firerule.Name != "")
                        netshCommand += " name = \"" + firerule.Name + "\"";
                    else
                        netshCommand += " name = \"DefaultName\"";

                    if (firerule.Action != null && firerule.Action != "")
                        netshCommand += (" mode = " + ((firerule.Action.ToLower() == "allow") ? "ENABLE" : "DISABLE"));
                    else
                        netshCommand += " mode = ENABLE";

                    System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                    try
                    {
                        netshProcess.StartInfo.FileName = "netsh";
                        netshProcess.StartInfo.Arguments = netshCommand;
                        netshProcess.StartInfo.UseShellExecute = false;
                        netshProcess.StartInfo.CreateNoWindow = true;
                        netshProcess.Start();
                    }
                    catch (System.Exception ex)
                    {
                        // Error occured
                        ComMisc.LogErrors(ex.ToString());
                    }
                }
                else if (firerule.LocalPorts != null && firerule.LocalPorts != "")
                {
                    String netshCommand = "";

                    netshCommand = "firewall add portopening";
                    if (firerule.Protocol != null && firerule.Protocol != "")
                    {
                        if (firerule.Protocol == "*")
                            netshCommand += " protocol = ALL";
                        else
                            netshCommand += " protocol = " + firerule.Protocol;
                    }

                    if (firerule.LocalPorts != null && firerule.LocalPorts != "")
                        netshCommand += " port = " + firerule.LocalPorts;
                    else
                        netshCommand += " port = *";

                    if (firerule.Name != null && firerule.Name != "")
                        netshCommand += " name = \"" + firerule.Name + "\"";
                    else
                        netshCommand += " name = \"DefaultName\"";

                    if (firerule.Action != null && firerule.Action != "")
                        netshCommand += (" mode = " + ((firerule.Action.ToLower().ToLower() == "allow") ? "ENABLE" : "DISABLE"));
                    else
                        netshCommand += " mode = ENABLE";

                    if (firerule.IcmpTypes != null && firerule.IcmpTypes != "" && firerule.IcmpTypes != "*")
                    {
                        netshCommand += " scope = CUSTOM addresses = " + firerule.IcmpTypes;
                    }

                    System.Diagnostics.Process netshProcess = new System.Diagnostics.Process();

                    try
                    {
                        netshProcess.StartInfo.FileName = "netsh";
                        netshProcess.StartInfo.Arguments = netshCommand;
                        netshProcess.StartInfo.UseShellExecute = false;
                        netshProcess.StartInfo.CreateNoWindow = true;
                        netshProcess.Start();
                    }
                    catch (System.Exception ex)
                    {
                        // Error occured
                        ComMisc.LogErrors(ex.ToString());
                    }
                }
                else    // Add to IP Security Policy list
                {
                    System.ServiceProcess.ServiceController scPAServ = new System.ServiceProcess.ServiceController("PolicyAgent"); //IPSec

                    if (scPAServ.Status != System.ServiceProcess.ServiceControllerStatus.Running)
                    {
                        scPAServ.Start(); //Start If Not Running
                    }
                    string strCommands = @"-w REG -p ""INMS_Firewall""";

                    if (firerule.Name != null && firerule.Name != "")
                        strCommands += " -r \"" + firerule.Name +  nCurRuleId + "\"";
                    else
                        strCommands += " -r \"DefaultName\"";

                    nCurRuleId++;

                    strCommands += " -f";

                    string strCurIP = firerule.RemoteAddr;
                    int nCommaId = strCurIP.IndexOf(',');

                    while (nCommaId != -1)
                    {
                        strCommands += " 0:*:*+";

                        string strip = strCurIP.Substring(0, nCommaId);
                        strCurIP = strCurIP.Substring(nCommaId + 1, strCurIP.Length - nCommaId - 1);

                        strCommands += strip + ":";

                        if (firerule.RemotePorts != null && firerule.RemotePorts != "")
                            strCommands += firerule.RemotePorts + ":";
                        else
                            strCommands += "*:";

                        if (firerule.Protocol != null && firerule.Protocol != "")
                            strCommands += firerule.Protocol;
                        else
                            strCommands += "*";

                        nCommaId = strCurIP.IndexOf(',');
                    }

                    strCommands += " 0:*:*+";
                    strCommands += strCurIP + ":";

                    if (firerule.RemotePorts != null && firerule.RemotePorts != "")
                        strCommands += firerule.RemotePorts + ":";
                    else
                        strCommands += "*:";

                    if (firerule.Protocol != null && firerule.Protocol != "")
                        strCommands += firerule.Protocol;
                    else
                        strCommands += "*";

                    if (firerule.Action != null && firerule.Action != "" && firerule.Action.ToLower() == "allow")
                        strCommands += " -n PASS";
                    else
                        strCommands += " -n BLOCK";

                    strCommands += " -x";

                    try
                    {
                        ProcessStartInfo psiStart = new ProcessStartInfo(); //Process To Start

                        psiStart.CreateNoWindow = true; //Invisible

                        psiStart.FileName = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\ipseccmd.exe"; //IPSEC

                        psiStart.Arguments = strCommands; //Break Command Strings Apart

                        psiStart.WindowStyle = ProcessWindowStyle.Hidden; //Invisible

                        Process p = System.Diagnostics.Process.Start(psiStart); //Start Process To Block Internet Connection
                    }
                    catch (System.Exception ex)
                    {
                        ComMisc.LogErrors(ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// GetFirewallRules
        /// </summary>
        /// <param name="pList"></param>
        public void GetFirewallRules(System.Collections.ArrayList pList)
        {
            if (pList == null)
                return;

            // Firewall Rules
            Type tNetFwMgr = Type.GetTypeFromProgID("HNetCfg.FwMgr");
            INetFwMgr fwMgr = (INetFwMgr)Activator.CreateInstance(tNetFwMgr);
            bool Firewallenabled = fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled;

            INetFwOpenPorts ports;
            INetFwOpenPort port;

            ports = (INetFwOpenPorts)fwMgr.LocalPolicy.CurrentProfile.GloballyOpenPorts; 
            System.Collections.IEnumerator enumerate = ports.GetEnumerator();
            while (enumerate.MoveNext()) 
            { 
                port = (INetFwOpenPort)enumerate.Current;
                AddPortRuleToList(port, ref pList);
            }

            INetFwAuthorizedApplications applications;
            INetFwAuthorizedApplication appInfo;

            applications = (INetFwAuthorizedApplications)fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications;

            enumerate = applications.GetEnumerator();
            while (enumerate.MoveNext())
            {
                appInfo = (INetFwAuthorizedApplication)enumerate.Current;
                AddAppRuleToList(appInfo, ref pList);
            }
            /*
            try
            {
                string strSpecFilter = "Specific Transport Filter";

                ProcessStartInfo psiStart = new ProcessStartInfo(); //Process To Start
                psiStart.CreateNoWindow = true; //Invisible
                psiStart.FileName = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ipseccmd.exe"; //IPSEC
                psiStart.Arguments = "show filters"; //Break Command Strings Apart
                psiStart.WindowStyle = ProcessWindowStyle.Hidden; //Invisible
                psiStart.RedirectStandardOutput = true;
                psiStart.UseShellExecute = false;
                Process p = System.Diagnostics.Process.Start(psiStart); //Start Process To Block Internet Connection
                //string output = p.StandardOutput.ReadToEnd();
                string line;
                bool bFilterStart = false;
                while ((line = p.StandardOutput.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) && bFilterStart == true)
                    {
                        AddCurrentFilter(ref pList);
                        bFilterStart = false;
                    }
                    else if (line.StartsWith(strSpecFilter) == true)
                    {
                        bFilterStart = true;
                        ResetCurrentInfo();
                    }
                    else if (bFilterStart == true)
                    {
                        if (line.StartsWith("---------"))
                            continue;

                        if (line.StartsWith("No filters"))
                            continue;

                        AddLineToFilter(line);
                    }
                }

                if (bFilterStart)
                {
                    bFilterStart = false;
                    AddCurrentFilter(ref pList);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }*/
        }

        private void AddCurrentFilter(ref ArrayList policylist)
        {
            if (policy == null || bPolicyValid == false)
                return;

            bPolicyValid = false;

            policylist.Add(policy);
        }

        private void GetProtocolFromStr(string strIndex, ref string strProtocol)
        {
            strIndex = strIndex.Trim();

            switch (strIndex)
            {
                case "1":
                    strProtocol = "ICMPv4";
                    break;
                case "2":
                    strProtocol = "IGMP";
                    break;
                case "6":
                    strProtocol = "TCP";
                    break;
                case "17":
                    strProtocol = "UDP";
                    break;
                case "41":
                    strProtocol = "IPv6";
                    break;
                case "47":
                    strProtocol = "GRE";
                    break;
                case "58":
                    strProtocol = "ICMPv6";
                    break;
                default:
                    strProtocol = "Any";
                    break;
            }
        }

        private void AddLineToFilter(string addline)
        {
            char seps = ':';
            string strkeywd = "";
            string strvalue = "";

            if (policy == null)
                return;

            bPolicyValid = true;
            int nSepId = addline.IndexOf(seps);
            if (nSepId != -1)
            {
                strkeywd = addline.Substring(0, nSepId);
                strvalue = addline.Substring(nSepId + 1, addline.Length - nSepId - 1);

                strkeywd = strkeywd.Trim();
                strvalue = strvalue.Trim();

                if (strkeywd.Equals("Name") == true)
                {
                    policy.Name = strvalue;
                }
                else if (strkeywd.Equals("Filter Id") == true)
                {

                }
                else if (strkeywd.Equals("Policy Id") == true)
                {

                }
                else if (strkeywd.Equals("Src Addr") == true)
                {
                    policy.LocalAddr = strvalue;
                }
                else if (strkeywd.Equals("Des Addr") == true)
                {
                    policy.RemoteAddr = strvalue;
                }
                else if (strkeywd.Equals("Direction") == true)
                {
                    if (strvalue.StartsWith("Inbound"))
                        policy.Direction = "Inbound";
                    else
                        policy.Direction = "Outbound";
                }
                else if (strkeywd.Equals("Interface Type") == true)
                {
                    policy.InterfaceTypes = strvalue;
                }
                else if (strkeywd.Equals("Protocol") == true)
                {
                    string protocol = "", strsrcport = "", strdstport = "";

                    // Get Protocol
                    nSepId = strvalue.IndexOf("  ");
                    if (nSepId != -1)
                    {
                        protocol = strvalue.Substring(0, nSepId);
                        GetProtocolFromStr(protocol, ref policy.Protocol);

                        strvalue = strvalue.Substring(nSepId + 2, strvalue.Length - nSepId - 2);
                        strvalue = strvalue.Trim();
                    }
                    else
                    {
                        protocol = strvalue;
                        GetProtocolFromStr(protocol, ref policy.Protocol);
                    }

                    // Get Source Port
                    nSepId = strvalue.IndexOf("  ");
                    if (nSepId != -1)
                    {
                        strsrcport = strvalue.Substring(0, nSepId);
                        strvalue = strvalue.Substring(nSepId + 2, strvalue.Length - nSepId - 2);
                        strvalue = strvalue.Trim();
                    }
                    else
                        strsrcport = strvalue;

                    nSepId = strsrcport.IndexOf(seps);
                    if (nSepId != -1)
                    {
                        strsrcport = strsrcport.Substring(nSepId + 1, strsrcport.Length - nSepId - 1);
                        strsrcport = strsrcport.Trim();
                    }

                    policy.LocalPorts = strsrcport;

                    // Get Dest Port
                    nSepId = strvalue.IndexOf("  ");
                    if (nSepId != -1)
                    {
                        strdstport = strvalue.Substring(0, nSepId);
                        strvalue = strvalue.Substring(nSepId + 2, strvalue.Length - nSepId - 2);
                        strvalue = strvalue.Trim();
                    }
                    else
                        strdstport = strvalue;

                    nSepId = strdstport.IndexOf(seps);
                    if (nSepId != -1)
                    {
                        strdstport = strdstport.Substring(nSepId + 1, strdstport.Length - nSepId - 1);
                        strdstport = strdstport.Trim();
                    }

                    policy.RemotePorts = strdstport;
                }
            }
            else
            {
                strkeywd = addline;
                strkeywd = strkeywd.Trim();

                if (strkeywd.Equals("Inbound Passthru") ||
                    strkeywd.Equals("Outbound Passthru"))
                    policy.Action = "Allow";
                else
                    policy.Action = "Block";
            }
        }

        private void ResetCurrentInfo()
        {
            policy = new M3PortRule();

            policy.Action = "Allow";
            policy.AppPath = "";
            policy.Description = "";
            policy.Direction = "Inbound";
            policy.edge_traversal = "";
            policy.Enabled = "Yes";
            policy.IcmpTypes = "";
            policy.Interfaces = "";
            policy.InterfaceTypes = "";
            policy.LocalAddr = "";
            policy.LocalPorts = "";
            policy.Name = "";
            policy.Profiles = "";
            policy.Protocol = "";
            policy.RemoteAddr = "";
            policy.RemotePorts = "";
            policy.ServiceName = "";

            bPolicyValid = false;
        }

        private void AddPortRuleToList(INetFwOpenPort portRule, ref ArrayList pList)
        {
            M3PortRule rule = new M3PortRule();

            rule.Action = (portRule.Enabled) ? "Allow" : "Block";
            rule.Name = portRule.Name;
            rule.Enabled = "Yes";
            rule.Direction = "Outbound";

            rule.AppPath = "";
            rule.Description = "";
            rule.edge_traversal = "";
            rule.GroupName = "";
            rule.IcmpTypes = "";
            rule.Interfaces = "";
            rule.InterfaceTypes = "";
            rule.LocalAddr = "";
            rule.LocalPorts = "";
            rule.nType = 0;
            rule.Profiles = "";
            rule.ServiceName = "";

            if (portRule.Protocol == NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP)
                rule.Protocol = "Tcp";
            else if (portRule.Protocol == NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP)
                rule.Protocol = "Udp";
            else
                rule.Protocol = "Any";

            rule.IcmpTypes = portRule.RemoteAddresses;
            rule.LocalPorts = portRule.Port.ToString();

            pList.Add(rule);
        }

        private void AddAppRuleToList(INetFwAuthorizedApplication appRule, ref ArrayList pList)
        {
            M3PortRule rule = new M3PortRule();

            rule.Action = (appRule.Enabled) ? "Allow" : "Block";
            rule.Name = appRule.Name;
            rule.Enabled = "Yes";
            rule.Direction = "Outbound";
            rule.AppPath = appRule.ProcessImageFileName;
            rule.RemoteAddr = appRule.RemoteAddresses;

            rule.Description = "";
            rule.edge_traversal = "";
            rule.GroupName = "";
            rule.IcmpTypes = "";
            rule.Interfaces = "";
            rule.InterfaceTypes = "";
            rule.LocalAddr = "";
            rule.LocalPorts = "";
            rule.nType = 0;
            rule.Profiles = "";
            rule.RemotePorts = "";
            rule.ServiceName = "";

            pList.Add(rule);
        }

        /// <summary>
        /// GetFirewallRules2
        /// </summary>
        /// <param name="pList"></param>
        public void GetFirewallRules2(System.Collections.ArrayList pList)
        {
            if (pList == null)
                return;

            Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

            foreach (INetFwRule rule in fwPolicy2.Rules)
            {
                M3PortRule portRule = new M3PortRule();

                if (rule.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN)
                {
                    portRule.Direction = "Inbound";
                }
                else if (rule.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT)
                {
                    portRule.Direction = "Outbound";
                }

                if (rule.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW)
                {
                    portRule.Action = "Allow";
                }
                else if (rule.Action == NET_FW_ACTION_.NET_FW_ACTION_BLOCK)
                {
                    portRule.Action = "Block";
                }

                portRule.AppPath = rule.ApplicationName;
                portRule.Enabled = rule.Enabled ? "Yes" : "No";
                portRule.Description = rule.Description;
                portRule.edge_traversal = rule.EdgeTraversal ? "yes" : "no";
                portRule.IcmpTypes = rule.IcmpTypesAndCodes;
                portRule.Interfaces = "";
                //portRule.Interfaces = rule.Interfaces;
                portRule.InterfaceTypes = rule.InterfaceTypes;
                if (portRule.InterfaceTypes.ToLower() == "all")
                    portRule.InterfaceTypes = "any";

                portRule.LocalAddr = rule.LocalAddresses;
                portRule.LocalPorts = rule.LocalPorts;
                portRule.Name = rule.Name;

                portRule.Profiles = "";
                if (rule.Profiles == 2147483647)
                    portRule.Profiles = "any";
                else
                {
                    if ((rule.Profiles & 0x1) != 0)
                    {
                        portRule.Profiles = "Domain";
                    }

                    if ((rule.Profiles & 0x2) != 0)
                    {
                        if (portRule.Profiles == "")
                            portRule.Profiles = "Private";
                        else
                            portRule.Profiles += "," + "Private";
                    }

                    if ((rule.Profiles & 0x4) != 0)
                    {
                        if (portRule.Profiles == "")
                            portRule.Profiles = "Public";
                        else
                            portRule.Profiles += "," + "Public";
                    }
                }

                portRule.Protocol = "";
                switch (rule.Protocol)
                {
                    case 1:
                        portRule.Protocol = "ICMPv4";
                        break;
                    case 2:
                        portRule.Protocol = "IGMP";
                        break;
                    case 6:
                        portRule.Protocol = "TCP";
                        break;
                    case 17:
                        portRule.Protocol = "UDP";
                        break;
                    case 41:
                        portRule.Protocol = "IPv6";
                        break;
                    case 47:
                        portRule.Protocol = "GRE";
                        break;
                    case 58:
                        portRule.Protocol = "ICMPv6";
                        break;
                    default:
                        portRule.Protocol = "Any";
                        break;
                }

                portRule.RemoteAddr = rule.RemoteAddresses;
                portRule.RemotePorts = rule.RemotePorts;
                portRule.ServiceName = rule.serviceName;

                pList.Add(portRule);
            }
        }

        public void GetNetWorkConList(ArrayList netHisList)
        {
            Monitor.Enter(netObj);
            foreach (M7NetAppHisItem item in netAppHisList)
            {
                M7NetAppHisItem newitem = new M7NetAppHisItem(item);
                netHisList.Add(newitem);
            }

            netAppHisList.Clear();
            ClearNetUrlHistory();
            Monitor.Exit(netObj);
        }

        public void GetNetMonList(ArrayList netMonList)
        {
            Monitor.Enter(netObj);
            ArrayList newList = new ArrayList();
            GetNetworkConnection(newList);
            foreach (M2NetAppItem item in newList)
            {
                M2NetAppItem newitem = new M2NetAppItem(item);
                netMonList.Add(newitem);
            }
            Monitor.Exit(netObj);
        }

        public void GetNetworkConnection(ArrayList appList)
        {
            Win32.MIB_TCPROW_OWNER_PID[] rowTable = Win32.GetAllTcpConnections();

            foreach (Win32.MIB_TCPROW_OWNER_PID item in rowTable)
            {
                if (item.State != "LISTEN" &&
                    item.State != "ESTAB")
                    continue;

                byte[] byteaddr = BitConverter.GetBytes(item.localAddr);
                String strLocalAddr = byteaddr[0] + "." + byteaddr[1] + "." + byteaddr[2] + "." + byteaddr[3] + ":" + item.LocalPort;

                byteaddr = BitConverter.GetBytes(item.remoteAddr);
                String strRemoteAddr = byteaddr[0] + "." + byteaddr[1] + "." + byteaddr[2] + "." + byteaddr[3] + ":" + item.RemotePort;

                if (IsValidAddress(byteaddr) == false)
                    continue;

                try
                {
                    System.Diagnostics.Process procFind = System.Diagnostics.Process.GetProcessById(item.owningPid);
                    if (procFind != null)
                    {
                        M2NetAppItem netItem = new M2NetAppItem();
                        netItem.strLocalAddr = strLocalAddr;
                        netItem.strRemoteAddr = strRemoteAddr;
                        netItem.strPgname = procFind.ProcessName;
                        try
                        {
                            if (netItem.strPgname.ToLower() == "system")
                                netItem.strPgpath = "";
                            else
                                netItem.strPgpath = procFind.MainModule.FileName;
                        }
                        catch (System.Exception ex)
                        {
                            netItem.strPgpath = "";
                        }
                        netItem.strStatus = item.State;

                        appList.Add(netItem);
                    }
                }
                catch (System.Exception ex)
                {
                    //ComMisc.LogErrors(ex.ToString());
                    //Console.WriteLine(ex.ToString());
                }
            }
        }

        public void UpdateConnectionHistory()
        {
            Monitor.Enter(netObj);
            ArrayList newList = new ArrayList();
            GetNetworkConnection(newList);

            foreach (M2NetAppItem netItem in newList)
            {
                bool bExist = false;

                foreach (M2NetAppItem netPrevItem in prevConList)
                {
                    if (netItem.strLocalAddr == netPrevItem.strLocalAddr &&
                        netItem.strRemoteAddr == netPrevItem.strRemoteAddr &&
                        netItem.strPgname == netPrevItem.strPgname &&
                        netItem.strStatus == netPrevItem.strStatus)
                    {
                        bExist = true;
                        netPrevItem.strStatus = "Exist";
                    }
                }

                if (bExist == false)
                {
                    if (netItem.strPgname.IndexOf(AgentEngine.Properties.Resources.INMS_NAME) == -1)
                    {
                        M7NetAppHisItem newItem = new M7NetAppHisItem();
                        newItem.strLocalAddr = netItem.strLocalAddr;
                        newItem.strRemoteAddr = netItem.strRemoteAddr;
                        newItem.strProgram = netItem.strPgname;
                        newItem.strPath = netItem.strPgpath;
                        newItem.strStatus = "Started";
                        newItem.strAccessTime = DateTime.Now;

                        if (IsNetHisIteminList(newItem, netAppHisList) == false)
                        {
                            netAppHisList.Add(newItem);
                            AddNetHistoryToFile(newItem);
                        }
                    }
                }
            }

            foreach (M2NetAppItem netPrevItem in prevConList)
            {
                if (netPrevItem.strStatus != "Exist")
                {
                    if (netPrevItem.strPgname.IndexOf(AgentEngine.Properties.Resources.INMS_NAME) == -1)
                    {
                        M7NetAppHisItem newItem = new M7NetAppHisItem();
                        newItem.strLocalAddr = netPrevItem.strLocalAddr;
                        newItem.strRemoteAddr = netPrevItem.strRemoteAddr;
                        newItem.strProgram = netPrevItem.strPgname;
                        newItem.strPath = netPrevItem.strPgpath;
                        newItem.strStatus = "Ended";
                        newItem.strAccessTime = DateTime.Now;

                        if (IsNetHisIteminList(newItem, netAppHisList) == false)
                        {
                            netAppHisList.Add(newItem);
                            AddNetHistoryToFile(newItem);
                        }
                    }
                }
            }

            prevConList.Clear();
            foreach (M2NetAppItem netItem in newList)
            {
                M2NetAppItem newitem = new M2NetAppItem(netItem);
                prevConList.Add(newitem);
            }

            Monitor.Exit(netObj);
        }

        public void DisAllowIPAddr(String strIPAddr, String strPorts)
        {
            M3PortRule portRule = new M3PortRule();

            portRule.Direction = "Inbound";
            portRule.Enabled = "Yes";
            portRule.GroupName = strGroupName;
            portRule.RemoteAddr = strIPAddr;
            portRule.RemotePorts = strPorts;
            portRule.Action = "Block";

            if (ComMisc.vistaOverOS == true)
            {
                portRule.Name = "INMS_IPProhibit";
                AddRuleToFirewall(portRule);

                portRule.Direction = "Outbound";
                AddRuleToFirewall(portRule);
            }
            else
            {
                portRule.Name = "INMS_Firewall";
                AddRuleToFirewall(portRule);
            }
        }

        public void AllowIPAddr(String strIPAddr, String strPorts)
        {
            M3PortRule portRule = new M3PortRule();

            portRule.Direction = "Inbound";
            portRule.Enabled = "Yes";
            portRule.GroupName = strGroupName;
            portRule.RemoteAddr = strIPAddr;
            portRule.RemotePorts = strPorts;

            if (ComMisc.vistaOverOS == true)
            {
                portRule.Name = "INMS_IPProhibit";
                DelRuleToFireWall(portRule);

                portRule.Direction = "Outbound";
                DelRuleToFireWall(portRule);
            }
            else
            {
                portRule.Name = "INMS_Firewall";
                DelRuleToFireWall(portRule);
                
                nCurRuleId = 0;
            }
        }

        public void RestoreNetHistoryFromFile()
        {
            String strDelimiter = "|";
            netAppHisList.Clear();

            try
            {
                StreamReader writeStream = File.OpenText(System.Environment.SystemDirectory + "\\" + strNetFileName + nCurSId);

                while (writeStream.EndOfStream == false)
                {
                    M7NetAppHisItem netItem = new M7NetAppHisItem();
                    String strLine = writeStream.ReadLine();

                    int nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Program Name
                    netItem.strProgram = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Program Path
                    netItem.strPath = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Process State
                    netItem.strStatus = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Local Addr
                    netItem.strLocalAddr = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Remote Addr
                    netItem.strRemoteAddr = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);
                    
                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                    {
                        // Get Process EndTime
                        netItem.strAccessTime = DateTime.Parse(strLine);
                    }
                    else
                    {
                        // Get Process EndTime
                        netItem.strAccessTime = DateTime.Parse(strLine.Substring(0, nKeyPos));
                    }

                    netAppHisList.Add(netItem);
                }
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void RestoreWebHistoryFromFile()
        {
            try
            {
                StreamReader writeStream = File.OpenText(System.Environment.SystemDirectory + "\\" + strWebFileName + nCurSId);

                if (writeStream.EndOfStream == false)
                {
                    String strLine = writeStream.ReadLine();
                    long nReadTime = long.Parse(strLine);

                    urlLastCheck = DateTime.FromFileTimeUtc(nReadTime).ToLocalTime();
                }
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }

            GetUrlHistory();
        }

        public DateTime GetNetAppSetTime()
        {
            return dtLastAccessNet;
        }

        public void RestoreNetAppFromFile()
        {
            String strDelimiter = "|";
            ipAppList.Clear();

            try
            {
                StreamReader writeStream = File.OpenText(System.Environment.SystemDirectory + "\\" + strNetAppFileName + nCurSId);

                if (writeStream.EndOfStream == false)
                {
                    String strLine = writeStream.ReadLine();
                    dtLastAccessNet = DateTime.Parse(strLine);
                }

                if (writeStream.EndOfStream == false)
                {
                    String strLine = writeStream.ReadLine();

                    int nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos != -1)
                    {
                        // Net Enable State
                        bNetEnable = (strLine.Substring(0, nKeyPos) == "1" ? true : false);
                        strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);
                    }

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos != -1)
                    {
                        // Start Enable State
                        bStartEnable = (strLine.Substring(0, nKeyPos) == "1" ? true : false);
                        strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);
                    }

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos != -1)
                    {
                        // Start Time
                        startTime = DateTime.Parse(strLine.Substring(0, nKeyPos));
                        strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);
                    }

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos != -1)
                    {
                        // End Enable State
                        bEndEnable = (strLine.Substring(0, nKeyPos) == "1" ? true : false);
                        strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);
                    }

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos != -1)
                    {
                        // End Time
                        endTime = DateTime.Parse(strLine.Substring(0, nKeyPos));
                        strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);
                    }
                    else
                        endTime = DateTime.Parse(strLine);
                }

                while (writeStream.EndOfStream == false)
                {
                    NetAllowIPItem netItem = new NetAllowIPItem();
                    String strLine = writeStream.ReadLine();

                    int nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Type
                    netItem.nType = int.Parse(strLine.Substring(0, nKeyPos));
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get IP/Url Path
                    netItem.strIp = strLine.Substring(0, nKeyPos);
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Start Time State
                    netItem.startSet = (strLine.Substring(0, nKeyPos) == "1") ? true : false;
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Start Time
                    netItem.startTime = DateTime.Parse(strLine.Substring(0, nKeyPos));
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get End Time State
                    netItem.endSet = (strLine.Substring(0, nKeyPos) == "1") ? true : false;
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get End Time
                    netItem.endTime = DateTime.Parse(strLine.Substring(0, nKeyPos));
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get AddBefore
                    netItem.bAddBefore = (strLine.Substring(0, nKeyPos) == "1") ? true : false;
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                        continue;

                    // Get Url
                    netItem.bUrl = (strLine.Substring(0, nKeyPos) == "1") ? true : false;
                    strLine = strLine.Substring(nKeyPos + strDelimiter.Length, strLine.Length - nKeyPos - strDelimiter.Length);

                    nKeyPos = strLine.IndexOf(strDelimiter);
                    if (nKeyPos == -1)
                    {
                        // Get DNS IP
                        netItem.strDnsIP = strLine;
                    }
                    else
                    {
                        // Get DNS IP
                        netItem.strDnsIP = strLine.Substring(0, nKeyPos);
                    }

                    ipAppList.Add(netItem);
                }
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void SetNetAppDataToFile(DateTime lastAccess, ArrayList dataList)
        {
            dtLastAccessNet = lastAccess;

            try
            {
                StreamWriter writeStream = File.CreateText(System.Environment.SystemDirectory + "\\" + strNetAppFileName + nCurSId);

                String strLine = "";
                strLine = lastAccess.ToString() + "\r\n";
                writeStream.Write(strLine);

                strLine = (bNetEnable ? "1" : "0") + "|" + (bStartEnable ? "1" : "0") + "|" + startTime.ToString() + "|" + (bEndEnable ? "1" : "0") + "|" + endTime.ToString() + "\r\n";
                writeStream.Write(strLine);

                foreach (NetAllowIPItem item in dataList)
                {
                    strLine = "";
                    strLine = item.nType.ToString() + "|" + item.strIp + "|" + (item.startSet ? "1" : "0") + "|" + item.startTime.ToString() + "|" + (item.endSet ? "1" : "0") + "|" + item.endTime.ToString() + "|" + (item.bAddBefore ? "1" : "0") + "|" + (item.bUrl ? "1" : "0") + "|" + item.strDnsIP + "\r\n";
                    writeStream.Write(strLine);
                }
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void AddNetHistoryToFile(M7NetAppHisItem procItem)
        {
            try
            {
                StreamWriter writeStream = File.AppendText(System.Environment.SystemDirectory + "\\" + strNetFileName + nCurSId);

                String strLine = "";
                strLine = procItem.strProgram + "|" + procItem.strPath + "|" + procItem.strStatus + "|" + procItem.strLocalAddr + "|" + procItem.strRemoteAddr + "|" + procItem.strAccessTime + "\r\n";
                writeStream.Write(strLine);
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void AddWebHistoryToFile()
        {
            try
            {
                StreamWriter writeStream = File.CreateText(System.Environment.SystemDirectory + "\\" + strWebFileName + nCurSId);

                String strLine = "";
                strLine = urlLastCheck.ToFileTimeUtc().ToString();
                writeStream.Write(strLine);
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void ClearNetUrlHistory()
        {
            netAppHisList.Clear();

            try
            {
                StreamWriter writeStream = File.CreateText(System.Environment.SystemDirectory + "\\" + strNetFileName + nCurSId);
                writeStream.Write("");
                writeStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        private bool IsHisItemInArrayList(String strUrl, String strTitle, DateTime accessTime, ArrayList itemList)
        {
            bool bExist = false;

            foreach (M7WebHisItem listitem in itemList)
            {
                if (listitem.strUrl == strUrl &&
                    listitem.strTitle == strTitle &&
                    listitem.strAccessTime == accessTime)
                {
                    bExist = true;
                    break;
                }
            }

            return bExist;
        }

        private bool IsNetHisIteminList(M7NetAppHisItem item, ArrayList itemList)
        {
            bool bExist = false;

            foreach (M7NetAppHisItem listitem in itemList)
            {
                if (listitem.strAccessTime == item.strAccessTime &&
                    listitem.strLocalAddr == item.strLocalAddr &&
                    listitem.strPath == item.strPath &&
                    listitem.strProgram == item.strProgram &&
                    listitem.strRemoteAddr == item.strRemoteAddr &&
                    listitem.strStatus == item.strStatus)
                {
                    bExist = true;
                    break;
                }
            }

            return bExist;
        }

        private bool IsValidAddress(byte[] addr)
        {
            bool bValid = true;

            if (addr[0] == 0 && addr[1] == 0 && addr[2] == 0)   // Address: 0.0.0.*
                bValid = false;

            if (addr[0] == 127 && addr[1] == 0 && addr[2] == 0 && addr[3] == 1) // Address: 127.0.0.1
                bValid = false;

            return bValid;
        }

        private String GetIPAddresses(IPAddress[] addrList)
        {
            String strRet = "";

            int iCnt = 0;
            for (iCnt = 0; iCnt < addrList.Length; iCnt++)
            {
                if (iCnt == 0)
                    strRet = addrList[iCnt].ToString();
                else
                    strRet += "," + addrList[iCnt];
            }

            return strRet;
        }
    }

    public class NetAllowIPItem : M2NetAllowAppItem
    {
        public bool bAddBefore = false;
        public bool bUrl = false;
        public string strDnsIP = "";

        public NetAllowIPItem()
        {

        }

        public NetAllowIPItem(NetAllowIPItem item)
        {
            nType = item.nType;
            strIp = item.strIp;
            startSet = item.startSet;
            startTime = item.startTime;
            endSet = item.endSet;
            endTime = item.endTime;

            bAddBefore = item.bAddBefore;
            bUrl = item.bUrl;
            strDnsIP = item.strDnsIP;
        }
    }
}
