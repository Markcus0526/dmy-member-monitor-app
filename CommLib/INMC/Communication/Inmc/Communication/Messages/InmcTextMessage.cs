using System;

namespace INMC.Communication.Inmc.Communication.Messages
{
    /// <summary>
    /// This message is used to send/receive a text as message data.
    /// </summary>
    [Serializable]
    public class InmcTextMessage : InmcMessage
    {
        /// <summary>
        /// Message text that is being transmitted.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Creates a new InmcTextMessage object.
        /// </summary>
        public InmcTextMessage()
        {
            
        }

        /// <summary>
        /// Creates a new InmcTextMessage object with Text property.
        /// </summary>
        /// <param name="text">Message text that is being transmitted</param>
        public InmcTextMessage(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Creates a new reply InmcTextMessage object with Text property.
        /// </summary>
        /// <param name="text">Message text that is being transmitted</param>
        /// <param name="repliedMessageId">
        /// Replied message id if this is a reply for
        /// a message.
        /// </param>
        public InmcTextMessage(string text, string repliedMessageId)
            : this(text)
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
                       ? string.Format("InmcTextMessage [{0}]: {1}", MessageId, Text)
                       : string.Format("InmcTextMessage [{0}] Replied To [{1}]: {2}", MessageId, RepliedMessageId, Text);
        }
    }
}
