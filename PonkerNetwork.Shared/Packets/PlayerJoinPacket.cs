namespace PonkerNetwork.Shared.Packets;

public struct PlayerJoinPacket : IPacket
{
    public byte Id;
    public string Name;

    public PlayerJoinPacket(byte id, string name)
    {
        Id = id;
        Name = name;
    }

    public void Write(NetMessageWriter writer)
    {
        writer.Write(Id);
        writer.Write(Name);
    }

    public void Read(NetMessageReader reader)
    {
        Id = reader.ReadByte();
        Name = reader.ReadString();
    }
}