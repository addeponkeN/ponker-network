namespace PonkerNetwork;

public class PacketEvent
{
    public Action<IPacket> Event;
}

public class PacketService
{
    private List<Type> _services = new();
    private Dictionary<Type, int> _hashIndexes = new();

    private PonkerNet _net;

    public PacketService(PonkerNet net)
    {
        _net = net;
        PacketListener.Init(_net);
    }

    public void Register<T>() where T : IPacket
    {
        _services.Add(typeof(T));
        _hashIndexes.Add(typeof(T), _hashIndexes.Count);
        PacketListener<T>.Init(_net);
    }

    public Type Get(int id)
    {
        return _services[id];
    }

    public int Get<T>() where T : IPacket
    {
        return _hashIndexes[typeof(T)];
    }

    private Dictionary<Type, Action<IPacket, NetPeer>> _subs = new();

    public void InvokeSub(Type packetType, IPacket packet, NetPeer netPeer)
    {
        var action = _subs[packetType];
        action.Invoke(packet, netPeer);
    }

    public void Subscribe<T>(Action<T, NetPeer> action) where T : IPacket
    {
        void Value(IPacket packet, NetPeer peer)
        {
            action((T)packet, peer);
        }

        _subs.Add(typeof(T), Value);
    }
}
