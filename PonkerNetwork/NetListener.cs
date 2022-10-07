using System.Net;
using System.Net.Sockets;

namespace PonkerNetwork;

public class NetListener
{
    public event Action<Socket> NetConnectedEvent;
    public event Action<Socket, byte[]> NetDataReceivedEvent;

    NetDataReceivedEvent _recycledEvent;

    private OmegaNet _net;
    
    private int Port => _net.Config.Port;

    private IPEndPoint _ipEndPoint;
    private EndPoint _endPoint;

    private IPPacketInformation _info;
    private SocketFlags _flag;

    public NetListener(IPEndPoint endPoint)
    {
        _ipEndPoint = endPoint;
        _endPoint = endPoint;
        _recycledEvent = new NetDataReceivedEvent();

        // if(_sender.Address.Equals(IPAddress.Any))
            // netMan.Socket.Bind(_senderRemote);
    }

    internal void OnDataReceived(Socket sender, byte[] data)
    {
        NetDataReceivedEvent?.Invoke(sender, data);
    }

    internal void OnConnectedEvent(Socket client)
    {
        NetConnectedEvent?.Invoke(client);
    }

    public int Read(byte[] buffer, out IPPacketInformation info)
    {
        return _net.Socket.ReceiveMessageFrom(buffer, ref _flag, ref _endPoint, out info);
    }
}