using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using INMC.INMMessage;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M4RealScreenMon : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public Image scrImg;

        /// <summary>
        /// 
        /// </summary>
        public M4RealScreenMon()
        {
            MsgKind = MessageType.KIND.MSG4_REALSCREENMON;
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
        }
    }
}
