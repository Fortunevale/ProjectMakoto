namespace Project_Ichigo;

internal class Program
{
    internal static void Main(string[] args)
    {
        Bot _bot = new();
        _bot.Init(args).GetAwaiter().GetResult();
    }
}