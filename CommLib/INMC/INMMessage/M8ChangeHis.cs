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
    public class M8ChangeHis : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string strComputerName;

        /// <summary>
        /// 
        /// </summary>
        public string strDomainName;

        /// <summary>
        /// 
        /// </summary>
        public string strJoinStatus;

        /// <summary>
        /// 
        /// </summary>
        public M8ChangeHis()
        {
            MsgKind = MessageType.KIND.MSG8_CHANGEHIS;
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
            MsgDataConverter.WriteStringField(memStream, strComputerName);
            MsgDataConverter.WriteStringField(memStream, strDomainName);
            MsgDataConverter.WriteStringField(memStream, strJoinStatus);
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
            strComputerName = MsgDataConverter.ReadStringField(memStream);
            strDomainName = MsgDataConverter.ReadStringField(memStream);
            strJoinStatus = MsgDataConverter.ReadStringField(memStream);
        }
    }
}
