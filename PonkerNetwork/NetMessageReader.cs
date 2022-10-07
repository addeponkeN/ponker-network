namespace PonkerNetwork;

public class NetMessageReader : NetMessage
{
    internal NetMessageReader(OmegaNet net, byte[] messageBuffer, byte[] writeBuffer) 
        : base(net, messageBuffer, writeBuffer)
    {
        
    }
}