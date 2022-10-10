using System.Text;

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
    
    public void Write(byte[] data)
    {
        Write(data, data.Length);
    }

    public void Write(byte data)
    {
        Buffer[Current] = data;
        Current++;
    }

    public void Write(string text)
    {
        Encoding.UTF8.GetBytes(text, 0, text.Length, WriteBuffer, 0);
        Write(WriteBuffer, text.Length);
    }
    
}