using System.Text;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

public class NetMessageReader : NetMessage
{
    private byte[] _readBuffer;
    private int _totalBytes;
    
    internal NetMessageReader(PonkerNet net, byte[] messageBuffer, byte[] writeBuffer) 
        : base(net, messageBuffer, writeBuffer)
    {
        
    }

    public void PrepareRead(byte[] buffer, int receivedBytes)
    {
        Current = PonkerNet.HeaderSize;
        _readBuffer = buffer;
        _totalBytes = receivedBytes;
    }

    public byte[] ReadBytes()
    {
        int length = BitConverter.ToInt32(_readBuffer, Current);
        var value = new byte[length];
        Array.Copy(_readBuffer, Current + Util.SIZE_INT, value, 0, length);
        Current += Util.SIZE_INT + length;
        return value;
    }
    
    public byte ReadByte()
    {
        return _readBuffer[Current++];
    }

    public int ReadInt()
    {
        int value = BitConverter.ToInt32(_readBuffer, Current);
        Current += Util.SIZE_INT;
        return value;
    }
    
    public uint ReadUInt()
    {
        uint value = BitConverter.ToUInt32(_readBuffer, Current);
        Current += Util.SIZE_INT;
        return value;
    }

    public short ReadInt16()
    {
        short value = BitConverter.ToInt16(_readBuffer, Current);
        Current += Util.SIZE_SHORT;
        return value;
    }
    
    public ushort ReadUInt16()
    {
        ushort value = BitConverter.ToUInt16(_readBuffer, Current);
        Current += Util.SIZE_SHORT;
        return value;
    }

    /// <summary>
    /// Read a string with a maximum length of 32 bits
    /// </summary>
    public void Read(out string v)
    {
        v = ReadString();
    }
    
    /// <summary>
    /// Read a string with a maximum length of 32 bits
    /// </summary>
    public string ReadString()
    {
        ReadString(out string str);
        return str;
        // int length = ReadInt();
        // var value = Encoding.UTF8.GetString(_readBuffer, Current, length);
        // Current += length;
        // return value;
    }
    
    /// <summary>
    /// Read a string with a maximum length of 32 bits
    /// </summary>
    public void ReadString(out string str)
    {
        int stringLength = ReadInt();
        str = Encoding.UTF8.GetString(_readBuffer, Current, stringLength);
        Current += stringLength;
    }
    
    /// <summary>
    /// Read a string with a maximum length of 8 bits (256)
    /// </summary>
    public void ReadString8(out string str)
    {
        int stringLength = ReadByte();
        str = Encoding.UTF8.GetString(_readBuffer, Current, stringLength);
        Current += stringLength;
    }
    
    /// <summary>
    /// Read a string with a maximum length of 16 bits (65535)
    /// </summary>
    public void ReadString16(out string str)
    {
        int stringLength = ReadUInt16();
        str = Encoding.UTF8.GetString(_readBuffer, Current, stringLength);
        Current += stringLength;
    }

    internal IPacket ReadPacket()
    {
        var id = ReadByte();
        var packet = Net.Services.CreatePacket(id);
        packet.Read(this);
        return packet;
    }
    
    internal IPacket ReadPacket(out Type packetType)
    {
        var id = ReadByte();
        packetType = Net.Services.Get(id);
        var packet = Net.Services.CreatePacket(id);
        packet.Read(this);
        return packet;
    }
}