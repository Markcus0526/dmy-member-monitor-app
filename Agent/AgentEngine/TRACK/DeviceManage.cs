using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Management;
using System.Threading;
using System.Collections;
using INMC.INMMessage;
using UsbEject.Library;
namespace AgentEngine
{
    public class DeviceManage
    {
        // Thread Management
        object devobj = new object();

        // Computer Name and Workgroup Status
        internal String strComputerName = "";
        internal String strDomainName = "";
        internal Win32.NetJoinStatus nJoinStatus = Win32.NetJoinStatus.NetSetupUnknownStatus;

        /* Description USBSTOR Reg Key
         * HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\USBSTOR\Start Key is used to change usb state. 3: enable, 4: disable
        */
        private static String subKey = @"SYSTEM\CurrentControlSet\services\USBSTOR";

        // Device Manager List
        private M8DeviceMon devMonList = new M8DeviceMon();

        // Device Category List
        private System.Collections.ArrayList deviceCom = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceDisk = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceDisplay = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceIDE = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceKeyboard = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceMice = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceMonitor = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceNetwork = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceOther = new System.Collections.ArrayList();
        private System.Collections.ArrayList devicePorts = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceProcessor = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceSound = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceSystem = new System.Collections.ArrayList();
        private System.Collections.ArrayList deviceUniversal = new System.Collections.ArrayList();

        // Device Category Class Name
        private static String[] strClassName = {"{4d36e966-e325-11ce-bfc1-08002be10318}", 
                                                   "{4d36e967-e325-11ce-bfc1-08002be10318}", 
                                                   "{4d36e968-e325-11ce-bfc1-08002be10318}", 
                                                   "{4d36e96a-e325-11ce-bfc1-08002be10318}",
                                                   "{4d36e96b-e325-11ce-bfc1-08002be10318}",
                                                   "{4d36e96f-e325-11ce-bfc1-08002be10318}",
                                                   "{4d36e96e-e325-11ce-bfc1-08002be10318}",
                                                   "{4d36e972-e325-11ce-bfc1-08002be10318}",
                                                   "",
                                                   "{4d36e978-e325-11ce-bfc1-08002be10318}",
                                                   "{50127dc3-0f36-415e-a6cc-4cb3be910b65}",
                                                   "{4d36e96c-e325-11ce-bfc1-08002be10318}",
                                                   "{4d36e97d-e325-11ce-bfc1-08002be10318}",
                                                   "{36fc9e60-c465-11cf-8056-444553540000}"
                                                   };

        public DeviceManage()
        {
            GetHardwareInfo();
        }

        public void GetHardwareInfo()
        {
            Monitor.Enter(devobj);

            deviceCom.Clear();
            deviceDisk.Clear();
            deviceDisplay.Clear();
            deviceIDE.Clear();
            deviceKeyboard.Clear();
            deviceMice.Clear();
            deviceMonitor.Clear();
            deviceNetwork.Clear();
            deviceOther.Clear();
            devicePorts.Clear();
            deviceProcessor.Clear();
            deviceSound.Clear();
            deviceSystem.Clear();
            deviceUniversal.Clear();

            GetDeviceInfo();

            if (deviceNetwork.Count > 0)
                ReFilterNetwork();

            devMonList.computer.Clear();
            foreach (String strName in deviceCom)
                devMonList.computer.Add(strName);
            devMonList.computer.Sort();

            devMonList.diskdrive.Clear();
            foreach (String strName in deviceDisk)
                devMonList.diskdrive.Add(strName);
            devMonList.diskdrive.Sort();

            devMonList.display.Clear();
            foreach (String strName in deviceDisplay)
                devMonList.display.Add(strName);
            devMonList.display.Sort();

            devMonList.ide.Clear();
            foreach (String strName in deviceIDE)
                devMonList.ide.Add(strName);
            devMonList.ide.Sort();

            devMonList.keyboard.Clear();
            foreach (String strName in deviceKeyboard)
                devMonList.keyboard.Add(strName);
            devMonList.keyboard.Sort();

            devMonList.mice_pointing.Clear();
            foreach (String strName in deviceMice)
                devMonList.mice_pointing.Add(strName);
            devMonList.mice_pointing.Sort();

            devMonList.monitor.Clear();
            foreach (String strName in deviceMonitor)
                devMonList.monitor.Add(strName);
            devMonList.monitor.Sort();

            devMonList.network.Clear();
            foreach (String strName in deviceNetwork)
                devMonList.network.Add(strName);
            devMonList.network.Sort();

            devMonList.other.Clear();
            foreach (String strName in deviceOther)
                devMonList.other.Add(strName);
            devMonList.other.Sort();

            devMonList.ports.Clear();
            foreach (String strName in devicePorts)
                devMonList.ports.Add(strName);
            devMonList.ports.Sort();

            devMonList.processor.Clear();
            foreach (String strName in deviceProcessor)
                devMonList.processor.Add(strName);
            devMonList.processor.Sort();

            devMonList.sound.Clear();
            foreach (String strName in deviceSound)
                devMonList.sound.Add(strName);
            devMonList.sound.Sort();

            devMonList.system.Clear();
            foreach (String strName in deviceSystem)
                devMonList.system.Add(strName);
            devMonList.system.Sort();

            devMonList.universal.Clear();
            foreach (String strName in deviceUniversal)
                devMonList.universal.Add(strName);
            devMonList.universal.Sort();

            Monitor.Exit(devobj);
        }

        public void GetDeviceList(M8DeviceMon devMon, bool bRefresh)
        {
            if (bRefresh)
                GetHardwareInfo();

            Monitor.Enter(devobj);

            devMon.computer.Clear();
            foreach (String strName in devMonList.computer)
                devMon.computer.Add(strName);
            devMon.computer.Sort();

            devMon.diskdrive.Clear();
            foreach (String strName in devMonList.diskdrive)
                devMon.diskdrive.Add(strName);
            devMon.diskdrive.Sort();

            devMon.display.Clear();
            foreach (String strName in devMonList.display)
                devMon.display.Add(strName);
            devMon.display.Sort();

            devMon.ide.Clear();
            foreach (String strName in devMonList.ide)
                devMon.ide.Add(strName);
            devMon.ide.Sort();

            devMon.keyboard.Clear();
            foreach (String strName in devMonList.keyboard)
                devMon.keyboard.Add(strName);
            devMon.keyboard.Sort();

            devMon.mice_pointing.Clear();
            foreach (String strName in devMonList.mice_pointing)
                devMon.mice_pointing.Add(strName);
            devMon.mice_pointing.Sort();

            devMon.monitor.Clear();
            foreach (String strName in devMonList.monitor)
                devMon.monitor.Add(strName);
            devMon.monitor.Sort();

            devMon.network.Clear();
            foreach (String strName in devMonList.network)
                devMon.network.Add(strName);
            devMon.network.Sort();

            devMon.other.Clear();
            foreach (String strName in devMonList.other)
                devMon.other.Add(strName);
            devMon.other.Sort();

            devMon.ports.Clear();
            foreach (String strName in devMonList.ports)
                devMon.ports.Add(strName);
            devMon.ports.Sort();

            devMon.processor.Clear();
            foreach (String strName in devMonList.processor)
                devMon.processor.Add(strName);
            devMon.processor.Sort();

            devMon.sound.Clear();
            foreach (String strName in devMonList.sound)
                devMon.sound.Add(strName);
            devMon.sound.Sort();

            devMon.system.Clear();
            foreach (String strName in devMonList.system)
                devMon.system.Add(strName);
            devMon.system.Sort();

            devMon.universal.Clear();
            foreach (String strName in devMonList.universal)
                devMon.universal.Add(strName);
            devMon.universal.Sort();

            Monitor.Exit(devobj);
        }

        private void ReFilterNetwork()
        {
            deviceNetwork.Clear();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_NetworkAdapter");

            try
            {
                foreach (ManagementObject share in searcher.Get())
                {
                    String strName = "";

                    try
                    {
                        strName = share["Name"].ToString();
                    }
                    catch
                    {
                        strName = share.ToString();
                    }

                    if (share.Properties.Count <= 0)
                    {
                        continue;
                    }

                    bool bEnable = false;
                    foreach (PropertyData PC in share.Properties)
                    {
                        String strTitle = PC.Name;
                        String strValue = "";

                        if (ComMisc.vistaOverOS == true)
                        {
                            if (strTitle != "PhysicalAdapter")
                                continue;
                        }
                        else
                        {
                            if (strTitle != "NetConnectionID")
                                continue;
                        }
                        

                        if (PC.Value != null && PC.Value.ToString() != "")
                        {
                            switch (PC.Value.GetType().ToString())
                            {
                                case "System.String[]":
                                    string[] str = (string[])PC.Value;

                                    string str2 = "";
                                    foreach (string st in str)
                                        str2 += st + " ";

                                    strValue = str2;

                                    break;
                                case "System.UInt16[]":
                                    ushort[] shortData = (ushort[])PC.Value;

                                    string tstr2 = "";
                                    foreach (ushort st in shortData)
                                        tstr2 += st.ToString() + " ";

                                    strValue = tstr2;

                                    break;

                                default:
                                    strValue = PC.Value.ToString();
                                    break;
                            }
                        }

                        if (ComMisc.vistaOverOS == true)
                        {
                            if (strValue == "True")
                                bEnable = true;
                        }
                        else
                        {
                            if (strValue.Length > 0)
                                bEnable = true;
                        }
                    }

                    if (bEnable == true)
                        deviceNetwork.Add(strName);
                }
            }
            catch (Exception exp)
            {
                // Error occured
                ComMisc.LogErrors(exp.ToString());
            }
        }

        private void GetDeviceInfo()
        {
            ManagementObjectSearcher devicelist = new ManagementObjectSearcher("select Name,Status,ClassGuid from Win32_PnPEntity");

            try
            {
                foreach (ManagementObject device in devicelist.Get())
                {                    
                    String strName = "";

                    try
                    {
                        if (device != null && device["Name"] != null)
                            strName = device["Name"].ToString();
                        else
                            strName = device.ToString();
                    }
                    catch
                    {
                        strName = device.ToString();
                    }

                    if (device.Properties.Count <= 0)
                    {
                        continue;
                    }
                    int category = -1;
                    foreach (PropertyData PC in device.Properties)
                    {
                        String strTitle = PC.Name;
                        String strValue = "";                        

                        if (strTitle != "Status" && strTitle != "ClassGuid")
                            continue;
                        
                        if (PC.Value != null && PC.Value.ToString() != "")
                        {
                            switch (PC.Value.GetType().ToString())
                            {
                                case "System.String[]":
                                    string[] str = (string[])PC.Value;

                                    string str2 = "";
                                    foreach (string st in str)
                                        str2 += st + " ";

                                    strValue = str2;

                                    break;
                                case "System.UInt16[]":
                                    ushort[] shortData = (ushort[])PC.Value;

                                    string tstr2 = "";
                                    foreach (ushort st in shortData)
                                        tstr2 += st.ToString() + " ";

                                    strValue = tstr2;

                                    break;

                                default:
                                    strValue = PC.Value.ToString();
                                    break;
                            }
                        }

                        if (strTitle == "Status")
                        {
                            if (strValue == "Error")
                            {
                                category = 7;
                                break;
                            }
                        }
                        else if (strTitle == "ClassGuid")
                        {
                            int iId = 0;
                            foreach (String strClass in strClassName)
                            {
                                if (strClass.Equals(strValue, StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    category = iId;
                                    break;
                                }
                                iId++;
                            }

                            if (category != -1)
                                break;
                        }
                    }

                    switch (category)
                    {
                        case 0:
                            ComMisc.LogErrors("000000000000000000000");
                            deviceCom.Add(strName);
                            break;
                        case 1:
                            ComMisc.LogErrors("111111111111111111111");
                            deviceDisk.Add(strName);
                            break;
                        case 2:
                            deviceDisplay.Add(strName);
                            break;
                        case 3:
                            deviceIDE.Add(strName);
                            break;
                        case 4:
                            deviceKeyboard.Add(strName);
                            break;
                        case 5:
                            deviceMice.Add(strName);
                            break;
                        case 6:
                            deviceMonitor.Add(strName);
                            break;
                        case 7:
                            deviceNetwork.Add(strName);
                            break;
                        case 8:
                            deviceOther.Add(strName);
                            break;
                        case 9:
                            devicePorts.Add(strName);
                            break;
                        case 10:
                            deviceProcessor.Add(strName);
                            break;
                        case 11:
                            deviceSound.Add(strName);
                            break;
                        case 12:
                            deviceSystem.Add(strName);
                            break;
                        case 13:
                            deviceUniversal.Add(strName);
                            break;
                    }
                    //deviceList.Add(devCat);
                }
            }
            catch (Exception exp)
            {
                // Error occured
                ComMisc.LogErrors(exp.ToString());
            }
        }

        public void ChangeUsbState(bool bEnable)
        {
            try
            {
                if (bEnable)
                {
                    VolumeDeviceClass volumeDeviceClass = new VolumeDeviceClass();
                    List<Device> devs;
                    //if (devs == null)
                    devs = new List<Device>();
                    foreach (Volume device in volumeDeviceClass.Devices)
                    {
                        if (!device.IsUsb) continue;
                        if (device == null)
                            return;
                        devs.Add(device);
                    }

                    foreach (Device device in devs)
                    {
                        string s = device.Eject(true);
                        //if (s != null)
                        //{
                        //MessageBox.Show(this, s, "Eject", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //}
                    }
                }

                // Open Registry Key
                RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, true);

                int nEnable = 4;                                //Eject Device
                if (!bEnable) nEnable = 3;

                // Set Start Key Value
                key.SetValue("Start", nEnable);

                // Close Registry Key
                key.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }            
        }

        public void GetSystemInfo(M8ChangeHis changeHis)
        {
            try
            {
                int result = 0;
                String pDomain = null;

                strComputerName = System.Environment.MachineName;
                strDomainName = "";
                nJoinStatus = Win32.NetJoinStatus.NetSetupUnknownStatus;

                result = Win32.NetGetJoinInformation(null, out pDomain, out nJoinStatus);
                if (result != Win32.ErrorSuccess)
                {
                    if (pDomain != null)
                        Win32.NetApiBufferFree(pDomain);
                }

                if (pDomain != null)
                {
                    strDomainName = pDomain;
                    //Win32.NetApiBufferFree(pDomain);
                    pDomain = null;
                }

                changeHis.strComputerName = strComputerName;
                changeHis.strDomainName = strDomainName;

                if (nJoinStatus == Win32.NetJoinStatus.NetSetupUnknownStatus)
                    changeHis.strJoinStatus = "Unknown";

                if (nJoinStatus == Win32.NetJoinStatus.NetSetupUnjoined)
                    changeHis.strJoinStatus = "Unjoined";

                if (nJoinStatus == Win32.NetJoinStatus.NetSetupDomainName)
                    changeHis.strJoinStatus = "Domain";

                if (nJoinStatus == Win32.NetJoinStatus.NetSetupWorkgroupName)
                    changeHis.strJoinStatus = "Workgroup";
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }
    }
}
