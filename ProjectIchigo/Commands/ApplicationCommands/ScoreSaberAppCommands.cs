namespace ProjectIchigo.ApplicationCommands;

internal class ScoreSaberAppCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("scoresaber", "Interact with the ScoreSaber API.")]
    public class ScoreSaberGroup : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("profile", "Show a users Score Saber profile by id")]
        public async Task ScoreSaberC(InteractionContext ctx, [Option("profile", "Id|@User")] string id = "")
        {
            Task.Run(async () =>
            {
                await new ScoreSaberProfileCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "id", id }
                });
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("search", "Search a user on Score Saber by name")]
        public async Task ScoreSaberSearch(InteractionContext ctx, [Option("name", "The name to search for")] string name)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberSearchCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "name", name }
                });
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("map-leaderboard", "Display a leaderboard off a specific map")]
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
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("unlink", "Unlink your Score Saber Profile from your Discord Account")]
        public async Task ScoreSaberUnlink(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new ScoreSaberUnlinkCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
}
