using System.Text;

namespace PonkerNetwork;

public class NetMessage
{
    internal NetPeer Peer;
    
    public byte[] Data;
    
    int _bufferLength = 2048;
    int _current = 0;

    internal NetMessage(int bufferLength)
    {
        _bufferLength = bufferLength;
        Data = new byte[_bufferLength];
        
        Recycle();
    }
    
    public void Recycle()
    {
        _current = NetPeer.HeaderSize;
    }

    internal void PrepareSend()
    {
        var dataLengthBytes = BitConverter.GetBytes((ushort)_current); 
        Array.Copy(dataLengthBytes, 0, Data, 0, dataLengthBytes.Length);
        Console.WriteLine($"message byte length: {_current}");
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