namespace ProjectMakoto.Commands.Playlists;

internal class ExportCommand : BaseCommand
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

            using (MemoryStream stream = new(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SelectedPlaylist, Formatting.Indented))))
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                {
                    Description = GetString(t.Commands.Music.Playlists.Export.Exported, true, new TVar("Name", SelectedPlaylist.PlaylistName)),
                }.AsInfo(ctx, GetString(t.Commands.Music.Playlists.Title))).WithFile($"{Guid.NewGuid().ToString().Replace("-", "").ToLower()}.json", stream));
            }
        });
    }
}