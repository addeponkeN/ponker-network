using System.Text;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

public class NetMessageWriter : NetMessage
{
    internal NetMessageWriter(OmegaNet net, byte[] messageBuffer, byte[] writeBuffer)
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
        var dataLengthBytes = BitConverter.GetBytes((ushort)Current);
        Array.Copy(dataLengthBytes, 0, Buffer, 0, dataLengthBytes.Length);
        Console.WriteLine($"message byte length: {Current - OmegaNet.HeaderSize} ({Current})");
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

    public void Write(string text)
    {
        Encoding.UTF8.GetBytes(text, 0, text.Length, WriteBuffer, 0);
        Write(WriteBuffer, text.Length);
    }
}