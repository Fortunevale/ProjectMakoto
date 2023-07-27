// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;

namespace ProjectMakoto.Util.Initializers;
internal sealed class DisCatSharpExtensionsLoader
{
    public static async Task Load(Bot bot, string[] args)
    {
        if (bot.status.LoadedConfig.Secrets.Discord.Token.Length <= 0)
        {
            _logger.LogFatal("No discord token provided");
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.NoToken);
            return;
        }

        _logger.AddLogLevelBlacklist(CustomLogLevel.Trace2);

        _logger.LogDebug("Registering DiscordClient..");

        bot.DiscordClient = new DiscordClient(new DiscordConfiguration
        {
            Token = bot.status.LoadedConfig.Secrets.Discord.Token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Trace,
            Intents = DiscordIntents.All,
            AutoReconnect = true,
            LoggerFactory = new LoggerFactory(new ILoggerProvider[] { _logger.Provider }),
            HttpTimeout = TimeSpan.FromSeconds(60),
            MessageCacheSize = 4096,
            EnableSentry = true,
            ReportMissingFields = true,
            AttachUserInfo = true,
            DeveloperUserId = 411950662662881290
        });

        bot.ExperienceHandler = new(bot);

        _logger.LogDebug("Registering CommandsNext..");

        var cNext = bot.DiscordClient.UseCommandsNext(new CommandsNextConfiguration
        {
            EnableDefaultHelp = false,
            EnableMentionPrefix = false,
            IgnoreExtraArguments = true,
            EnableDms = false,
            ServiceProvider = new ServiceCollection()
                            .AddSingleton(bot)
                            .BuildServiceProvider(),
            PrefixResolver = new PrefixResolverDelegate(bot.GetPrefix)
        });



        _logger.LogDebug("Registering Lavalink..");

        var endpoint = new ConnectionEndpoint
        {
            Hostname = bot.status.LoadedConfig.Secrets.Lavalink.Host,
            Port = bot.status.LoadedConfig.Secrets.Lavalink.Port
        };

        var lavalinkConfig = new LavalinkConfiguration
        {
            Password = bot.status.LoadedConfig.Secrets.Lavalink.Password,
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };

        bot.DiscordClient.UseLavalink();

        _logger.LogDebug("Registering DisCatSharp TwoFactor..");

        var tfa = bot.DiscordClient.UseTwoFactor(new TwoFactorConfiguration
        {
            ResponseConfiguration = new TwoFactorResponseConfiguration
            {
                ShowResponse = false,
                AuthenticatorAccountPrefix = "Project Makoto"
            }
        });

        DiscordEventHandler.SetupEvents(bot);
        bot.DiscordClient.GuildDownloadCompleted += bot.GuildDownloadCompleted;

        _logger.LogDebug("Registering Interactivity..");
        bot.DiscordClient.UseInteractivity(new InteractivityConfiguration { });

        _ = Task.Delay(60000).ContinueWith(t =>
        {
            if (!bot.status.DiscordInitialized)
            {
                _logger.LogError("An exception occurred while trying to log into discord: {0}", "The log in took longer than 60 seconds");
                Environment.Exit((int)ExitCodes.FailedDiscordLogin);
                return;
            }
        });

        var appCommands = bot.DiscordClient.UseApplicationCommands(new ApplicationCommandsConfiguration
        {
            ServiceProvider = new ServiceCollection()
                                .AddSingleton(bot)
                                .BuildServiceProvider(),
            EnableDefaultHelp = false,
            EnableLocalization = true,
            DebugStartup = true
        });

        void GetCommandTranslations(ApplicationCommandsTranslationContext x)
        {
            x.AddSingleTranslation(File.ReadAllText("Translations/single_commands.json"));
            x.AddGroupTranslation(File.ReadAllText("Translations/group_commands.json"));
        }

        if (!bot.status.LoadedConfig.IsDev)
        {
            appCommands.RegisterGlobalCommands<ApplicationCommands.MaintainersAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.ConfigurationAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.ModerationAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.SocialAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.ScoreSaberAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.MusicAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.UtilityAppCommands>(GetCommandTranslations);
        }
        else
        {
            appCommands.RegisterGuildCommands<ApplicationCommands.UtilityAppCommands>(bot.status.LoadedConfig.Channels.Assets, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.MaintainersAppCommands>(bot.status.LoadedConfig.Channels.Assets, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ConfigurationAppCommands>(bot.status.LoadedConfig.Channels.Assets, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ModerationAppCommands>(bot.status.LoadedConfig.Channels.Assets, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.SocialAppCommands>(bot.status.LoadedConfig.Channels.Assets, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ScoreSaberAppCommands>(bot.status.LoadedConfig.Channels.Assets, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.MusicAppCommands>(bot.status.LoadedConfig.Channels.Assets, GetCommandTranslations);
        }

        _logger.LogDebug("Registering Commands..");
        cNext.RegisterCommands<PrefixCommands.UtilityPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.MusicPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.SocialPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.ScoreSaberPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.ModerationPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.ConfigurationPrefixCommands>();

        _logger.LogDebug("Registering Command Converters..");
        cNext.RegisterConverter(new CustomArgumentConverter.BoolConverter());

        var commandsNextTypes = new List<Type>();
        var applicationCommandTypes = new List<Type>();

        await Util.Initializers.PluginLoader.LoadPluginCommands(bot, cNext, appCommands);

        _ = Task.Run(async () =>
        {
            while (!bot.status.DiscordInitialized)
                Thread.Sleep(100);

            try
            {
                _logger.LogInfo("Connecting and authenticating with Lavalink..");
                bot.LavalinkSession = await bot.DiscordClient.GetLavalink().ConnectAsync(lavalinkConfig);
                _logger.LogInfo("Connected and authenticated with Lavalink.");

                bot.status.LavalinkInitialized = true;

                try
                {
                    _logger.LogInfo("Lavalink is running on {Version}.", await bot.LavalinkSession.Rest.GetVersionAsync());
                }
                catch { }
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while trying to log into Lavalink", ex);
                return;
            }
        });

        _ = Task.Run(async () =>
        {
            while (!bot.status.DiscordInitialized)
                Thread.Sleep(100);

            _ = bot.DiscordClient.UpdateStatusAsync(userStatus: UserStatus.Online, activity: new DiscordActivity("Registering commands..", ActivityType.Playing));

            while (bot.DiscordClient.GetApplicationCommands().RegisteredCommands.Count == 0)
                Thread.Sleep(1000);

            bot.status.DiscordCommandsRegistered = true;

            while (true)
            {
                try
                {
                    if (bot.DatabaseClient.IsDisposed())
                        return;

                    List<ulong> users = new();

                    foreach (var b in bot.Guilds)
                        foreach (var c in b.Value.Members)
                            if (!users.Contains(c.Key))
                                users.Add(c.Key);

                    foreach (var b in bot.Users)
                        if (!users.Contains(b.Key))
                            users.Add(b.Key);

                    await bot.DiscordClient.UpdateStatusAsync(activity: new DiscordActivity($"{bot.DiscordClient.Guilds.Count.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))} guilds | Up for {Math.Round((DateTime.UtcNow - bot.status.startupTime).TotalHours, 2).ToString(CultureInfo.CreateSpecificCulture("en-US"))}h", ActivityType.Playing));
                    await Task.Delay(30000);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to update user status", ex);
                    await Task.Delay(30000);
                }
            }
        });
    }
}
