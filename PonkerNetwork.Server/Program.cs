using System.Net.Sockets;
using PonkerNetwork.Shared;

namespace PonkerNetwork.Server;

class Program
{
    static NetManager Server;
    
    static void Main(params string[] args)
    {
        var c = new NetConfig()
        {
            Secret = NetSettings.HelloMsg
        };

        var server = new OmegaNet(c);
        server.Start(NetSettings.Port);

        Console.WriteLine("server started");

        Console.ReadLine();

    }

}