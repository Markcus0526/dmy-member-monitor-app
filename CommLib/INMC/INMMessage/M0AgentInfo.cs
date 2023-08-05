using System;
using System.Runtime.InteropServices;
using INMC.INMMessage;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// Represents a chat agent.
    /// This object particularly used in Login of a agent.
    /// </summary>
    [Serializable]
    public class M0AgentInfo : MessageBase
    {
        /// <summary>
        /// AgentId of agent.
        /// </summary>
        public string AgentId;
        /// <summary>
        /// MAC address of agent
        /// </summary>
        public string MacAddr;

        /// <summary>
        /// name of logoned local user
        /// </summary>
        public string IpAddr;

        /// <summary>
        /// name of logoned local user
        /// </summary>
        public string LogonUser;

        /// <summary>
        /// name of Computer.
        /// </summary>
        public string MachineName;

        /// <summary>
        /// name of Operating System
        /// </summary>
        public string OSName;

        /// <summary>
        /// osNumber
        /// </summary>
        public bool vistaOS;

        /// <summary>
        /// bit of Operating System
        /// </summary>
        public bool x64bitOS;

        /// <summary>
        /// 
        /// </summary>
        public M0AgentInfo()
        {
            MsgKind = MessageType.KIND.MSG0_AGENTINFO;
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
            MsgDataConverter.WriteStringField(memStream, AgentId);
            MsgDataConverter.WriteStringField(memStream, MacAddr);
            MsgDataConverter.WriteStringField(memStream, IpAddr);
            MsgDataConverter.WriteStringField(memStream, LogonUser);
            MsgDataConverter.WriteStringField(memStream, MachineName);
            MsgDataConverter.WriteStringField(memStream, OSName);
            MsgDataConverter.WriteBoolDataField(memStream, vistaOS);
            MsgDataConverter.WriteBoolDataField(memStream, x64bitOS);

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
            AgentId = MsgDataConverter.ReadStringField(memStream);
            MacAddr = MsgDataConverter.ReadStringField(memStream);
            IpAddr = MsgDataConverter.ReadStringField(memStream);
            LogonUser = MsgDataConverter.ReadStringField(memStream);
            MachineName = MsgDataConverter.ReadStringField(memStream);
            OSName = MsgDataConverter.ReadStringField(memStream);
            vistaOS = MsgDataConverter.ReadBoolDataField(memStream);
            x64bitOS = MsgDataConverter.ReadBoolDataField(memStream);
        }
    }
}
