using System.Net;

namespace PonkerNetwork;

//  server start
//  client start
//  client connect
//      send secret
//      handshaking
//  server listen port x
//  server connect & validate secret
//  

public class NetPeer
{
    // internal Socket Socket;

    public EndPoint Ep;

    public NetPeer(EndPoint resRemoteEndPoint)
    {
        Ep = resRemoteEndPoint;
        
        // Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        // Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
    }
    
}