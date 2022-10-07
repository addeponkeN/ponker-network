using System.Net;
using PonkerNetwork.Shared;

namespace PonkerNetwork.Client;

internal static class Program
{
    static async Task Main(params string[] args)
    {
        var c = new NetConfig()
        {
            Secret = NetSettings.HelloMsg
        };

        var listener = new NetEventListener();
        var client = new OmegaNet(listener, c);
        client.Start();

        Console.WriteLine("client started - enter to connect");
        Console.ReadLine();

        client.Connect(IPAddress.Loopback, NetSettings.Port, NetSettings.HelloMsg);

        string input;
        
        NetMessageWriter msg = client.CreateMessage();
        
        while(true)
        {
            input = Console.ReadLine();

            if(string.IsNullOrEmpty(input))
                continue;
            
            msg.Write(input);

            await client.Send(msg);

            msg.Recycle();

            Console.WriteLine($"sent: {input}");
        }

    }
}