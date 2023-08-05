using System;
using INMC.Communication.Inmc.Communication;
using INMC.Communication.Inmc.Communication.EndPoints;
using INMC.Communication.Inmc.Communication.Messengers;

namespace INMC.Communication.Inmc.Server
{
    /// <summary>
    /// Represents a client from a perspective of a server.
    /// </summary>
    public interface IInmcServerClient : IMessenger
    {
        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        event EventHandler Disconnected;
        
        /// <summary>
        /// Unique identifier for this client in server.
        /// </summary>
        long ClientId { get; }

        ///<summary>
        /// Gets endpoint of remote application.
        ///</summary>
        InmcEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the current communication state.
        /// </summary>
        CommunicationStates CommunicationState { get; }

        /// <summary>
        /// Disconnects from server.
        /// </summary>
        void Disconnect();
    }
}
