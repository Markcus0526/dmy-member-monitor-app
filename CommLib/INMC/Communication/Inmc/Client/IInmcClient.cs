using INMC.Communication.Inmc.Communication;
using INMC.Communication.Inmc.Communication.Messengers;

namespace INMC.Communication.Inmc.Client
{
    /// <summary>
    /// Represents a client to connect to server.
    /// </summary>
    public interface IInmcClient : IMessenger, IConnectableClient
    {
        //Does not define any additional member
    }
}
