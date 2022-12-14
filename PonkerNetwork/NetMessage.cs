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
        Net = net;
        WriteBuffer = writeBuffer;
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
}