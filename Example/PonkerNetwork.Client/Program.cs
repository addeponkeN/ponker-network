using System.Diagnostics;
using System.Net;
using PonkerNetwork.Shared;
using PonkerNetwork.Shared.Packets;
using PonkerNetwork.Utility;

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
        client.RegisterPackets();
        client.Start();

        Console.WriteLine("client started - enter to connect");
        Console.ReadLine();

        await client.Connect(IPAddress.Loopback, NetSettings.Port, NetSettings.HelloMsg);

        string input;

        NetMessageWriter writer = client.CreateMessage();

        while(true)
        {
            input = Console.ReadLine();

            if(string.IsNullOrEmpty(input))
                continue;

            var sw = Stopwatch.StartNew();

            var pkMsg = new ChatMessagePacket(input);
            writer.WritePacket(pkMsg);

            await client.Send(writer);

            sw.Stop();

            Log.D($"write & send time: {sw.Elapsed.TotalMilliseconds}ms");

            writer.Recycle();

            Console.WriteLine($"sent: {input}");
        }
    }
}