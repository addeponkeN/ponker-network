using System.Net;
using System.Text;
using PonkerNetwork.Tests.TestPackets;

namespace PonkerNetwork.Tests;

[TestFixture]
public class ConnectionTests
{
    private const int Timeout = 4000;

    private NetConfig cfg;
    private PonkerNet _client;
    private PonkerNet _server;
    private NetMessageWriter _writer;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        cfg = new NetConfig("ago");

        _server = new PonkerNet(cfg)
        {
            Name = "SERVER"
        };
        _server.Start(4000);

        _client = new PonkerNet(cfg)
        {
            Name = "CLIENT"
        };
        _client.Start(4001);

        _writer = _client.CreateMessage();
    }

    [SetUp]
    public void Setup()
    {
        _writer.Recycle();
        _server.Services.ClearSubscriptions();
    }

    [Test, Timeout(Timeout)]
    public void TestConnect()
    {
        var connectedEvent = new AutoResetEvent(false);

        void OnClientOnOnConnectedEvent(NetPeer _) => connectedEvent.Set();

        _client.OnConnectedEvent += OnClientOnOnConnectedEvent;

        _client.Connect(IPAddress.Loopback, 4000);

        while(_server.State != NetStateTypes.Running)
        {
            Thread.Sleep(1);
            // _server.ReadMessagesAsync();
            // _client.ReadMessagesAsync();
        }

        bool connected = _client.State == NetStateTypes.Running;

        Assert.IsTrue(connected, "Failed to connect");

        _client.OnConnectedEvent -= OnClientOnOnConnectedEvent;
    }

    [Test, Timeout(Timeout)]
    public void TestSendPacketString()
    {
        string message = "This is a test string.";

        bool correctMessage = false;
        bool packetReceived = false;

        _server.Services.Sub<StringTestPacket>((pkt, _) =>
        {
            packetReceived = true;
            correctMessage = pkt.Value == message;
        });

        var pkt = new StringTestPacket(message);
        _writer.Write(pkt);
        _client.Send(_writer);

        while(!correctMessage)
        {
            Thread.Sleep(1);
            // _server.ReadMessagesAsync();
        }

        Assert.IsTrue(packetReceived, "Server never received the packet");
        Assert.IsTrue(correctMessage, "Server received the wrong message");
    }

    [Test, Timeout(Timeout)]
    public void TestSend100StringPackets()
    {
        const int packetCount = 100;
        const int maxStringLength = 20;

        int correctMessagesCounter = 0;
        int index = 0;

        string[] strings = new string[packetCount];

        Span<byte> charBuffer = stackalloc byte[maxStringLength];

        for(int i = 0; i < strings.Length; i++)
        {
            Random.Shared.NextBytes(charBuffer);
            strings[i] = Encoding.ASCII.GetString(charBuffer);
        }

        _server.Sub<StringTestPacket>((pkt, _) =>
        {
            string str = strings[index++];
            if(pkt.Value == str)
            {
                correctMessagesCounter++;
            }
        });

        for(int i = 0; i < packetCount; i++)
        {
            var pkt = new StringTestPacket(strings[i]);
            _writer.Recycle();
            _writer.Write(pkt);
            _client.Send(_writer);
        }

        while(correctMessagesCounter < packetCount)
        {
            Thread.Sleep(1);
            // _server.ReadMessagesAsync();
        }

        Assert.IsTrue(correctMessagesCounter == packetCount, "Server never received a packet");
    }
}