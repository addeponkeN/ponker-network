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