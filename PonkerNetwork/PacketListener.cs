namespace PonkerNetwork;

public class PacketListener<T> where T : IPacket
{
    public static event Action<T> OnReceived;
    
    private static PonkerNet _net;
    
    internal static void Init(PonkerNet net)
    {
        _net = net;
    }
}

public static class PacketListener
{
    public static event Action<IPacket> OnReceived;
    
    private static PonkerNet _net;
    
    internal static void Init(PonkerNet net)
    {
        _net = net;
    }

    public static void Trigger(IPacket packet)
    {
        OnReceived.Invoke(packet);
    }
}