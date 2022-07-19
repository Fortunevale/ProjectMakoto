namespace ProjectIchigo.PrefixCommands;

internal class ScoreSaber : BaseCommandModule
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("profile"), Description("Get show a users Score Saber profile by id")]
        public async Task ScoreSaberC(CommandContext ctx, [Description("Id|@User")] string id = "")
        {
            Task.Run(async () =>
            {
                await new ScoreSaberProfileCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "id", id }
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("search"), Description("Search a user on Score Saber by name")]
        public async Task ScoreSaberSearch(CommandContext ctx, [Description("Name")][RemainingText] string name)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberSearchCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "name", name }
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("map-leaderboard"), Description("Display a leaderboard off a specific map")]
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
            }).Add(_bot._watcher, ctx);
        }

        [Command("unlink"), Description("Unlink your Score Saber Profile from your Discord Account")]
        public async Task ScoreSaberUnlink(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberUnlinkCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
}
