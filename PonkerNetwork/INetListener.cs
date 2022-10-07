namespace PonkerNetwork;

public interface INetListener
{
    void OnDataReceived(NetPeer peer, NetMessageReader reader);
    void OnConnected(NetPeer peer);
}