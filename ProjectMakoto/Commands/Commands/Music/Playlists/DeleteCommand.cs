namespace ProjectMakoto.Commands.Playlists;

internal class DeleteCommand : BaseCommand
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

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Music.Playlists.Delete.Deleting, true, new TVar("Name", SelectedPlaylist.PlaylistName)),
            }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title))));

            ctx.Bot.users[ctx.Member.Id].UserPlaylists.Remove(SelectedPlaylist);

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Music.Playlists.Delete.Deleted, true, new TVar("Name", SelectedPlaylist.PlaylistName)),
            }.AsSuccess(ctx, GetString(t.Commands.Music.Playlists.Title))));
            await Task.Delay(5000);
            return;
        });
    }
}