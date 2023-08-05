using System;
using System.Runtime.Remoting.Proxies;
using INMC.Communication.Inmc.Communication;
using INMC.Communication.Inmc.Communication.EndPoints;
using INMC.Communication.Inmc.Communication.Messengers;
using INMC.Communication.Inmc.Server;
using INMC.Communication.InmcServices.Communication;

namespace INMC.Communication.InmcServices.Service
{
    /// <summary>
    /// Implements IInmcServiceClient.
    /// It is used to manage and monitor a service client.
    /// </summary>
    internal class InmcServiceClient : IInmcServiceClient
    {
        #region Public events

        /// <summary>
        /// This event is raised when this client is disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        #endregion

        #region Public properties

        /// <summary>
        /// Unique identifier for this client.
        /// </summary>
        public long ClientId
        {
            get { return _serverClient.ClientId; }
        }

        ///<summary>
        /// Gets endpoint of remote application.
        ///</summary>
        public InmcEndPoint RemoteEndPoint
        {
            get { return _serverClient.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets the communication state of the Client.
        /// </summary>
        public CommunicationStates CommunicationState
        {
            get
            {
                return _serverClient.CommunicationState;
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to underlying IInmcServerClient object.
        /// </summary>
        private readonly IInmcServerClient _serverClient;

        /// <summary>
        /// This object is used to send messages to client.
        /// </summary>
        private readonly RequestReplyMessenger<IInmcServerClient> _requestReplyMessenger;

        /// <summary>
        /// Last created proxy object to invoke remote medhods.
        /// </summary>
        private RealProxy _realProxy;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new InmcServiceClient object.
        /// </summary>
        /// <param name="serverClient">Reference to underlying IInmcServerClient object</param>
        /// <param name="requestReplyMessenger">RequestReplyMessenger to send messages</param>
        public InmcServiceClient(IInmcServerClient serverClient, RequestReplyMessenger<IInmcServerClient> requestReplyMessenger)
        {
            _serverClient = serverClient;
            _serverClient.Disconnected += Client_Disconnected;
            _requestReplyMessenger = requestReplyMessenger;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Closes client connection.
        /// </summary>
        public void Disconnect()
        {
            _serverClient.Disconnect();
        }

        /// <summary>
        /// Gets the client proxy interface that provides calling client methods remotely.
        /// </summary>
        /// <typeparam name="T">Type of client interface</typeparam>
        /// <returns>Client interface</returns>
        public T GetClientProxy<T>() where T : class
        {
            _realProxy = new RemoteInvokeProxy<T, IInmcServerClient>(_requestReplyMessenger);
            return (T)_realProxy.GetTransparentProxy();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles disconnect event of _serverClient object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            _requestReplyMessenger.Stop();
            OnDisconnected();
        }

        #endregion
        
        #region Event raising methods

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        private void OnDisconnected()
        {
            var handler = Disconnected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
