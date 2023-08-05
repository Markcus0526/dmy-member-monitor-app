using INMC.Communication.Inmc.Communication.EndPoints;

namespace INMC.Communication.Inmc.Client
{
    /// <summary>
    /// This class is used to create SCS Clients to connect to a SCS server.
    /// </summary>
    public static class InmcClientFactory
    {
        /// <summary>
        /// Creates a new client to connect to a server using an end point.
        /// </summary>
        /// <param name="endpoint">End point of the server to connect it</param>
        /// <returns>Created TCP client</returns>
        public static IInmcClient CreateClient(InmcEndPoint endpoint)
        {
            return endpoint.CreateClient();
        }

        /// <summary>
        /// Creates a new client to connect to a server using an end point.
        /// </summary>
        /// <param name="endpointAddress">End point address of the server to connect it</param>
        /// <returns>Created TCP client</returns>
        public static IInmcClient CreateClient(string endpointAddress)
        {
            return CreateClient(InmcEndPoint.CreateEndPoint(endpointAddress));
        }
    }
}
