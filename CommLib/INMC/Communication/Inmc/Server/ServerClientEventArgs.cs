using System;

namespace INMC.Communication.Inmc.Server
{
    /// <summary>
    /// Stores client information to be used by an event.
    /// </summary>
    public class ServerClientEventArgs : EventArgs
    {
        /// <summary>
        /// Client that is associated with this event.
        /// </summary>
        public IInmcServerClient Client { get; private set; }

        /// <summary>
        /// Creates a new ServerClientEventArgs object.
        /// </summary>
        /// <param name="client">Client that is associated with this event</param>
        public ServerClientEventArgs(IInmcServerClient client)
        {
            Client = client;
        }
    }
}
