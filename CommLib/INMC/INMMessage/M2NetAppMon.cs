using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INMC.INMMessage;
using System.Collections;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M2NetAppMon : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList netAppList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M2NetAppMon()
        {
            MsgKind = MessageType.KIND.MSG2_NETAPPMON;
            MsgType = MessageType.TYPE.REQUESTREPLY;
        }

        //********************************************
        //ADDED BY JUJ. 2012.10.4.
        //********************************************

        /// <summary>
        /// Make byte array of inmc message
        /// </summary>
        public override byte[] GetByteArray()
        {
            base.GetByteArray();

            //Write message-specific field data
            //Add your codes here
            M2NetAppItem item;
            MsgDataConverter.WriteLenField(memStream, netAppList.Count);
            for (int i = 0; i < netAppList.Count; i++)
            {
                item = (M2NetAppItem)netAppList[i];
                MsgDataConverter.WriteStringField(memStream, item.strPgname);
                MsgDataConverter.WriteStringField(memStream, item.strPgpath);
                MsgDataConverter.WriteStringField(memStream, item.strLocalAddr);
                MsgDataConverter.WriteStringField(memStream, item.strRemoteAddr);
                MsgDataConverter.WriteStringField(memStream, item.strStatus);
            }
            return memStream.ToArray();
        }

        /// <summary>
        /// initialize message field from byte array
        /// </summary>
        public override void Initialize(byte[] byteArray)
        {
            base.Initialize(byteArray);

            //To initialize message fields
            //Add your codes here
            M2NetAppItem item;
            netAppList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M2NetAppItem();
                item.strPgname = MsgDataConverter.ReadStringField(memStream);
                item.strPgpath = MsgDataConverter.ReadStringField(memStream);
                item.strLocalAddr = MsgDataConverter.ReadStringField(memStream);
                item.strRemoteAddr = MsgDataConverter.ReadStringField(memStream);
                item.strStatus = MsgDataConverter.ReadStringField(memStream);
                netAppList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M2NetAppItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string strPgname = "";

        /// <summary>
        /// 
        /// </summary>
        public string strPgpath = "";

        /// <summary>
        /// 
        /// </summary>
        public string strLocalAddr = "";

        /// <summary>
        /// 
        /// </summary>
        public string strRemoteAddr = "";

        /// <summary>
        /// 
        /// </summary>
        public string strStatus = "";

        /// <summary>
        /// 
        /// </summary>
        public M2NetAppItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M2NetAppItem(M2NetAppItem item)
        {
            strPgname = item.strPgname;
            strPgpath = item.strPgpath;
            strLocalAddr = item.strLocalAddr;
            strRemoteAddr = item.strRemoteAddr;
            strStatus = item.strStatus;
        }
    }
}
