using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

struct NetHeader
{
    // public ushort ArrSize;
    // public byte DataType;
}

public class PonkerNet
{
    // internal static readonly unsafe byte HeaderSize = (byte)(sizeof(NetHeader));
    internal static readonly byte HeaderSize = 0;

    public NetConfig Config;
    public PacketService Services;

    internal Socket Socket;

    private byte[] _messageBuffer;
    private byte[] _buffer;
    private EndPoint _ep;
    private ArraySegment<byte> _bufferSeg;
    private Dictionary<EndPoint, NetPeer> _acceptedPeers = new();
    private List<NetPeer> _acceptedPeersList = new();

    private INetListener _listener;
    private NetMessageReader _reader;

    public PonkerNet(INetListener listener, NetConfig cfg)
    {
        _listener = listener;
        Config = cfg;
        Services = new(this);
        RegisterBaseServices();
    }

    private void RegisterBaseServices()
    {
        Services.Register<IPacket.PingPacket>();
    }

    public NetMessageWriter CreateMessage()
    {
        return new NetMessageWriter(this, _buffer, _messageBuffer);
    }

    internal NetMessageWriter CreateHandshakeMessage()
    {
        return new NetMessageWriter(this, _buffer, _messageBuffer);
    }

    public void Start(int port = 0)
    {
        _buffer = new byte[Config.BufferSize];
        _messageBuffer = new byte[1024];
        _bufferSeg = new(_buffer);

        _reader = new NetMessageReader(this, _buffer, _messageBuffer);

        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);

        //  client returns
        if(port == 0)
            return;

        //  server
        _ep = new IPEndPoint(IPAddress.Any, port);
        Socket.Bind(_ep);
        StartListen();
    }

    public async Task Connect(IPAddress ip, int port, string secret)
    {
        _ep = new IPEndPoint(ip, port);
        await Socket.ConnectAsync(_ep);

        StartListen();

        Thread.Sleep(50);

        var msg = CreateHandshakeMessage();
        msg.Recycle(0);
        msg.Write((byte)UnconnectedMessageTypes.HandshakeRequest);
        msg.Write(secret);
        await SendUnconnected(msg);
    }

    public void StartListen()
    {
        _ = Task.Run(async () =>
        {
            SocketReceiveMessageFromResult res;
            while(true)
            {
                Thread.Sleep(1);

                res = await Socket.ReceiveMessageFromAsync(_bufferSeg, SocketFlags.None, _ep);

                if(res.ReceivedBytes == 0)
                {
                    continue;
                }

                Console.WriteLine($"received {res.ReceivedBytes} bytes");

                if(IsPeerAccepted(res.RemoteEndPoint, out NetPeer peer))
                {
                    ReadConnectedData(res, peer);
                }
                else
                {
                    await ReadUnconnectedData(res);
                }
            }
        });
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

        var sw = Stopwatch.StartNew();

        _reader.PrepareRead(_buffer, receivedBytes);

        while(_reader.Current < receivedBytes)
        {
            IPacket packet = _reader.ReadPacket(out Type packetType);
            Services.InvokeSub(packetType, packet, peer);
        }

        sw.Stop();

        // Log.D($"total time: {sw.Elapsed.TotalMilliseconds}ms");
        // Log.D($"packet create time: {swPacketCreate.Elapsed.TotalMilliseconds}ms");
        // Log.D($"invoke time: {swInvoke.Elapsed.TotalMilliseconds}ms");
    }

    private async Task ReadUnconnectedData(SocketReceiveMessageFromResult res)
    {
        Console.WriteLine("received unconnected data");

        _reader.PrepareRead(_buffer, res.ReceivedBytes);
        var unconnectedMessageType = (UnconnectedMessageTypes)_reader.ReadByte(); //_buffer[0];

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

                _listener.OnConnectionAccepted(peer);

                await SendToUnconnected(msg, res.RemoteEndPoint);

                break;
            }

            case UnconnectedMessageTypes.HandshakeResponse:
            {
                _reader.ReadString(out string secret);
                Console.WriteLine($"HandshakeResponse: {secret}");

                var ep = (IPEndPoint)res.RemoteEndPoint;
                if(!secret.Equals(Config.Secret))
                {
                    Log.D($"Handshake response failed - secret mismatch ({ep})");
                    break;
                }

                Log.D($"Handshake response success ({ep})");

                var peer = AcceptPeer(ep);
                _listener.OnConnectionAccepted(peer);

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
        await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, _ep);
    }

    private async Task SendToUnconnected(NetMessageWriter msg, EndPoint recipient)
    {
        msg.PrepareSendUnconnected();
        await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, recipient);
    }

    public async Task SendTo(NetMessageWriter msg, EndPoint recipient)
    {
        msg.PrepareSend();
        await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, recipient);
    }

    public async Task SendToAll(NetMessageWriter msg)
    {
        msg.PrepareSend();
        for(int i = 0; i < _acceptedPeersList.Count; i++)
        {
            await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, _acceptedPeersList[i].EndPoint);
        }
    }

    public async Task Send(NetMessageWriter msg)
    {
        msg.PrepareSend();
        await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, _ep);
    }
}