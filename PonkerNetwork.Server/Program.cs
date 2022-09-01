using System.Net.Sockets;
using PonkerNetwork.Client.PonkerNetwork.Shared;

namespace PonkerNetwork.Server;

class Program
{
    static NetServer Server;
    
    static void Main(params string[] args)
    {
        var listener = new NetListener();
        var c = new NetConfig(NetSettings.Port);
        Server = new NetServer(c, listener);
        Server.Start();
        
        listener.NetConnectedEvent += ListenerOnNetConnectedEvent;
        listener.NetDataReceivedEvent += ListenerOnNetDataReceivedEvent;

        Console.WriteLine("client started");
        while(true)
        {
            listener.PollEvents();
            Console.ReadLine();
        }
        
    }

    static void ListenerOnNetDataReceivedEvent(Socket sender, byte[] data)
    {
        Console.WriteLine("net data received event");
        var msg = Server.CreateMessage();
        
        Server.Send(msg);
    }

    static void ListenerOnNetConnectedEvent(Socket client)
    {
        Console.WriteLine("net connected event");
    }
    
}