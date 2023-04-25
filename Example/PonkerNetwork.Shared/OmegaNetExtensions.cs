using PonkerNetwork.Shared.Packets;

namespace PonkerNetwork.Shared;

public static class OmegaNetExtensions
{
    public static void RegisterPackets(this OmegaNet net)
    {
        net.Services.Register<ChatMessagePacket>();
        net.Services.Register<PlayerJoinPacket>();
    }
}
