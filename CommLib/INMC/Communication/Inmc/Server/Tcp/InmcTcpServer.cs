using INMC.Communication.Inmc.Communication.Channels;
using INMC.Communication.Inmc.Communication.Channels.Tcp;
using INMC.Communication.Inmc.Communication.EndPoints.Tcp;

namespace INMC.Communication.Inmc.Server.Tcp
{
    /// <summary>
    /// This class is used to create a TCP server.
    /// </summary>
    internal class InmcTcpServer : InmcServerBase
    {
        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly InmcTcpEndPoint _endPoint;

        /// <summary>
        /// Creates a new InmcTcpServer object.
        /// </summary>
        /// <param name="endPoint">The endpoint address of the server to listen incoming connections</param>
        public InmcTcpServer(InmcTcpEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        /// <summary>
        /// Creates a TCP connection listener.
        /// </summary>
        /// <returns>Created listener object</returns>
        protected override IConnectionListener CreateConnectionListener()
        {
            return new TcpConnectionListener(_endPoint);
        }
    }
}
