namespace ProjectMakoto.Commands.Playlists;

internal class ShareCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx))
                return;

            string playlistId = (string)arguments["id"];

            if (!ctx.Bot.users[ctx.Member.Id].UserPlaylists.Any(x => x.PlaylistId == playlistId))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.Playlists.NoPlaylist, true),
                }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title)));
                return;
            }

            UserPlaylist SelectedPlaylist = ctx.Bot.users[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == playlistId);

            string ShareCode = $"{Guid.NewGuid()}";

            if (!Directory.Exists("PlaylistShares"))
                Directory.CreateDirectory("PlaylistShares");

            if (!Directory.Exists($"PlaylistShares/{ctx.User.Id}"))
                Directory.CreateDirectory($"PlaylistShares/{ctx.User.Id}");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            {
                Description = GetString(t.Commands.Music.Playlists.Share.Shared, true, 
                new TVar("Command", $"{ctx.Prefix}playlists load-share {ctx.User.Id} {ShareCode}")),
            }.AsInfo(ctx, GetString(t.Commands.Music.Playlists.Title))));

            File.WriteAllText($"PlaylistShares/{ctx.User.Id}/{ShareCode}.json", JsonConvert.SerializeObject(SelectedPlaylist, Formatting.Indented));
        });
    }
}