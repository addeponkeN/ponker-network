namespace PonkerNetwork;

public class NetDataReceivedEvent
{
    public NetPeer Sender;
    public byte[] Data;

    
    
    public void Set(byte[] data)
    {
        Data = data;
    }
}