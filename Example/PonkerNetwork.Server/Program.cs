using PonkerNetwork.Shared;
using PonkerNetwork.Shared.Packets;
using PonkerNetwork.Utility;

namespace PonkerNetwork.Server;

class Program
{
    private static PonkerNet server;
    private static NetMessageWriter _writer;
    static async Task Main(params string[] args)
    {
        Console.Title = "SERVER";

        var c = new NetConfig()
        {
            Secret = NetSettings.HelloMsg
        };

        server = new PonkerNet(c);

        server.RegisterPackets();
        server.Start(NetSettings.Port);

        _writer = server.CreateMessage();
        
        server.Services.Subscribe<ChatMessagePacket>(ChatMessageReceive);
        server.Services.Subscribe<PlayerJoinPacket>(PlayerJoined);

        Console.WriteLine("<ENTER> to start server");
        Console.ReadLine();
        Console.WriteLine("server started");

        while(true)
        {
            Thread.Sleep(50);
            await server.ReadMessagesAsync();
        }
    }

    private static void PlayerJoined(PlayerJoinPacket pkt, NetPeer peer)
    {
        Log.D($"Player joined: {pkt.Name} ({pkt.Id})");
    }

    static async void ChatMessageReceive(ChatMessagePacket pkt, NetPeer peer)
    {
        Log.D($"ChatMessage: {pkt.Message}");
        _writer.Recycle();
        _writer.WritePacket(pkt);
        await server.SendToAll(_writer);
    }
}