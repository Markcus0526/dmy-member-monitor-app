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
    public class M4RealAppMon : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList appList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M4RealAppMon()
        {
            MsgKind = MessageType.KIND.MSG4_REALAPPMON;
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
            M4AppItem item;
            MsgDataConverter.WriteLenField(memStream, appList.Count);
            for (int i = 0; i < appList.Count; i++)
            {
                item = (M4AppItem)appList[i];
                MsgDataConverter.WriteStringField(memStream, item.appName);
                MsgDataConverter.WriteStringField(memStream, item.appStatus);
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
            M4AppItem item;
            appList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M4AppItem();
                item.appName = MsgDataConverter.ReadStringField(memStream);
                item.appStatus = MsgDataConverter.ReadStringField(memStream);
                appList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M4AppItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string appName = "";

        /// <summary>
        /// 
        /// </summary>
        public string appStatus = "";

        /// <summary>
        /// 
        /// </summary>
        public M4AppItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M4AppItem(M4AppItem item)
        {
            appName = item.appName;
            appStatus = item.appStatus;
        }
    }
}
