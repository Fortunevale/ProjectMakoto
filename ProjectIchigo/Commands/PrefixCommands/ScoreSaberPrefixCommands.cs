namespace ProjectIchigo.PrefixCommands;

internal class ScoreSaberPrefixCommands : BaseCommandModule
{
    [Group("scoresaber"), Aliases("ss"),
    CommandModule("scoresaber"),
    Description("Interact with the ScoreSaber API.")]
    internal class ScoreSaberGroup : BaseCommandModule
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
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot.watcher, ctx);
        }

        [Command("profile"), Description("Displays you the registered profile of the mentioned user or looks up a profile by a ScoreSaber Id.")]
        public async Task ScoreSaberC(CommandContext ctx, [Description("ScoreSaber Id or @User")] string id = "")
        {
            Task.Run(async () =>
            {
                await new ScoreSaberProfileCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "id", id }
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("search"), Description("Search a user on ScoreSaber by name.")]
        public async Task ScoreSaberSearch(CommandContext ctx, [Description("Name")][RemainingText] string name)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberSearchCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "name", name }
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("map-leaderboard"), Description("Display the leaderboard off a specific map.")]
        public async Task ScoreSaberMapLeaderboard(CommandContext ctx, [Description("LeaderboardId")] int boardId, [Description("Page")] int Page = 1, [Description("Internal Page")] int Internal_Page = 0)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberMapLeaderboardCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "boardId", boardId },
                    { "Page", Page },
                    { "Internal_Page", Internal_Page },
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("unlink"), Description("Allows you to remove the saved ScoreSaber profile from your Discord account.")]
        public async Task ScoreSaberUnlink(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberUnlinkCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
}
