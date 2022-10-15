namespace PonkerNetwork;

public interface INetListener
{
    void OnDataReceived(NetPeer peer, NetMessageReader reader);
    void OnConnectionAccepted(NetPeer peer);
}