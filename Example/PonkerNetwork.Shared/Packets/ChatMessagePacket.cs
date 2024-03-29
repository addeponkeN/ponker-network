namespace PonkerNetwork.Shared.Packets;

public struct ChatMessagePacket : IPacket
{
    public string Message;
    public void Write(NetMessageWriter writer) => writer.WriteString8(Message);
    public void Read(NetMessageReader reader) => reader.ReadString8(out Message);
}