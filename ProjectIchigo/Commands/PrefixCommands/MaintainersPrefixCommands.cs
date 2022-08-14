namespace ProjectIchigo.PrefixCommands;
internal class MaintainersPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }

    [Group("dev_tools"),
    CommandModule("maintainence"),
    Description("Developer Tools used to develop/manage Ichigo.")]
    public class DevTools : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, "", "", "");
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, "", "", "");
            }).Add(_bot._watcher, ctx);
        }

        [Command("info"), Description("Shows information about the current server and bot.")]
        public async Task InfoCommand(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new InfoCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
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
            }).Add(_bot._watcher, ctx);
        }

        [Command("globalban"), Description("Bans a user from all servers opted into global bans.")]
        public async Task Globalban(CommandContext ctx, DiscordUser victim, [RemainingText][Description("Reason")] string reason = "-")
        {
            Task.Run(async () =>
            {
                await new GlobalBanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                    { "reason", reason },
                });
            }).Add(_bot._watcher, ctx);
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
            }).Add(_bot._watcher, ctx);
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
            }).Add(_bot._watcher, ctx);
        }

        [Command("register"), Description("Register Slash Commands. Debug only.")]
        public async Task RegisterCommands(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new RegisterCommandsCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
        
        [Command("test"), Description("Check description lenghts.")]
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
            }).Add(_bot._watcher, ctx);
        }

        [Command("stop"), Description("Shuts down the bot.")]
        public async Task Stop(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new StopCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("save"), Description("Save all data to Database.")]
        public async Task Save(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new SaveCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
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
            }).Add(_bot._watcher, ctx);
        }

#if DEBUG
        [Command("db-test1"), Description(" ")]
        public async Task DbTest1(CommandContext ctx, bool UseOldTagsSelector = false)
        {
            var v = new List<DatabaseQueue.RequestQueue>
            {
                new DatabaseQueue.RequestQueue
                {
                    RequestType = DatabaseRequestType.Ping, Connection = _bot._databaseClient.mainDatabaseConnection, Priority = QueuePriority.Low
                },
                new DatabaseQueue.RequestQueue
                {
                    RequestType = DatabaseRequestType.Ping, Connection = _bot._databaseClient.mainDatabaseConnection, Priority = QueuePriority.Normal
                },
                new DatabaseQueue.RequestQueue
                {
                    RequestType = DatabaseRequestType.Ping, Connection = _bot._databaseClient.mainDatabaseConnection, Priority = QueuePriority.Low
                },
                new DatabaseQueue.RequestQueue
                {
                    RequestType = DatabaseRequestType.Ping, Connection = _bot._databaseClient.mainDatabaseConnection, Priority = QueuePriority.High
                }
            };

            v.Sort((a, b) => ((int)a.Priority).CompareTo((int)b.Priority));

            _bot._databaseClient._queue.Queue = v;
        }
#endif
    }
}
