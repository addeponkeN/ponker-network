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
client.Sub<ChatMessagePacket>((chatMessagePacket, peerSender) =>
{
    Console.WriteLine($"'{peerSender}' says: {chatMessagePacket.Message}");
});

client.Shutdown();
```
### Server example
``` csharp
PonkerNet server = new PonkerNet(connectKey: "ponkernetexample");
server.Start(port: 4000);   //  enter port to listen to

server.Sub<ChatMessagePacket>((chatMessagePacket, peerSender) =>
{
    //  received chat message from client 
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
