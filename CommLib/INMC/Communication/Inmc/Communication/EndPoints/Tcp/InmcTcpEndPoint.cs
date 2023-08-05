using System;
using INMC.Communication.Inmc.Client;
using INMC.Communication.Inmc.Client.Tcp;
using INMC.Communication.Inmc.Server;
using INMC.Communication.Inmc.Server.Tcp;

namespace INMC.Communication.Inmc.Communication.EndPoints.Tcp
{
    /// <summary>
    /// Represens a TCP end point in SCS.
    /// </summary>
    public sealed class InmcTcpEndPoint : InmcEndPoint
    {
        ///<summary>
        /// IP address of the server.
        ///</summary>
        public string IpAddress { get; set; }

        ///<summary>
        /// Listening TCP Port for incoming connection requests on server.
        ///</summary>
        public int TcpPort { get; private set; }

        /// <summary>
        /// Creates a new InmcTcpEndPoint object with specified port number.
        /// </summary>
        /// <param name="tcpPort">Listening TCP Port for incoming connection requests on server</param>
        public InmcTcpEndPoint(int tcpPort)
        {
            TcpPort = tcpPort;
        }

        /// <summary>
        /// Creates a new InmcTcpEndPoint object with specified IP address and port number.
        /// </summary>
        /// <param name="ipAddress">IP address of the server</param>
        /// <param name="port">Listening TCP Port for incoming connection requests on server</param>
        public InmcTcpEndPoint(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            TcpPort = port;
        }
        
        /// <summary>
        /// Creates a new InmcTcpEndPoint from a string address.
        /// Address format must be like IPAddress:Port (For example: 127.0.0.1:10085).
        /// </summary>
        /// <param name="address">TCP end point Address</param>
        /// <returns>Created InmcTcpEndpoint object</returns>
        public InmcTcpEndPoint(string address)
        {
            var splittedAddress = address.Trim().Split(':');
            IpAddress = splittedAddress[0].Trim();
            TcpPort = Convert.ToInt32(splittedAddress[1].Trim());
        }

        /// <summary>
        /// Creates a Inmc Server that uses this end point to listen incoming connections.
        /// </summary>
        /// <returns>Inmc Server</returns>
        internal override IInmcServer CreateServer()
        {
            return new InmcTcpServer(this);
        }

        /// <summary>
        /// Creates a Inmc Client that uses this end point to connect to server.
        /// </summary>
        /// <returns>Inmc Client</returns>
        internal override IInmcClient CreateClient()
        {
            return new InmcTcpClient(this);
        }

        /// <summary>
        /// Generates a string representation of this end point object.
        /// </summary>
        /// <returns>String representation of this end point object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(IpAddress) ? ("tcp://" + TcpPort) : ("tcp://" + IpAddress + ":" + TcpPort);
        }
    }
}