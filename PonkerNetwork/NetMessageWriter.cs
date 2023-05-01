using System.Text;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

public class NetMessageWriter : NetMessage
{
    internal NetMessageWriter(PonkerNet net, byte[] messageBuffer, byte[] writeBuffer)
        : base(net, messageBuffer, writeBuffer)
    {
    }

    public void Write(byte[] data, int count)
    {
        if(count + Current > Net.Config.BufferSize)
        {
            throw new Exception("BUFFER OVERFLOW");
        }

        Array.Copy(data, 0, Buffer, Current, count);
        Current += count;
    }

    internal void PrepareSendUnconnected()
    {
        DataSegmentOut = DataSegment.Slice(0, Current);
    }

    internal void PrepareSend()
    {
        // Console.WriteLine($"message byte length: {Current - PonkerNet.HeaderSize} ({Current})");
        DataSegmentOut = DataSegment.Slice(0, Current);
    }

    public void Write(byte[] data)
    {
        Write(data, data.Length);
    }

    public void Write(byte data)
    {
        Buffer[Current] = data;
        Current++;
    }

    public void Write(int data)
    {
        Array.Copy(BitConverter.GetBytes(data), 0, Buffer, Current, Util.SIZE_INT);
        Current += Util.SIZE_INT;
    }
    
    public void Write(uint data)
    {
        Array.Copy(BitConverter.GetBytes(data), 0, Buffer, Current, Util.SIZE_INT);
        Current += Util.SIZE_INT;
    }
    
    public void Write(short data)
    {
        Array.Copy(BitConverter.GetBytes(data), 0, Buffer, Current, Util.SIZE_SHORT);
        Current += Util.SIZE_SHORT;
    }
    
    public void Write(ushort data)
    {
        Array.Copy(BitConverter.GetBytes(data), 0, Buffer, Current, Util.SIZE_SHORT);
        Current += Util.SIZE_SHORT;
    }

    public void Write(string text)
    {
        Write(text.Length);
        Encoding.UTF8.GetBytes(text, 0, text.Length, WriteBuffer, 0);
        Write(WriteBuffer, text.Length);
    }
    
    public void WriteString8(string text)
    {
        Write((byte)text.Length);
        Encoding.UTF8.GetBytes(text, 0, text.Length, WriteBuffer, 0);
        Write(WriteBuffer, text.Length);
    }
    
    public void WriteString16(string text)
    {
        Write((ushort)text.Length);
        Encoding.UTF8.GetBytes(text, 0, text.Length, WriteBuffer, 0);
        Write(WriteBuffer, text.Length);
    }

    public void Write<T>(T pkMsg) where T : IPacket
    {
        byte packetId = (byte)Net.Services.Get<T>();
        Write(packetId);
        pkMsg.Write(this);
    }
    
    public void WritePacket<T>(T pkMsg) where T : IPacket
    {
        Write(pkMsg);
    }
    
}