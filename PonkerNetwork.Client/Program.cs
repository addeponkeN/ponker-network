using System.Net;
using PonkerNetwork.Shared;

namespace PonkerNetwork.Client;

class Program
{
    static async Task Main(params string[] args)
    {
        var c = new NetConfig()
        {
            Secret = NetSettings.HelloMsg
        };

        var client = new OmegaNet(c);
        client.Start();

        Console.WriteLine("client started - enter to connect");
        Console.ReadLine();

        client.Connect(IPAddress.Loopback, NetSettings.Port, NetSettings.HelloMsg);

        string input;
        NetMessage msg = client.CreateMessage();
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