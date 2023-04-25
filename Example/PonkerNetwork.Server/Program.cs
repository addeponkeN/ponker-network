using System;
using PonkerNetwork.Shared;
using PonkerNetwork.Shared.Packets;
using PonkerNetwork.Utility;

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

        server.RegisterPackets();
        server.Services.Subscribe<ChatMessagePacket>(ChatMessageReceive);
        server.Services.Subscribe<PlayerJoinPacket>(PlayerJoined);

        server.Start(NetSettings.Port);

        Console.WriteLine("server started");

        while(true)
        {
            Console.ReadLine();
        }
    }

    private static void PlayerJoined(PlayerJoinPacket pkt)
    {
        Log.D($"Player joined: {pkt.Name} ({pkt.Id})");
    }

    static void ChatMessageReceive(ChatMessagePacket pkt)
    {
        Log.D($"ChatMessage: {pkt.Message}");
    }
}