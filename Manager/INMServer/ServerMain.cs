using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INMC.Communication.Inmc.Server;
using INMC.Communication.Inmc.Communication.EndPoints.Tcp;
using INMC.Communication.Inmc.Communication.Messages;
using INMC.Communication.InmcServices.Service;
using INMC.Communication.Inmc.Client;
using INMC.Collections;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using INMC.INMMessage;

namespace INMServer
{
    /// <summary>
    /// ServerMain class
    /// </summary>
    internal class ServerMain
    {
        #region Fields and Properties
        internal IInmcServer commServer {get; set; }
        //internal CommService commService {get; set; }
        internal int iPort { get; set; }

        
        #endregion

        #region Constructors
        /// <summary>
        /// ServerMain
        /// </summary>
        internal ServerMain()
        {
            iPort = ComMisc.SERVERMAIN_PORT;            
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// StartServer
        /// </summary>
        internal void StartServer()
        {
            try
            {
                commServer = InmcServerFactory.CreateServer(new InmcTcpEndPoint(iPort));
                //commService = new CommService();
                //commServer.AddService<INMService, CommService>(commService);
                commServer.ClientConnected += new EventHandler<ServerClientEventArgs>(commServer_ClientConnected);
                commServer.ClientDisconnected += new EventHandler<ServerClientEventArgs>(commServer_ClientDisconnected);
                //commService.AgentListChanged += new EventHandler(commService_AgentListChanged);
                commServer.Start();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }            
        }

        /// <summary>
        /// StopServer
        /// </summary>
        internal void StopServer()
        {
            try
            {
                if (commServer == null)
                {
                    return;
                }

                commServer.Stop();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// SendTextMessageToAgent
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pKind"></param>
        /// <param name="pType"></param>
        /// <param name="pMsg"></param>
        internal void SendTextMessageToAgent(string pAid, MessageType.KIND pKind, MessageType.TYPE pType, string pMsg)
        {
            try
            {
                long id = long.Parse(pAid);
                IInmcServerClient client = commServer.Clients[id];

                string Msg = pKind + "|" + pType + "|" + pMsg;
                client.SendMessage(new InmcTextMessage(Msg));
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// SendRawDataMessageToAgent
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pMsg"></param>
        internal void SendRawDataMessageToAgent(string pAid, byte[] pMsg)
        {
            try
            {
                long id = long.Parse(pAid);
                IInmcServerClient client = commServer.Clients[id];

                if (client != null)
                    client.SendMessage(new InmcRawDataMessage(pMsg));
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// ReceivedTextMessage
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pMsg"></param>
        private void ReceivedTextMessage(long pAid, string pMsg)
        {
            try
            {
                string[] arrMsg = pMsg.Split('|');

                MessageType.KIND MsgKind;
                Enum.TryParse<MessageType.KIND>(arrMsg[0], out MsgKind);
                bool retValue = arrMsg[1] == "success" ? true : false;

                switch (MsgKind)
                {
                    case MessageType.KIND.MSG1_PROHIBITSET:
                        if (ComMisc.frmMain.ctrl1ProhibitSet == null)
                            break;
                        if (ComMisc.frmMain.ctrl1ProhibitSet.Visible == false)
                            break;
                        ComMisc.frmMain.ReturnViewPage(MsgKind, retValue);
                        break;     
               
                    case MessageType.KIND.MSG1_APPDISABLESET:
                        if (ComMisc.frmMain.ctrl1ProhibitSet == null)
                            break;
                        if (ComMisc.frmMain.ctrl1ProhibitSet.Visible == false)
                            break;
                        ComMisc.frmMain.ReturnViewPage(MsgKind, retValue);
                        break;

                    case MessageType.KIND.MSG2_NETALLOWSET:
                        if (ComMisc.frmMain.ctrl2NetSet == null)
                            break;
                        if (ComMisc.frmMain.ctrl2NetSet.Visible == false)
                            break;
                        ComMisc.frmMain.ReturnViewPage(MsgKind, retValue);
                        break;       
             
                    case MessageType.KIND.MSG3_PORTSET:
                        if (ComMisc.frmMain.ctrl3PortSet == null)
                            break;
                        if (ComMisc.frmMain.ctrl3PortSet.Visible == false)
                            break;
                        ComMisc.frmMain.ReturnViewPage(MsgKind, retValue);
                        break;      
              
                    case MessageType.KIND.MSG6_MSGSEND:
                        if (ComMisc.frmMain.ctrl6MsgSend == null)
                            break;
                        if (ComMisc.frmMain.ctrl6MsgSend.Visible == false)
                            break;
                        ComMisc.frmMain.ReturnViewPage(MsgKind, retValue);
                        break;

                    case MessageType.KIND.MSG6_COMLOCK:
                        if (ComMisc.frmMain.ctrl6ComLock == null)
                            break;
                        if (ComMisc.frmMain.ctrl6ComLock.Visible == false)
                            break;
                        ComMisc.frmMain.ReturnViewPage(MsgKind, retValue);
                        break;      

                    case MessageType.KIND.MSG6_USERSET:
                        if (ComMisc.frmMain.ctrl6UserSet == null)
                            break;
                        if (ComMisc.frmMain.ctrl6UserSet.Visible == false)
                            break;
                        ComMisc.frmMain.ReturnViewPage(MsgKind, retValue);
                        break; 

                    case MessageType.KIND.MSG6_PROCACT:
                        if (ComMisc.frmMain.ctrl6ProcMon == null)
                            break;
                        if (ComMisc.frmMain.ctrl6ProcMon.Visible == false)
                            break;
                        ComMisc.frmMain.ReturnViewPage(MsgKind, retValue);
                        break;

                    case MessageType.KIND.MSG8_CHANGEHIS:
                        break;

                    default:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }            
        }

        /// <summary>
        /// ReceivedRawDataMessage
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pMsg"></param>
        private void ReceivedRawDataMessage(long pAid, byte[] pMsg)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                MemoryStream mStream = new MemoryStream(pMsg);

                MessageBase MessageKind = (MessageBase)formatter.Deserialize(mStream);

                mStream.Position = 0;
                switch (MessageKind.MsgKind)
                {
                    case MessageType.KIND.MSG0_AGENTINFO:
                        ReceivedAgentInfo(pAid, (M0AgentInfo)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG1_PROHIBITSET:
                        break;

                    case MessageType.KIND.MSG1_BANDMON:
                        ReceivedBandMon((M1BandMon)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG2_NETALLOWSET:
                        break;

                    case MessageType.KIND.MSG2_NETAPPMON:
                        ReceivedNetIpMon((M2NetAppMon)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG3_PORTMON:
                        ReceivedPortMon((M3PortMon)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG3_PORTSET:
                        ReceivedPortSet((M3PortSet)formatter.Deserialize(mStream));
                        break;  
                  
                    case MessageType.KIND.MSG4_REALAPPMON:
                        ReceivedRealAppMon((M4RealAppMon)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG4_REALSCREENMON:
                        ReceivedRealScreenMon((M4RealScreenMon)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG5_FILEHIS:
                        ReceivedFileHis((M5FileHis)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG5_PRINTHIS:
                        ReceivedPrintHis((M5PrintHis)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG5_MAILHIS:
                        break;

                    case MessageType.KIND.MSG6_MSGSEND:
                        break;

                    case MessageType.KIND.MSG6_COMLOCK:
                        break;

                    case MessageType.KIND.MSG6_RDPMON:
                        break;

                    case MessageType.KIND.MSG6_USERSET:
                        ReceivedUserSet((M6UserSet)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG6_PROCMON:
                        ReceivedProcMon((M6ProcMon)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG7_WEBHIS:
                        ReceivedWebHis((M7WebHis)formatter.Deserialize(mStream));                        
                        break;

                    case MessageType.KIND.MSG7_NETAPPHIS:
                        ReceivedNetAppHis((M7NetAppHis)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG8_INSTAPPHIS:
                        ReceivedInstAppHis((M8InstAppHis)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG8_PROCMON:
                        ReceivedRealProcMon((M8ProcMon)formatter.Deserialize(mStream));
                        break;

                    case MessageType.KIND.MSG8_CHANGEHIS:
                        break;

                    case MessageType.KIND.MSG8_DEVICEMON:
                        ReceivedDeviceMon((M8DeviceMon)formatter.Deserialize(mStream));
                        break;

                    default:
                        break;
                }
                mStream.Close();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedAgentInfo
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pAgent"></param>
        private void ReceivedAgentInfo(long pAid, M0AgentInfo pAgent)
        {
            try
            {
                if (pAgent != null) //Add
                {
                    ComMisc.frmMain.SetUserIdToDB(pAgent);

                    ComMisc.frmMain.ReviewNodeItem(pAid, pAgent, true, true);
                }
                else // Remove
                {
                    ComMisc.frmMain.ReviewNodeItem(pAid, pAgent, false, true);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedBandMon
        /// </summary>
        /// <param name="pBandMon"></param>
        private void ReceivedBandMon(M1BandMon pBandMon)
        {
            try
            {
                if (ComMisc.frmMain.ctrl1NicMon == null)
                    return;

                ComMisc.frmMain.ctrl1NicMon.bandMon = pBandMon;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedNetIpMon
        /// </summary>
        /// <param name="pNetMon"></param>
        private void ReceivedNetIpMon(M2NetAppMon pNetMon)
        {
            try
            {
                if (ComMisc.frmMain.ctrl2NetAppMon == null)
                    return;

                ComMisc.frmMain.ctrl2NetAppMon.netMon = pNetMon;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedPortSet
        /// </summary>
        /// <param name="pPortInfo"></param>
        private void ReceivedPortSet(M3PortSet pPortInfo)
        {
            try
            {
                if (ComMisc.frmMain.ctrl3PortSet == null)
                    return;

                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedPortMon
        /// </summary>
        /// <param name="pPortInfo"></param>
        private void ReceivedPortMon(M3PortMon pPortInfo)
        {
            try
            {
                if (ComMisc.frmMain.ctrl3PortMon == null)
                    return;

                ComMisc.frmMain.ctrl3PortMon.portMon = pPortInfo;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedRealScreenMon
        /// </summary>
        /// <param name="pImg"></param>
        private void ReceivedRealScreenMon(M4RealScreenMon pImg)
        {
            try
            {
                if (ComMisc.frmMain.ctrl4RealScreenMon == null)
                    return;

                ComMisc.frmMain.ctrl4RealScreenMon.scrImage = pImg.scrImg;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedFileHis
        /// </summary>
        /// <param name="pFileHis"></param>
        private void ReceivedFileHis(M5FileHis pFileHis)
        {
            try
            {
                if (ComMisc.frmMain.ctrl5FileHis == null)
                    return;

                ComMisc.frmMain.ctrl5FileHis.fileHis = pFileHis;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedPrintHis
        /// </summary>
        /// <param name="pPrintHis"></param>
        private void ReceivedPrintHis(M5PrintHis pPrintHis)
        {
            try
            {
                if (ComMisc.frmMain.ctrl5PrintHis == null)
                    return;

                ComMisc.frmMain.ctrl5PrintHis.printHis = pPrintHis;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedRealAppMon
        /// </summary>
        /// <param name="realAppMon"></param>
        private void ReceivedRealAppMon(M4RealAppMon realAppMon)
        {
            try
            {
                if (ComMisc.frmMain.ctrl4RealAppMon == null)
                    return;

                ComMisc.frmMain.ctrl4RealAppMon.realAppMon = realAppMon;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedDeviceMon
        /// </summary>
        /// <param name="pDeviceMon"></param>
        private void ReceivedDeviceMon(M8DeviceMon pDeviceMon)
        {
            try
            {
                if (ComMisc.frmMain.ctrl8HardMon == null)
                    return;

                ComMisc.frmMain.ctrl8HardMon.deviceMon = pDeviceMon;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedRealProcMon
        /// </summary>
        /// <param name="pProcMon"></param>
        private void ReceivedRealProcMon(M8ProcMon pProcMon)
        {
            try
            {
                if (ComMisc.frmMain.ctrl8ProcMon == null)
                    return;

                ComMisc.frmMain.ctrl8ProcMon.procMon = pProcMon;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedInstAppHis
        /// </summary>
        /// <param name="pInstAppHis"></param>
        private void ReceivedInstAppHis(M8InstAppHis pInstAppHis)
        {
            try
            {
                if (ComMisc.frmMain.ctrl8InstAppHis == null)
                    return;

                ComMisc.frmMain.ctrl8InstAppHis.instAppHis = pInstAppHis;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedProcMon
        /// </summary>
        /// <param name="pProcMon"></param>
        private void ReceivedProcMon(M6ProcMon pProcMon)
        {
            try
            {
                if (ComMisc.frmMain.ctrl6ProcMon == null)
                    return;

                ComMisc.frmMain.ctrl6ProcMon.procMon = pProcMon;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedNetAppHis
        /// </summary>
        /// <param name="pAppHis"></param>
        private void ReceivedNetAppHis(M7NetAppHis pAppHis)
        {
            try
            {
                if (ComMisc.frmMain.ctrl7NetAppHis == null)
                    return;

                ComMisc.frmMain.ctrl7NetAppHis.netAppHis = pAppHis;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedWebHis
        /// </summary>
        /// <param name="pWebHis"></param>
        private void ReceivedWebHis(M7WebHis pWebHis)
        {
            try
            {
                if (ComMisc.frmMain.ctrl7WebHis == null)
                    return;

                ComMisc.frmMain.ctrl7WebHis.webHis = pWebHis;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedUserSet
        /// </summary>
        /// <param name="pUserSet"></param>
        private void ReceivedUserSet(M6UserSet pUserSet)
        {
            try
            {
                if (ComMisc.frmMain.ctrl6UserSet == null)
                    return;

                ComMisc.frmMain.ctrl6UserSet.userSet = pUserSet;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }
        #endregion

        #region Event Methods

        /// <summary>
        /// commServer_ClientConnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void commServer_ClientConnected(object sender, ServerClientEventArgs e)
        {
            try
            {
                e.Client.MessageReceived += new EventHandler<MessageEventArgs>(Client_MessageReceived);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// commServer_ClientDisconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void commServer_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            try
            {
                var client = (IInmcServerClient)e.Client;
                ReceivedAgentInfo(client.ClientId, null);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// Client_MessageReceived
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var client = (IInmcServerClient)sender; 

                if (e.Message is InmcTextMessage)
                {
                    var message = e.Message as InmcTextMessage;

                    ReceivedTextMessage(client.ClientId, message.Text);
                }
                else if (e.Message is InmcRawDataMessage)
                {
                    var message = e.Message as InmcRawDataMessage;

                    ReceivedRawDataMessage(client.ClientId, message.MessageData);
                }
                else
                {
                    return;
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }
        #endregion
    }
}
