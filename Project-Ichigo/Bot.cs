namespace Project_Ichigo;

internal class Bot
{
    internal DiscordClient? DiscordClient;
    internal CommandsNextExtension? CommandsNextExtension;
    internal LavalinkExtension? LavalinkExtension;

    internal async Task Init(string[] args)
    {
        if (!Directory.Exists("logs"))
            Directory.CreateDirectory("logs");

        StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", LogLevel.DEBUG, DateTime.UtcNow.AddDays(-3), false);

        LogInfo("Starting up..");

        LogDebug($"Enviroment Details\n\n" +
                $"Dotnet Version: {Environment.Version}\n" +
                $"OS & Version: {Environment.OSVersion}\n\n" +
                $"OS 64x: {Environment.Is64BitOperatingSystem}\n" +
                $"Process 64x: {Environment.Is64BitProcess}\n\n" +
                $"MachineName: {Environment.MachineName}\n" +
                $"UserName: {Environment.UserName}\n" +
                $"UserDomain: {Environment.UserDomainName}\n\n" +
                $"Current Directory: {Environment.CurrentDirectory}\n" +
                $"Commandline: {Environment.CommandLine}\n");

        if (!Directory.Exists("config-backups"))
            Directory.CreateDirectory("config-backups");

        var logInToDiscord = Task.Run(async () =>
        {
            
        });

        await Task.Delay(-1);
    }
}
