using System;
using System.Runtime.Serialization;

namespace INMC.Communication.InmcServices.Communication.Messages
{
    /// <summary>
    /// Represents a SCS Remote Exception.
    /// This exception is used to send an exception from an application to another application.
    /// </summary>
    [Serializable]
    public class InmcRemoteException : Exception
    {
        /// <summary>
        /// Contstructor.
        /// </summary>
        public InmcRemoteException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        public InmcRemoteException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
            
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public InmcRemoteException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public InmcRemoteException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
