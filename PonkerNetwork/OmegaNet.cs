using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PonkerNetwork;

public class OmegaNet
{
    internal const byte HeaderSize = 4;
    
    public NetConfig Config;
    
    internal Socket Socket;
    
    private byte[] _writeBuffer;
    private byte[] _buffer;
    private EndPoint ep;
    private ArraySegment<byte> _bufferSeg;
    private Dictionary<EndPoint, NetPeer> _acceptedPeers = new();

    private INetListener _listener;
    
    public OmegaNet(INetListener listener, NetConfig cfg)
    {
        _listener = listener;
        Config = cfg;
    }

    public NetMessageWriter CreateMessage()
    {
        return new NetMessageWriter(this, _buffer, _writeBuffer);
    }

    internal NetMessageWriter CreateHandshakeMessage()
    {
        return new NetMessageWriter(this, _buffer, _writeBuffer);
    }

    public void Start()
    {
        Start(0);
    }

    public void Start(int port)
    {
        _buffer = new byte[Config.BufferSize];
        _writeBuffer = new byte[1024];
        _bufferSeg = new(_buffer);

        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);

        //  client returns
        if(port == 0)
            return;

        //  server
        ep = new IPEndPoint(IPAddress.Any, port);
        Socket.Bind(ep);
        StartListen();
    }

    public void Connect(IPAddress ip, int port, string secret)
    {
        ep = new IPEndPoint(ip, port);
        Socket.Connect(ep);

        StartListen();

        Thread.Sleep(50);

        var msg = CreateHandshakeMessage();
        msg.Recycle(0);
        msg.Write((byte)UnconnectedMessageTypes.HandshakeRequest);
        msg.Write(secret);
        SendUnconnected(msg);
    }

    public void StartListen()
    {
        _ = Task.Run(async () =>
        {
            SocketReceiveMessageFromResult res;
            while(true)
            {
                Thread.Sleep(10);

                res = await Socket.ReceiveMessageFromAsync(_bufferSeg, SocketFlags.None, ep);

                if(res.ReceivedBytes == 0)
                {
                    continue;
                }

                Console.WriteLine($"received {res.ReceivedBytes} bytes");

                if(_acceptedPeers.ContainsKey(res.RemoteEndPoint))
                {
                    ReadConnectedData(ref res);
                }
                else
                {
                    ReadUnconnectedData(ref res);
                }
            }
        });
    }

    private void ReadConnectedData(ref SocketReceiveMessageFromResult res)
    {
        // _listener.OnDataReceived();
        int receivedBytes = BitConverter.ToInt16(_buffer, 0) - HeaderSize;
        Console.WriteLine($"received connected data - bytes: {receivedBytes} (+{receivedBytes + HeaderSize})");

        string text = Encoding.UTF8.GetString(_buffer, HeaderSize, receivedBytes);

        Console.WriteLine($"received: {text}");
    }

    private void ReadUnconnectedData(ref SocketReceiveMessageFromResult res)
    {
        Console.WriteLine("received unconnected data");

        var unconnectedMessageType = (UnconnectedMessageTypes)_buffer[0];

        switch(unconnectedMessageType)
        {
            case UnconnectedMessageTypes.HandshakeRequest:
            {
                string secret = Encoding.UTF8.GetString(_buffer, 1, Config.Secret.Length);
                Console.WriteLine($"HandshakeRequest: '{secret}' from '{res.RemoteEndPoint}'");

                //  respond to handshake
                var msg = CreateHandshakeMessage();
                msg.Recycle(0);
                msg.Write((byte)UnconnectedMessageTypes.HandshakeResponse);
                msg.Write(secret);

                var peer = new NetPeer(res.RemoteEndPoint);
                _acceptedPeers.Add(peer.Ep, peer);

                SendToUnconnected(msg, res.RemoteEndPoint);
                break;
            }

            case UnconnectedMessageTypes.HandshakeResponse:
            {
                string secret = Encoding.UTF8.GetString(_buffer, 1, Config.Secret.Length);
                Console.WriteLine($"HandshakeResponse: {secret}");
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

    private async Task SendUnconnected(NetMessage msg)
    {
        msg.PrepareSendUnconnected();
        await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, ep);
    }

    private async Task SendToUnconnected(NetMessage msg, EndPoint recipient)
    {
        msg.PrepareSendUnconnected();
        await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, recipient);
    }

    public async Task SendTo(NetMessage msg, EndPoint recipient)
    {
        msg.PrepareSend();
        await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, recipient);
    }

    public async Task Send(NetMessage msg)
    {
        msg.PrepareSend();
        await Socket.SendToAsync(msg.DataSegmentOut, SocketFlags.None, ep);
    }
}