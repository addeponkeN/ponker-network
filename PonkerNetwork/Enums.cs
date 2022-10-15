namespace PonkerNetwork;

public enum NetStates
{
    Disconnected,
    Connecting,
    Handshaking,
    Connected,
}

public enum UnconnectedMessageTypes : byte
{
    None,
    HandshakeRequest = 69,
    HandshakeResponse = 70,
}

public enum ConnectedMessageTypes : byte
{
    Unknown,
    Data,
    Ping,
}