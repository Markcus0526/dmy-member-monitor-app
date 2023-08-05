namespace INMC.Communication.Inmc.Communication.Protocols
{
    ///<summary>
    /// Defines a Wire Protocol Factory class that is used to create Wire Protocol objects.
    ///</summary>
    public interface IInmcWireProtocolFactory
    {
        /// <summary>
        /// Creates a new Wire Protocol object.
        /// </summary>
        /// <returns>Newly created wire protocol object</returns>
        IInmcWireProtocol CreateWireProtocol();
    }
}
