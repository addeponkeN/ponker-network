namespace PonkerNetwork.Utility;

public static class Log
{
    public static void D(string msg)
    {
        Console.WriteLine($"[Debug]: {msg}");
    }
    
    public static void W(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[Warning]: {msg}");
        Console.ResetColor();
    }
    
    public static void E(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[Error]: {msg}");
        Console.ResetColor();
    }
}