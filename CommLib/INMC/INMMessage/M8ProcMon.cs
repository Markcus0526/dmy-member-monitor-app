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
    public class M8ProcMon : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList procList = new ArrayList();
        
        /// <summary>
        /// 
        /// </summary>
        public M8ProcMon()
        {
            MsgKind = MessageType.KIND.MSG8_PROCMON;
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
            M8ProcItem item;
            MsgDataConverter.WriteLenField(memStream, procList.Count);
            for (int i = 0; i < procList.Count; i++)
            {
                item = (M8ProcItem)procList[i];
                MsgDataConverter.WriteInt32(memStream, item.sessionId);
                MsgDataConverter.WriteInt32(memStream, item.pId);
                MsgDataConverter.WriteStringField(memStream, item.procName);
                MsgDataConverter.WriteStringField(memStream, item.procPath);
                MsgDataConverter.WriteStringField(memStream, item.memUsage);
                MsgDataConverter.WriteStringField(memStream, item.cpuUsage);
                MsgDataConverter.WriteDateField(memStream, item.startTime);
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
            M8ProcItem item;
            procList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M8ProcItem();
                item.sessionId = MsgDataConverter.ReadInt32(memStream);
                item.pId = MsgDataConverter.ReadInt32(memStream);
                item.procName = MsgDataConverter.ReadStringField(memStream);
                item.procPath = MsgDataConverter.ReadStringField(memStream);
                item.memUsage = MsgDataConverter.ReadStringField(memStream);
                item.cpuUsage = MsgDataConverter.ReadStringField(memStream);
                item.startTime = MsgDataConverter.ReadDateField(memStream);
                procList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M8ProcItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int sessionId;

        /// <summary>
        /// 
        /// </summary>
        public int pId;

        /// <summary>
        /// 
        /// </summary>
        public string procName;

        /// <summary>
        /// 
        /// </summary>
        public string procPath;

        /// <summary>
        /// 
        /// </summary>
        public string memUsage;

        /// <summary>
        /// 
        /// </summary>
        public string cpuUsage;

        /// <summary>
        /// 
        /// </summary>
        public DateTime startTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public M8ProcItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M8ProcItem(M8ProcItem item)
        {
            pId = item.pId;
            procName = item.procName;
            procPath = item.procPath;
            memUsage = item.memUsage;
            cpuUsage = item.cpuUsage;;
            sessionId = item.sessionId;
            startTime = item.startTime;
        }
    }
}
