using System;

namespace INMC.Communication.Inmc.Communication.Messages
{
    /// <summary>
    /// Represents a message that is sent and received by server and client.
    /// This is the base class for all messages.
    /// </summary>
    [Serializable]
    public class InmcMessage : IInmcMessage
    {
        /// <summary>
        /// Unique identified for this message.
        /// Default value: New GUID.
        /// Do not change if you do not want to do low level changes
        /// such as custom wire protocols.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// This property is used to indicate that this is
        /// a Reply message to a message.
        /// It may be null if this is not a reply message.
        /// </summary>
        public string RepliedMessageId { get; set; }

        /// <summary>
        /// Creates a new InmcMessage.
        /// </summary>
        public InmcMessage()
        {
            MessageId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new reply InmcMessage.
        /// </summary>
        /// <param name="repliedMessageId">
        /// Replied message id if this is a reply for
        /// a message.
        /// </param>
        public InmcMessage(string repliedMessageId)
            : this()
        {
            RepliedMessageId = repliedMessageId;
        }

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? string.Format("InmcMessage [{0}]", MessageId)
                       : string.Format("InmcMessage [{0}] Replied To [{1}]", MessageId, RepliedMessageId);
        }
    }
}
