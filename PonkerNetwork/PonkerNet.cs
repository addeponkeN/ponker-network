using System.Net;
using System.Net.Sockets;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

struct NetHeader
{
    // public ushort ArrSize;
    // public byte DataType;
}

public enum NetStatusTypes
{
    None,
    Connected,
    Unconnected,
    Connecting,
    Handshaking,
    Disconnected,
    Disconnecting,
    Host,
}

public class PonkerNet
{
    // internal static readonly unsafe byte HeaderSize = (byte)(sizeof(NetHeader));
    internal static readonly byte HeaderSize = 0;

    public string Name;

    public NetConfig Config;
    public PacketService Services;

    internal Socket socket;

    private byte[] _messageBuffer;
    private byte[] _buffer;
    private EndPoint _ep;
    private ArraySegment<byte> _bufferSeg;
    private Dictionary<EndPoint, NetPeer> _acceptedPeers = new();
    private List<NetPeer> _acceptedPeersList = new();

    /// <summary>
    /// peers that you are connecting to
    /// </summary>
    private List<NetPeer> _peersToConnectTo = new();

    private NetMessageReader _reader;

    public event OnConnectedEvent OnConnectedEvent;
    public event OnConnectionAccepted OnConnectionAccepted;

    private NetStatusTypes _netStatus;

    public NetStatusTypes NetStatus
    {
        get => _netStatus;
        internal set
        {
            if(_netStatus != value)
            {
                Log.D($"NetStatus: {_netStatus}");
                _netStatus = value;
            }
        }
    }

    public PonkerNet(NetConfig cfg)
    {
        Config = cfg;
        Services = new(this);
        RegisterBaseServices();
        NetStatus = NetStatusTypes.Disconnected;
    }

    private void RegisterBaseServices()
    {
        // Services.Register<IPacket.PingPacket>();
    }

    public NetMessageWriter CreateMessage()
    {
        return new NetMessageWriter(this, _buffer, _messageBuffer);
    }

    internal NetMessageWriter CreateHandshakeMessage()
    {
        return new NetMessageWriter(this, _buffer, _messageBuffer);
    }

    /// <summary>
    /// </summary>
    /// <param name="port">Listen port</param>
    public void Start(int port = 0)
    {
        _buffer = new byte[Config.BufferSize];
        _messageBuffer = new byte[1024];
        _bufferSeg = new(_buffer);

        _reader = new NetMessageReader(this, _buffer, _messageBuffer);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);

        //  client returns
        if(port <= 0)
            return;

        //  server
        NetStatus = NetStatusTypes.Host;
        _ep = new IPEndPoint(IPAddress.Any, port);
        socket.Bind(_ep);
        // StartListen();
    }

    public async Task Connect(string ip, int port, string secret)
    {
        await Connect(IPAddress.Parse(ip), port, secret);
    }

    public async Task Connect(IPAddress ip, int port, string secret)
    {
        _ep = new IPEndPoint(ip, port);

        NetStatus = NetStatusTypes.Connecting;

        var peer = new NetPeer((IPEndPoint)_ep);
        _peersToConnectTo.Add(peer);

        ConnectToPeer(peer);
    }

    private bool ConnectToPeer(NetPeer peer)
    {
        if(NetStatus == NetStatusTypes.Unconnected ||
           NetStatus == NetStatusTypes.Connected)
            return true;

        Log.D($"Connecting to '{peer.EndPoint}'...");
        socket.BeginConnect(peer.EndPoint, res =>
        {
            socket.EndConnect(res);

            bool connectStatus = socket.Poll(1000, SelectMode.SelectRead);
            bool available = socket.Available == 0;

            if(connectStatus && available)
            {
                NetStatus = NetStatusTypes.Connecting;
            }
            else
            {
                NetStatus = NetStatusTypes.Unconnected;
            }
        }, null);

        return NetStatus == NetStatusTypes.Unconnected ||
               NetStatus == NetStatusTypes.Connected;
    }

    private void OnConnectedToPeer(NetPeer peer)
    {
        OnConnectedEvent?.Invoke(peer);
        var msg = CreateHandshakeMessage();
        msg.Recycle(0);
        msg.Write((byte)UnconnectedMessageTypes.HandshakeRequest);
        msg.Write(Config.Secret);
        SendUnconnected(msg);
    }

    public async Task ReadMessagesAsync()
    {
        for(int i = 0; i < _peersToConnectTo.Count; i++)
        {
            var conPeer = _peersToConnectTo[i];
            if(ConnectToPeer(conPeer))
            {
                OnConnectedToPeer(conPeer);
                _peersToConnectTo.RemoveAt(i--);
            }
        }

        SocketReceiveMessageFromResult res;

        res = await socket.ReceiveMessageFromAsync(_bufferSeg, SocketFlags.None, _ep);

        if(res.ReceivedBytes == 0)
        {
            return;
        }

        if(IsPeerAccepted(res.RemoteEndPoint, out NetPeer peer))
        {
            Log.D($"[{Name}]Received '{res.ReceivedBytes}' connected bytes");
            ReadConnectedData(res, peer);
        }
        else
        {
            Log.D($"[{Name}]Received '{res.ReceivedBytes}' unconnected bytes");
            await ReadUnconnectedData(res);
        }
    }

    private NetPeer AcceptPeer(EndPoint ep)
    {
        var peer = new NetPeer((IPEndPoint)ep);
        _acceptedPeers.Add(peer.EndPoint, peer);
        _acceptedPeersList.Add(peer);
        return peer;
    }

    private bool IsPeerAccepted(EndPoint endPoint)
        => _acceptedPeers.ContainsKey(endPoint);

    private bool IsPeerAccepted(EndPoint endPoint, out NetPeer peer)
        => _acceptedPeers.TryGetValue(endPoint, out peer!);

    private void ReadConnectedData(SocketReceiveMessageFromResult res, NetPeer peer)
    {
        int receivedBytes = res.ReceivedBytes;

        _reader.PrepareRead(_buffer, receivedBytes);

        while(_reader.Current < receivedBytes)
        {
            IPacket packet = _reader.ReadPacket(out Type packetType);
            Services.TriggerPacket(packetType, packet, peer);
        }
    }

    private async Task ReadUnconnectedData(SocketReceiveMessageFromResult res)
    {
        Console.WriteLine("received unconnected data");

        _reader.PrepareRead(_buffer, res.ReceivedBytes);
        var unconnectedMessageType = (UnconnectedMessageTypes)_reader.ReadByte(); //_buffer[0];

        Log.D(Name);

        switch(unconnectedMessageType)
        {
            case UnconnectedMessageTypes.HandshakeRequest:
            {
                _reader.ReadString(out string secret);

                var ep = (IPEndPoint)res.RemoteEndPoint;
                if(!secret.Equals(Config.Secret))
                {
                    Log.D($"Handshake Request failed - secret mismatch ({ep})");
                    break;
                }

                Log.D($"Handshake Request success ({ep})");

                //  respond to handshake
                var msg = CreateHandshakeMessage();
                msg.Recycle(0);
                msg.Write((byte)UnconnectedMessageTypes.HandshakeResponse);
                msg.Write(secret);

                var peer = AcceptPeer(res.RemoteEndPoint);

                OnConnectedEvent?.Invoke(peer);

                await SendToUnconnected(msg, res.RemoteEndPoint);

                break;
            }

            case UnconnectedMessageTypes.HandshakeResponse:
            {
                _reader.Read(out string secret);
                Console.WriteLine($"HandshakeResponse: {secret}");

                var ep = (IPEndPoint)res.RemoteEndPoint;
                if(!secret.Equals(Config.Secret))
                {
                    Log.D($"Handshake response failed - secret mismatch ({ep})");
                    break;
                }

                NetStatus = NetStatusTypes.Connected;
                Log.D($"Handshake response success ({ep})");

                var peer = AcceptPeer(ep);
                OnConnectionAccepted?.Invoke(peer);

                break;
            }

            default:
            {
                Console.WriteLine("!! Unrecognized data received !!");
                Console.WriteLine($"-> {(int)unconnectedMessageType}");
                break;
            }
        }
    }

    private async Task SendUnconnected(NetMessageWriter msg)
    {
        msg.PrepareSendUnconnected();
        await socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, _ep);
    }

    private async Task SendToUnconnected(NetMessageWriter msg, EndPoint recipient)
    {
        msg.PrepareSendUnconnected();
        await socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, recipient);
    }

    public async Task SendTo(NetMessageWriter msg, EndPoint recipient)
    {
        msg.PrepareSend();
        await socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, recipient);
    }

    public async Task SendToAll(NetMessageWriter msg)
    {
        msg.PrepareSend();
        for(int i = 0; i < _acceptedPeersList.Count; i++)
        {
            await socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, _acceptedPeersList[i].EndPoint);
        }
    }

    public async Task Send(NetMessageWriter msg)
    {
        msg.PrepareSend();
        await socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, _ep);
    }
}