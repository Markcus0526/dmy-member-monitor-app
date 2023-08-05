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
    public class M6ProcMon : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public int Interval;

        /// <summary>
        /// 
        /// </summary>
        public ArrayList procMonList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M6ProcMon()
        {
            MsgKind = MessageType.KIND.MSG6_PROCMON;
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
            M6ProcMonItem item;
            MsgDataConverter.WriteLenField(memStream, procMonList.Count);
            for (int i = 0; i < procMonList.Count; i++)
            {
                item = (M6ProcMonItem)procMonList[i];
                MsgDataConverter.WriteInt32(memStream, item.sessionId);
                MsgDataConverter.WriteInt32(memStream, item.pId);
                MsgDataConverter.WriteStringField(memStream, item.strProcName);
                MsgDataConverter.WriteStringField(memStream, item.strProcPath);
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
            Interval = MsgDataConverter.ReadInt32(memStream);
            M6ProcMonItem item;
            procMonList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M6ProcMonItem();
                item.sessionId = MsgDataConverter.ReadInt32(memStream);
                item.pId = MsgDataConverter.ReadInt32(memStream);
                item.strProcName = MsgDataConverter.ReadStringField(memStream);
                item.strProcPath = MsgDataConverter.ReadStringField(memStream);
                item.startTime = MsgDataConverter.ReadDateField(memStream);
                procMonList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M6ProcMonItem
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
        public string strProcName = "";

        /// <summary>
        /// 
        /// </summary>
        public string strProcPath = "";

        /// <summary>
        /// 
        /// </summary>
        public DateTime startTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public M6ProcMonItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M6ProcMonItem(M6ProcMonItem item)
        {
            sessionId = item.sessionId;
            pId = item.pId;
            strProcName = item.strProcName;
            strProcPath = item.strProcPath;
            startTime = item.startTime;
        }
    }
}
