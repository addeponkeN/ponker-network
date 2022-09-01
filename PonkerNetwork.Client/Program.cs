using System.Net.Sockets;
using PonkerNetwork.Client.PonkerNetwork.Shared;

namespace PonkerNetwork.Client;

class Program
{
    static void Main(params string[] args)
    {
        string ipAddress = "185.98.245.209";
        int port = NetSettings.Port;

        var listener = new NetListener();
        var cf = new NetConfig(NetSettings.Port); 
        var client = new NetClient(cf, listener);

        Console.WriteLine("<Enter> to connect");
        
        client.Start();
        client.Connect("localhost", NetSettings.Port);
        
        listener.NetConnectedEvent += Listener_NetConnectedEvent;
        listener.NetDataReceivedEvent += ListenerOnNetDataReceivedEvent;

        Console.WriteLine("Client Started");

        NetMessage msg = client.CreateMessage();
        string input;
        
        while(true)
        {
            input = Console.ReadLine();
            if(string.IsNullOrEmpty(input))
                continue;
            msg.Write(input);
            client.Send(msg);
            Console.WriteLine($"Sent: {input}");
            msg.Recycle();
        }
        
    }

    static void ListenerOnNetDataReceivedEvent(Socket sender, byte[] data)
    {
        Console.WriteLine("net data received event");
    }

    static void Listener_NetConnectedEvent(Socket obj)
    {
        Console.WriteLine("net connected event");
    }
}