using System.Net;
using System.Security.Cryptography.X509Certificates;
using PonkerNetwork.Shared;
using PonkerNetwork.Shared.Packets;
using PonkerNetwork.Utility;

namespace PonkerNetwork.Client;

internal static class Program
{
    private static PonkerNet client;

    static void ServerExample()
    {
        var server = new PonkerNet(connectKey: "ponkernetexample");
        server.Start(port: 4000); //  enter port to listen to
        
        server.Services.Register<ChatMessagePacket>(deliveryMethod: DeliveryMethod.Reliable);

        server.Sub<ChatMessagePacket>((chatMessagePacket, peerSender) =>
        {
            //  received chat message from client 
            NetMessageWriter writer = client.CreateMessage();   //  get writer
            writer.WritePacket(chatMessagePacket);              //  reuse and write packet
            client.SendToAll(writer);                           //  send message
        });

        server.Shutdown();
    }

    static void ClientExample()
    {
        var client = new PonkerNet(connectKey: "ponkernetexample");
        client.Start();
        client.Connect(ipAddress: "localhost", port: 4000);
        client.OnConnectionAccepted += peer =>
        {
            Console.WriteLine($"Successfully connected to host '{peer}'!");
            ChatMessagePacket chatMsg = new() {Message = "Wololo"};     //  create packet
            NetMessageWriter writer = client.CreateMessage();           //  get writer
            writer.Write(chatMsg);                                      //  write packet
            client.Send(writer);                                        //  send
            writer.Recycle();                                           //  recycle writer
        };
        
        //  subscribe to packet
        client.Sub<ChatMessagePacket>((chatMessagePacket, peerSender) =>
        {
            Console.WriteLine($"'{peerSender}' says: {chatMessagePacket.Message}");
        });

        client.Shutdown();
    }

    static void Shared()
    {
    }

    static void Main(params string[] args)
    {
        Console.Title = "CLIENT";

        client = new PonkerNet(NetSettings.HelloMsg) {Name = "CLIENT"};
        client.Start();

        //  connect to server
        client.Connect(ipAddress: "localhost", port: 4000);

        //  subscribe to packets
        client.Sub<ChatMessagePacket>(ChatMessage /* callback function */);
        client.Sub<PlayerPositionPacket>(PlayerPosition /* callback function */);

        //  successfully connected to server
        client.OnConnectionAccepted += peer => { Console.WriteLine($"Connected and accepted to {peer}!"); };


        GameLoop();
    }


    private static void PlayerPosition(PlayerPositionPacket packet, NetPeer sender)
    {
    }

    private static void ChatMessage(ChatMessagePacket packet, NetPeer peer)
    {
        Log.D($"ChatMessage1: {packet.Message}");
    }

    private static void GameLoop()
    {
        NetMessageWriter writer = client.CreateMessage();

        while(true)
        {
            Thread.Sleep(1);
            string input = Console.ReadLine();

            if(client.State != NetStateTypes.Running)
            {
                continue;
            }

            if(string.IsNullOrEmpty(input))
                continue;

            var pkMsg = new ChatMessagePacket() {Message = input};
            writer.WritePacket(pkMsg);

            client.Send(writer);

            writer.Recycle();
        }
    }
}