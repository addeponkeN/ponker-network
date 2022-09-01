using System.Net.Sockets;
using System.Text;

namespace PonkerNetwork;

public class NetListener
{
    public event Action<Socket> NetConnectedEvent;
    public event Action<Socket, byte[]> NetDataReceivedEvent;

    NetPeer _peer;
    Socket listener => _peer._socket;

    NetDataReceivedEvent _recylcedEvent;

    public NetListener()
    {
        _recylcedEvent = new NetDataReceivedEvent();
    }
    internal void OnDataReceived(Socket sender, byte[] data)
    {
        NetDataReceivedEvent?.Invoke(sender, data);
    }

    internal void OnConnectedEvent(Socket client)
    {
        NetConnectedEvent?.Invoke(client);
    }
    
    internal void Start()
    {
        listener.Bind(_peer.EndPoint);
        listener.Listen(10);

        Console.WriteLine("Listening...");

        Socket handler = listener.Accept();

        string text = string.Empty;
        byte[] bytes = null;

        while(true)
        {
            Thread.Sleep(500);
            bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);
            if(bytesRec == 0)
            {
                Console.WriteLine("no data");
                continue;
            }
            
            text = Encoding.ASCII.GetString(bytes, 0, bytesRec);

            if(!string.IsNullOrEmpty(text))
            {
                Console.WriteLine($"text: {text}");
            }
        }
    } 
    
    public void PollEvents()
    {
        
    }
    
}