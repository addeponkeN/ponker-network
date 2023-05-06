using PonkerNetwork.Shared;
using PonkerNetwork.Shared.Packets;
using PonkerNetwork.Utility;

namespace PonkerNetwork.Server;

class Program
{
    private static PonkerNet _server;
    private static NetMessageWriter _writer;
    private static NetConfig cfg;

    private static void PlayerJoined(PlayerJoinPacket pkt, NetPeer peer)
    {
        Log.D($"Player joined: {pkt.Name} ({pkt.Id})");
    }

    static void ChatMessageReceive(ChatMessagePacket pkt, NetPeer peer)
    {
        Log.D($"ChatMessage: {pkt.Message}");
        _writer.Recycle();
        _writer.WritePacket(pkt);
        _server.SendToAll(_writer);
    }

    static void Main(params string[] args)
    {
        Console.Title = "SERVER";

        cfg = new NetConfig(NetSettings.HelloMsg);

        _server = new PonkerNet(cfg) {Name = "SERVER"};
        _server.Start(NetSettings.Port);

        _writer = _server.CreateMessage();

        _server.Sub<ChatMessagePacket>(ChatMessageReceive);
        _server.Sub<PlayerJoinPacket>(PlayerJoined);

        GameLoop();
    }

    private static void GameLoop()
    {
        NetMessageWriter writer = _server.CreateMessage();

        string[] options =
        {
            "Kill server for 2 seconds",
        };
        while(true)
        {
            Thread.Sleep(1);
            string input = Console.ReadLine();

            int choice = ConsoleExtended.RequestChoice("What do",
                options) + 1;

            switch(choice)
            {
                //  kill server for 5 seconds
                case 1:
                {
                    const int killTime = 5_000;
                    _server.Shutdown();
                    Log.D($"Server shut down for {killTime / 1000}seconds");
                    Log.D("...");
                    Thread.Sleep(killTime);
                    Log.D("Server starting...");
                    _server.Start(NetSettings.Port);
                    break;
                }
            }

            if(string.IsNullOrEmpty(input))
                continue;


            writer.Recycle();
        }
    }

}