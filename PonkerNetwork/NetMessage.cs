namespace PonkerNetwork;

public class NetMessage
{
    public readonly byte[] Buffer;
    public ArraySegment<byte> DataSegment;

    internal ArraySegment<byte> DataSegmentOut;

    protected readonly byte[] WriteBuffer;
    protected readonly OmegaNet Net;
    
    protected int Current = 0;

    internal NetMessage(OmegaNet net, byte[] messageBuffer, byte[] writeBuffer)
    {
        WriteBuffer = writeBuffer;
        Net = net;
        Buffer = messageBuffer;
        DataSegment = new ArraySegment<byte>(Buffer);
        Recycle();
    }

    public void Recycle()
    {
        Recycle(OmegaNet.HeaderSize);
    }

    internal void Recycle(int headerSize)
    {
        Current = headerSize;
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

}