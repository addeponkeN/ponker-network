using System.Net;

namespace PonkerNetwork;

public delegate void OnConnectedEvent(NetPeer peer);
public delegate void OnConnectionAccepted(NetPeer peer);
public delegate void OnUnconnectedMessageReceived(IPEndPoint endPoint, NetMessageReader reader);
