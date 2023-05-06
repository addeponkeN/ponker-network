namespace PonkerNetwork;

public partial class PonkerNet
{
    public void Sub<T>(PacketHandler<T> packetHandler) where T : IPacket
    {
        Services.Sub(packetHandler);
    }
    
    public void UnSub<T>(PacketHandler<T> packetHandler) where T : IPacket
    {
        Services.UnSub(packetHandler);
    }
}
