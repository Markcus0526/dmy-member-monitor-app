using System;
using INMC.Communication.Inmc.Communication.EndPoints;
using INMC.Communication.Inmc.Communication.Messengers;

namespace INMC.Communication.Inmc.Communication.Channels
{
    /// <summary>
    /// Represents a communication channel.
    /// A communication channel is used to communicate (send/receive messages) with a remote application.
    /// </summary>
    internal interface ICommunicationChannel : IMessenger
    {
        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        event EventHandler Disconnected;

        ///<summary>
        /// Gets endpoint of remote application.
        ///</summary>
        InmcEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the current communication state.
        /// </summary>
        CommunicationStates CommunicationState { get; }

        /// <summary>
        /// Starts the communication with remote application.
        /// </summary>
        void Start();

        /// <summary>
        /// Closes messenger.
        /// </summary>
        void Disconnect();
    }
}
