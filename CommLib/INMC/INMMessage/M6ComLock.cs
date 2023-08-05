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
    public class M6ComLock : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public bool bLockKeybd = false;

        /// <summary>
        /// 
        /// </summary>
        public bool bLockMouse = false;

        /// <summary>
        /// 
        /// </summary>
        public bool bComRestart = false;

        /// <summary>
        /// 
        /// </summary>
        public bool bComShutdown = false;

        /// <summary>
        /// 
        /// </summary>
        public M6ComLock()
        {
            MsgKind = MessageType.KIND.MSG6_COMLOCK;
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
            MsgDataConverter.WriteBoolDataField(memStream, bLockKeybd);
            MsgDataConverter.WriteBoolDataField(memStream, bLockMouse);
            MsgDataConverter.WriteBoolDataField(memStream, bComRestart);
            MsgDataConverter.WriteBoolDataField(memStream, bComShutdown);
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
            bLockKeybd = MsgDataConverter.ReadBoolDataField(memStream);
            bLockMouse = MsgDataConverter.ReadBoolDataField(memStream);
            bComRestart = MsgDataConverter.ReadBoolDataField(memStream);
            bComShutdown = MsgDataConverter.ReadBoolDataField(memStream);
        }
    }
}
