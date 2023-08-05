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
using System.Data;
using INMC.Communication.Inmc.Communication.EndPoints;

namespace INMServer
{
    internal class ServerInit
    {
        #region Fields and Properties
        internal IInmcServer commInitServer { get; set; }
        internal int iPort { get; set; }

        private List<M0AgentInfo> _AgentBackList;

        private InmsDataSet.tblUserListDataTable _tblUserList;
        private InmsDataSetTableAdapters.tblUserListTableAdapter _tblUserListTableAdapter;

        private InmsDataSet.tbl6ProcHisDataTable _tblProcHis;
        private InmsDataSetTableAdapters.tbl6ProcHisTableAdapter _tbl6ProcHisTableAdapter;

        private InmsDataSet.tbl7WebHisDataTable _tblWebHis;
        private InmsDataSetTableAdapters.tbl7WebHisTableAdapter _tbl7WebHisTableAdapter;

        private InmsDataSet.tbl7NetappHisDataTable _tblNetAppHis;
        private InmsDataSetTableAdapters.tbl7NetappHisTableAdapter _tbl7NetappHisTableAdapter;

        //private InmsDataSet.tbl8InstappHisDataTable _tblInstAppHis;
        //private InmsDataSetTableAdapters.tbl8InstappHisTableAdapter _tbl8InstappHisTableAdapter;

        #endregion

        #region Constructors
        internal ServerInit()
        {
            iPort = ComMisc.SERVERINIT_PORT;

            _AgentBackList = new List<M0AgentInfo>();

            _tblUserList = new InmsDataSet.tblUserListDataTable();
            _tblUserListTableAdapter = new INMServer.InmsDataSetTableAdapters.tblUserListTableAdapter();
            _tblUserListTableAdapter.Connection = ComMisc.GetDBConnection();

            _tblProcHis = new InmsDataSet.tbl6ProcHisDataTable();
            _tbl6ProcHisTableAdapter = new InmsDataSetTableAdapters.tbl6ProcHisTableAdapter();
            _tbl6ProcHisTableAdapter.Connection = ComMisc.GetDBConnection();

            _tblWebHis = new InmsDataSet.tbl7WebHisDataTable();
            _tbl7WebHisTableAdapter = new InmsDataSetTableAdapters.tbl7WebHisTableAdapter();
            _tbl7WebHisTableAdapter.Connection = ComMisc.GetDBConnection();

            _tblNetAppHis = new InmsDataSet.tbl7NetappHisDataTable();
            _tbl7NetappHisTableAdapter = new InmsDataSetTableAdapters.tbl7NetappHisTableAdapter();
            _tbl7NetappHisTableAdapter.Connection = ComMisc.GetDBConnection();

            //_tblInstAppHis = new InmsDataSet.tbl8InstappHisDataTable();
            //_tbl8InstappHisTableAdapter = new InmsDataSetTableAdapters.tbl8InstappHisTableAdapter();
            //_tbl8InstappHisTableAdapter.Connection = ComMisc.DConn;
        }
        #endregion

        #region Internal Methods
        internal void StartServer()
        {
            try
            {
                commInitServer = InmcServerFactory.CreateServer(new InmcTcpEndPoint(iPort));
                //commService = new CommService();
                //commServer.AddService<INMService, CommService>(commService);
                commInitServer.ClientConnected += new EventHandler<ServerClientEventArgs>(commBackServer_ClientConnected);
                commInitServer.ClientDisconnected += new EventHandler<ServerClientEventArgs>(commBackServer_ClientDisconnected);
                //commService.AgentListChanged += new EventHandler(commService_AgentListChanged);
                commInitServer.Start();
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
                if (commInitServer == null)
                {
                    return;
                }

                commInitServer.Stop();
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
                IInmcServerClient client = commInitServer.Clients[id];

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
                IInmcServerClient client = commInitServer.Clients[id];

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

        internal int GetUserIdFromAgentId(long pAid)
        {
            try
            {
                int id = 0;

                if (_tblUserListTableAdapter.Connection.State == ConnectionState.Open)
                    _tblUserListTableAdapter.Connection.Close();

                _tblUserListTableAdapter.Fill(_tblUserList);

                for (int i = 0; i < _AgentBackList.Count; i++)
                {
                    if (_AgentBackList[i].AgentId == pAid.ToString())
                    {
                        string strFilter = "macaddr='" + _AgentBackList[i].MacAddr.ToString() +
                            "' AND username='" + _AgentBackList[i].LogonUser.ToString() + "'";

                        DataRow[] row = _tblUserList.Select(strFilter);
                        if (row.Length == 1)
                        {
                            id = Convert.ToInt32(row[0]["userid"].ToString());
                            break;
                        }
                    }
                }

                return id;
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

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
                        //ReceivedBandMon((M1BandMon)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG2_NETALLOWSET:
                        break;
                    case MessageType.KIND.MSG2_NETAPPMON:
                        //ReceivedNetIpMon((M2NetAppMon)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG3_PORTMON:
                        //ReceivedPortMon((M3PortMon)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG3_PORTSET:
                        //ReceivedPortSet((M3PortSet)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG4_REALAPPMON:
                        //ReceivedRealAppMon((M4RealAppMon)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG4_REALSCREENMON:
                        //ReceivedRealScreenMon((M4RealScreenMon)formatter.Deserialize(mStream));
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
                        //ReceivedUserSet((M6UserSet)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG6_PROCHIS:
                        ReceivedProcHis(pAid, (M6ProcHis)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG7_WEBHIS:
                        ReceivedWebHis(pAid, (M7WebHis)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG7_NETAPPHIS:
                        ReceivedNetAppHis(pAid, (M7NetAppHis)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG8_INSTAPPHIS:
                        //ReceivedInstAppHis(pAid, (M8InstAppHis)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG8_PROCMON:
                        //ReceivedRealProcMon((M8ProcMon)formatter.Deserialize(mStream));
                        break;
                    case MessageType.KIND.MSG8_CHANGEHIS:
                        break;
                    case MessageType.KIND.MSG8_DEVICEMON:
                        //ReceivedDeviceMon((M8DeviceMon)formatter.Deserialize(mStream));
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
                    M0AgentInfo newAgent = new M0AgentInfo();
                    newAgent.AgentId = pAid.ToString();
                    newAgent.IpAddr = pAgent.IpAddr;
                    newAgent.MacAddr = pAgent.MacAddr;
                    newAgent.LogonUser = pAgent.LogonUser;
                    newAgent.MachineName = pAgent.MachineName;

                    _AgentBackList.Add(newAgent);
                }
                else // Remove
                {
                    ComMisc.frmMain.ReviewNodeItem(pAid, pAgent, false);
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
                if (ComMisc.frmMain.ctrl1NicMon == null)
                    return;

                ComMisc.frmMain.ctrl1NicMon.bandMon = pBandMon;
                ComMisc.frmMain.ReviewViewPage(true);
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
                if (ComMisc.frmMain.ctrl2NetAppMon == null)
                    return;

                ComMisc.frmMain.ctrl2NetAppMon.netMon = pNetMon;
                ComMisc.frmMain.ReviewViewPage(true);
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
                if (ComMisc.frmMain.ctrl3PortSet == null)
                    return;

                ComMisc.frmMain.ReviewViewPage(true);
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
                if (ComMisc.frmMain.ctrl3PortMon == null)
                    return;

                ComMisc.frmMain.ctrl3PortMon.portMon = pPortInfo;
                ComMisc.frmMain.ReviewViewPage(true);
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
                if (ComMisc.frmMain.ctrl4RealScreenMon == null)
                    return;

                ComMisc.frmMain.ctrl4RealScreenMon.scrImage = pImg.scrImg;
                ComMisc.frmMain.ReviewViewPage(true);
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
                if (ComMisc.frmMain.ctrl4RealAppMon == null)
                    return;

                ComMisc.frmMain.ctrl4RealAppMon.realAppMon = realAppMon;
                ComMisc.frmMain.ReviewViewPage(true);
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
                if (ComMisc.frmMain.ctrl8HardMon == null)
                    return;

                ComMisc.frmMain.ctrl8HardMon.deviceMon = pDeviceMon;
                ComMisc.frmMain.ReviewViewPage(true);
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
                if (ComMisc.frmMain.ctrl8ProcMon == null)
                    return;

                ComMisc.frmMain.ctrl8ProcMon.procMon = pProcMon;
                ComMisc.frmMain.ReviewViewPage(true);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedInstAppHis(long pAid, M8InstAppHis pInstAppHis)
        {
            try
            {
                //if (_tblInstAppHis == null)
                //    return;

                //if (pInstAppHis.installList.Count > 0)
                //{
                //    _tblProcHis.Clear();
                //    _tbl8InstappHisTableAdapter.Update(_tblInstAppHis);
                //}

                //for (int i = 0; i < pInstAppHis.installList.Count; i++)
                //{
                //    installItem proc = (installItem)(pInstAppHis.installList[i]);
                //    DataRow row = _tblInstAppHis.NewRow();
                //    row["userid"] = GetUserIdFromAgentId(pAid);
                //    row["app_name"] = proc.strProgram;
                //    row["app_path"] = proc.strPath;
                //    row["inst_time"] = proc.installTime;
                //    _tblInstAppHis.Rows.Add(row);
                //}

                //if (_tblProcHis.GetChanges() != null)
                //{
                //    _tbl8InstappHisTableAdapter.Update(_tblInstAppHis);
                //}
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedProcHis(long pAid, M6ProcHis pProcHis)
        {
            try
            {
                if (pProcHis == null)
                    return;

                for (int i = 0; i < pProcHis.procHisList.Count; i++)
                {
                    M6ProcHisItem proc = (M6ProcHisItem)(pProcHis.procHisList[i]);
                    DataRow row = _tblProcHis.NewRow();

                    row["userid"] = GetUserIdFromAgentId(pAid);
                    row["proc_name"] = proc.strProcName;
                    row["proc_path"] = proc.strProcPath;
                    row["start_time"] = proc.startTime;
                    row["stop_time"] = proc.stopTime;
                    row["status"] = proc.strStatus;
                    _tblProcHis.Rows.Add(row);
                }

                if (_tblProcHis.GetChanges() != null)
                    _tbl6ProcHisTableAdapter.Update(_tblProcHis);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedNetAppHis(long pAid, M7NetAppHis pAppHis)
        {
            try
            {
                if (pAppHis == null)
                    return;

                for (int i = 0; i < pAppHis.netAppList.Count; i++)
                {
                    M7NetAppHisItem app = (M7NetAppHisItem)(pAppHis.netAppList[i]);
                    DataRow row = _tblNetAppHis.NewRow();

                    row["userid"] = GetUserIdFromAgentId(pAid);
                    row["app_name"] = app.strProgram;
                    row["app_path"] = app.strPath;
                    row["access_time"] = app.strAccessTime;
                    row["local_addr"] = app.strLocalAddr;
                    row["remote_addr"] = app.strRemoteAddr;
                    row["status"] = app.strStatus;
                    _tblNetAppHis.Rows.Add(row);
                }

                if (_tblNetAppHis.GetChanges() != null)
                    _tbl7NetappHisTableAdapter.Update(_tblNetAppHis);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
                throw;
            }
        }

        private void ReceivedWebHis(long pAid, M7WebHis pWebHis)
        {
            try
            {
                if (pWebHis == null)
                    return;

                for (int i = 0; i < pWebHis.webHisList.Count; i++)
                {
                    M7WebHisItem web = (M7WebHisItem)(pWebHis.webHisList[i]);
                    DataRow row = _tblWebHis.NewRow();

                    row["userid"] = GetUserIdFromAgentId(pAid);
                    if (web.strUrl.Length > 50)
                        row["url_addr"] = web.strUrl.Substring(0, 50);
                    else
                        row["url_addr"] = web.strUrl;
                    row["title_name"] = web.strTitle;
                    row["access_time"] = web.strAccessTime;
                    _tblWebHis.Rows.Add(row);
                }

                if (_tblWebHis.GetChanges() != null)
                    _tbl7WebHisTableAdapter.Update(_tblWebHis);
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
                if (ComMisc.frmMain.ctrl6UserSet == null)
                    return;

                ComMisc.frmMain.ctrl6UserSet.userSet = pUserSet;
                ComMisc.frmMain.ReviewViewPage(true);
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
                M6ProcHis procInfo = new M6ProcHis();
                procInfo.MsgKind = MessageType.KIND.MSG6_PROCHIS;
                procInfo.MsgType = MessageType.TYPE.REQUEST;
                procInfo.Interval = 60;

                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, procInfo);

                e.Client.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                ///////////////////////////////////////////////////////
                M7NetAppHis appInfo = new M7NetAppHis();
                appInfo.MsgKind = MessageType.KIND.MSG7_NETAPPHIS;
                appInfo.MsgType = MessageType.TYPE.REQUEST;
                appInfo.Interval = 80;

                formatter = new BinaryFormatter();
                stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, appInfo);

                e.Client.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                ///////////////////////////////////////////////////////
                M7WebHis webInfo = new M7WebHis();
                webInfo.MsgKind = MessageType.KIND.MSG7_WEBHIS;
                webInfo.MsgType = MessageType.TYPE.REQUEST;
                webInfo.Interval = 100;

                formatter = new BinaryFormatter();
                stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                stream.Position = 0;
                formatter.Serialize(stream, webInfo);                                

                e.Client.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                e.Client.MessageReceived += new EventHandler<MessageEventArgs>(Client_MessageReceived);

                ///////////////////////////////////////////////////////
                //M8InstAppHis instInfo = new M8InstAppHis();
                //instInfo.MsgKind = MessageType.KIND.MSG8_INSTAPPHIS;
                //instInfo.MsgType = MessageType.TYPE.REQUEST;
                ////instInfo.Interval = 100;

                //formatter = new BinaryFormatter();
                //stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                //stream.Position = 0;
                //formatter.Serialize(stream, instInfo);

                //e.Client.SendMessage(new InmcRawDataMessage(stream.ToArray()));

                //e.Client.MessageReceived += new EventHandler<MessageEventArgs>(Client_MessageReceived);
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
                for (int i = 0; i < _AgentBackList.Count; i++)
                {
                    if (_AgentBackList[i].AgentId == e.Client.ClientId.ToString())
                    {
                        _AgentBackList.RemoveAt(i);
                        break;
                    }
                }   
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
