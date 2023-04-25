namespace PonkerNetwork;

public interface IPacket
{
    void Write(NetMessageWriter writer);
    void Read(NetMessageReader reader);
}

public struct PingPacket : IPacket
{
    public PingPacket()
    {
    }

    public void Write(NetMessageWriter writer)
    {
    }

    public void Read(NetMessageReader reader)
    {
    }
}