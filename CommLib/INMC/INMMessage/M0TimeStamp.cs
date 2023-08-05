using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// Represents a chat agent.
    /// This object particularly used in Login of a agent.
    /// </summary>
    [Serializable]
    public class M0TimeStamp : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList timeItems = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M0TimeStamp()
        {
            MsgKind = MessageType.KIND.MSG0_TIMESTAMP;
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
            M0TimeItem timeItem;
            MsgDataConverter.WriteLenField(memStream, timeItems.Count);
            for (int i = 0; i < timeItems.Count; i++)
            {
                timeItem = (M0TimeItem)timeItems[i];
                memStream.WriteByte((byte)timeItem.settingMessage);
                MsgDataConverter.WriteDateField(memStream, timeItem.settingTime);
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
            M0TimeItem timeItem;
            timeItems.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                timeItem = new M0TimeItem();
                timeItem.settingMessage = (MessageType.KIND)memStream.ReadByte();
                timeItem.settingTime = MsgDataConverter.ReadDateField(memStream);
                timeItems.Add(timeItem);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M0TimeItem
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageType.KIND settingMessage;

        /// <summary>
        /// 
        /// </summary>
        public DateTime settingTime;

        /// <summary>
        /// 
        /// </summary>
        public M0TimeItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M0TimeItem(M0TimeItem item)
        {
            settingMessage = item.settingMessage;
            settingTime = item.settingTime;
        }
    }
}
