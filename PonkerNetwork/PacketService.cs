namespace PonkerNetwork;

public class PacketEvent
{
    public Action<IPacket> Event;
}

public class PacketService
{
    private List<Type> _services = new();
    private Dictionary<Type, int> _hashIndexes = new();

    private OmegaNet _net;

    public PacketService(OmegaNet net)
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

    private Dictionary<Type, Action<IPacket>> _subs = new();

    public void InvokeSub(Type packetType, IPacket packet)
    {
        var action = _subs[packetType];
        action.Invoke(packet);
    }

    public void Subscribe<T>(Action<T> action) where T : IPacket
    {
        void Value(IPacket packet)
        {
            action((T)packet);
        }

        _subs.Add(typeof(T), Value);
    }
}

public interface IPacket
{
    void Write(NetMessageWriter writer);
    void Read(NetMessageReader reader);
}

public struct PingPacket : IPacket
{
    public void Write(NetMessageWriter writer)
    {
    }

    public void Read(NetMessageReader reader)
    {
    }
}