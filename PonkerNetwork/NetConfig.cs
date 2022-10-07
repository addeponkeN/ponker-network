namespace PonkerNetwork;

public class NetConfig
{
    public int Port;
    public string Secret;
    public uint BufferSize;

    public NetConfig()
    {
        BufferSize = 2048;
    }

    public NetConfig(int port) : this()
    {
        Port = port;
    }
}