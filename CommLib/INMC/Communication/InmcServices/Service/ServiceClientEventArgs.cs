using System;

namespace INMC.Communication.InmcServices.Service
{
    /// <summary>
    /// Stores service client informations to be used by an event.
    /// </summary>
    public class ServiceClientEventArgs : EventArgs
    {
        /// <summary>
        /// Client that is associated with this event.
        /// </summary>
        public IInmcServiceClient Client { get; private set; }

        /// <summary>
        /// Creates a new ServiceClientEventArgs object.
        /// </summary>
        /// <param name="client">Client that is associated with this event</param>
        public ServiceClientEventArgs(IInmcServiceClient client)
        {
            Client = client;
        }
    }
}
