using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M6ProcAction : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public int Action = 0;

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
        public M6ProcAction()
        {
            MsgKind = MessageType.KIND.MSG6_PROCACT;
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
            MsgDataConverter.WriteInt32(memStream, Action);
            MsgDataConverter.WriteInt32(memStream, sessionId);
            MsgDataConverter.WriteInt32(memStream, pId);
            MsgDataConverter.WriteStringField(memStream, strProcName);
            MsgDataConverter.WriteStringField(memStream, strProcPath);
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
            Action = MsgDataConverter.ReadInt32(memStream);
            sessionId = MsgDataConverter.ReadInt32(memStream);
            pId = MsgDataConverter.ReadInt32(memStream);
            strProcName = MsgDataConverter.ReadStringField(memStream);
            strProcPath = MsgDataConverter.ReadStringField(memStream);
        }
    }
}
