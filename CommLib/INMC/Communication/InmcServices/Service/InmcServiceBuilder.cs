using INMC.Communication.Inmc.Communication.EndPoints;
using INMC.Communication.Inmc.Server;

namespace INMC.Communication.InmcServices.Service
{
    /// <summary>
    /// This class is used to build InmcService applications.
    /// </summary>
    public static class InmcServiceBuilder
    {
        /// <summary>
        /// Creates a new SCS Service application using an EndPoint.
        /// </summary>
        /// <param name="endPoint">EndPoint that represents address of the service</param>
        /// <returns>Created SCS service application</returns>
        public static IInmcServiceApplication CreateService(InmcEndPoint endPoint)
        {
            return new InmcServiceApplication(InmcServerFactory.CreateServer(endPoint));
        }
    }
}
