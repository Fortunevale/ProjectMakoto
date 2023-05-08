// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Plugins.Commands;

namespace ProjectMakoto.ApplicationCommands;

public class MaintainersAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    public class MaintainerAutoComplete : IAutocompleteProvider
    {
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
        };

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

    [SlashCommand("developertools", "Developer Tools used to manage Makoto.", dmPermission: false, defaultMemberPermissions: (long)Permissions.None)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0075:Simplify conditional expression", Justification = "<Pending>")]
    public async Task DevTools(InteractionContext ctx, [Autocomplete(typeof(MaintainerAutoComplete))] [Option("command", "The command to run.", true)]string command, [Option("argument1", "Argument 1, if required")]string argument1 = "", [Option("argument2", "Argument 2, if required")]string argument2 = "", [Option("argument3", "Argument 3, if required")]string argument3 = "")
    {
        bool Require1()
        {
            if (argument1.IsNullOrWhiteSpace())
                return false;
            else
                return true;
        }

        bool Require2()
        {
            if (argument1.IsNullOrWhiteSpace())
                return false;
            else
                return true;
        }

        bool Require3()
        {
            if (argument1.IsNullOrWhiteSpace())
                return false;
            else
                return true;
        }

        if (!ctx.User.IsMaintenance(_bot.status))
        {
            DummyCommand dummyCommand = new();
            await dummyCommand.ExecuteCommand(ctx, this._bot);

            dummyCommand.SendMaintenanceError();
            return;
        }

        MaintainerAutoComplete.Commands Command;

        try
        {
            Command = (MaintainerAutoComplete.Commands)Enum.Parse(typeof(MaintainerAutoComplete.Commands), command);
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
                case MaintainerAutoComplete.Commands.Info:
                    Task.Run(async () =>
                    {
                        await new InfoCommand().ExecuteCommand(ctx, _bot);
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.RawGuild:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 required").AsEphemeral());
                            return;
                        }

                        await new RawGuildCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "guild", argument1 is not null ? Convert.ToUInt64(argument1) : null }
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.BotNick:
                    Task.Run(async () =>
                    {
                        await new BotnickCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "newNickname", argument1 }
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.BanUser:
                    Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 & 2 required").AsEphemeral());
                            return;
                        }

                        await new BanUserCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "reason", argument2 },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.UnbanUser:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 required").AsEphemeral());
                            return;
                        }

                        await new UnbanUserCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.BanGuild:
                    Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 & 2 required").AsEphemeral());
                            return;
                        }

                        await new BanGuildCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "guild", Convert.ToUInt64(argument1) },
                            { "reason", argument2 },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.UnbanGuild:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 required").AsEphemeral());
                            return;
                        }

                        await new UnbanGuildCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "guild", Convert.ToUInt64(argument1) },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.GlobalBan:
                    Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 & 2 required").AsEphemeral());
                            return;
                        }

                        await new GlobalBanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "reason", argument2 },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.GlobalUnban:
                    Task.Run(async () =>
                    {
                        if (!Require1() || !Require2())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 & 2 required").AsEphemeral());
                            return;
                        }

                        await new GlobalUnbanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                        { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                            { "UnbanFromGuilds", bool.Parse(argument2) },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.GlobalNotes:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 required").AsEphemeral());
                            return;
                        }

                        await new GlobalNotesCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "victim", await DiscordExtensions.ParseStringAsUser(argument1, ctx.Client) },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.Log:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 required").AsEphemeral());
                            return;
                        }

                        await new Commands.LogCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "Level", (LogLevel)Enum.Parse(typeof(LogLevel), argument1) },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.Stop:
                    Task.Run(async () =>
                    {
                        await new StopCommand().ExecuteCommand(ctx, _bot);
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.Save:
                    Task.Run(async () =>
                    {
                        await new SaveCommand().ExecuteCommand(ctx, _bot);
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.BatchLookup:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 required").AsEphemeral());
                            return;
                        }

                        await new BatchLookupCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "IDs", argument1 },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.CreateIssue:
                    Task.Run(async () =>
                    {
                        await new Commands.CreateIssueCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "UseOldTagsSelector", (bool.TryParse(argument1, out var result) ? result : true) },
                        }, InitiateInteraction: false);
                    }).Add(_bot.watcher, ctx);
                    break;
                case MaintainerAutoComplete.Commands.Evaluate:
                    Task.Run(async () =>
                    {
                        if (!Require1())
                        {
                            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Argument 1 required").AsEphemeral());
                            return;
                        }

                        var id = Convert.ToUInt64(argument1);
                        var message = await ctx.Channel.GetMessageAsync(id);

                        await new EvaluationCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                        {
                            { "code", message.Content },
                        });
                    }).Add(_bot.watcher, ctx);
                    break;
            }
        }
        catch (Exception)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Exception occured, check console.").AsEphemeral());
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
            }).Add(_bot.watcher, ctx);
        }
    }
#endif
}
