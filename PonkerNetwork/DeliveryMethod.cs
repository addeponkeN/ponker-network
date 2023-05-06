namespace PonkerNetwork;

[Flags]
public enum DeliveryMethod : byte
{
    None,
    Unreliable,
    Reliable,
    ReliableOrdered,
}