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
        var client = new PonkerNet(listener, c);
        client.RegisterPackets();
        client.Start();
        
        client.Services.Subscribe<ChatMessagePacket>(ChatMessage);

        Console.WriteLine("client started - enter to connect");
        Console.ReadLine();

        await client.Connect(IPAddress.Loopback, NetSettings.Port, NetSettings.HelloMsg);

        NetMessageWriter writer = client.CreateMessage();

        while(true)
        {
            string input = Console.ReadLine();

            if(string.IsNullOrEmpty(input))
                continue;

            var pkMsg = new ChatMessagePacket(input);
            writer.WritePacket(pkMsg);

            await client.Send(writer);

            writer.Recycle();

            Console.WriteLine($"sent: {input}");
        }
    }

    private static void ChatMessage(ChatMessagePacket packet, NetPeer peer)
    {
        Log.D($"ChatMessage: {packet.Message}");
    }
}