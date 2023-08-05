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
    public class M7NetAppHis : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public int Interval;

        /// <summary>
        /// 
        /// </summary>
        public ArrayList netAppList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M7NetAppHis()
        {
            MsgKind = MessageType.KIND.MSG7_NETAPPHIS;
            MsgType = MessageType.TYPE.REPLY;
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
            MsgDataConverter.WriteInt32(memStream, Interval);
            M7NetAppHisItem item;
            MsgDataConverter.WriteLenField(memStream, netAppList.Count);
            for (int i = 0; i < netAppList.Count; i++)
            {
                item = (M7NetAppHisItem)netAppList[i];
                MsgDataConverter.WriteStringField(memStream, item.strStatus);
                MsgDataConverter.WriteStringField(memStream, item.strProgram);
                MsgDataConverter.WriteStringField(memStream, item.strPath);
                MsgDataConverter.WriteDateField(memStream, item.strAccessTime);
                MsgDataConverter.WriteStringField(memStream, item.strLocalAddr);
                MsgDataConverter.WriteStringField(memStream, item.strRemoteAddr);
                
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
            Interval = MsgDataConverter.ReadInt32(memStream);
            M7NetAppHisItem item;
            netAppList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M7NetAppHisItem();
                item.strStatus = MsgDataConverter.ReadStringField(memStream);
                item.strProgram = MsgDataConverter.ReadStringField(memStream);
                item.strPath = MsgDataConverter.ReadStringField(memStream);
                item.strAccessTime = MsgDataConverter.ReadDateField(memStream);
                item.strLocalAddr = MsgDataConverter.ReadStringField(memStream);
                item.strRemoteAddr = MsgDataConverter.ReadStringField(memStream);
                netAppList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M7NetAppHisItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string strStatus = "";

        /// <summary>
        /// 
        /// </summary>
        public string strProgram = "";

        /// <summary>
        /// 
        /// </summary>
        public string strPath = "";

        /// <summary>
        /// 
        /// </summary>
        public DateTime strAccessTime = DateTime.Now;

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
        public M7NetAppHisItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M7NetAppHisItem(M7NetAppHisItem item)
        {
            strStatus = item.strStatus;
            strProgram = item.strProgram;
            strPath = item.strPath;
            strAccessTime = item.strAccessTime;
            strLocalAddr = item.strLocalAddr;
            strRemoteAddr = item.strRemoteAddr;
        }
    }
}
