namespace PonkerNetwork.Utility;

internal static class Util
{
    public const byte SIZE_INT = sizeof(int);
    public const byte SIZE_UINT = sizeof(uint);
    public const byte SIZE_BYTE = sizeof(byte);
    public const byte SIZE_SHORT = sizeof(short);
    public const byte SIZE_USHORT = sizeof(ushort);
    
    public static string FormatAddress(string ipAddress)
    {
        if(ipAddress == "localhost")
            return "127.0.0.1";
        return ipAddress;
    }
}