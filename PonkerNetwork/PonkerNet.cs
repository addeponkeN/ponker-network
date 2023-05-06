using System.Net;
using System.Net.Sockets;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

public partial class PonkerNet
{
    internal static readonly byte HeaderSize = 0;

    public string Name;

    public NetConfig Config { get; private set; }
    public PacketService Services { get; private set; }
    internal readonly PeerCollection PeerCollection = new();

    internal Socket Socket;

    private byte[] _messageBuffer;
    private byte[] _buffer;
    private EndPoint _ep;
    private ArraySegment<byte> _bufferSeg;
    

    private Thread _netLogicThread;
    private bool _isReceiving;
    private NetMessageReader _reader;

    public bool IsRunning => State == NetStateTypes.Running;

    public event OnConnectedEvent OnConnectedEvent;
    public event OnConnectionAccepted OnConnectionAccepted;
    public event OnUnconnectedMessageReceived OnUnconnectedMessageReceived;

    public NetStateTypes State { get; internal set; }


    public NetStats Stats { get; }

    public PonkerNet(string connectKey) : this(new NetConfig(connectKey)) { }
    public PonkerNet(NetConfig cfg)
    {
        Config = cfg;
        Stats = new();
        Services = new(this);
        RegisterBaseServices();
        
        Sub<PingPongPacket>(HandlePingPongPangPacket);

        // NetStatus = NetConnectionTypes.Disconnected;

        //  init
        _buffer = new byte[Config.BufferSize];
        _messageBuffer = new byte[1024];
        _bufferSeg = new(_buffer);
        _reader = new NetMessageReader(this, _buffer, _messageBuffer);
        
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
        if(IsRunning)
        {
            Log.E("Network is already running");
            return;
        }
        
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        Socket.Blocking = true;
        
        StartNetThreads();

        State = NetStateTypes.Running;

        //  client returns
        if(port <= 0)
            return;

        //  server
        // NetStatus = NetConnectionTypes.Host;

        _ep = new IPEndPoint(IPAddress.Any, port);
        Socket.Bind(_ep);
        Log.I($"Listening on port '{port}' ({_ep})");
    }


    private async Task ReceiveLoop()
    {
        //  wait a bit before starting the loop to let everything else set up. just in case..
        Thread.Sleep(50);

        Log.D("~ Receive Thread Running ~");

        while(IsRunning)
        {
            await UpdateReceive();
        }

        Log.D("~ Receive Thread Exit ~");
    }


    private void StartNetThreads()
    {
        _netLogicThread = new Thread(() => Task.Run(LogicLoop))
        {
            Name = "PonkerNetwork-AsyncLogicThread",
            IsBackground = true
        };
        _netLogicThread.Start();

        // _netReceiveThread = new Thread(() => Task.Run(ReceiveLoop))
        // {
        //     Name = "PonkerNetwork-AsyncReceiveThread",
        //     IsBackground = true,
        // };
        // _netReceiveThread.Start();
    }

    private async Task LogicLoop()
    {
        //  wait a bit before starting the loop to let everything else set up. just in case..
        Thread.Sleep(10);

        Log.D("~ Logic Thread Running ~");

        while(IsRunning)
        {
            await UpdateLogic();

            if(!_isReceiving)
                UpdateReceive();
        }

        Log.D("~ Logic Thread Exit ~");
    }

    //  network logic
    private async Task UpdateLogic()
    {
        for(int i = 0; i < PeerCollection.Peers.Count; i++)
        {
            PeerCollection.Peers[i].Update();
        }
    }


    //  network receive data
    private async Task UpdateReceive()
    {
        if(State != NetStateTypes.Running)
            return;

        if(Socket.Available == 0)
            return;

        try
        {
            _isReceiving = true;
            var result = await Socket.ReceiveFromAsync(_bufferSeg, SocketFlags.None, _ep);

            if(result.ReceivedBytes == 0)
            {
                return;
            }

            Stats.TotalBytesReceived += result.ReceivedBytes;

            if(IsPeerAccepted(result.RemoteEndPoint, out NetPeer peer))
            {
                ReadConnectedData(result.ReceivedBytes, peer);
            }
            else
            {
                ReadUnconnectedData(result.ReceivedBytes, (IPEndPoint)result.RemoteEndPoint);
            }
        }
        catch(SocketException)
        {
            Log.W($"Lost connection to remote host ({_ep})");
        }
        catch(Exception e)
        {
            Log.E(e.ToString());
        }
        finally
        {
            _isReceiving = false;
        }
    }

    public void Shutdown()
    {
        State = NetStateTypes.Shutdown;
        Socket.Close();
    }

    public void Connect(string ipAddress, int port)
    {
        Connect(IPAddress.Parse(Util.FormatAddress(ipAddress)), port);
    }

    public void Connect(IPAddress ip, int port)
    {
        _ep = new IPEndPoint(ip, port);
        var peer = CreatePeer(_ep);
        Thread.Sleep(2);
        peer.IsConnector = true;
        peer.State = NetConnectionTypes.Connecting;
    }

    public void OnPeerConnected(NetPeer peer)
    {
        OnConnectedEvent?.Invoke(peer);
        PeerCollection.AddHandshakePeer(peer);
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

    private NetPeer CreatePeer(EndPoint ep)
    {
        var peer = new NetPeer(this, (IPEndPoint)ep);
        PeerCollection.AddPeer(peer);
        return peer;
    }

    private NetPeer AcceptPeer(NetPeer peer)
    {
        peer.Accept();
        PeerCollection.AddAcceptedPeer(peer);
        return peer;
    }

    private bool IsPeerAccepted(EndPoint endPoint)
        => PeerCollection.GetAcceptedPeer(endPoint, out _);

    private bool IsPeerAccepted(EndPoint endPoint, out NetPeer peer)
        => PeerCollection.GetAcceptedPeer(endPoint, out peer);

    private void ReadConnectedData(int receivedBytes, NetPeer peer)
    {
        _reader.PrepareRead(_buffer, receivedBytes);

        while(_reader.Current < receivedBytes)
        {
            IPacket packet = _reader.ReadPacket(out Type packetType);
            Services.Trigger(packetType, packet, peer);
        }
    }

    private void ReadUnconnectedData(int receivedBytes, IPEndPoint ep)
    {
        _reader.PrepareRead(_buffer, receivedBytes);
        var unconnectedMessageType = (UnconnectedMessageTypes)_reader.ReadByte(); //_buffer[0];

        switch(unconnectedMessageType)
        {
            case UnconnectedMessageTypes.HandshakeRequest:
            {
                _reader.ReadString(out string secret);

                if(!secret.Equals(Config.Secret))
                {
                    Log.D($"Handshake request failed - secret mismatch ({ep})");
                    break;
                }

                Log.D($"Handshake request success ({ep})  |  Sending HandshakeResponse");

                //  respond to handshake
                var msg = CreateHandshakeMessage();
                msg.Recycle(0);
                msg.Write((byte)UnconnectedMessageTypes.HandshakeResponse);
                msg.Write(secret);

                var peer = CreatePeer(ep);
                AcceptPeer(peer);

                OnConnectedEvent?.Invoke(peer);

                SendToUnconnected(msg, ep);

                break;
            }

            case UnconnectedMessageTypes.HandshakeResponse:
            {
                if(!PeerCollection.GetHandshakingPeer(ep, out NetPeer peer))
                {
                    Log.W($"Was not awaiting handshake response from this address ({ep})");
                    break;
                }

                _reader.Read(out string secret);

                if(!secret.Equals(Config.Secret))
                {
                    Log.D($"Handshake response failed - secret mismatch ({ep})");
                    break;
                }

                // NetStatus = NetConnectionTypes.Connected;
                Log.D($"Handshake response success ({ep})");

                AcceptPeer(peer);
                PeerCollection.RemoveHandshake(peer.EndPoint);
                OnConnectionAccepted?.Invoke(peer);

                break;
            }

            default:
            {
                Log.D($"!! Unrecognized data received !! -> {(int)unconnectedMessageType} ");
                OnUnconnectedMessageReceived?.Invoke(ep, _reader);
                break;
            }
        }
    }

    public void SendUnconnected(NetMessageWriter msg)
    {
        msg.PrepareSendUnconnected();
        Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, _ep);
    }

    public void SendToUnconnected(NetMessageWriter msg, EndPoint recipient)
    {
        msg.PrepareSendUnconnected();
        Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, recipient);
    }

    public void SendTo(NetMessageWriter msg, NetPeer peer)
    {
        msg.PrepareSend();
        peer.EnqueueOutgoingMessage(msg);
    }

    public void SendToAll(NetMessageWriter msg)
    {
        msg.PrepareSend();
        var peers = PeerCollection.AcceptedPeers;
        for(int i = 0; i < peers.Count; i++)
        {
            peers[i].EnqueueOutgoingMessage(msg);
        }
    }

    public void Send(NetMessageWriter msg)
    {
        msg.PrepareSend();
        PeerCollection.AcceptedPeers[0].EnqueueOutgoingMessage(msg);
    }

    internal void SendPing(NetPeer peer)
    {
        var pingPacket = new PingPongPacket();
        var writer = CreateMessage();
        writer.Write(pingPacket);
        SendTo(writer, peer);
        PeerCollection.AddPing(peer.EndPoint);
    }

    internal void SendPong(NetPeer peer)
    {
        var pingPacket = new PingPongPacket();
        var writer = CreateMessage();
        writer.Write(pingPacket);
        SendTo(writer, peer);
        PeerCollection.AddPong(peer.EndPoint);
    }

    /// <summary>
    /// 3-way ping,pong,pang
    /// </summary>
    private void HandlePingPongPangPacket(PingPongPacket pkt, NetPeer peer)
    {
        //  was awaiting a pong, send pang
        if(PeerCollection.IsPing(peer.EndPoint))
        {
            var now = DateTime.Now;
            SendPong(peer); // send pang
            var diff = now.Subtract(peer.LastPingDate);
            diff = diff.Subtract(TimeSpan.FromMilliseconds(NetConst.AvgPingProcessTime));
            Console.Title = $"{Name} - {diff.TotalMilliseconds}ms";
            PeerCollection.RemovePing(peer.EndPoint);
            Stats.Latency = (int)diff.TotalMilliseconds;
        }

        //  was awaiting pang
        else if(PeerCollection.IsPong(peer.EndPoint))
        {
            var now = DateTime.Now;
            var diff = now.Subtract(peer.LastPingDate);
            diff = diff.Subtract(TimeSpan.FromMilliseconds(NetConst.AvgPingProcessTime));
            Console.Title = $"{Name} - {diff.TotalMilliseconds}ms";
            PeerCollection.RemovePong(peer.EndPoint);
        }

        //  received ping, send pong
        else
        {
            SendPong(peer);
            peer.LastPingDate = DateTime.Now;
            //  received a ping, send pong
            PeerCollection.AddPong(peer.EndPoint);
        }
    }
}