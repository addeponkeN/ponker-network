using System.Linq.Expressions;

namespace PonkerNetwork;

public delegate void PacketHandler<in T>(T packet, NetPeer sender) where T : IPacket;

internal interface IPacketSubscriber
{
    void Trigger(IPacket packet, NetPeer peer);
    void Clear();
}

internal class PacketSubscription<T> : IPacketSubscriber where T : IPacket
{
    private event PacketHandler<T> Event;

    public void Add(PacketHandler<T> packetHandler)
    {
        Event += packetHandler;
    }

    /// <summary>
    /// Removes 'packetHandler' from the internal Event and returns the status of the Event.
    /// </summary>
    /// <returns>True if event is null</returns>
    public bool Remove(PacketHandler<T> packetHandler)
    {
        Event -= packetHandler;
        return Event == null;
    }

    public void Trigger(IPacket packet, NetPeer peer)
    {
        Event.Invoke((T)packet, peer);
    }

    public void Clear()
    {
        Event = null;
    }
}

public class PacketService
{
    private List<Type> _services = new();

    private Dictionary<Type, int> _hashIndexes = new();

    // private Dictionary<Type, PacketHandler<IPacket>> _subs = new();
    private Dictionary<Type, IPacketSubscriber> _subscribers = new();

    private Dictionary<Type, Func<IPacket>> _compiledPacketConstructors;
    private Dictionary<int, Func<IPacket>> _compiledPacketConstructorsId;

    private PonkerNet _net;

    public PacketService(PonkerNet net)
    {
        _net = net;
        RegisterAllPackets();
    }

    private void RegisterAllPackets()
    {
        _compiledPacketConstructors = new();
        _compiledPacketConstructorsId = new();

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(y => y.IsValueType && typeof(IPacket).IsAssignableFrom(y));

        int i = 0;
        foreach(var type in types)
        {
            var ctor = Expression.New(type);
            var convertExpr = Expression.Convert(ctor, typeof(IPacket));
            var lambda = Expression.Lambda<Func<IPacket>>(convertExpr);
            var expr = lambda.Compile();
            _compiledPacketConstructors.Add(type, expr);
            _compiledPacketConstructorsId.Add(i++, expr);
            Register(type);
        }
    }

    private void Register(Type type)
    {
        _services.Add(type);
        _hashIndexes.Add(type, _hashIndexes.Count);
    }

    public void Register<T>() where T : IPacket
    {
        _services.Add(typeof(T));
        _hashIndexes.Add(typeof(T), _hashIndexes.Count);
    }

    public void Sub<T>(PacketHandler<T> packetHandler) where T : IPacket
    {
        Type type = typeof(T);
        if(!_subscribers.TryGetValue(type, out var sub))
        {
            sub = new PacketSubscription<T>();
            _subscribers.Add(type, sub);
        }

        (sub as PacketSubscription<T>).Add(packetHandler);
    }

    public void UnSub<T>(PacketHandler<T> packetHandler) where T : IPacket
    {
        Type type = typeof(T);
        if(_subscribers.TryGetValue(type, out var sub))
        {
            var packetSub = (sub as PacketSubscription<T>);
            if(packetSub.Remove(packetHandler))
            {
                _subscribers.Remove(type);
            }
        }
    }

    public void Trigger(Type type, IPacket packet, NetPeer peer)
    {
        if(_subscribers.TryGetValue(type, out var sub))
        {
            sub.Trigger(packet, peer);
        }
    }

    public void ClearSubscriptions()
    {
        foreach(var sub in _subscribers.Values)
        {
            sub.Clear();
        }
        
        _subscribers.Clear();
    }

    internal IPacket CreatePacket(int id)
    {
        return _compiledPacketConstructorsId[id]();
    }

    internal Type Get(int id)
    {
        return _services[id];
    }

    internal int Get<T>() where T : IPacket
    {
        return _hashIndexes[typeof(T)];
    }
}