using System;
using INMC.Communication.InmcServices.Client;
using INMC.Communication.Inmc.Communication.EndPoints;
using INMC.Communication.Inmc.Communication.EndPoints.Tcp;
using INMC.Communication.Inmc.Communication;
using System.Threading;
using System.Management;
using INMC.Communication.Inmc.Client;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using INMC.Communication.Inmc.Communication.Messages;
using INMC.INMMessage;
using System.Runtime.InteropServices;
using AgentEngine;
using System.Windows.Forms;

namespace INMAgent
{
    /// <summary>
    /// This class is a mediator with view and SCS system.
    /// </summary>
    internal class CommClient
    {
        #region Fields and Properties
        private static bool bMainConnected = false;
        private static bool bBackConnected = false;
        //private static bool bInitConnected = false;

        internal IInmcClient clientMain { get; private set; }
        internal IInmcClient clientBack { get; private set; }
        //internal IInmcClient clientInit { get; private set; }

        private INMSAgent _mainAgent = null;
        private M0AgentInfo _curAgentInfo;
        private Thread _threadFindServer;
        private bool _bFindStop { get; set; }
        #endregion

        #region Constructors
        internal CommClient()
        {
            _mainAgent = ComMisc.mainAgent;

            //_curAgentInfo = new M0AgentInfo();
            _curAgentInfo = GetCurrentAgentInfo();

            clientMain = InmcClientFactory.CreateClient(new InmcTcpEndPoint(ComMisc.strManAddress, ComMisc.SERVERMAIN_PORT));
            clientMain.Connected += new EventHandler(clientMain_Connected);
            clientMain.Disconnected += new EventHandler(clientMain_Disconnected);
            clientMain.MessageReceived += new EventHandler<INMC.Communication.Inmc.Communication.Messages.MessageEventArgs>(clientMain_MessageReceived);

            clientBack = InmcClientFactory.CreateClient(new InmcTcpEndPoint(ComMisc.strManAddress, ComMisc.SERVERBACK_PORT));
            clientBack.Connected += new EventHandler(clientBack_Connected);
            clientBack.Disconnected += new EventHandler(clientBack_Disconnected);
            clientBack.MessageReceived += new EventHandler<INMC.Communication.Inmc.Communication.Messages.MessageEventArgs>(clientBack_MessageReceived);

            //clientInit = InmcClientFactory.CreateClient(new InmcTcpEndPoint(ComMisc.strServerIP, ComMisc.SERVERINIT_PORT));
            //clientInit.Connected += new EventHandler(clientInit_Connected);
            //clientInit.Disconnected += new EventHandler(clientInit_Disconnected);
            //clientInit.MessageReceived += new EventHandler<INMC.Communication.Inmc.Communication.Messages.MessageEventArgs>(clientInit_MessageReceived);
        }
        #endregion

        #region Internal Methods
        internal void StartClient()
        {
            _bFindStop = false;

            //Connect to the server
            _threadFindServer = new Thread(new ThreadStart(ConnectToServerHandler));
            _threadFindServer.IsBackground = true;
            _threadFindServer.Priority = ThreadPriority.Lowest;
            _threadFindServer.Start();
        }

        internal void StopClient()
        {
            if (_threadFindServer.IsAlive)
            {
                _bFindStop = true;
            }
        }

        public void SendMessage(byte[] pMsg)
        {
            clientMain.SendMessage(new InmcRawDataMessage(pMsg));
        }

        #endregion

        #region Private Methods
        private M0AgentInfo GetCurrentAgentInfo()
        {
            try
            {
                M0AgentInfo agent = new M0AgentInfo();

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
                            agent.IpAddr = s[0];
                            agent.MacAddr = mo["MacAddress"].ToString();
                            break;
                        }
                    }
                    mo.Dispose();
                }

                agent.MachineName = System.Environment.MachineName;
                agent.LogonUser = System.Environment.UserName;

                agent.OSName = ComMisc.GetOSInfo();

                OperatingSystem os = Environment.OSVersion;
                Version vs = os.Version;
                if (vs.Major >= 6)
                    agent.vistaOS = true;

                string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                if (String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0)
                    agent.x64bitOS = false;
                else
                    agent.x64bitOS = true;

                return agent;
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }            
        }

        private void ConnectToServerHandler()
        {
            while (true)
            {
                if (_bFindStop == true)
                {
                    clientMain.Disconnect();
                    clientBack.Disconnect();
                    //clientInit.Disconnect();
                    return;
                }

                if (clientMain == null)
                    continue;

                if (clientBack == null)
                    continue;

                //if (clientInit == null)
                //    continue;

                if (bMainConnected == false)
                {
                    clientMain.Connect();
                }

                if (bBackConnected == false)
                {
                    clientBack.Connect();
                }

                //if (bInitConnected == false)
                //{
                //    clientInit.Connect();
                //}

                Thread.Sleep(6000);
            }
        }

        private void ReceivedTextMessage(string pMsg)
        {
            try
            {
                //string[] arrMsg = pMsg.Split('|');                

                //INMC.INMMessage.MessageType.KIND MsgKind;
                //Enum.TryParse<INMC.INMMessage.MessageType.KIND>(arrMsg[0], out MsgKind);

                //switch (MsgKind)
                //{
                //    case INMC.INMMessage.MessageType.KIND.MSG4_REALSCREENVIEW:
                //        ReceivedRealScreenView();
                //        break;
                //    default:
                //        break;
                //}
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedRawDataMessage(byte[] pMsg)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream mStream = new MemoryStream(pMsg);

                MessageBase MessageKind = (MessageBase)formatter.Deserialize(mStream);

                mStream.Position = 0;
                switch (MessageKind.MsgKind)
                {
                    case INMC.INMMessage.MessageType.KIND.MSG0_AGENTINFO:
                        MessageBox.Show("MSG0_AGENTINFO");
                        ReceivedAgentInfo(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG1_PROHIBITSET:
                        MessageBox.Show("MSG1_PROHIBITSET");
                        ReceivedProhibitSet(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG1_APPDISABLESET:
                        MessageBox.Show("MSG1_APPDISABLESET");
                        ReceivedAppdisableSet(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG1_BANDMON:
                        MessageBox.Show("MSG1_BANDMON");
                        ReceivedBandMon(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG2_NETALLOWSET:
                        MessageBox.Show("MSG2_NETALLOWSET");
                        ReceivedNetAllowSet(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG2_NETAPPMON:
                        MessageBox.Show("MSG2_NETAPPMON");
                        ReceivedNetAppMon(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG3_FIREWALLSET:
                        MessageBox.Show("MSG3_FIREWALLSET");
                        ReceivedFirewallSet(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG3_PORTSET:
                        MessageBox.Show("MSG3_PORTSET");
                        ReceivedPortSet(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG3_PORTMON:
                        MessageBox.Show("MSG3_PORTMON");
                        ReceivedPortMon(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG4_REALAPPMON:
                        MessageBox.Show("MSG4_REALAPPMON");
                        ReceivedRealAppMon(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG4_REALSCREENMON:
                        MessageBox.Show("MSG4_REALSCREENMON");
                        ReceivedRealScreenMon(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG5_FILEHIS:
                        MessageBox.Show("MSG5_FILEHIS");
                        ReceivedFileHis(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG5_PRINTHIS:
                        MessageBox.Show("MSG5_PRINTHIS");
                        ReceivedPrintHis(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG5_MAILHIS:
                        MessageBox.Show("MSG5_MAILHIS");
                        ReceivedMailHis(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG6_MSGSEND:
                        MessageBox.Show("MSG6_MSGSEND");
                        ReceivedMsgSend(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG6_COMLOCK:
                        MessageBox.Show("MSG6_COMLOCK");
                        ReceivedComLock(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG6_COMSHUT:
                        MessageBox.Show("MSG6_COMSHUT");
                        ReceivedComShut(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG6_RDPMON:
                        MessageBox.Show("MSG6_RDPMON");
                        ReceivedRdpMon(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG6_USERSET:
                        MessageBox.Show("MSG6_USERSET");
                        ReceivedUserSet(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG6_PROCMON:
                        MessageBox.Show("MSG6_PROCMON");
                        ReceivedCurProcMon(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG6_PROCACT:
                        MessageBox.Show("MSG6_PROCACT");
                        ReceivedProcAction(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG7_WEBHIS:
                        MessageBox.Show("MSG7_WEBHIS");
                        ReceivedWebHis(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG7_NETAPPHIS:
                        MessageBox.Show("MSG7_NETAPPHIS");
                        ReceivedNetAppHis(MessageKind);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG8_DEVICEMON:
                        MessageBox.Show("MSG8_DEVICEMON");
                        ReceivedHardMon(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG8_INSTAPPHIS:
                        MessageBox.Show("MSG8_INSTAPPHIS");
                        ReceivedInstsAppHis(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG8_PROCMON:
                        MessageBox.Show("MSG8_PROCMON");
                        ReceivedProcMon(MessageKind.MsgType);
                        break;
                    case INMC.INMMessage.MessageType.KIND.MSG8_CHANGEHIS:
                        MessageBox.Show("MSG8_CHANGEHIS");
                        ReceivedChangeHis(MessageKind.MsgType);
                        break;
                    default:
                        break;
                }
                mStream.Close();

            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedAgentInfo
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedAgentInfo(MessageType.TYPE pType)
        {
            try
            {
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedProhibitSet
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedProhibitSet(MessageBase pMsg)
        {
            try
            {
                M1ProhibitSet msgContent = (M1ProhibitSet)pMsg;
                if (msgContent.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.MainEngine.devManage.ChangeUsbState(msgContent.bEnable);
                    _mainAgent.MainEngine.netManage.SetBandWidth(msgContent.upSpeed, msgContent.downSpeed);

                    // Send Reply
                    String strResultMsg = GetReturnMessage(MessageType.KIND.MSG1_PROHIBITSET, true);
                    clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedAppdisableSet
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedAppdisableSet(MessageBase pMsg)
        {
            try
            {
                M1AppdisableSet msgContent = (M1AppdisableSet)pMsg;
                if (pMsg.MsgType == MessageType.TYPE.INITSET)
                {
                    if (msgContent.appItems != null)
                    {
                        _mainAgent.MainEngine.processTracker.InitList(pMsg.msgTime, msgContent.appItems);
                    }
                }
                else if (msgContent.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    if (msgContent.appItems != null)
                    {
                        _mainAgent.MainEngine.processTracker.StopApps(pMsg.msgTime, msgContent.appItems);
                        // Send Reply
                        String strResultMsg = GetReturnMessage(MessageType.KIND.MSG1_APPDISABLESET, true);
                        clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                    }
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedBandWidth
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedBandMon(MessageType.TYPE pType)
        {
            try
            {
                if (pType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.MainEngine.netManage.WatchNetwork(true);
                }
                else
                {
                    _mainAgent.MainEngine.netManage.WatchNetwork(false);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedNetAllowSet
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedNetAllowSet(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType == MessageType.TYPE.INITSET)
                {
                    M2NetAllowSet allowset = (M2NetAllowSet)pMsg;
                    _mainAgent.MainEngine.netManage.InitInternetInfo(pMsg.msgTime, allowset);
                }
                else if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M2NetAllowSet allowset = (M2NetAllowSet)pMsg;
                    _mainAgent.MainEngine.netManage.SetInternetInfo(pMsg.msgTime, allowset);

                    // Send Reply
                    String strResultMsg = GetReturnMessage(MessageType.KIND.MSG2_NETALLOWSET, true);
                    clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedNetAppView
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedNetAppMon(MessageType.TYPE pType)
        {
            try
            {
                if (pType != MessageType.TYPE.REQUESTSTOP)
                {
                    M2NetAppMon netMon = new M2NetAppMon();
                    _mainAgent.MainEngine.netManage.GetNetMonList(netMon.netAppList);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, netMon);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedFirewallSet
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedFirewallSet(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M3FirewallSet pFirewallSet = (M3FirewallSet)pMsg;
                    _mainAgent.MainEngine.netManage.ChangeFirewallSettings(pFirewallSet.bDomain, pFirewallSet.bPrivate, pFirewallSet.bPublic);
                    
                    // Send Reply
                    String strResultMsg = GetReturnMessage(MessageType.KIND.MSG3_FIREWALLSET, true);
                    clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedPortSet
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedPortSet(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M3PortSet pInPortSet = (M3PortSet)pMsg;
                    int nCount = pInPortSet.portRule.Count;

                    for (int i = 0; i < nCount; i++)
                    {
                        M3PortRule rule = (M3PortRule)pInPortSet.portRule[i];
                        if (rule.nType == 0)
                        {
                            _mainAgent.MainEngine.netManage.AddRuleToFirewall(rule);
                        }
                        else if (rule.nType == 1)
                        {
                            if (i != nCount - 1)
                            {
                                M3PortRule ruleNew = (M3PortRule)pInPortSet.portRule[i + 1];
                                _mainAgent.MainEngine.netManage.SelRuleToFireWall(rule, ruleNew);
                                i++;
                            }
                        }
                        else if (rule.nType == 2)
                        {
                            _mainAgent.MainEngine.netManage.DelRuleToFireWall(rule);
                        }
                    }
                    
                    // Send Reply
                    String strResultMsg = GetReturnMessage(MessageType.KIND.MSG3_PORTSET, true);
                    clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedPortView
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedPortMon(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M3PortMon pRuleSet = new M3PortMon();

                    if (AgentEngine.ComMisc.vistaOverOS == true)
                        _mainAgent.MainEngine.netManage.GetFirewallRules2(pRuleSet.portRule);
                    else
                        _mainAgent.MainEngine.netManage.GetFirewallRules(pRuleSet.portRule);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, pRuleSet);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedRealAppView
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedRealAppMon(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.MainEngine.processTracker.bCheckRunningApps = true;

                    M4RealAppMon realappMon = new M4RealAppMon();
                    _mainAgent.MainEngine.processTracker.GetRunningApps(realappMon.appList);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, realappMon);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
                else
                {
                    _mainAgent.MainEngine.processTracker.bCheckRunningApps = false;
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedRealScreenView
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedRealScreenMon(MessageType.TYPE pType)
        {
            try
            {
                if (pType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.MainEngine.screenCapture.CaptureScreen(true);
                }
                else
                {
                    _mainAgent.MainEngine.screenCapture.CaptureScreen(false);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedFileVew
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedFileHis(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M5FileHis fileHis = new M5FileHis();
                    _mainAgent.MainEngine.cFileOp.GetFileHistory(fileHis.fileList);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, fileHis);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedPrintView
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedPrintHis(MessageType.TYPE pType)
        {
            try
            {
                if (pType != MessageType.TYPE.REQUESTSTOP)
                {
                    M5PrintHis printhis = new M5PrintHis();
                    _mainAgent.MainEngine.printTracker.GetPrinterHistory(printhis.printList);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, printhis);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedMailView
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedMailHis(MessageType.TYPE pType)
        {
            try
            {
                M5MailHis mailHis = new M5MailHis();
                //_mainAgent.MainEngine.mailTracker.GetMailerHistory(mailHis.MailList);

                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, mailHis);

                clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedMsgSend
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedMsgSend(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M6MsgSend msgSend = (M6MsgSend)pMsg;
                    _mainAgent.ShowMessage(msgSend.strMessage);

                    String strResultMsg = GetReturnMessage(MessageType.KIND.MSG6_MSGSEND, true);
                    clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedCurProcMon
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedCurProcMon(MessageBase pMsg)
        {
            try
            {
                M6ProcMon procMon = (M6ProcMon)pMsg;
                if (procMon.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M6ProcMon procHisMon = new M6ProcMon();
                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                    _mainAgent.MainEngine.processTracker.GetCurProcList(procHisMon.procMonList, false);

                    stream.Position = 0;
                    formatter.Serialize(stream, procHisMon);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedProcAction
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedProcAction(MessageBase pMsg)
        {
            try
            {
                M6ProcAction procAction = (M6ProcAction)pMsg;

                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.MainEngine.processTracker.ControlProcesses(procAction);
                    String strResultMsg = GetReturnMessage(MessageType.KIND.MSG6_PROCACT, true);
                    clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedMachineLock
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedComLock(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M6ComLock lockSet = (M6ComLock)pMsg;
                    _mainAgent.LockComputer(lockSet.bLockMouse, lockSet.bLockKeybd);
                    _mainAgent.ResetComputer(lockSet.bComShutdown, lockSet.bComRestart);
                    // Send Reply
                    String strResultMsg = GetReturnMessage(MessageType.KIND.MSG6_COMLOCK, true);
                    clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedMachineShutdown
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedComShut(MessageType.TYPE pType)
        {
            try
            {
                // Send Reply
                String strResultMsg = GetReturnMessage(MessageType.KIND.MSG6_COMSHUT, true);
                clientMain.SendMessage(new InmcTextMessage(strResultMsg));
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedRdpPView
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedRdpMon(MessageType.TYPE pType)
        {
            try
            {
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedUserSet
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedUserSet(MessageBase pMsg)
        {
            try
            {
                if (pMsg.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    M6UserSet userSet = (M6UserSet)pMsg;

                    // Change User Password;
                    foreach (M6UserItem userItem in userSet.userList)
                    {
                        if (userItem.nType == 0)
                            _mainAgent.MainEngine.userManage.AddUser(userItem);
                        else if (userItem.nType == 1)
                            _mainAgent.MainEngine.userManage.SetUserPass(userItem);
                        else
                            _mainAgent.MainEngine.userManage.DeleteUser(userItem);
                    }

                    _mainAgent.MainEngine.userManage.GetUserList(userSet);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, userSet);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();

                    // Send Reply
                    String strResultMsg = GetReturnMessage(MessageType.KIND.MSG6_USERSET, true);
                    clientMain.SendMessage(new InmcTextMessage(strResultMsg));
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedWebHis
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedWebHis(MessageBase pMsg)
        {
            try
            {
                M7WebHis webhis = (M7WebHis)pMsg;
                if (webhis.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.WEBURL_HIS_INTERVAL = webhis.Interval;

                    M7WebHis webHis = new M7WebHis();
                    _mainAgent.MainEngine.netManage.GetLatestHistory(webHis.webHisList);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, webHis);

                    clientBack.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedNetAppHis
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedNetAppHis(MessageBase pMsg)
        {
            try
            {
                M7NetAppHis netHis = (M7NetAppHis)pMsg;

                if (netHis.MsgType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.NETAPP_HIS_INTERVAL = netHis.Interval;

                    M7NetAppHis netAppHis = new M7NetAppHis();
                    _mainAgent.MainEngine.netManage.GetNetWorkConList(netAppHis.netAppList);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, netAppHis);

                    clientBack.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedChangeHis
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedChangeHis(MessageType.TYPE pType)
        {
            try
            {
                if (pType != MessageType.TYPE.REQUESTSTOP)
                {
                    M8ChangeHis changeMon = new M8ChangeHis();
                    _mainAgent.MainEngine.devManage.GetSystemInfo(changeMon);

                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                    stream.Position = 0;
                    formatter.Serialize(stream, changeMon);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedHardMon
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedHardMon(MessageType.TYPE pType)
        {
            try
            {
                if (pType != MessageType.TYPE.REQUESTSTOP)
                {
                    M8DeviceMon devMon = new M8DeviceMon();
                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                    _mainAgent.MainEngine.devManage.GetDeviceList(devMon, false);

                    stream.Position = 0;
                    formatter.Serialize(stream, devMon);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedInstsAppHis
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedInstsAppHis(MessageType.TYPE pType)
        {
            try
            {
                if (pType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.MainEngine.installTracker.SetEnableCheck(true);

                    M8InstAppHis instAppHis = new M8InstAppHis();
                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(ComMisc.STREAMSIZE_MAX);

                    _mainAgent.MainEngine.installTracker.getInstallList(instAppHis.installList);

                    stream.Position = 0;
                    formatter.Serialize(stream, instAppHis);

                    clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                    stream.Close();
                }
                else
                {
                    _mainAgent.MainEngine.installTracker.SetEnableCheck(false);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// ReceivedProcMon
        /// </summary>
        /// <param name="pType"></param>
        private void ReceivedProcMon(MessageType.TYPE pType)
        {
            try
            {
                if (pType != MessageType.TYPE.REQUESTSTOP)
                {
                    _mainAgent.MainEngine.processTracker.SetCheckProcess(true);
                }
                else
                {
                    _mainAgent.MainEngine.processTracker.SetCheckProcess(false);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        public bool SendWebUrlHistory()
        {
            if (bBackConnected == false)
                return false;

            M7WebHis webHis = new M7WebHis();
            _mainAgent.MainEngine.netManage.GetLatestHistory(webHis.webHisList);

            if (webHis.webHisList.Count > 0)
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, webHis);

                clientBack.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }

            return true;
        }

        public bool SendNetAppHistory()
        {
            if (bBackConnected == false)
                return false;

            M7NetAppHis netHis = new M7NetAppHis();
            _mainAgent.MainEngine.netManage.GetNetWorkConList(netHis.netAppList);

            if (netHis.netAppList.Count > 0)
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, netHis);

                clientBack.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }

            return true;
        }

        #endregion

        #region Event Methods
        void clientMain_Connected(object sender, EventArgs e)
        {
            try
            {
                bMainConnected = true;

                _mainAgent.MainEngine.InitState();

                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, _curAgentInfo);

                clientMain.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }
            catch (System.Exception ex)
            {
                bMainConnected = false;
                _mainAgent.MainEngine.InitState();
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        void clientMain_Disconnected(object sender, EventArgs e)
        {
            try
            {
                bMainConnected = false;
                _mainAgent.MainEngine.InitState();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        void clientMain_MessageReceived(object sender, INMC.Communication.Inmc.Communication.Messages.MessageEventArgs e)
        {
            try
            {
                if (e.Message is InmcTextMessage)
                {
                    var message = e.Message as InmcTextMessage;

                    ReceivedTextMessage(message.Text);

                }
                else if (e.Message is InmcRawDataMessage)
                {
                    var message = e.Message as InmcRawDataMessage;

                    ReceivedRawDataMessage(message.MessageData);
                }
                else
                {
                    return;
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// clientBack_Connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientBack_Connected(object sender, EventArgs e)
        {
            try
            {
                bBackConnected = true;

                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, _curAgentInfo);

                clientBack.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                //stream.Close();

                M0TimeStamp timeStamp = new M0TimeStamp();
                
                M0TimeItem netAppTime = new M0TimeItem();
                netAppTime.settingMessage = MessageType.KIND.MSG2_NETALLOWSET;
                netAppTime.settingTime = _mainAgent.MainEngine.netManage.GetNetAppSetTime();

                M0TimeItem appConTime = new M0TimeItem();
                appConTime.settingMessage = MessageType.KIND.MSG1_APPDISABLESET;
                appConTime.settingTime = _mainAgent.MainEngine.processTracker.GetAppConSetTime();

                timeStamp.timeItems.Add(netAppTime);
                timeStamp.timeItems.Add(appConTime);

                stream.Position = 0;
                formatter.Serialize(stream, timeStamp);

                clientBack.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                stream.Close();
            }
            catch (System.Exception ex)
            {
                bBackConnected = false;
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        void clientBack_Disconnected(object sender, EventArgs e)
        {
            try
            {
                bBackConnected = false;
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        void clientBack_MessageReceived(object sender, INMC.Communication.Inmc.Communication.Messages.MessageEventArgs e)
        {
            try
            {
                if (e.Message is InmcTextMessage)
                {
                    var message = e.Message as InmcTextMessage;

                    ReceivedTextMessage(message.Text);

                }
                else if (e.Message is InmcRawDataMessage)
                {
                    var message = e.Message as InmcRawDataMessage;

                    ReceivedRawDataMessage(message.MessageData);
                }
                else
                {
                    return;
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        String GetReturnMessage(MessageType.KIND msgkind, bool bSuccess)
        {
            String strRet = "";
            strRet = msgkind.ToString();
            strRet += "|";

            strRet += ((bSuccess) ? ComMisc.strSuccessMsg : ComMisc.strFailMsg);

            return strRet;
        }

        //////////////////////////////////////////////////////////////////////////
        //void clientInit_Connected(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        bInitConnected = true;

        //        IFormatter formatter = new BinaryFormatter();
        //        MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

        //        stream.Position = 0;
        //        formatter.Serialize(stream, _curAgentInfo);

        //        //clientInit.SendMessage(new InmcRawDataMessage(stream.ToArray()));

        //        stream.Close();
        //    }
        //    catch (System.Exception ex)
        //    {
        //        bInitConnected = false;
        //        ComMisc.LogErrors(ex.ToString());
        //        throw;
        //    }
        //}

        //void clientInit_Disconnected(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        bInitConnected = false;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        ComMisc.LogErrors(ex.ToString());
        //        throw;
        //    }
        //}

        //void clientInit_MessageReceived(object sender, INMC.Communication.Inmc.Communication.Messages.MessageEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Message is InmcTextMessage)
        //        {
        //            var message = e.Message as InmcTextMessage;

        //            ReceivedTextMessage(message.Text);

        //        }
        //        else if (e.Message is InmcRawDataMessage)
        //        {
        //            var message = e.Message as InmcRawDataMessage;

        //            ReceivedRawDataMessage(message.MessageData);
        //        }
        //        else
        //        {
        //            return;
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        ComMisc.LogErrors(ex.ToString());
        //        throw;
        //    }
        //}
        
        #endregion
    }

}
