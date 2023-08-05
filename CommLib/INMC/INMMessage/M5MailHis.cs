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
    public class M5MailHis : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList MailList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M5MailHis()
        {
            MsgKind = MessageType.KIND.MSG5_MAILHIS;
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
            M5MailItem item;
            MsgDataConverter.WriteLenField(memStream, MailList.Count);
            for (int i = 0; i < MailList.Count; i++)
            {
                item = (M5MailItem)MailList[i];
                MsgDataConverter.WriteDateField(memStream, item.SendTime);
                MsgDataConverter.WriteStringField(memStream, item.MailFrom);
                MsgDataConverter.WriteStringField(memStream, item.MailTo);
                MsgDataConverter.WriteStringField(memStream, item.MailSubject);
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
            M5MailItem item;
            MailList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M5MailItem();
                item.SendTime = MsgDataConverter.ReadDateField(memStream);
                item.MailFrom = MsgDataConverter.ReadStringField(memStream);
                item.MailTo = MsgDataConverter.ReadStringField(memStream);
                item.MailSubject = MsgDataConverter.ReadStringField(memStream);
                MailList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M5MailItem
    {
        /// <summary>
        /// 메일 보낸 시간.
        /// </summary>
        public DateTime SendTime;

        /// <summary>
        /// 보낸사람.
        /// </summary>
        public string MailFrom = "";

        /// <summary>
        /// 받을사람.
        /// </summary>
        public string MailTo = "";

        /// <summary>
        /// 메일 제목.
        /// </summary>
        public string MailSubject = "";

        /// <summary>
        /// 
        /// </summary>
        public M5MailItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M5MailItem(M5MailItem item)
        {
            SendTime = item.SendTime;
            MailFrom = item.MailFrom;
            MailTo = item.MailTo;
            MailSubject = item.MailSubject;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M5MailHisRequest : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string   serverAddr;

        /// <summary>
        /// 
        /// </summary>
        public int portNum;

        /// <summary>
        /// 
        /// </summary>
        public string userAccount;

        /// <summary>
        /// 
        /// </summary>
        public string userPassword;

#region  Constructor
        /// <summary>
        /// 
        /// </summary>
        public M5MailHisRequest()
        {
            serverAddr = "";
            portNum = 0;
            userAccount = "";
            userPassword = "";
        }
#endregion
        
        
    }
}
