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
            ReportMissingFields = bot.status.LoadedConfig.IsDev,
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
            },
            Issuer = "Project Makoto",
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
            List<DisCatSharp.ApplicationCommands.Entities.CommandTranslator> singleCommandTranslations = new();
            List<DisCatSharp.ApplicationCommands.Entities.GroupTranslator> groupCommandTranslations = new();

            object CreateTranslationRecursively(Type typeToCreate, CommandTranslation translation)
            {
                try
                {
                    SingleTranslationKey nameValues = translation.Names;

                    if (nameValues is null)
                        return null;

                    SingleTranslationKey? descriptionValues = translation.Descriptions;
                    int? typeValue = translation.Type;
                    CommandTranslation[]? optionsValues = translation.Options;
                    CommandTranslation[] choicesValues = translation.Choices;
                    CommandTranslation[] groupsValues = translation.Groups;
                    CommandTranslation[] commandsValues = translation.Commands;

                    _logger.LogTrace("Creating instance of '{type}'", typeToCreate.Name);
                    var translator = Activator.CreateInstance(typeToCreate);

                    PropertyInfo[] createTypeProperties = typeToCreate.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
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
                        _logger.LogTrace("Creating sub-command translations for command '{name}'", nameValues.First());

                        PropertyInfo commandProperty = createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "commands"));
                        commandProperty.SetValue(translator, new List<DisCatSharp.ApplicationCommands.Entities.CommandTranslator>());
                        foreach (var value in commandsValues)
                        {
                            var obj = (DisCatSharp.ApplicationCommands.Entities.CommandTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.CommandTranslator), value);

                            if (obj is null)
                                continue;

                            ((List<DisCatSharp.ApplicationCommands.Entities.CommandTranslator>)commandProperty.GetValue(translator)).Add(obj);
                        }

                        if (!((List<DisCatSharp.ApplicationCommands.Entities.CommandTranslator>)commandProperty.GetValue(translator)).Any())
                            commandProperty.SetValue(translator, null);
                    }

                    if (optionsValues is not null && createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "options")))
                    {
                        _logger.LogTrace("Creating option translations for command '{name}'", nameValues.First());

                        PropertyInfo optionProperty = createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "options"));
                        optionProperty.SetValue(translator, new List<DisCatSharp.ApplicationCommands.Entities.OptionTranslator>());
                        foreach (var value in optionsValues)
                        {
                            var obj = (DisCatSharp.ApplicationCommands.Entities.OptionTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.OptionTranslator), value);

                            if (obj is null)
                                continue;

                            ((List<DisCatSharp.ApplicationCommands.Entities.OptionTranslator>)optionProperty.GetValue(translator)).Add(obj);
                        }

                        if (!((List<DisCatSharp.ApplicationCommands.Entities.OptionTranslator>)optionProperty.GetValue(translator)).Any())
                            optionProperty.SetValue(translator, null);
                    }

                    if (choicesValues is not null && createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "choices")))
                    {
                        _logger.LogTrace("Creating choice translations for command '{name}'", nameValues.First());

                        PropertyInfo choiceProperty = createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "choices"));
                        choiceProperty.SetValue(translator, new List<DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator>());
                        foreach (var value in choicesValues)
                        {
                            var obj = (DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator), value);

                            if (obj is null)
                                continue;

                            ((List<DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator>)choiceProperty.GetValue(translator)).Add(obj);
                        }

                        if (!((List<DisCatSharp.ApplicationCommands.Entities.ChoiceTranslator>)choiceProperty.GetValue(translator)).Any())
                            choiceProperty.SetValue(translator, null);
                    }

                    if (groupsValues is not null && createTypeProperties.Any(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "groups")))
                    {
                        _logger.LogTrace("Creating group translations for command '{name}'", nameValues.First());

                        PropertyInfo groupProperty = createTypeProperties.First(x => x.GetCustomAttributes().Any(attr => attr is JsonPropertyAttribute attribute && attribute.PropertyName == "groups"));
                        groupProperty.SetValue(translator, new List<DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator>());

                        foreach (var value in groupsValues)
                        {
                            var obj = (DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator), value);

                            if (obj is null)
                                continue;

                            ((List<DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator>)groupProperty.GetValue(translator)).Add(obj);
                        }

                        if (!((List<DisCatSharp.ApplicationCommands.Entities.SubGroupTranslator>)groupProperty.GetValue(translator)).Any())
                            groupProperty.SetValue(translator, null);
                    }

                    return translator;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to generate DCS-Compatible Translations", ex);
                    throw;
                }
            }

            foreach (var translation in bot.LoadedTranslations.Commands.CommandList)
                singleCommandTranslations.Add(
                    (DisCatSharp.ApplicationCommands.Entities.CommandTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.CommandTranslator), translation));
            
            foreach (var translation in bot.LoadedTranslations.Commands.CommandList)
                groupCommandTranslations.Add(
                    (DisCatSharp.ApplicationCommands.Entities.GroupTranslator)CreateTranslationRecursively(typeof(DisCatSharp.ApplicationCommands.Entities.GroupTranslator), translation));

            x.AddSingleTranslation(JsonConvert.SerializeObject(singleCommandTranslations));
            x.AddGroupTranslation(JsonConvert.SerializeObject(groupCommandTranslations));
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
            appCommands.RegisterGuildCommands<ApplicationCommands.UtilityAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.MaintainersAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ConfigurationAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ModerationAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.SocialAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.ScoreSaberAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
            appCommands.RegisterGuildCommands<ApplicationCommands.MusicAppCommands>(bot.status.LoadedConfig.Discord.DevelopmentGuild, GetCommandTranslations);
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
                    _logger.LogInfo("Lavalink is running on {Version}.", (await bot.LavalinkSession.GetLavalinkInfoAsync()).Version.Semver);
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

            while (bot.DiscordClient.GetApplicationCommands()?.RegisteredCommands?.Count == 0)
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
