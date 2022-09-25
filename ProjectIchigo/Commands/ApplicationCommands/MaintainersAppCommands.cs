namespace ProjectIchigo.ApplicationCommands;

public class MaintainersAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("dev_tools", "Developer Tools used to develop/manage Ichigo.")]
    public class DevTools : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("info", "Shows information about the current server and bot.")]
        public async Task InfoCommand(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new InfoCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("botnick", "Changes the bot's nickname on the current server.")]
        public async Task BotNick(InteractionContext ctx, [Option("nickname", "The new nickname")] string newNickname = "")
        {
            Task.Run(async () =>
            {
                await new BotnickCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "newNickname", newNickname }
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("ban-user", "Bans a user from usage of the bot.")]
        public async Task BanUser(InteractionContext ctx, [Option("victim", "The user to ban")] DiscordUser victim, [Option("reason", "The reason")] string reason = "")
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

        [SlashCommand("unban-user", "Unbans a user from usage of the bot.")]
        public async Task UnbanUser(InteractionContext ctx, [Option("victim", "The user to unban")] DiscordUser victim)
        {
            Task.Run(async () =>
            {
                await new UnbanUserCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("ban-guild", "Bans a guild from usage of the bot.")]
        public async Task BanGuild(InteractionContext ctx, [Option("guild", "The guild to ban")] string guild, [Option("reason", "The reason")] string reason = "")
        {
            Task.Run(async () =>
            {
                await new BanGuildCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "guild", Convert.ToUInt64(guild) },
                    { "reason", reason },
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("unban-guild", "Unbans a guild from usage of the bot.")]
        public async Task UnbanGuild(InteractionContext ctx, [Option("guild", "The guild to unban")] string guild)
        {
            Task.Run(async () =>
            {
                await new UnbanGuildCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "guild", Convert.ToUInt64(guild) },
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("globalban", "Bans a user from all servers opted into global bans.")]
        public async Task GlobalBanCommand(InteractionContext ctx, [Option("user", "The user to ban")]DiscordUser victim, [Option("reason", "The reason")]string reason)
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
        
        [SlashCommand("globalunban", "Removes a user from global bans.")]
        public async Task GlobalUnnanCommand(InteractionContext ctx, [Option("user", "The user to unban")]DiscordUser victim, [Option("unbanfromguilds", "Unban user from all guilds.")] bool UnbanFromGuilds = true)
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

        [SlashCommand("log", "Change the bot's log level.")]
        public async Task Log(InteractionContext ctx, [Option("loglevel", "The new loglevel")] LogLevel Level)
        {
            Task.Run(async () =>
            {
                await new Commands.LogCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "Level", Level },
                });
            }).Add(_bot.watcher, ctx);
        }
        
        [SlashCommand("stop", "Shuts down the bot.")]
        public async Task Stop(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new StopCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("save", "Save all data to Database.")]
        public async Task Save(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new SaveCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("batch-lookup", "Looks up multiple users. Separate by spaces.")]
        public async Task BatchLookupCommand(InteractionContext ctx, [Option("users", "The users to look up")] string victims)
        {
            Task.Run(async () =>
            {
                await new BatchLookupCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "IDs", victims },
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("create-issue", "Create a new issue on Ichigo's Github Repository.")]
        public async Task CreateIssue(InteractionContext ctx, [Option("use_old_tag_selector", "Allows the use of the legacy tag selector.")] bool UseOldTagsSelector = false)
        {
            Task.Run(async () =>
            {
                await new Commands.CreateIssueCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "UseOldTagsSelector", UseOldTagsSelector },
                }, InitiateInteraction: false);
            }).Add(_bot.watcher, ctx);
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
                throw new InvalidCastException();
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("test-component-modify", "Debug")]
        public async Task TestComponentModify(InteractionContext ctx, [Option("Refetch", "Whether to refetch the message")] bool Refetch)
        {
            try
            {
                _ = ctx.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var msg = await ctx.Channel.SendMessageAsync("Test Message: This could be showing the user that something is loading");

                await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("Loading could be done").AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "a", "button")));

                // Refetch the message to hopefully update it's components object
                // This doesn't make a difference right now

                if (Refetch)
                    msg = await msg.Channel.GetMessageAsync(msg.Id);

                var x = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(1)); // This will throw because there's no components in the message object

                if (x.TimedOut)
                    return;

                await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("button worked 😎"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        
        [SlashCommand("rawuserinfo", "Debug")]
        public async Task RawUserInfo(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"```json\n{JsonConvert.SerializeObject(await user.GetFromApiAsync(), Formatting.Indented)}\n```"));
        }
    }
#endif
}
