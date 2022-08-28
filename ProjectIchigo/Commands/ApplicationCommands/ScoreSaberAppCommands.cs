namespace ProjectIchigo.ApplicationCommands;

public class ScoreSaberAppCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("scoresaber", "Interact with the ScoreSaber API.", dmPermission: false)]
    public class ScoreSaberGroup : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("profile", "Displays you the registered profile of the mentioned user or looks up a profile by a ScoreSaber Id.", dmPermission: false)]
        public async Task ScoreSaberC(InteractionContext ctx, [Option("profile", "Id|@User")] string id = "")
        {
            Task.Run(async () =>
            {
                await new ScoreSaberProfileCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "id", id }
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("search", "Search a user on Score Saber by name.", dmPermission: false)]
        public async Task ScoreSaberSearch(InteractionContext ctx, [Option("name", "The name to search for")] string name)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberSearchCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "name", name }
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("map-leaderboard", "Display the leaderboard off a specific map.", dmPermission: false)]
        public async Task ScoreSaberMapLeaderboard(InteractionContext ctx, [Option("LeaderboardId", "The LeaderboardId")] int boardId, [Option("Page", "The page")] int Page = 1, [Option("internal_page", "The internal page")] int Internal_Page = 0)
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

        [SlashCommand("unlink", "Allows you to remove the saved ScoreSaber profile from your Discord account.", dmPermission: false)]
        public async Task ScoreSaberUnlink(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberUnlinkCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
}
