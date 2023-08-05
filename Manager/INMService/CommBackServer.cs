using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INMC.Communication.Inmc.Server;
using INMC.Communication.Inmc.Communication.EndPoints.Tcp;
using INMC.INMMessage;
using INMC.Communication.Inmc.Communication.Messages;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace INMServer
{
    internal class CommBackServer
    {
        #region Fields and Properties
        internal IInmcServer commBackServer { get; set; }
        //internal CommService commService {get; set; }
        internal int iPort { get; set; }


        #endregion

        #region Constructors
        internal CommBackServer()
        {
            iPort = ComMisc.SERVERINIT_PORT;
        }
        #endregion

        #region Internal Methods
        internal void StartServer()
        {
            try
            {
                commBackServer = InmcServerFactory.CreateServer(new InmcTcpEndPoint(iPort));
                //commService = new CommService();
                //commServer.AddService<INMService, CommService>(commService);
                commBackServer.ClientConnected += new EventHandler<ServerClientEventArgs>(commBackServer_ClientConnected);
                commBackServer.ClientDisconnected += new EventHandler<ServerClientEventArgs>(commBackServer_ClientDisconnected);
                //commService.AgentListChanged += new EventHandler(commService_AgentListChanged);
                commBackServer.Start();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        internal void StopServer()
        {
            try
            {
                if (commBackServer == null)
                {
                    return;
                }

                commBackServer.Stop();
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        internal void SendTextMessageToAgent(string pAid, MessageType.KIND pKind, MessageType.TYPE pType, string pMsg)
        {
            try
            {
                long id = long.Parse(pAid);
                IInmcServerClient client = commBackServer.Clients[id];

                string Msg = pKind + "|" + pType + "|" + pMsg;
                client.SendMessage(new InmcTextMessage(Msg));
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        internal void SendRawDataMessageToAgent(string pAid, byte[] pMsg)
        {
            try
            {
                long id = long.Parse(pAid);
                IInmcServerClient client = commBackServer.Clients[id];

                if (client != null)
                    client.SendMessage(new InmcRawDataMessage(pMsg));
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        #endregion

        #region Private Methods
        private void ReceivedTextMessage(long pAid, string pMsg)
        {
            try
            {
                //string[] arrMsg = pMsg.Split('|');

                //MessageType.KIND MsgKind;
                //Enum.TryParse<MessageType.KIND>(arrMsg[0], out MsgKind);

                //switch (MsgKind)
                //{
                //    case MessageType.KIND.MSG0_AGENTINFO:
                //        ReceivedAgentInfo(pAid, arrMsg);
                //        break;
                //    case MessageType.KIND.MSG4_REALSCREENVIEW:
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
                        break;
                    case MessageType.KIND.MSG5_PRINTHIS:
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
                throw;
            }

        }

        private void ReceivedAgentInfo(long pAid, M0AgentInfo pAgent)
        {
            try
            {
                if (pAgent != null) //Add
                {
                    //ComMisc.frmMain.SetUserIdToDB(pAgent);

                    //ComMisc.frmMain.ReviewNodeItem(pAid, pAgent, true);
                }
                else // Remove
                {
                    //ComMisc.frmMain.ReviewNodeItem(pAid, pAgent, false);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedBandMon(M1BandMon pBandMon)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl1ProhibitSet == null)
                //    return;

                //ComMisc.frmMain.ctrl1ProhibitSet.bandMon = pBandMon;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedNetIpMon(M2NetAppMon pNetMon)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl2NetAppMon == null)
                //    return;

                //ComMisc.frmMain.ctrl2NetAppMon.netMon = pNetMon;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedPortSet(M3PortSet pPortInfo)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl3PortSet == null)
                //    return;

                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedPortMon(M3PortMon pPortInfo)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl3PortMon == null)
                //    return;

                //ComMisc.frmMain.ctrl3PortMon.portMon = pPortInfo;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedRealScreenMon(M4RealScreenMon pImg)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl4RealScreenMon == null)
                //    return;

                //ComMisc.frmMain.ctrl4RealScreenMon.scrImage = pImg.scrImg;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedRealAppMon(M4RealAppMon realAppMon)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl4RealAppMon == null)
                //    return;

                //ComMisc.frmMain.ctrl4RealAppMon.realAppMon = realAppMon;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedDeviceMon(M8DeviceMon pDeviceMon)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl8HardMon == null)
                //    return;

                //ComMisc.frmMain.ctrl8HardMon.deviceMon = pDeviceMon;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedRealProcMon(M8ProcMon pProcMon)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl8ProcMon == null)
                //    return;

                //ComMisc.frmMain.ctrl8ProcMon.procMon = pProcMon;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedInstAppHis(M8InstAppHis pInstAppHis)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl8InstAppHis == null)
                //    return;

                //ComMisc.frmMain.ctrl8InstAppHis.instAppHis = pInstAppHis;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedProcMon(M6ProcMon pProcMon)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl6ProcHis == null)
                //    return;

                //ComMisc.frmMain.ctrl6ProcHis.procHis = pProcHis;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedNetAppHis(M7NetAppHis pAppHis)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl7NetAppHis == null)
                //    return;

                //ComMisc.frmMain.ctrl7NetAppHis.netAppHis = pAppHis;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedWebHis(M7WebHis pWebHis)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl7WebHis == null)
                //    return;

                //ComMisc.frmMain.ctrl7WebHis.webHis = pWebHis;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedUserSet(M6UserSet pUserSet)
        {
            try
            {
                //if (ComMisc.frmMain.ctrl6UserSet == null)
                //    return;

                //ComMisc.frmMain.ctrl6UserSet.userSet = pUserSet;
                //ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }
        #endregion

        #region Event Methods


        private void commBackServer_ClientConnected(object sender, ServerClientEventArgs e)
        {
            try
            {
                e.Client.MessageReceived += new EventHandler<MessageEventArgs>(Client_MessageReceived);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void commBackServer_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            try
            {
                var client = (IInmcServerClient)e.Client;
                ReceivedAgentInfo(client.ClientId, null);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

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
                //AgentInfo clientInfo = new AgentInfo();




                //Get a reference to the client

            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }
        #endregion
    }
}
