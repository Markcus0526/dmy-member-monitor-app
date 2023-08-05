using INMC.Communication.Inmc.Communication.EndPoints;

namespace INMC.Communication.Inmc.Server
{
    /// <summary>
    /// This class is used to create SCS servers.
    /// </summary>
    public static class InmcServerFactory
    {
        /// <summary>
        /// Creates a new SCS Server using an EndPoint.
        /// </summary>
        /// <param name="endPoint">Endpoint that represents address of the server</param>
        /// <returns>Created TCP server</returns>
        public static IInmcServer CreateServer(InmcEndPoint endPoint)
        {
            return endPoint.CreateServer();
        }
    }
}
