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
    /// <summary>
    /// ServerBack  class
    /// </summary>
    internal class ServerBack
    {
        #region Fields and Properties
        internal IInmcServer commBackServer { get; set; }
        internal int iPort { get; set; }

        private List<M0AgentInfo> _AgentBackList;

        private InmsDataSet.tblUserListDataTable _tblUserList;
        private InmsDataSetTableAdapters.tblUserListTableAdapter _tblUserListTableAdapter;

        private InmsDataSet.tblTimestampSetDataTable _tblTimeStamp;
        private InmsDataSetTableAdapters.tblTimestampSetTableAdapter _tblTimestampSetTableAdapter;

        private InmsDataSet.tbl1AppdisableSetDataTable _tbl1Appdisable;
        private InmsDataSetTableAdapters.tbl1AppdisableSetTableAdapter _tbl1AppdisableSetTableAdapter;

        private InmsDataSet.tbl2NetipSetDataTable _tbl2NetipAllow;
        private InmsDataSetTableAdapters.tbl2NetipSetTableAdapter _tbl2NetipSetTableAdapter;

        private InmsDataSet.tbl7WebHisDataTable _tbl7WebHis;
        private InmsDataSetTableAdapters.tbl7WebHisTableAdapter _tbl7WebHisTableAdapter;

        private InmsDataSet.tbl7NetappHisDataTable _tbl7NetAppHis;
        private InmsDataSetTableAdapters.tbl7NetappHisTableAdapter _tbl7NetappHisTableAdapter;

        //private InmsDataSet.tbl8InstappHisDataTable _tblInstAppHis;
        //private InmsDataSetTableAdapters.tbl8InstappHisTableAdapter _tbl8InstappHisTableAdapter;

        #endregion

        #region Constructors
        /// <summary>
        /// ServerBack
        /// </summary>
        internal ServerBack()
        {
            iPort = ComMisc.SERVERBACK_PORT;

            _AgentBackList = new List<M0AgentInfo>();

            _tblUserList = new InmsDataSet.tblUserListDataTable();
            _tblUserListTableAdapter = new INMServer.InmsDataSetTableAdapters.tblUserListTableAdapter();
            _tblUserListTableAdapter.Connection = ComMisc.GetDBConnection();

            _tblTimeStamp = new InmsDataSet.tblTimestampSetDataTable();
            _tblTimestampSetTableAdapter = new INMServer.InmsDataSetTableAdapters.tblTimestampSetTableAdapter();
            _tblTimestampSetTableAdapter.Connection = ComMisc.GetDBConnection();

            _tbl1Appdisable = new InmsDataSet.tbl1AppdisableSetDataTable();
            _tbl1AppdisableSetTableAdapter = new InmsDataSetTableAdapters.tbl1AppdisableSetTableAdapter();
            _tbl1AppdisableSetTableAdapter.Connection = ComMisc.GetDBConnection();

            _tbl2NetipAllow = new InmsDataSet.tbl2NetipSetDataTable();
            _tbl2NetipSetTableAdapter = new InmsDataSetTableAdapters.tbl2NetipSetTableAdapter();
            _tbl2NetipSetTableAdapter.Connection = ComMisc.GetDBConnection();

            _tbl7WebHis = new InmsDataSet.tbl7WebHisDataTable();
            _tbl7WebHisTableAdapter = new InmsDataSetTableAdapters.tbl7WebHisTableAdapter();
            _tbl7WebHisTableAdapter.Connection = ComMisc.GetDBConnection();

            _tbl7NetAppHis = new InmsDataSet.tbl7NetappHisDataTable();
            _tbl7NetappHisTableAdapter = new InmsDataSetTableAdapters.tbl7NetappHisTableAdapter();
            _tbl7NetappHisTableAdapter.Connection = ComMisc.GetDBConnection();

            //_tblInstAppHis = new InmsDataSet.tbl8InstappHisDataTable();
            //_tbl8InstappHisTableAdapter = new InmsDataSetTableAdapters.tbl8InstappHisTableAdapter();
            //_tbl8InstappHisTableAdapter.Connection = ComMisc.DConn;
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
            }
        }

        /// <summary>
        /// StopServer
        /// </summary>
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
                IInmcServerClient client = commBackServer.Clients[id];

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
                IInmcServerClient client = commBackServer.Clients[id];

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
        /// GetUserIdFromAgentId
        /// </summary>
        /// <param name="pAid"></param>
        /// <returns></returns>
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
            }
            return 0;
        }

        /// <summary>
        /// ReceivedTextMessage
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pMsg"></param>
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
                    case MessageType.KIND.MSG0_TIMESTAMP:
                        ReceivedTimeStamp(pAid, (M0TimeStamp)formatter.Deserialize(mStream));
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
                    case MessageType.KIND.MSG8_CHANGEHIS:
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
                    ComMisc.frmMain.ReviewNodeItem(pAid, pAgent, false, true);
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedTimeStamp
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pTimeStamp"></param>
        private void ReceivedTimeStamp(long pAid, M0TimeStamp pTimeStamp)
        {
            try
            {
                if (pTimeStamp == null)
                    return;

                int userid = 0;
                
                userid = GetUserIdFromAgentId(pAid);
                if (userid == 0)
                    return;

                if (_tblTimestampSetTableAdapter.Connection.State == ConnectionState.Open)
                    _tblTimestampSetTableAdapter.Connection.Close();

                _tblTimestampSetTableAdapter.Fill(_tblTimeStamp);
                _tblTimeStamp.DefaultView.RowFilter = "userid=" + userid;
                if (_tblTimeStamp.DefaultView.Count != 1)
                    return;

                DateTime tblTime = DateTime.MinValue;

                for (int i = 0; i < pTimeStamp.timeItems.Count; i++)
                {
                    M0TimeItem item = (M0TimeItem)(pTimeStamp.timeItems[i]);

                    switch (item.settingMessage)
                    {
                        case MessageType.KIND.MSG1_APPDISABLESET:
                            if (_tblTimeStamp.DefaultView[0]["tbl1appdisableset"].ToString() == string.Empty)
                                break;

                            tblTime = (DateTime)(_tblTimeStamp.DefaultView[0]["tbl1appdisableset"]);

                            if (item.settingTime < tblTime)
                            {
                                if (_tbl1AppdisableSetTableAdapter.Connection.State == ConnectionState.Open)
                                    _tbl1AppdisableSetTableAdapter.Connection.Close();

                                _tbl1AppdisableSetTableAdapter.Fill(_tbl1Appdisable);
                                _tbl1Appdisable.DefaultView.RowFilter = "userid=" + userid;

                                M1AppdisableSet disableInfo = new M1AppdisableSet();

                                for (int j = 0; j < _tbl1Appdisable.DefaultView.Count; j++)
                                {
                                    DataRow row = _tbl1Appdisable.DefaultView[j].Row;
                                    M1DisableAppItem appItem = new M1DisableAppItem();
                                    appItem.nType = 0;

                                    appItem.bDisable = (row["disable"].ToString() == @"1") ? true : false;
                                    appItem.strProcName = row["name"].ToString();

                                    disableInfo.appItems.Add(appItem);
                                }

                                disableInfo.MsgKind = MessageType.KIND.MSG1_APPDISABLESET;
                                disableInfo.MsgType = MessageType.TYPE.INITSET;
                                disableInfo.msgTime = tblTime;

                                IFormatter formatter = new BinaryFormatter();
                                MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                                stream.Position = 0;
                                formatter.Serialize(stream, disableInfo);

                                SendRawDataMessageToAgent(pAid.ToString(), stream.ToArray());
                                stream.Close();
                            }
                            break;
                        case MessageType.KIND.MSG2_NETALLOWSET:
                            if (_tblTimeStamp.DefaultView[0]["tbl2netset"].ToString() == string.Empty)
                                break;

                            tblTime = (DateTime)(_tblTimeStamp.DefaultView[0]["tbl2netset"]);

                            if (item.settingTime < tblTime)
                            {
                                if (_tbl2NetipSetTableAdapter.Connection.State == ConnectionState.Open)
                                    _tbl2NetipSetTableAdapter.Connection.Close();

                                _tbl2NetipSetTableAdapter.Fill(_tbl2NetipAllow);
                                _tbl2NetipAllow.DefaultView.RowFilter = "userid=" + userid;

                                M2NetAllowSet allowInfo = new M2NetAllowSet();

                                for (int j = 0; j < _tbl2NetipAllow.DefaultView.Count; j++)
                                {
                                    DataRow row = _tbl2NetipAllow.DefaultView[j].Row;
                                    M2NetAllowAppItem ipItem = new M2NetAllowAppItem();
                                    ipItem.nType = 0;

                                    ipItem.strIp = row["ipaddr"].ToString();
                                    ipItem.startSet = (row["start_set"].ToString() == @"1") ? true : false;
                                    ipItem.endSet = (row["end_set"].ToString() == @"1") ? true : false;
                                    ipItem.startTime = (DateTime)(row["ipstart_time"]);
                                    ipItem.endTime = (DateTime)(row["ipend_time"]);

                                    allowInfo.ipList.Add(ipItem);
                                }

                                allowInfo.MsgKind = MessageType.KIND.MSG2_NETALLOWSET;
                                allowInfo.MsgType = MessageType.TYPE.INITSET;
                                allowInfo.msgTime = tblTime;

                                IFormatter formatter = new BinaryFormatter();
                                MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

                                stream.Position = 0;
                                formatter.Serialize(stream, allowInfo);

                                SendRawDataMessageToAgent(pAid.ToString(), stream.ToArray());
                                stream.Close();
                            }
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedInstAppHis
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pInstAppHis"></param>
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
            }
        }

        /// <summary>
        /// ReceivedNetAppHis
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pAppHis"></param>
        private void ReceivedNetAppHis(long pAid, M7NetAppHis pAppHis)
        {
            try
            {
                if (pAppHis == null)
                    return;

                for (int i = 0; i < pAppHis.netAppList.Count; i++)
                {
                    M7NetAppHisItem app = (M7NetAppHisItem)(pAppHis.netAppList[i]);
                    if (app == null)
                        continue;

                    DataRow row = _tbl7NetAppHis.NewRow();
                    row["userid"] = GetUserIdFromAgentId(pAid);
                    row["app_name"] = app.strProgram;
                    row["app_path"] = app.strPath;
                    row["access_time"] = app.strAccessTime;
                    row["local_addr"] = app.strLocalAddr;
                    row["remote_addr"] = app.strRemoteAddr;
                    row["status"] = app.strStatus;
                    _tbl7NetAppHis.Rows.Add(row);
                }

                if (_tbl7NetAppHis.GetChanges() != null)
                    _tbl7NetappHisTableAdapter.Update(_tbl7NetAppHis);
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// ReceivedWebHis
        /// </summary>
        /// <param name="pAid"></param>
        /// <param name="pWebHis"></param>
        private void ReceivedWebHis(long pAid, M7WebHis pWebHis)
        {
            try
            {
                if (pWebHis == null)
                    return;

                for (int i = 0; i < pWebHis.webHisList.Count; i++)
                {
                    M7WebHisItem web = (M7WebHisItem)(pWebHis.webHisList[i]);
                    if (web == null)
                        continue;

                    DataRow row = _tbl7WebHis.NewRow();
                    row["userid"] = GetUserIdFromAgentId(pAid);
                    if (web.strUrl.Length > 50)
                        row["url_addr"] = web.strUrl.Substring(0, 50);
                    else
                        row["url_addr"] = web.strUrl;
                    row["title_name"] = web.strTitle;
                    row["access_time"] = web.strAccessTime;
                    _tbl7WebHis.Rows.Add(row);
                }

                if (_tbl7WebHis.GetChanges() != null)
                    _tbl7WebHisTableAdapter.Update(_tbl7WebHis);
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
        /// commBackServer_ClientConnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void commBackServer_ClientConnected(object sender, ServerClientEventArgs e)
        {
            try
            {
                M7NetAppHis appInfo = new M7NetAppHis();
                appInfo.MsgKind = MessageType.KIND.MSG7_NETAPPHIS;
                appInfo.MsgType = MessageType.TYPE.REQUEST;
                appInfo.Interval = 80;

                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(ComMisc.MSGLENGTH_MAX);

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
            }
            catch (System.Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        /// <summary>
        /// commBackServer_ClientDisconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
