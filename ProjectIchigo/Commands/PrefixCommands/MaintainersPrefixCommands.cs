namespace ProjectIchigo.PrefixCommands;
public class MaintainersPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }

    [Group("dev_tools"),
    CommandModule("maintenance"),
    Description("Developer Tools used to develop/manage Ichigo.")]
    public class DevTools : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, "", "", "");
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, "", "", "");
            }).Add(_bot.watcher, ctx);
        }

        [Command("info"), Description("Shows information about the current server and bot.")]
        public async Task InfoCommand(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new InfoCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("raw-guild"), Description("RawGuild Entry")]
        public async Task RawGuild(CommandContext ctx, [Description("GuildId")] ulong? guild = null)
        {
            Task.Run(async () =>
            {
                await new RawGuildCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "guild", guild }
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("botnick"), Description("Changes the bot's nickname on the current server.")]
        public async Task BotNick(CommandContext ctx, string newNickname = "")
        {
            Task.Run(async () =>
            {
                await new BotnickCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "newNickname", newNickname }
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("ban-user"), Description("Bans a user from usage of the bot.")]
        public async Task BanUser(CommandContext ctx, DiscordUser victim, [RemainingText][Description("Reason")] string reason = "")
        {
            Task.Run(async () =>
            {
                await new BanUserCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                    { "reason", reason },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("unban-user"), Description("Unbans a user from usage of the bot.")]
        public async Task UnbanUser(CommandContext ctx, DiscordUser victim)
        {
            Task.Run(async () =>
            {
                await new UnbanUserCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("ban-guild"), Description("Bans a guild from usage of the bot.")]
        public async Task BanGuild(CommandContext ctx, ulong guild, [RemainingText][Description("Reason")] string reason = "")
        {
            Task.Run(async () =>
            {
                await new BanGuildCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "guild", guild },
                    { "reason", reason },
                });
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("unban-guild"), Description("Unbans a guild from usage of the bot.")]
        public async Task UnbanGuild(CommandContext ctx, ulong guild)
        {
            Task.Run(async () =>
            {
                await new UnbanGuildCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "guild", guild },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("globalban"), Description("Bans a user from all servers opted into global bans.")]
        public async Task Globalban(CommandContext ctx, DiscordUser victim, [RemainingText][Description("Reason")] string reason)
        {
            Task.Run(async () =>
            {
                await new GlobalBanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                    { "reason", reason },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("globalnotes"), Description("Add and remove global notes of a user.")]
        public async Task GlobalBanCommand(CommandContext ctx, DiscordUser victim)
        {
            Task.Run(async () =>
            {
                await new GlobalNotesCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("globalunban"), Description("Removes a user from global bans. (doesn't unban user from all servers)")]
        public async Task GlobalUnbanCommand(CommandContext ctx, DiscordUser victim, bool UnbanFromGuilds = true)
        {
            Task.Run(async () =>
            {
                await new GlobalUnbanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                    { "UnbanFromGuilds", UnbanFromGuilds },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("log"), Description("Change the bot's log level.")]
        public async Task Log(CommandContext ctx, int Level)
        {
            Task.Run(async () =>
            {
                await new LogCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "Level", Level },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("test"), Description("Check description lengths.")]
        public async Task Test(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                List<string> list = new();

                var cCtx = ctx.Client.GetCommandsNext();

                foreach (var b in cCtx.RegisteredCommands)
                {
                    if (b.Value.Description.Length > 100)
                        if (!list.Contains(b.Value.Name))
                            list.Add(b.Value.Name);

                    try
                    {
                        foreach (var c in ((CommandGroup)b.Value).Children)
                        {
                            if (c.Description.Length > 100)
                                if (!list.Contains($"{b.Value.Name} {c.Name}"))
                                    list.Add($"{b.Value.Name} {c.Name}");
                        }
                    }
                    catch (Exception ex) { _logger.LogError(b.Value.Name, ex); }
                }

                await ctx.RespondAsync(string.Join("\n", list.Select(x => $"`{x}`")));
            }).Add(_bot.watcher, ctx);
        }

        [Command("stop"), Description("Shuts down the bot.")]
        public async Task Stop(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new StopCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("save"), Description("Save all data to Database.")]
        public async Task Save(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new SaveCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("batch-lookup"), Description("Looks up multiple users. Separate by spaces.")]
        public async Task BatchLookupCommand(CommandContext ctx, [RemainingText][Description("List of Users")] string victims)
        {
            Task.Run(async () =>
            {
                await new BatchLookupCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "IDs", victims },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("create-issue"), Description("Create a new issue on Ichigo's Github Repository.")]
        public async Task CreateIssue(CommandContext ctx, bool UseOldTagsSelector = false)
        {
            Task.Run(async () =>
            {
                await new CreateIssueCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "UseOldTagsSelector", UseOldTagsSelector },
                });
            }).Add(_bot.watcher, ctx);
        }
    }
}
