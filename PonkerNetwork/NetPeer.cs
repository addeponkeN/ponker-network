using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PonkerNetwork;

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
}

public class NetServer : NetPeer
{
    public NetServer(NetConfig conf, NetListener listener) : base(conf, listener)
    {
    }

    public override void Start()
    {
        base.Start();
    }
}

public class NetClient : NetPeer
{
    public NetClient(NetConfig conf, NetListener listener) : base(conf, listener)
    {
    }

    public override void Start()
    {
    }
}

public class NetPeer
{
    internal Socket _socket;
    internal EndPoint EndPoint;

    public NetListener Listener { get; private set; }

    public NetConfig Config { get; private set; }

    public List<Socket> Connections;

    //  size of short
    internal static ushort HeaderSize = 4;
    static int bufferSize = 2048;
    static int messageBufferSize = 2048;
    byte[] buffer = new byte[bufferSize];

    IPEndPoint _sender;
    EndPoint _senderRemote;
    
    public NetPeer(NetConfig conf, NetListener listener)
    {
        Config = conf;
        Listener = listener;
        Connections = new List<Socket>();
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        
        _sender = new IPEndPoint(IPAddress.Any, Config.Port);
        _senderRemote = (EndPoint)_sender;
    }

    public NetMessage CreateMessage()
    {
        return new NetMessage(messageBufferSize) {Peer = this};
    }

    public virtual void Start()
    {
        _socket.Bind(_sender);
        _socket.BeginReceiveFrom(buffer, 0, bufferSize, SocketFlags.None, ref _senderRemote, ReceiveCallback, null);
    }

    public void Connect(IPAddress address)
    {
        EndPoint = new IPEndPoint(address, Config.Port);

        int attempts = 0;

        while(!_socket.Connected)
        {
            try
            {
                attempts++;
                Console.WriteLine($"Connecting attempt {attempts}...");
                _socket.Connect(EndPoint);
            }
            catch(SocketException)
            {
                Console.WriteLine("Failed to connect");
                Thread.Sleep(100);
            }
        }

        Console.WriteLine("## CONNECTED ?##");
    }

    public void CloseAllSockets()
    {
        for(int i = 0; i < Connections.Count; i++)
        {
            Connections[i].Shutdown(SocketShutdown.Both);
            Connections[i].Close();
        }

        _socket.Close();
        Connections.Clear();
    }

    public void AcceptCallback(IAsyncResult? res)
    {
        var socket = _socket.EndAccept(res);
        Connections.Add(socket);
        socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, ReceiveCallback, socket);
        Console.WriteLine("Client connected");
        Listener.OnConnectedEvent(socket);
    }

    public void ReceiveCallback(IAsyncResult res)
    {
        int dataLength = BitConverter.ToUInt16(buffer, 0);
        
        string text = Encoding.UTF8.GetString(buffer, HeaderSize, dataLength);
        Console.WriteLine($"Text received: {text}");

        _socket.BeginReceiveFrom(buffer, 0, bufferSize, SocketFlags.None, ref _senderRemote, ReceiveCallback, null);
    }

    public void Connect(string ipAddress)
    {
        Connect(ipAddress, Config.Port);
    }

    public void Connect(string ipAddress, int port)
    {
        if(ipAddress == "localhost")
            ipAddress = "127.0.0.1";
        IPAddress.TryParse(ipAddress, out IPAddress address);
        Connect(address);
    }

    internal void Update()
    {
        var buffer = new byte[1024];
        var received = _socket.Receive(buffer, SocketFlags.None);

        if(received <= 0)
        {
            Console.WriteLine("received: 0");
            return;
        }

        Console.WriteLine("received something!!");
    }

    public void Send(NetMessage message)
    {
        message.PrepareSend();
        _socket.Send(message.Data);
    }
}