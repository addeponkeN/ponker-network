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
    private static uint _idPool = 1;
    
    public uint UniqueId;
    public IPEndPoint EndPoint;

    public NetPeer(IPEndPoint resRemoteEndPoint)
    {
        EndPoint = resRemoteEndPoint;
        UniqueId = _idPool++;
    }

    public override string ToString()
    {
        return $"({UniqueId}){EndPoint}";
    }
}