using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PonkerNetwork;

public enum NetStates
{
    Disconnected,
    Connecting,
    Handshaking,
    Connected,
}

public enum UnconnectedMessageTypes
{
    None,
    HandshakeRequest = 69,
    HandshakeResponse = 70,
}

public class NetConfig
{
    public int Port;

    public NetConfig()
    {
    }

    public NetConfig(int port)
    {
        Port = port;
    }

    public string Secret { get; set; }
}

public class NetManager
{
    private static int bufferSize = 1024;
    private static int messageBufferSize = 1024;
    
    private byte[] buffer;
    private ArraySegment<byte> bufferSegment;
    
    internal static ushort HeaderSize = 4;

    public List<Socket> Connections;
    public NetStates NetState;

    private Dictionary<IPAddress, NetPeer> _acceptedPeers = new();

    internal EndPoint EndPoint;

    internal Socket Socket;

    public NetConfig Config;

    private List<NetListener> _listeners = new();
    private Thread listenerThread;

    private static object _listenerLock = new();

    /// <summary>
    /// self
    /// </summary>
    // public NetMessage CreateMessage()
    // {
        // return new NetMessage(messageBufferSize);
    // }

    // internal NetMessage CreateHandshakeMessage()
    // {
        // return new NetMessage(messageBufferSize);
    // }

    public NetManager(NetConfig config)
    {
        this.Config = config;
        Connections = new List<Socket>();

        buffer = new byte[bufferSize];
        bufferSegment = new(buffer);
        
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        
        listenerThread = new Thread(Listen)
        {
            IsBackground = true
        };
        listenerThread.Start();
    }

    private void Listen()
    {
        IPPacketInformation info;
        while(true)
        {
            lock(_listenerLock)
            {
                for(int i = 0; i < _listeners.Count; i++)
                {
                    int res = _listeners[i].Read(buffer, out info);
                    if(res == 0)
                        continue;

                    Console.WriteLine($"Received {res} bytes");
                    if(_acceptedPeers.ContainsKey(info.Address))
                    {
                        ReadConnectedData(res, ref info);
                    }
                    else
                    {
                        ReadUnconnectedData(res, ref info);
                    }

                }
            }

            Thread.Sleep(1);
        }
    }

    private void ReadConnectedData(int res, ref IPPacketInformation info)
    {
        
    }

    private void ReadUnconnectedData(int res, ref IPPacketInformation info)
    {
        var unconnectedMessageType = (UnconnectedMessageTypes)buffer[0];

        switch(unconnectedMessageType)
        {
            case UnconnectedMessageTypes.HandshakeRequest:
            {
                string secret = Encoding.UTF8.GetString(buffer, 1, Config.Secret.Length);
                Console.WriteLine($"HandshakeRequest: '{secret}' from {info.Address}");

                //  handle handshake
                // var msg = CreateHandshakeMessage();
                // msg.Recycle(0);
                // msg.Write((byte)UnconnectedMessageTypes.HandshakeResponse);
                // msg.Write(secret);
                // var senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                // var endPoint = (EndPoint)new IPEndPoint(info.Address, Config.Port);
                // SendTo(msg, endPoint);
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

    public virtual void Start()
    {
        Start(0);
    }

    public virtual void Start(int port)
    {
        //  start client
        if(port == 0)
        {
            return;
        }
        
        //  start listen to port (server)
        StartListenUnconnected(port);
    }

    private void StartListenUnconnected(int port)
    {
        EndPoint = new IPEndPoint(IPAddress.Any, port);
        Socket.Bind(EndPoint);
        StartListenConnected(IPAddress.Any, port);
    }

    private void StartListenConnected(IPAddress address, int port)
    {
        Console.WriteLine($"listening to ip:port {address}:{port}");
        var endPoint = new IPEndPoint(address, port);
        var listener = new NetListener(this, endPoint);
        lock(_listenerLock)
        {
            _listeners.Add(listener);
        }
    }

    public void Send(NetMessage msg)
    {
        SendTo(msg, EndPoint);
    }
    
    public void SendTo(NetMessage msg, EndPoint recipient)
    {
        Socket.SendToAsync(bufferSegment, SocketFlags.None, recipient);
    }

    public void Connect(string ipAddress, int port, string secret)
    {
        ipAddress = Util.FormatAddress(ipAddress);
        IPAddress.TryParse(ipAddress, out IPAddress address);
        Connect(address, port, secret);
    }

    public void Connect(IPAddress address, int port, string secret)
    {
        EndPoint = new IPEndPoint(address, port);
        NetState = NetStates.Connecting;
        Console.WriteLine("## Connecting... ##");

        StartListenConnected(address, port);

        // var msg = CreateHandshakeMessage();
        // msg.Recycle(0);
        // msg.Write((byte)UnconnectedMessageTypes.HandshakeRequest);
        // msg.Write(secret);
        // SendTo(msg, EndPoint);
        
    }
}