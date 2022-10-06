namespace PonkerNetwork;

internal static class Util
{
    public static string FormatAddress(string ipAddress)
    {
        if(ipAddress == "localhost")
            return "127.0.0.1";
        return ipAddress;
    }
}