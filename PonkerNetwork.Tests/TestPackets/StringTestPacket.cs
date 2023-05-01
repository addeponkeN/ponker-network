namespace PonkerNetwork.Tests.TestPackets;

public struct StringTestPacket : IPacket
{
    public string Value;

    public StringTestPacket(string value)
    {
        Value = value;
    }

    public void Write(NetMessageWriter writer)
    {
        writer.Write(Value);
    }

    public void Read(NetMessageReader reader)
    {
        reader.Read(out Value);
    }
}