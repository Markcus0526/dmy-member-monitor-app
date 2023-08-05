namespace INMC.Communication.Inmc.Communication.Protocols.BinarySerialization
{
    /// <summary>
    /// This class is used to create Binary Serialization Protocol objects.
    /// </summary>
    public class BinarySerializationProtocolFactory : IInmcWireProtocolFactory
    {
        /// <summary>
        /// Creates a new Wire Protocol object.
        /// </summary>
        /// <returns>Newly created wire protocol object</returns>
        public IInmcWireProtocol CreateWireProtocol()
        {
            return new BinarySerializationProtocol();
        }
    }
}
