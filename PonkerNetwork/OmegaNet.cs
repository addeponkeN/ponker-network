using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PonkerNetwork;

public class OmegaNet
{
    private static int messageBufferSize = 1024;

    private Socket socket;
    private EndPoint ep;

    private const byte HeaderSize = 4;

    private byte[] buffer;
    private byte[] messagebuffer;
    private ArraySegment<byte> buffer_seg;

    private Dictionary<EndPoint, NetPeer> _acceptedPeers = new();

    public NetConfig Config;

    public OmegaNet(NetConfig cfg)
    {
        Config = cfg;
    }

    public NetMessage CreateMessage()
    {
        return new NetMessage(messagebuffer);
    }

    internal NetMessage CreateHandshakeMessage()
    {
        return new NetMessage(messagebuffer);
    }

    public void Start()
    {
        Start(0);
    }

    public void Start(int port)
    {
        buffer = new byte[1024];
        buffer_seg = new(buffer);
        messagebuffer = new byte[messageBufferSize];

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);

        if(port == 0)
            return;

        ep = new IPEndPoint(IPAddress.Any, port);
        socket.Bind(ep);
        StartListen();
    }

    public void Connect(IPAddress ip, int port, string secret)
    {
        ep = new IPEndPoint(ip, port);
        socket.Connect(ep);
        
        StartListen();

        Thread.Sleep(50);
        
        var msg = CreateHandshakeMessage();
        msg.Recycle(0);
        msg.Write((byte)UnconnectedMessageTypes.HandshakeRequest);
        msg.Write(secret);
        Send(msg);
    }

    void StartListen()
    {
        _ = Task.Run(async () =>
        {
            SocketReceiveMessageFromResult res;
            while(true)
            {
                Thread.Sleep(10);

                res = await socket.ReceiveMessageFromAsync(buffer_seg, SocketFlags.None, ep);

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
        Console.WriteLine("received connected data");
        int receivedBytes = BitConverter.ToInt16(buffer, 0);
    }

    private void ReadUnconnectedData(ref SocketReceiveMessageFromResult res)
    {
        Console.WriteLine("received unconnected data");
        
        var unconnectedMessageType = (UnconnectedMessageTypes)buffer[0];

        switch(unconnectedMessageType)
        {
            case UnconnectedMessageTypes.HandshakeRequest:
            {
                string secret = Encoding.UTF8.GetString(buffer, 1, Config.Secret.Length);
                Console.WriteLine($"HandshakeRequest: '{secret}' from {res.RemoteEndPoint}");

                //  handle handshake
                var msg = CreateHandshakeMessage();
                msg.Recycle(0);
                msg.Write((byte)UnconnectedMessageTypes.HandshakeResponse);
                msg.Write(secret);

                var peer = new NetPeer(res.RemoteEndPoint);
                _acceptedPeers.Add(peer.Ep, peer);

                SendTo(msg, res.RemoteEndPoint);
                break;
            }

            case UnconnectedMessageTypes.HandshakeResponse:
            {
                string secret = Encoding.UTF8.GetString(buffer, 1, Config.Secret.Length);
                Console.WriteLine($"HandshakeResponse: {secret}");
                break;
            }

            default:
            {
                Console.WriteLine("!! Unrecognized data received !!");
                break;
            }
        }
    }

    public async Task SendTo(NetMessage msg, EndPoint recipient)
    {
        await socket.SendToAsync(msg.DataSegment, SocketFlags.None, recipient);
    }

    public async Task Send(NetMessage msg)
    {
        await socket.SendToAsync(msg.Data, SocketFlags.None, ep);
    }
    
}