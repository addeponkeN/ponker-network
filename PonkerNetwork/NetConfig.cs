namespace PonkerNetwork;

public class NetConfig
{
    public int Port;
    public string Secret;
    public uint BufferSize;

    public float TimeoutTime = 10_000;
    public float PingPongTime = 3_000;

    public NetConfig()
    {
        BufferSize = 4096;
    }

    public NetConfig(int port) : this()
    {
        Port = port;
    }
}