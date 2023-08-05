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
    public class M3FirewallSet : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public bool bDomain = false;

        /// <summary>
        /// 
        /// </summary>
        public bool bPublic = false;

        /// <summary>
        /// 
        /// </summary>
        public bool bPrivate = false;

        /// <summary>
        /// 
        /// </summary>
        public M3FirewallSet()
        {
            MsgKind = MessageType.KIND.MSG3_PORTMON;
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
            MsgDataConverter.WriteBoolDataField(memStream, bDomain);
            MsgDataConverter.WriteBoolDataField(memStream, bPublic);
            MsgDataConverter.WriteBoolDataField(memStream, bPrivate);
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
            bDomain = MsgDataConverter.ReadBoolDataField(memStream);
            bPublic = MsgDataConverter.ReadBoolDataField(memStream);
            bPrivate = MsgDataConverter.ReadBoolDataField(memStream);
        }
    }
}
