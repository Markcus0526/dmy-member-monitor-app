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
    public class M1ProhibitSet : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public bool bEnable = true;

        /// <summary>
        /// 
        /// </summary>
        public double upSpeed = 100.0;        // Reset with Default BandWidth Value

        /// <summary>
        /// 
        /// </summary>
        public double downSpeed = 100.0;        // Reset with Default BandWidth Value

        /// <summary>
        /// 
        /// </summary>
        public M1ProhibitSet()
        {
            MsgKind = MessageType.KIND.MSG1_PROHIBITSET;
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
            MsgDataConverter.WriteBoolDataField(memStream, bEnable);
            MsgDataConverter.WriteDoubleField(memStream, upSpeed);
            MsgDataConverter.WriteDoubleField(memStream, downSpeed);
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
            bEnable = MsgDataConverter.ReadBoolDataField(memStream);
            upSpeed = MsgDataConverter.ReadDoubleField(memStream);
            downSpeed = MsgDataConverter.ReadDoubleField(memStream);
        }
    }
}
