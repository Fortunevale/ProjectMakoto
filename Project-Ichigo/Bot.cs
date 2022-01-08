namespace Project_Ichigo;

internal class Bot
{
    internal DiscordClient? DiscordClient;
    internal LavalinkNodeConnection? LavalinkNodeConnection;

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
                $"Commandline: {Regex.Replace(Environment.CommandLine, @"(--token \S*)", "")}\n");

        if (!Directory.Exists("config-backups"))
            Directory.CreateDirectory("config-backups");

        var logInToDiscord = Task.Run(async () =>
        {
            string token = "";

            try
            {
                if (args.Contains("--token"))
                    token = args[ Array.IndexOf(args, "--token") + 1 ];
            }
            catch (Exception ex)
            {
                LogError($"An exception occured while trying to parse a token commandline argument: {ex}");
            }

            if (File.Exists("token.cfg") && !args.Contains("--token"))
                token = File.ReadAllText("token.cfg");

            if (!(token.Length > 0))
            {
                LogFatal("No token provided");
                File.WriteAllText("token.cfg", "");
                await Task.Delay(1000);
                Environment.Exit(ExitCodes.NoToken);
                return;
            }

            LogDebug($"Registering DiscordClient..");

            DiscordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = $"{token}",
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Error,
                Intents = DiscordIntents.All,
                LogTimestampFormat = "dd.MM.yyyy HH:mm:ss",
                AutoReconnect = true
            });

            LogDebug($"Registering CommandsNext..");

            DiscordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "-" },
                EnableDefaultHelp = false,
                EnableMentionPrefix = false,
                IgnoreExtraArguments = true,
                EnableDms = false
            });

            LogDebug($"Registering Lavalink..");

            var endpoint = new ConnectionEndpoint
            {
                Hostname = Secrets.Secrets.LavalinkUrl,
                Port = Secrets.Secrets.LavalinkPort
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = Secrets.Secrets.LavalinkPassword,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            DiscordClient.UseLavalink();

            LogDebug($"Registering Command Converters..");
            DiscordClient.GetCommandsNext().RegisterConverter(new CustomArgumentConverter.DiscordUserConverter());
            DiscordClient.GetCommandsNext().RegisterConverter(new CustomArgumentConverter.BoolConverter());

            LogDebug($"Registering Interactivity..");

            DiscordClient.UseInteractivity(new InteractivityConfiguration { });

            try
            {
                var discordLoginSc = new Stopwatch();
                discordLoginSc.Start();

                LogInfo("Connecting and authenticating with Discord..");
                await DiscordClient.ConnectAsync();

                discordLoginSc.Stop();
                LogInfo($"Connected and authenticated with Discord. ({discordLoginSc.ElapsedMilliseconds}ms)");
            }
            catch (Exception ex)
            {
                LogError($"An exception occured while trying to log into discord: {ex}");
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDiscordLogin);
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var lavalinkSc = new Stopwatch();
                    lavalinkSc.Start();
                    LogInfo("Connecting and authenticating with Lavalink..");

                    LavalinkNodeConnection = await DiscordClient.GetLavalink().ConnectAsync(lavalinkConfig);
                    lavalinkSc.Stop();
                    LogInfo($"Connected and authenticated with Lavalink. ({lavalinkSc.ElapsedMilliseconds}ms)");
                }
                catch (Exception ex)
                {
                    LogError($"An exception occured while trying to log into Lavalink: {ex}");
                    return;
                }
            });
        });

        await Task.Delay(-1);
    }
}
