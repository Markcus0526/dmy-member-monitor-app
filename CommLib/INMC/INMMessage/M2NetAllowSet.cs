using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INMC.INMMessage;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M2NetAllowSet : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public bool bNetEnable = true;

        /// <summary>
        /// 
        /// </summary>
        public bool bValidStartTime = false;

        /// <summary>
        /// 
        /// </summary>
        public DateTime startTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public bool bValidEndTime = false;

        /// <summary>
        /// 
        /// </summary>
        public DateTime endTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public System.Collections.ArrayList ipList = new System.Collections.ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M2NetAllowSet()
        {
            MsgKind = MessageType.KIND.MSG2_NETALLOWSET;
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
            MsgDataConverter.WriteBoolDataField(memStream, bNetEnable);
            MsgDataConverter.WriteBoolDataField(memStream, bValidStartTime);
            MsgDataConverter.WriteDateField(memStream, startTime);
            MsgDataConverter.WriteBoolDataField(memStream, bValidEndTime);
            MsgDataConverter.WriteDateField(memStream, endTime);

            M2NetAllowAppItem item;
            MsgDataConverter.WriteLenField(memStream, ipList.Count);
            for (int i = 0; i < ipList.Count; i++)
            {
                item = (M2NetAllowAppItem)ipList[i];
                MsgDataConverter.WriteInt32(memStream, item.nType);
                MsgDataConverter.WriteStringField(memStream, item.strIp);
                MsgDataConverter.WriteBoolDataField(memStream, item.startSet);
                MsgDataConverter.WriteDateField(memStream, item.startTime);
                MsgDataConverter.WriteBoolDataField(memStream, item.endSet);
                MsgDataConverter.WriteDateField(memStream, item.endTime);
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
            bNetEnable = MsgDataConverter.ReadBoolDataField(memStream);
            bValidStartTime = MsgDataConverter.ReadBoolDataField(memStream);
            startTime = MsgDataConverter.ReadDateField(memStream);
            bValidEndTime = MsgDataConverter.ReadBoolDataField(memStream);
            endTime = MsgDataConverter.ReadDateField(memStream);

            M2NetAllowAppItem item;
            ipList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M2NetAllowAppItem();
                item.nType = MsgDataConverter.ReadInt32(memStream);
                item.strIp = MsgDataConverter.ReadStringField(memStream);
                item.startSet = MsgDataConverter.ReadBoolDataField(memStream);
                item.startTime = MsgDataConverter.ReadDateField(memStream);
                item.endSet = MsgDataConverter.ReadBoolDataField(memStream);
                item.endTime = MsgDataConverter.ReadDateField(memStream);
                ipList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M2NetAllowAppItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int nType = 0;

        /// <summary>
        /// 
        /// </summary>
        public string strIp = "";

        /// <summary>
        /// 
        /// </summary>
        public bool startSet = false;

        /// <summary>
        /// 
        /// </summary>
        public DateTime startTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public bool endSet = false;

        /// <summary>
        /// 
        /// </summary>
        public DateTime endTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public M2NetAllowAppItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M2NetAllowAppItem(M2NetAllowAppItem item)
        {
            nType = item.nType;
            strIp = item.strIp;
            startSet = item.startSet;
            startTime = item.startTime;
            endSet = item.endSet;
            endTime = item.endTime;
        }
    }
}
