namespace Project_Ichigo;

internal class Program
{
    internal static void Main(string[] args)
    {
        Bot _bot = new Bot();
        _bot.Init(args).GetAwaiter().GetResult();
    }
}