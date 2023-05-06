# WIP - ponker-network 
Early stages of a .NET6 UDP Networking library



## Features/Goals

+ **0 byte** packetsize overhead (both reliable & unreliable)
+ Easy to connect
+ Easy to send & receive



## How to use

### Client example
``` csharp
        var client = new PonkerNet(connectKey: "ponkernetexample");
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
