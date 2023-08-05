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
    public class M6UserSet : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList userList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M6UserSet()
        {
            MsgKind = MessageType.KIND.MSG6_USERSET;
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
            M6UserItem item;
            MsgDataConverter.WriteLenField(memStream, userList.Count);
            for (int i = 0; i < userList.Count; i++)
            {
                item = (M6UserItem)userList[i];
                MsgDataConverter.WriteInt32(memStream, item.nType);
                MsgDataConverter.WriteStringField(memStream, item.strUserName);
                MsgDataConverter.WriteStringField(memStream, item.strPrivilege);
                MsgDataConverter.WriteStringField(memStream, item.strLogonState);
                MsgDataConverter.WriteDateField(memStream, item.strLogonTime);
                MsgDataConverter.WriteDateField(memStream, item.strLogoffTime);
                MsgDataConverter.WriteStringField(memStream, item.strPassword);
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
            M6UserItem item;
            userList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M6UserItem();
                item.nType = MsgDataConverter.ReadInt32(memStream);
                item.strUserName = MsgDataConverter.ReadStringField(memStream);
                item.strPrivilege = MsgDataConverter.ReadStringField(memStream);
                item.strLogonState = MsgDataConverter.ReadStringField(memStream);
                item.strLogonTime = MsgDataConverter.ReadDateField(memStream);
                item.strLogoffTime = MsgDataConverter.ReadDateField(memStream);
                item.strPassword = MsgDataConverter.ReadStringField(memStream);
                userList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M6UserItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int nType = 0;

        /// <summary>
        /// 
        /// </summary>
        public string strUserName = "";

        /// <summary>
        /// 
        /// </summary>
        public string strPrivilege = "";

        /// <summary>
        /// 
        /// </summary>
        public string strLogonState = "";

        /// <summary>
        /// 
        /// </summary>
        public DateTime strLogonTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public DateTime strLogoffTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public string strPassword = "";

        /// <summary>
        /// 
        /// </summary>
        public M6UserItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M6UserItem(M6UserItem item)
        {
            nType = item.nType;
            strUserName = item.strUserName;
            strPrivilege = item.strPrivilege;
            strLogonState = item.strLogonState;
            strLogonTime = item.strLogonTime;
            strLogoffTime = item.strLogoffTime;
            strPassword = item.strPassword;
        }
    }
}
