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
    public class M7WebHis : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public int Interval;

        /// <summary>
        /// 
        /// </summary>
        public ArrayList webHisList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M7WebHis()
        {
            MsgKind = MessageType.KIND.MSG7_WEBHIS;
            MsgType = MessageType.TYPE.REQUEST;
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
            M7WebHisItem item;
            MsgDataConverter.WriteLenField(memStream, webHisList.Count);
            for (int i = 0; i < webHisList.Count; i++)
            {
                item = (M7WebHisItem)webHisList[i];
                MsgDataConverter.WriteStringField(memStream, item.strUrl);
                MsgDataConverter.WriteStringField(memStream, item.strTitle);
                MsgDataConverter.WriteDateField(memStream, item.strAccessTime);
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
            M7WebHisItem item;
            webHisList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M7WebHisItem();
                item.strUrl = MsgDataConverter.ReadStringField(memStream);
                item.strTitle = MsgDataConverter.ReadStringField(memStream);
                item.strAccessTime = MsgDataConverter.ReadDateField(memStream);
                webHisList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M7WebHisItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string strUrl = "";

        /// <summary>
        /// 
        /// </summary>
        public string strTitle = "";

        /// <summary>
        /// 
        /// </summary>
        public DateTime strAccessTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public M7WebHisItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M7WebHisItem(M7WebHisItem item)
        {
            strUrl = item.strUrl;
            strTitle = item.strTitle;
            strAccessTime = item.strAccessTime;
        }
    }
}
