namespace PonkerNetwork;

public enum NetConnectionTypes : byte
{
    None,
    Disconnected,
    Connecting,
    Handshaking,
    Connected,
    Disconnecting,
    Host,
}

public enum NetStateTypes : byte
{
    None,
    Running,
    Shutdown
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