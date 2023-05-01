namespace PonkerNetwork;

public interface IPacket
{
    void Write(NetMessageWriter writer);
    void Read(NetMessageReader reader);
}

public struct PingPongPacket : IPacket
{
    public void Write(NetMessageWriter writer)
    {
    }

    public void Read(NetMessageReader reader)
    {
    }
}