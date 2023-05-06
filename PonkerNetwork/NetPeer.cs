using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

public class NetPeer
{
    private static uint _idPool = 1;

    public uint UniqueId;
    public IPEndPoint EndPoint;

    public DateTime LastPingDate;

    private Stopwatch _pingTimer = new Stopwatch();
    private Stopwatch _reconnectTimer = new Stopwatch();
    private Stopwatch _handshakeTimer = new Stopwatch();

    internal bool _sendPings;

    public PonkerNet Net;

    /// <summary>
    /// is the one who connected
    /// </summary>
    public bool IsConnector;

    private NetConnectionTypes _netStatus;

    public NetConnectionTypes State
    {
        get => _netStatus;
        internal set
        {
            if(_netStatus != value)
            {
                Log.D($"PeerStatus({EndPoint}): {_netStatus} -> {value}");
                _netStatus = value;
            }
        }
    }

    public NetPeer(PonkerNet net, IPEndPoint resRemoteEndPoint)
    {
        Net = net;
        EndPoint = resRemoteEndPoint;
        UniqueId = _idPool++;

        _outgoingMessages = new();
        _incomingMessages = new();

        State = NetConnectionTypes.Disconnected;
    }

    ConcurrentQueue<NetMessageWriter> _outgoingMessages;
    ConcurrentQueue<NetMessageReader> _incomingMessages;

    public override string ToString()
    {
        return $"({UniqueId}){EndPoint}";
    }

    public void EnqueueOutgoingMessage(NetMessageWriter msg)
    {
        _outgoingMessages.Enqueue(msg);
    }

    public async Task ProcessOutgoingMessages()
    {
        while(!_outgoingMessages.IsEmpty)
        {
            if(_outgoingMessages.TryDequeue(out NetMessageWriter msg))
                await Net.Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, EndPoint);
        }
    }

    public void EnqueueIncomingMessage(NetMessageReader msg)
    {
        _incomingMessages.Enqueue(msg);
    }

    public async Task ProcessIncomingMessages(NetMessageReader msg)
    {
    }

    private async Task SendHandshake()
    {
        State = NetConnectionTypes.Handshaking;
        _handshakeTimer.Restart();
        var msg = Net.CreateHandshakeMessage();
        msg.Recycle(0);
        msg.Write((byte)UnconnectedMessageTypes.HandshakeRequest);
        msg.Write(Net.Config.Secret);
        Net.SendUnconnected(msg);
    }

    internal void Accept()
    {
        State = NetConnectionTypes.Connected;
    }

    public async Task Connect()
    {
        if(State != NetConnectionTypes.Connecting)
        {
            Log.W($"Was in '{State}' state when trying to connect");
            return;
        }

        try
        {
            Log.D($"Connecting to '{EndPoint}'...");
            Net.Socket.ConnectAsync(EndPoint).ContinueWith(_ =>
            {
                if(Net.Socket.Available == 0 && Net.Socket.Poll(1_000, SelectMode.SelectRead))
                {
                    State = NetConnectionTypes.Connecting;
                }
                else
                {
                    //  connect success
                    Net.OnPeerConnected(this);

                    //  enter handshaking state
                    State = NetConnectionTypes.Handshaking;
                }
            });
        }
        catch(SocketException e)
        {
            Log.W(e.Message);
            return;
        }
        catch(Exception e)
        {
            Log.E(e.Message);
            return;
        }
    }

    public async Task Update()
    {
        switch(State)
        {
            case NetConnectionTypes.Disconnected:
                break;

            case NetConnectionTypes.Connecting:

                if(!_reconnectTimer.IsRunning ||
                   _reconnectTimer.Elapsed.TotalMilliseconds > Net.Config.RetryConnectTime)
                {
                    _reconnectTimer.Restart();
                    Connect();
                }

                break;

            case NetConnectionTypes.Handshaking:

                if(!_handshakeTimer.IsRunning || _handshakeTimer.Elapsed.TotalMilliseconds > 5_000)
                {
                    _handshakeTimer.Restart();
                    SendHandshake();
                }

                break;

            case NetConnectionTypes.Connected:

                ProcessOutgoingMessages();

                if(!IsConnector)
                    break;

                if(!_pingTimer.IsRunning)
                    _pingTimer.Restart();
                if(_pingTimer.ElapsedMilliseconds > Net.Config.PingPongInterval)
                {
                    _pingTimer.Restart();
                    Net.SendPing(this);
                    LastPingDate = DateTime.Now;
                }

                break;
        }
    }
}