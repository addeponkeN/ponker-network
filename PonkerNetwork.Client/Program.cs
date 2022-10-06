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
        
        Console.WriteLine("client started - enter to send");
        Console.ReadLine();
        
        client.Connect(IPAddress.Loopback, NetSettings.Port, NetSettings.HelloMsg);

        // await client.Send();

        Console.ReadLine();
        
    }

}