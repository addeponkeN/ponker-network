using System.Text;
using PonkerNetwork.Utility;

namespace PonkerNetwork;

public class NetMessageReader : NetMessage
{
    private byte[] _readBuffer;
    private int _totalBytes;
    
    internal NetMessageReader(OmegaNet net, byte[] messageBuffer, byte[] writeBuffer) 
        : base(net, messageBuffer, writeBuffer)
    {
        
    }

    public void PrepareRead(byte[] buffer, int receivedBytes)
    {
        Current = OmegaNet.HeaderSize;
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

    public string ReadString()
    {
        int length = BitConverter.ToInt32(_readBuffer, Current);
        var value = Encoding.UTF8.GetString(_readBuffer, Current + Util.SIZE_INT, length);
        Current += (Util.SIZE_UINT + length);
        return value;
    }
}