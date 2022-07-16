namespace ProjectIchigo.PrefixCommands;
internal class Maintainers : BaseCommandModule
{
    public Bot _bot { private get; set; }

    [Group("dev_tools"),
    CommandModule("maintainence"),
    Description("Developer Tools used to develop/manage Project Ichigo")]
    public class DevTools : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, "", "", "");
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, "", "", "");
            }).Add(_bot._watcher, ctx);
        }

        [Command("info"), Description("Shows information about the current guild and bot")]
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

        [Command("globalban"), Description("Bans a user from all servers opted into globalbans")]
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

        [Command("globalunban"), Description("Removes a user from global bans (doesn't unban user from all servers)")]
        public async Task GlobalUnbanCommand(CommandContext ctx, DiscordUser victim)
        {
            Task.Run(async () =>
            {
                await new GlobalUnbanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("log"), Description("Change the bot's log level")]
        public async Task Log(CommandContext ctx, int Level)
        {
            Task.Run(async () =>
            {
                await new Commands.LogCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "Level", Level },
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("stop"), Description("Shuts down the bot")]
        public async Task Stop(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new StopCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [Command("save"), Description("Save all data to Database")]
        public async Task Save(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new SaveCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
}
