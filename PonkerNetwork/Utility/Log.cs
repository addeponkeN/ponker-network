namespace PonkerNetwork.Utility;

public static class Log
{
    private static int GetThreadId => Thread.CurrentThread.ManagedThreadId;
    
    public static void D(string msg)
    {
        Console.WriteLine($"[Debug][{GetThreadId}]: {msg}");
    }
    
    public static void I(string msg)
    {
        Print($"[Info][{GetThreadId}]: {msg}", ConsoleColor.Cyan);
    }
    
    public static void W(string msg)
    {
        Print($"[Warning][{GetThreadId}]: {msg}", ConsoleColor.Yellow);
    }
    
    public static void E(string msg)
    {
        Print($"[Error][{GetThreadId}]: {msg}", ConsoleColor.Red);
    }

    public static void Print(string msg, ConsoleColor clr)
    {
        Console.ForegroundColor = clr;
        Console.WriteLine(msg);
        Console.ResetColor();
    }
}