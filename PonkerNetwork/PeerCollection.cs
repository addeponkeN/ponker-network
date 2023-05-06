using System.Net;

namespace PonkerNetwork;

public class PeerCollection
{
    public List<NetPeer> Peers => _peersList;
    public List<NetPeer> AcceptedPeers => _acceptedPeersList;
    
    private List<NetPeer> _peersList = new();
    private List<NetPeer> _acceptedPeersList = new();
    
    private Dictionary<EndPoint, NetPeer> _acceptedPeers = new();
    private Dictionary<EndPoint, NetPeer> _handshakingPeers = new();

    private HashSet<EndPoint> _pingEndPoints = new();
    private HashSet<EndPoint> _pongEndPoints = new();

    public bool GetPeer(EndPoint ep, out NetPeer peer)
    {
        return _acceptedPeers.TryGetValue(ep, out peer!);
    }

    public bool GetHandshakingPeer(EndPoint ep, out NetPeer peer)
    {
        return _handshakingPeers.TryGetValue(ep, out peer!);
    }

    public bool GetAcceptedPeer(EndPoint ep, out NetPeer peer)
    {
        return _acceptedPeers.TryGetValue(ep, out peer!);
    }

    public bool IsPing(EndPoint ep)
    {
        return _pingEndPoints.Contains(ep);
    }

    public bool IsPong(EndPoint ep)
    {
        return _pongEndPoints.Contains(ep);
    }

    public void Clear()
    {
        _peersList.Clear();

        _acceptedPeers.Clear();
        _acceptedPeersList.Clear();

        _handshakingPeers.Clear();

        _pingEndPoints.Clear();
        _pongEndPoints.Clear();
    }


    public void AddPeer(NetPeer peer)
    {
        _peersList.Add(peer);
    }

    public void AddAcceptedPeer(NetPeer peer)
    {
        _acceptedPeers.Add(peer.EndPoint, peer);
        _acceptedPeersList.Add(peer);
    }

    public void AddHandshakePeer(NetPeer peer)
    {
        _handshakingPeers.Add(peer.EndPoint, peer);
    }

    public void RemoveHandshake(IPEndPoint endPoint)
    {
        _handshakingPeers.Remove(endPoint);
    }

    public void AddPing(IPEndPoint endPoint)
    {
        _pingEndPoints.Add(endPoint);
    }

    public void AddPong(IPEndPoint endPoint)
    {
        _pongEndPoints.Add(endPoint);
    }

    public void RemovePing(IPEndPoint endPoint)
    {
        _pingEndPoints.Remove(endPoint);
    }
    
    public void RemovePong(IPEndPoint endPoint)
    {
        _pongEndPoints.Remove(endPoint);
    }
}