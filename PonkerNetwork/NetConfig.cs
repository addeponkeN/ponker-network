namespace PonkerNetwork;

public class NetConfig
{
    public int Port;
    public string Secret;
    public uint BufferSize;

    public int TimeoutTime = 10_000;
    public int PingPongTime = 3_000;

    public int PingPongInterval = 1_000;

    public int RetryConnectTime = 2_000;

    public NetConfig()
    {
        BufferSize = 4096;
    }

    public NetConfig(int port) : this()
    {
        Port = port;
    }
}