// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;

public class MaintainersAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    internal enum Commands
    {
        Info,
        RawGuild,
        BotNick,
        BanUser,
        UnbanUser,
        BanGuild,
        UnbanGuild,
        GlobalBan,
        GlobalUnban,
        GlobalNotes,
        Log,
        Stop,
        Save,
        BatchLookup,
        CreateIssue,
        Evaluate,
        Enroll2FA,
        Quit2FASession,
        Disenroll2FAUser,
    };

    public class MaintainerAutoComplete : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
        {
            try
            {
                Bot bot = ((Bot)ctx.Services.GetService(typeof(Bot)));

                if (!ctx.User.IsMaintenance(bot.status))
                    return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();

                IEnumerable<string> filteredCommands = Enum.GetNames(typeof(Commands))
                    .Where(x => x.Contains(ctx.FocusedOption.Value.ToString(), StringComparison.InvariantCultureIgnoreCase)).Take(25);

                List<DiscordApplicationCommandAutocompleteChoice> options = filteredCommands
                    .Select(x => new DiscordApplicationCommandAutocompleteChoice(x, x)).ToList();
                return options.AsEnumerable();
            }
            catch (Exception ex)
            {
                return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
            }
        }
    }

    public class ArgumentAutoComplete : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
        {
            try
            {
                Bot bot = ((Bot)ctx.Services.GetService(typeof(Bot)));

                if (!ctx.User.IsMaintenance(bot.status))
                    return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();

                if (ctx.Options.Any(x => x.Name == "command"))
                {
                    int currentArgument = ctx.FocusedOption.Name switch
                    {
                        "argument1" => 1,
                        "argument2" => 2,
                        "argument3" => 3,
                        "argument4" => 4,
                        _ => -1,
                    };

                    if (ctx.FocusedOption.Value.ToString().Length > 1)
                        return new List<DiscordApplicationCommandAutocompleteChoice>() { };

                    Commands Command = (Commands)Enum.Parse(typeof(Commands), ctx.Options.First(x => x.Name == "command").Value.ToString());

                    return Command switch
                    {
                        Commands.RawGuild => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("GuildId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.BotNick => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("NewNickName", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.BanUser => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            2 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("Reason", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.UnbanUser => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.BanGuild => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("GuildId", "") },
                            2 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("Reason", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.UnbanGuild => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("GuildId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.GlobalBan => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            2 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("Reason", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.GlobalUnban => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            2 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UnbanFromServers (True/False)", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.GlobalNotes => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.Log => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("LogLevel", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.BatchLookup => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId, UserId, UserId, ...", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.CreateIssue => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UseOldSelector (True/False)", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.Evaluate => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("MessageId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        Commands.Disenroll2FAUser => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                    };
                }

                return new List<DiscordApplicationCommandAutocompleteChoice>() { };
            }
            catch (Exception ex)
            {
                return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
            }
        }
    }

    [SlashCommand("developertools", "Developer Tools used to manage Makoto.", dmPermission: false, defaultMemberPermissions: (long)Permissions.None)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0075:Simplify conditional expression", Justification = "<Pending>")]
    public async Task DevTools(InteractionContext ctx, 
        [Autocomplete(typeof(MaintainerAutoComplete))] [Option("command", "The command to run.", true)]string command, 
        [Autocomplete(typeof(ArgumentAutoComplete))][Option("argument1", "Argument 1, if required", true)] string argument1 = "", 
        [Autocomplete(typeof(ArgumentAutoComplete))][Option("argument2", "Argument 2, if required", true)] string argument2 = "",
        [Autocomplete(typeof(ArgumentAutoComplete))][Option("argument3", "Argument 3, if required", true)] string argument3 = "")
    {
        bool Require1()
        {
            if (argument1.IsNullOrWhiteSpace())
            {
                _ = ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 required").AsEphemeral());
                return false;
            }
            else
                return true;
        }

        bool Require2()
        {
            if (argument2.IsNullOrWhiteSpace())
            {
                _ = ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 2 required").AsEphemeral());
                return false;
            }
            else
                return true;
        }

        bool Require3()
        {
            if (argument3.IsNullOrWhiteSpace())
            {
                _ = ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 3 required").AsEphemeral());
                return false;
            }
            else
                return true;
        }



        if (!ctx.User.IsMaintenance(this._bot.status))
        {
            DummyCommand dummyCommand = new();
            await dummyCommand.ExecuteCommand(ctx, this._bot);

            dummyCommand.SendMaintenanceError();
            return;
        }

        Commands Command;

        try
        {
            Command = (Commands)Enum.Parse(typeof(Commands), command);
        }
        catch (Exception)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Specified Command does not exist.").AsEphemeral());
            return;
        }

        try
        {
            switch (Command)
            {
                case Commands.Info:
                    Task.Run(async () =>
                    {
                        await new InfoCommand().ExecuteCommand(ctx, this._bot);
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.RawGuild:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new RawGuildCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "guild", argument1 is not null ? Convert.ToUInt64(argument1) : null }
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.BotNick:
                    Task.Run(async () =>
                    {
                        await new BotnickCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "newNickname", argument1 }
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.BanUser:
                    Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                            return;

                        await new BanUserCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "reason", argument2 },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.UnbanUser:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new UnbanUserCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.BanGuild:
                    Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                            return;

                        await new BanGuildCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "guild", Convert.ToUInt64(argument1) },
                            { "reason", argument2 }
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.UnbanGuild:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new UnbanGuildCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "guild", Convert.ToUInt64(argument1) },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.GlobalBan:
                    Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                            return;

                        await new GlobalBanCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "reason", argument2 },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.GlobalUnban:
                    Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                            return;

                        await new GlobalUnbanCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "UnbanFromGuilds", bool.Parse(argument2) },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.GlobalNotes:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new GlobalNotesCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.Log:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new LogCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "Level", (LogLevel)Enum.Parse(typeof(LogLevel), argument1) },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.Stop:
                    Task.Run(async () =>
                    {
                        await new StopCommand().ExecuteCommandWith2FA(ctx, this._bot, null);
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.Save:
                    Task.Run(async () =>
                    {
                        await new SaveCommand().ExecuteCommand(ctx, this._bot);
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.BatchLookup:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new BatchLookupCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "IDs", argument1 },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.CreateIssue:
                    Task.Run(async () =>
                    {
                        await new CreateIssueCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "UseOldTagsSelector", (bool.TryParse(argument1, out var result) ? result : true) },
                        }, InitiateInteraction: false);
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.Evaluate:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        var id = Convert.ToUInt64(argument1);
                        var message = await ctx.Channel.GetMessageAsync(id);

                        await new EvaluationCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "code", message.Content },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.Enroll2FA:
                    Task.Run(async () =>
                    {
                        await new EnrollTwoFactorCommand().ExecuteCommand(ctx, this._bot);
                    }).Add(this._bot.watcher, ctx);
                    break;
                case Commands.Quit2FASession:
                    Task.Run(async () =>
                    {
                        await new Quit2FASessionCommand().ExecuteCommand(ctx, this._bot);
                    }).Add(this._bot.watcher, ctx);
                    break;

                case Commands.Disenroll2FAUser:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new Disenroll2FAUserCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                        });
                    }).Add(this._bot.watcher, ctx);
                    break;
            }
        }
        catch (Exception)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Exception occurred, check console.").AsEphemeral());
        }
    }

#if DEBUG
    [SlashCommandGroup("debug", "Debug commands, only registered in this server.")]
    public class Debug : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("throw", "Throw.")]
        public async Task Throw(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
                throw new InvalidCastException();
            }).Add(this._bot.watcher, ctx);
        }
    }
#endif
}
