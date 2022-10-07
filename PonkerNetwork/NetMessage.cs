using System.Text;

namespace PonkerNetwork;

public class NetMessage
{
    public byte[] Data;
    public ArraySegment<byte> DataSegment;
    internal ArraySegment<byte> DataSegmentOut;

    public int Size => _current;

    private int _bufferLength = 2048;
    private int _current = 0;

    internal NetMessage(byte[] messageBuffer)
    {
        Data = messageBuffer;
        DataSegment = new ArraySegment<byte>(Data);

        Recycle();
    }

    public void Recycle()
    {
        Recycle(NetManager.HeaderSize);
    }
    
    internal void Recycle(int headerSize)
    {
        _current = headerSize;
    }

    internal void PrepareSendUnconnected()
    {
        DataSegmentOut = DataSegment.Slice(0, _current);
    }
    
    internal void PrepareSend()
    {
        var dataLengthBytes = BitConverter.GetBytes((ushort)_current);
        Array.Copy(dataLengthBytes, 0, Data, 0, dataLengthBytes.Length);
        Console.WriteLine($"message byte length: {_current}");
        DataSegmentOut = DataSegment.Slice(0, _current);
    }

    public void Write(byte[] data)
    {
        if(data.Length + _current > _bufferLength)
        {
            throw new Exception("BUFFER OVERFLOW");
        }

        Array.Copy(data, 0, Data, _current, data.Length);
        _current += data.Length;
    }

    public void Write(byte data)
    {
        Data[_current] = data;
        _current++;
    }

    public void Write(string text)
    {
        var textData = Encoding.UTF8.GetBytes(text);
        Write(textData);
    }
}