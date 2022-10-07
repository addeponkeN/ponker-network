using PonkerNetwork.Shared;

namespace PonkerNetwork.Server;

class Program
{
    static void Main(params string[] args)
    {
        var c = new NetConfig()
        {
            Secret = NetSettings.HelloMsg
        };

        var listener = new NetEventListener();
        var server = new OmegaNet(listener, c);
        server.Start(NetSettings.Port);

        Console.WriteLine("server started");

        Console.ReadLine();

    }

}