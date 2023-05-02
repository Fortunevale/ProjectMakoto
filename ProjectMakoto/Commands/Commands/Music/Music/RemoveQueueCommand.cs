namespace ProjectMakoto.Commands.Music;

internal class RemoveQueueCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string selection = (string)arguments["selection"];

            if (string.IsNullOrWhiteSpace(selection))
            {
                SendSyntaxError();
                return;
            }

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.NotSameChannel, true),
                }.AsError(ctx));
                return;
            }

            Lavalink.QueueInfo info = null;

            if (selection.IsDigitsOnly())
            {
                int Index = Convert.ToInt32(selection) - 1;

                if (Index < 0 || Index >= ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count)
                {
                    await RespondOrEdit(embed: new DiscordEmbedBuilder
                    {
                        Description = GetString(t.Commands.Music.RemoveQueue.OutOfRange, true, new TVar("Min", 1), new TVar("Max", ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count)),
                    }.AsError(ctx));
                    return;
                }

                info = ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue[Index];
            }
            else
            {
                if (!ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Any(x => x.VideoTitle.ToLower() == selection.ToLower()))
                {
                    await RespondOrEdit(embed: new DiscordEmbedBuilder
                    {
                        Description = GetString(t.Commands.Music.RemoveQueue.NoSong, true),
                    }.AsError(ctx));
                    return;
                }

                info = ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.First(x => x.VideoTitle.ToLower() == selection.ToLower());
            }

            if (info is null)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.RemoveQueue.NoSong, true),
                }.AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Remove(info);

            await RespondOrEdit(embed: new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Music.RemoveQueue.Removed, true, new TVar("Track", $"`[`{info.VideoTitle}`]({info.Url})`")),
            }.AsSuccess(ctx));
        });
    }
}