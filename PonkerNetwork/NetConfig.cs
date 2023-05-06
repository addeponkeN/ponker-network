namespace PonkerNetwork;

public class NetConfig
{
    public int Port;
    public string Secret;
    public int BufferSize;

    public int TimeoutTime = 10_000;
    public int PingPongTime = 3_000;

    public int PingPongInterval = 2_500;

    public int RetryConnectTime = 1_000;

    public NetConfig(string secretMessage)
    {
        Secret = secretMessage;
        BufferSize = 4096;
    }

    public NetConfig(string secretMessage, int port) : this(secretMessage)
    {
        Port = port;
    }
}