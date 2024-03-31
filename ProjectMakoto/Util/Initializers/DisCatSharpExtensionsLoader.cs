// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;

namespace ProjectMakoto.Util.Initializers;
internal static class DisCatSharpExtensionsLoader
{
    static Bot bot = null;
    static List<DisCatSharp.ApplicationCommands.Entities.CommandTranslator> singleCommandTranslations = new();
    static List<DisCatSharp.ApplicationCommands.Entities.GroupTranslator> groupCommandTranslations = new();

    internal static void GetCommandTranslations(ApplicationCommandsTranslationContext x)
    {
        if (singleCommandTranslations.IsNotNullAndNotEmpty() && groupCommandTranslations.IsNotNullAndNotEmpty())
        {
            x.AddSingleTranslation(JsonConvert.SerializeObject(singleCommandTranslations));
            x.AddGroupTranslation(JsonConvert.SerializeObject(groupCommandTranslations));
            return;
        }

        object CreateTranslationRecursively(Type typeToCreate, CommandTranslation translation)
        {
            try
            {
                var nameValues = translation.Names;

                if (nameValues is null)
                    return null;

                var descriptionValues = translation.Descriptions;
                var typeValue = translation.Type;
                var optionsValues = translation.Options;
                var choicesValues = translation.Choices;
                var groupsValues = translation.Groups;
                var commandsValues = translation.Commands;

                Log.Verbose("Creating instance of '{type}'", typeToCreate.Name);
                var translator = Activator.CreateInstance(typeToCreate);

                var createTypeProperties = typeToCreate.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "name")).SetValue(translator, nameValues["en"]);

                if (typeToCreate == typeof(DisCatSharp.ApplicationCommands.Entities.GroupTranslator) || typeToCreate == typeof(DisCatSharp.ApplicationCommands.Entities.CommandTranslator))
                    createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "type")).SetValue(translator, (ApplicationCommandType?)typeValue);

                if (createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "description")))
                    createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "description")).SetValue(translator, descriptionValues["en"]);

                Dictionary<string, string> NameTranslationDictionary = new();
                foreach (var nameTranslation in nameValues ?? new())
                {
                    if (nameTranslation.Key == "en")
                    {
                        NameTranslationDictionary.Add("en-GB", nameTranslation.Value);
                        NameTranslationDictionary.Add("en-US", nameTranslation.Value);
                        continue;
                    }

                    NameTranslationDictionary.Add(nameTranslation.Key, nameTranslation.Value);
                }
                if (createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "name_translations")))
                    createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "name_translations")).SetValue(translator, NameTranslationDictionary);

                Dictionary<string, string> DescriptionTranslationDictionary = new();
                foreach (var descriptionTranslations in descriptionValues ?? new())
                {
                    if (descriptionTranslations.Key == "en")
                    {
                        DescriptionTranslationDictionary.Add("en-GB", descriptionTranslations.Value);
                        DescriptionTranslationDictionary.Add("en-US", descriptionTranslations.Value);
                        continue;
                    }

                    DescriptionTranslationDictionary.Add(descriptionTranslations.Key, descriptionTranslations.Value);
                }
                if (createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "description_translations")))
                    createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "description_translations")).SetValue(translator, DescriptionTranslationDictionary);

                if (commandsValues is not null && createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "commands")))
                {
                    Log.Verbose("Creating sub-command translations for command '{name}'", nameValues.First());

                    var commandProperty = createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "commands"));
                    commandProperty.SetValue(translator, new List<DisCatSharp.ApplicationCommands.Entities.CommandTranslator>());
                    foreach (var value in commandsValues)
                    {
                        var obj = (DisCatSharp.ApplicationCommands.Entities.CommandTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.CommandTranslator), value);

                        if (obj is null)
                            continue;

                        ((List<DisCatSharp.ApplicationCommands.Entities.CommandTranslator>)commandProperty.GetValue(translator)).Add(obj);
                    }

                    if (((List<DisCatSharp.ApplicationCommands.Entities.CommandTranslator>)commandProperty.GetValue(translator)).Count == 0)
                        commandProperty.SetValue(translator, null);
                }

                if (optionsValues is not null && createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "options")))
                {
                    Log.Verbose("Creating option translations for command '{name}'", nameValues.First());

                    var optionProperty = createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "options"));
                    optionProperty.SetValue(translator, new List<DisCatSharp.ApplicationCommands.Entities.OptionTranslator>());
                    foreach (var value in optionsValues)
                    {
                        var obj = (DisCatSharp.ApplicationCommands.Entities.OptionTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.OptionTranslator), value);

                        if (obj is null)
                            continue;

                        ((List<DisCatSharp.ApplicationCommands.Entities.OptionTranslator>)optionProperty.GetValue(translator)).Add(obj);
                    }

                    if (((List<DisCatSharp.ApplicationCommands.Entities.OptionTranslator>)optionProperty.GetValue(translator)).Count == 0)
                        optionProperty.SetValue(translator, null);
                }

                if (choicesValues is not null && createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "choices")))
                {
                    Log.Verbose("Creating choice translations for command '{name}'", nameValues.First());

                    var choiceProperty = createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "choices"));
                    choiceProperty.SetValue(translator, new List<DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator>());
                    foreach (var value in choicesValues)
                    {
                        var obj = (DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator), value);

                        if (obj is null)
                            continue;

                        ((List<DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator>)choiceProperty.GetValue(translator)).Add(obj);
                    }

                    if (((List<DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator>)choiceProperty.GetValue(translator)).Count == 0)
                        choiceProperty.SetValue(translator, null);
                }

                if (groupsValues is not null && createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "groups")))
                {
                    Log.Verbose("Creating group translations for command '{name}'", nameValues.First());

                    var groupProperty = createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "groups"));
                    groupProperty.SetValue(translator, new List<DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator>());

                    foreach (var value in groupsValues)
                    {
                        var obj = (DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator), value);

                        if (obj is null)
                            continue;

                        ((List<DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator>)groupProperty.GetValue(translator)).Add(obj);
                    }

                    if (((List<DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator>)groupProperty.GetValue(translator)).Count == 0)
                        groupProperty.SetValue(translator, null);
                }

                return translator;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate DCS-Compatible Translations");
                throw;
            }
        }

        foreach (var translation in bot.LoadedTranslations.CommandList)
            singleCommandTranslations.Add(
                (DisCatSharp.ApplicationCommands.Entities.CommandTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.CommandTranslator), translation));

        foreach (var translation in bot.LoadedTranslations.CommandList)
            groupCommandTranslations.Add(
                (DisCatSharp.ApplicationCommands.Entities.GroupTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.GroupTranslator), translation));

        x.AddSingleTranslation(JsonConvert.SerializeObject(singleCommandTranslations));
        x.AddGroupTranslation(JsonConvert.SerializeObject(groupCommandTranslations));
    }

    public static async Task Load(Bot bot)
    {
        DisCatSharpExtensionsLoader.bot = bot;

        if (bot.status.LoadedConfig.Secrets.Discord.Token.Length <= 0)
        {
            Log.Fatal("No discord token provided");
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.NoToken);
            return;
        }

        Log.Debug("Registering DiscordClient..");

        bot.DiscordClient = new DiscordShardedClient(new DiscordConfiguration
        {
            Token = bot.status.LoadedConfig.Secrets.Discord.Token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Trace,
            Intents = DiscordIntents.All,
            AutoReconnect = true,
            LoggerFactory = bot.msLoggerFactory,
            HttpTimeout = TimeSpan.FromSeconds(60),
            MessageCacheSize = 4096,
            EnableSentry = true,
            ReportMissingFields = bot.status.LoadedConfig.IsDev,
            AttachUserInfo = true,
            DeveloperUserId = 411950662662881290,
            DisableUpdateCheck = true,
        });

        bot.ExperienceHandler = new(bot);

        Log.Debug("Registering CommandsNext..");

        var cNext = await bot.DiscordClient.UseCommandsNextAsync(new CommandsNextConfiguration
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



        Log.Debug("Registering Lavalink..");

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

        _ = await bot.DiscordClient.UseLavalinkAsync();

        Log.Debug("Registering DisCatSharp TwoFactor..");

        var tfa = bot.DiscordClient.UseTwoFactorAsync(new TwoFactorConfiguration
        {
            ResponseConfiguration = new TwoFactorResponseConfiguration
            {
                ShowResponse = false,
                AuthenticatorAccountPrefix = "Project Makoto"
            },
            Issuer = "Project Makoto",
        });

        DiscordEventHandler.SetupEvents(bot);
        bot.DiscordClient.GuildDownloadCompleted += bot.GuildDownloadCompleted;

        Log.Debug("Registering Interactivity..");
        _ = await bot.DiscordClient.UseInteractivityAsync(new InteractivityConfiguration { });

        var appCommands = await bot.DiscordClient.UseApplicationCommandsAsync(new ApplicationCommandsConfiguration
        {
            ServiceProvider = new ServiceCollection()
                                .AddSingleton(bot)
                                .BuildServiceProvider(),
            EnableDefaultHelp = false,
            EnableLocalization = true,
            DebugStartup = true
        });

        if (bot.status.CurrentAppHash != bot.status.LoadedConfig.DontModify.LastKnownHash)
        {
            Log.Debug("Clearing cached Commands..");
            await FileExtensions.CleanupFilesAndDirectories(new(), Directory.GetFiles("CompiledCommands").ToList());
        }
        if (!bot.status.LoadedConfig.IsDev)
        {
            appCommands.RegisterGlobalCommands<ApplicationCommands.MaintainersAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.ConfigurationAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.ModerationAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.ScoreSaberAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.MusicAppCommands>(GetCommandTranslations);
            appCommands.RegisterGlobalCommands<ApplicationCommands.UtilityAppCommands>(GetCommandTranslations);
        }
        else
        {
            appCommands.RegisterGuildCommands<ApplicationCommands.UtilityAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.MaintainersAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ConfigurationAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ModerationAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ScoreSaberAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.MusicAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
        }

        Log.Debug("Registering Commands..");
        cNext.RegisterCommands<PrefixCommands.UtilityPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.MusicPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.ScoreSaberPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.ModerationPrefixCommands>();
        cNext.RegisterCommands<PrefixCommands.ConfigurationPrefixCommands>();

        Log.Debug("Registering Command Converters..");
        cNext.RegisterConverter(new CustomArgumentConverter.BoolConverter());

        var commandsNextTypes = new List<Type>();
        var applicationCommandTypes = new List<Type>();

        await Util.Initializers.PluginLoader.LoadPluginCommands(bot, cNext, appCommands);

        _ = Task.Run(async () =>
        {
            while (!bot.status.DiscordInitialized)
                await Task.Delay(100);

            try
            {
                Log.Information("Connecting and authenticating with Lavalink..");
                bot.LavalinkSession = await bot.DiscordClient.GetFirstShard().GetLavalink().ConnectAsync(lavalinkConfig);
                Log.Information("Connected and authenticated with Lavalink.");

                bot.status.LavalinkInitialized = true;

                try
                {
                    Log.Information("Lavalink is running on {Version}.", (await bot.LavalinkSession.GetLavalinkInfoAsync()).Version.Semver);
                }
                catch { }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception occurred while trying to log into Lavalink");
                return;
            }
        });

        _ = Task.Run(async () =>
        {
            while (!bot.status.DiscordInitialized)
                await Task.Delay(100);

            Stopwatch sw = new();
            sw.Start();

            _ = bot.DiscordClient.UpdateStatusAsync(userStatus: UserStatus.Online, activity: new DiscordActivity("Registering commands..", ActivityType.Custom));

            var applicationCommandsExtension = bot.DiscordClient.GetFirstShard().GetApplicationCommands();
            while (applicationCommandsExtension?.RegisteredCommands?.Count == 0 && sw.ElapsedMilliseconds < TimeSpan.FromMinutes(5).TotalMilliseconds)
                await Task.Delay(1000);

            if (applicationCommandsExtension?.RegisteredCommands?.Count == 0)
            {
                Log.Fatal("Commands did not register.");
                _ = bot.ExitApplication(true);
                return;
            }

            bot.status.DiscordCommandsRegistered = true;

            while (true)
            {
                try
                {
                    if (bot.DatabaseClient.Disposed)
                        return;

                    await bot.DiscordClient.UpdateStatusAsync(activity: new DiscordActivity($"{bot.DiscordClient.GetGuilds().Count.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))} guilds | Up for {Math.Round((DateTime.UtcNow - bot.status.startupTime).TotalHours, 1).ToString(CultureInfo.CreateSpecificCulture("en-US"))}h", ActivityType.Custom));
                    await Task.Delay(30000);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to update user status");
                    await Task.Delay(30000);
                }
            }
        });
    }
}
