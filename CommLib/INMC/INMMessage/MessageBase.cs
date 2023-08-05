using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INMC.INMMessage;
using System.IO;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
         public MessageType.TYPE MsgType;

        /// <summary>
        /// 
        /// </summary>
         public MessageType.KIND MsgKind;

         /// <summary>
         /// 
         /// </summary>
         public DateTime msgTime;

         //********************************************
         //ADDED BY JUJ. 2012.10.4.
         //********************************************
         /// <summary>
         /// 
         /// </summary>
         public MemoryStream memStream;

         /// <summary>
         /// Make byte array of inmc message
         /// </summary>
         public virtual byte[] GetByteArray()
         {
             memStream = new MemoryStream();
             memStream.WriteByte((byte)MsgType);
             memStream.WriteByte((byte)MsgKind);
             MsgDataConverter.WriteDateField(memStream, msgTime);
             return null;
         }
         
         /// <summary>
         /// initialize message field from byte array
         /// </summary>
         public virtual void Initialize(byte[] byteArray)
         {
             memStream = new MemoryStream(byteArray);
             MsgType = (MessageType.TYPE)memStream.ReadByte();
             MsgKind = (MessageType.KIND)memStream.ReadByte();
             msgTime = MsgDataConverter.ReadDateField(memStream);
         }
    }
}
