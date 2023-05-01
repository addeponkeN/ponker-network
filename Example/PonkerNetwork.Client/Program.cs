using System.Net;
using PonkerNetwork.Shared;
using PonkerNetwork.Shared.Packets;
using PonkerNetwork.Utility;

namespace PonkerNetwork.Client;

internal static class Program
{
    private static PonkerNet client;

    static async Task Main(params string[] args)
    {
        Console.Title = "CLIENT";
        var c = new NetConfig()
        {
            Secret = NetSettings.HelloMsg
        };

        client = new PonkerNet(c){Name = "CLIENT"};
        client.RegisterPackets();
        client.Start();

        client.Services.Subscribe<ChatMessagePacket>(ChatMessage);

        client.OnConnectedEvent += _ =>
        {
            Log.D("~~ Connected ~~");
        };

        new Thread(GameLoop) {IsBackground = true,}.Start();

        Log.D("ENTER to connect");
        Console.ReadLine();
        Log.D("Connecting...");
        
        await client.Connect(IPAddress.Loopback, NetSettings.Port, NetSettings.HelloMsg);
        
        while(true)
        {
            Thread.Sleep(1);
            client.ReadMessagesAsync();
        }
        
    }

    private static async void GameLoop()
    {
        NetMessageWriter writer = client.CreateMessage();

        while(true)
        {
            Thread.Sleep(100);
            // string input = Console.ReadLine();
            string input = String.Empty;

            if(client.NetStatus != NetStatusTypes.Connected)
            {
                Log.D("not connected");
                continue;
            }

            if(string.IsNullOrEmpty(input))
                continue;

            var pkMsg = new ChatMessagePacket(input);
            writer.WritePacket(pkMsg);

            await client.Send(writer);

            writer.Recycle();
        }
    }

    private static void ChatMessage(ChatMessagePacket packet, NetPeer peer)
    {
        Log.D($"ChatMessage: {packet.Message}");
    }
}