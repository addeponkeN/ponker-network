using PonkerNetwork.Shared;
using PonkerNetwork.Shared.Packets;
using PonkerNetwork.Utility;

namespace PonkerNetwork.Server;

class Program
{
    private static PonkerNet server;
    private static NetMessageWriter _writer;
    static void Main(params string[] args)
    {
        var c = new NetConfig()
        {
            Secret = NetSettings.HelloMsg
        };

        var listener = new NetEventListener();
        server = new PonkerNet(listener, c);

        server.RegisterPackets();
        server.Start(NetSettings.Port);
        _writer = server.CreateMessage();
        
        server.Services.Subscribe<ChatMessagePacket>(ChatMessageReceive);
        server.Services.Subscribe<PlayerJoinPacket>(PlayerJoined);

        Console.WriteLine("server started");

        while(true)
        {
            Console.ReadLine();
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