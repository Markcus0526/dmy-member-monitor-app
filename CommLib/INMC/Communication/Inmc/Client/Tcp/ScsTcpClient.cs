using INMC.Communication.Inmc.Communication.Channels;
using INMC.Communication.Inmc.Communication.Channels.Tcp;
using INMC.Communication.Inmc.Communication.EndPoints.Tcp;
using System.Net;
using System.Net.Sockets;

namespace INMC.Communication.Inmc.Client.Tcp
{
    /// <summary>
    /// This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    internal class InmcTcpClient : InmcClientBase
    {
        /// <summary>
        /// The endpoint address of the server.
        /// </summary>
        private readonly InmcTcpEndPoint _serverEndPoint;

        /// <summary>
        /// Creates a new InmcTcpClient object.
        /// </summary>
        /// <param name="serverEndPoint">The endpoint address to connect to the server</param>
        public InmcTcpClient(InmcTcpEndPoint serverEndPoint)
        {
            _serverEndPoint = serverEndPoint;
        }

        /// <summary>
        /// Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            Socket sock = TcpHelper.ConnectToServer(
                     new IPEndPoint(IPAddress.Parse(_serverEndPoint.IpAddress), _serverEndPoint.TcpPort),
                     ConnectTimeout
                     );

            if (sock != null)
                return new TcpCommunicationChannel(sock);
            else
                return null;
        }
    }
}
