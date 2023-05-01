using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

public class PacketService
{
    private List<Type> _services = new();
    private Dictionary<Type, int> _hashIndexes = new();
    private Dictionary<Type, Action<IPacket, NetPeer>> _subs = new();

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

    public void Subscribe<T>(Action<T, NetPeer> action) where T : IPacket
    {
        void Value(IPacket packet, NetPeer peer)
        {
            action((T)packet, peer);
        }

        _subs.Add(typeof(T), Value);
    }
    
    public bool Unsubscribe<T>() where T : IPacket
    {
        return _subs.Remove(typeof(T));
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

    internal void TriggerPacket(Type packetType, IPacket packet, NetPeer netPeer)
    {
        var action = _subs[packetType];
        action.Invoke(packet, netPeer);
    }

    public void ClearSubscriptions()
    {
        _subs.Clear();
    }
}