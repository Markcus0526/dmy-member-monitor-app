using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M1AppdisableSet : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList appItems = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M1AppdisableSet()
        {
            MsgKind = MessageType.KIND.MSG1_APPDISABLESET;
            MsgType = MessageType.TYPE.REQUEST;
        }

        /// <summary>
        /// Make byte array of inmc message
        /// </summary>
        public override byte[] GetByteArray()
        {
            base.GetByteArray();

            //Write message-specific field data
            //Add your codes here
            M1DisableAppItem appItem;
            MsgDataConverter.WriteLenField(memStream, appItems.Count);
            for (int i = 0; i < appItems.Count; i++)
            {
                appItem = (M1DisableAppItem)appItems[i];
                MsgDataConverter.WriteInt32(memStream, appItem.nType);
                MsgDataConverter.WriteBoolDataField(memStream, appItem.bDisable);
                MsgDataConverter.WriteBoolDataField(memStream, appItem.bMatchClass);
                MsgDataConverter.WriteStringField(memStream, appItem.strClassName);
                MsgDataConverter.WriteBoolDataField(memStream, appItem.bMatchTitle);
                MsgDataConverter.WriteStringField(memStream, appItem.strTitle);
                MsgDataConverter.WriteBoolDataField(memStream, appItem.bMatchProc);
                MsgDataConverter.WriteStringField(memStream, appItem.strProcName);
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
            M1DisableAppItem appItem;
            appItems.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                appItem = new M1DisableAppItem();
                appItem.nType = MsgDataConverter.ReadInt32(memStream);
                appItem.bDisable = MsgDataConverter.ReadBoolDataField(memStream);
                appItem.bMatchClass = MsgDataConverter.ReadBoolDataField(memStream);
                appItem.strClassName = MsgDataConverter.ReadStringField(memStream);
                appItem.bMatchTitle = MsgDataConverter.ReadBoolDataField(memStream);
                appItem.strTitle = MsgDataConverter.ReadStringField(memStream);
                appItem.bMatchProc = MsgDataConverter.ReadBoolDataField(memStream);
                appItem.strProcName = MsgDataConverter.ReadStringField(memStream);
                appItems.Add(appItem);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M1DisableAppItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int nType = 0;

        /// <summary>
        /// 
        /// </summary>
        public bool bDisable = false;

        /// <summary>
        /// 
        /// </summary>
        public bool bMatchClass = false;

        /// <summary>
        /// 
        /// </summary>
        public string strClassName = "";

        /// <summary>
        /// 
        /// </summary>
        public bool bMatchTitle = false;

        /// <summary>
        /// 
        /// </summary>
        public string strTitle = "";

        /// <summary>
        /// 
        /// </summary>
        public bool bMatchProc = false;

        /// <summary>
        /// 
        /// </summary>
        public string strProcName = "";

        /// <summary>
        /// 
        /// </summary>
        public M1DisableAppItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M1DisableAppItem(M1DisableAppItem item)
        {
            nType = item.nType;
            bDisable = item.bDisable;
            bMatchClass = item.bMatchClass;
            strClassName = item.strClassName;
            bMatchTitle = item.bMatchTitle;
            strTitle = item.strTitle;
            bMatchProc = item.bMatchProc;
            strProcName = item.strProcName;
        }
    }
}
