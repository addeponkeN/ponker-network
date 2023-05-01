using System.Drawing;

namespace PonkerNetwork.Shared.Packets;

public struct PlayerPositionPacket : IPacket
{
    public Point Position;
    
    public void Write(NetMessageWriter writer)
    {
        writer.Write((short)Position.X);
        writer.Write((short)Position.Y);
    }

    public void Read(NetMessageReader reader)
    {
        Position.X = reader.ReadInt16();
        Position.Y = reader.ReadInt16();
    }
}
