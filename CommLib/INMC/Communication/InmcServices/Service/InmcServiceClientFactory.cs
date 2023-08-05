using INMC.Communication.Inmc.Communication;
using INMC.Communication.Inmc.Communication.Messengers;
using INMC.Communication.Inmc.Server;

namespace INMC.Communication.InmcServices.Service
{
    /// <summary>
    /// This class is used to create service client objects that is used in server-side.
    /// </summary>
    internal static class InmcServiceClientFactory
    {
        /// <summary>
        /// Creates a new service client object that is used in server-side.
        /// </summary>
        /// <param name="serverClient">Underlying server client object</param>
        /// <param name="requestReplyMessenger">RequestReplyMessenger object to send/receive messages over serverClient</param>
        /// <returns></returns>
        public static IInmcServiceClient CreateServiceClient(IInmcServerClient serverClient, RequestReplyMessenger<IInmcServerClient> requestReplyMessenger)
        {
            return new InmcServiceClient(serverClient, requestReplyMessenger);
        }
    }
}
