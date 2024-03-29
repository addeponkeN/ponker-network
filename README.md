# WIP - ponker-network 
Early stages of a .NET6 UDP Networking library



## Features/Goals

+ **0 byte** packetsize overhead (both reliable & unreliable)
+ Easy to connect
+ Easy to send & receive



## How to use

### Client example
``` csharp
PonkerNet client = new PonkerNet(connectKey: "ponkernetexample");
client.Start();
client.Connect(ipAddress: "localhost", port: 4000);
client.OnConnectionAccepted += peer =>
{
    Console.WriteLine($"Successfully connected to host '{peer}'!");
};

//  subscribe to the chat message packet
client.Sub<ChatMessagePacket>((chatMessagePacket, peerSender) =>
{
    //  received a chat message packet
    Console.WriteLine($"{chatMessagePacket.Message}");
});

clioe

client.Shutdown();
```
### Server example
``` csharp
PonkerNet server = new PonkerNet(connectKey: "ponkernetexample");
server.Start(port: 4000);   //  enter port to listen to

//  subscribe to the chat message packet
server.Sub<ChatMessagePacket>((chatMessagePacket, peerSender) =>
{
    //  received a chat message
    NetMessageWriter writer = client.CreateMessage();   //  get writer
    writer.WritePacket(chatMessagePacket);              //  write the packet
    client.SendToAll(writer);                           //  send message
});

server.Shutdown();
```

### Shared
``` csharp
public struct ChatMessagePacket : IPacket
{
    public string Message;
    public void Write(NetMessageWriter writer) => writer.WriteString8(Message);
    public void Read(NetMessageReader reader) => reader.ReadString8(out Message);
}
```
