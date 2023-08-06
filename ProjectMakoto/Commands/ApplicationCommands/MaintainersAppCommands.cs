// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;

[ModulePriority(-999)]

public sealed class MaintainersAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    internal enum DevCommands
    {
        Info,
        Log,
        Save,
        Stop,
        BotNick,
        Evaluate,
        CreateIssue,
        Enroll2FA,
        Quit2FASession,
        Disenroll2FAUser,
        ManageCommands,
        GlobalBan,
        GlobalUnban,
        GlobalNotes,
        BanUser,
        UnbanUser,
        BanGuild,
        UnbanGuild,
        BatchLookup,
        RawGuild,
    };

    public sealed class MaintainerAutoComplete : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
        {
            try
            {
                var bot = ((Bot)ctx.Services.GetService(typeof(Bot)));

                if (!ctx.User.IsMaintenance(bot.status))
                    return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();

                var filteredCommands = Enum.GetNames(typeof(DevCommands))
                    .Where(x => x.Contains(ctx.FocusedOption.Value.ToString(), StringComparison.InvariantCultureIgnoreCase)).Take(25);

                var options = filteredCommands
                    .Select(x => new DiscordApplicationCommandAutocompleteChoice(x, x)).ToList();
                return options.AsEnumerable();
            }
            catch (Exception)
            {
                return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
            }
        }
    }

    public sealed class ArgumentAutoComplete : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
        {
            try
            {
                var bot = ((Bot)ctx.Services.GetService(typeof(Bot)));

                if (!ctx.User.IsMaintenance(bot.status))
                    return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();

                if (ctx.Options.Any(x => x.Name == "command"))
                {
                    var currentArgument = ctx.FocusedOption.Name switch
                    {
                        "argument1" => 1,
                        "argument2" => 2,
                        "argument3" => 3,
                        "argument4" => 4,
                        _ => -1,
                    };

                    if (ctx.FocusedOption.Value.ToString().Length > 1)
                        return new List<DiscordApplicationCommandAutocompleteChoice>() { };

                    var Command = (DevCommands)Enum.Parse(typeof(DevCommands), ctx.Options.First(x => x.Name == "command").Value.ToString());

                    return Command switch
                    {
                        DevCommands.RawGuild => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("GuildId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.BotNick => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("NewNickName", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.BanUser => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            2 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("Reason", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.UnbanUser => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.BanGuild => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("GuildId", "") },
                            2 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("Reason", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.UnbanGuild => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("GuildId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.GlobalBan => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            2 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("Reason", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.GlobalUnban => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            2 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UnbanFromServers (True/False)", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.GlobalNotes => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.Log => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("LogLevel", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.BatchLookup => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId, UserId, UserId, ...", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.CreateIssue => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UseOldSelector (True/False)", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.Evaluate => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("MessageId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        DevCommands.Disenroll2FAUser => currentArgument switch
                        {
                            1 => new List<DiscordApplicationCommandAutocompleteChoice>() { new("UserId", "") },
                            _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                        },
                        _ => new List<DiscordApplicationCommandAutocompleteChoice>() { },
                    };
                }

                return new List<DiscordApplicationCommandAutocompleteChoice>() { };
            }
            catch (Exception)
            {
                return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
            }
        }
    }

    [SlashCommand("developertools", "Developer Tools used to manage Makoto.", dmPermission: false, defaultMemberPermissions: (long)Permissions.None)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0075:Simplify conditional expression", Justification = "<Pending>")]
    public async Task DevTools(InteractionContext ctx,
        [Autocomplete(typeof(MaintainerAutoComplete))][Option("command", "The command to run.", true)] string command,
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

#pragma warning disable CS8321 // Local function is declared but never used
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

        DevCommands Command;

        try
        {
            Command = (DevCommands)Enum.Parse(typeof(DevCommands), command);
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
                case DevCommands.Info:
                    _ = Task.Run(async () =>
                    {
                        await new Commands.DevTools.InfoCommand().ExecuteCommand(ctx, this._bot);
                    });
                    break;
                case DevCommands.RawGuild:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new Commands.DevTools.RawGuildCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "guild", argument1 is not null ? Convert.ToUInt64(argument1) : null }
                        });
                    });
                    break;
                case DevCommands.BotNick:
                    _ = Task.Run(async () =>
                    {
                        await new Commands.DevTools.BotnickCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "newNickname", argument1 }
                        });
                    });
                    break;
                case DevCommands.BanUser:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                            return;

                        await new Commands.DevTools.BanUserCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "reason", argument2 },
                        });
                    });
                    break;
                case DevCommands.UnbanUser:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new Commands.DevTools.UnbanUserCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                        });
                    });
                    break;
                case DevCommands.BanGuild:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                            return;

                        await new Commands.DevTools.BanGuildCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "guild", Convert.ToUInt64(argument1) },
                            { "reason", argument2 }
                        });
                    });
                    break;
                case DevCommands.UnbanGuild:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new Commands.DevTools.UnbanGuildCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "guild", Convert.ToUInt64(argument1) },
                        });
                    });
                    break;
                case DevCommands.GlobalBan:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                            return;

                        await new Commands.DevTools.GlobalBanCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "reason", argument2 },
                        });
                    });
                    break;
                case DevCommands.GlobalUnban:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                            return;

                        await new Commands.DevTools.GlobalUnbanCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "UnbanFromGuilds", bool.Parse(argument2) },
                        });
                    });
                    break;
                case DevCommands.GlobalNotes:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new Commands.DevTools.GlobalNotesCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                        });
                    });
                    break;
                case DevCommands.Log:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new Commands.DevTools.LogCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "Level", (CustomLogLevel)Enum.Parse(typeof(CustomLogLevel), argument1) },
                        });
                    });
                    break;
                case DevCommands.Stop:
                    _ = Task.Run(async () =>
                    {
                        await new Commands.DevTools.StopCommand().ExecuteCommandWith2FA(ctx, this._bot, null);
                    });
                    break;
                case DevCommands.Save:
                    _ = Task.Run(async () =>
                    {
                        await new Commands.DevTools.SaveCommand().ExecuteCommand(ctx, this._bot);
                    });
                    break;
                case DevCommands.BatchLookup:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new Commands.DevTools.BatchLookupCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "IDs", argument1 },
                        });
                    });
                    break;
                case DevCommands.CreateIssue:
                    _ = Task.Run(async () =>
                    {
                        await new Commands.DevTools.CreateIssueCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "UseOldTagsSelector", (bool.TryParse(argument1, out var result) ? result : true) },
                        }, InitiateInteraction: false);
                    });
                    break;
                case DevCommands.Evaluate:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        var id = Convert.ToUInt64(argument1);
                        var message = await ctx.Channel.GetMessageAsync(id);

                        await new Commands.DevTools.EvaluationCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "code", message.Content },
                        });
                    });
                    break;
                case DevCommands.Enroll2FA:
                    _ = Task.Run(async () =>
                    {
                        await new Commands.DevTools.EnrollTwoFactorCommand().ExecuteCommand(ctx, this._bot);
                    });
                    break;
                case DevCommands.Quit2FASession:
                    _ = Task.Run(async () =>
                    {
                        await new Commands.DevTools.Quit2FASessionCommand().ExecuteCommand(ctx, this._bot);
                    });
                    break;

                case DevCommands.Disenroll2FAUser:
                    _ = Task.Run(async () =>
                    {
                        if (!Require1())
                            return;

                        await new Commands.DevTools.Disenroll2FAUserCommand().ExecuteCommandWith2FA(ctx, this._bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                        });
                    });
                    break;
                case DevCommands.ManageCommands:
                    _ = Task.Run(async () =>
                    {
                        await new Commands.DevTools.CommandManageCommand().ExecuteCommand(ctx, this._bot);
                    });
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
    public sealed class Debug : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("throw", "Throw.")]
        public async Task Throw(InteractionContext ctx)
            => _ = new Commands.Debug.ThrowCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("test", "Test.")]
        public async Task Test(InteractionContext ctx)
        {
            _ = Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
                await ctx.Channel.ModifyAsync(x => x.PermissionOverwrites = ctx.Channel.PermissionOverwrites.Merge(ctx.Member, Permissions.UseExternalEmojis, Permissions.None));
                await ctx.Channel.ModifyAsync(x => x.PermissionOverwrites = ctx.Channel.PermissionOverwrites.Merge(ctx.Member, Permissions.None, Permissions.UseExternalEmojis));
            });
        }
    }
#endif
}
