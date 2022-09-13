namespace ProjectIchigo.Commands.Music;

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

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`The bot is not in a voice channel.`",
                }.SetError(ctx));
                return;
            }

            if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`You aren't in the same channel as the bot.`",
                }.SetError(ctx));
                return;
            }

            Lavalink.QueueInfo info = null;

            if (selection.IsDigitsOnly())
            {
                int Index = Convert.ToInt32(selection) - 1;

                if (Index < 0 || Index >= ctx.Bot.guilds[ctx.Guild.Id].Lavalink.SongQueue.Count)
                {
                    await RespondOrEdit(embed: new DiscordEmbedBuilder
                    {
                        Description = $"`Your value is out of range. Currently, the range is 1-{ctx.Bot.guilds[ctx.Guild.Id].Lavalink.SongQueue.Count}.`",
                    }.SetError(ctx));
                    return;
                }

                info = ctx.Bot.guilds[ctx.Guild.Id].Lavalink.SongQueue[Index];
            }
            else
            {
                if (!ctx.Bot.guilds[ctx.Guild.Id].Lavalink.SongQueue.Any(x => x.VideoTitle.ToLower() == selection.ToLower()))
                {
                    await RespondOrEdit(embed: new DiscordEmbedBuilder
                    {
                        Description = $"`There is no such song queued.`",
                    }.SetError(ctx));
                    return;
                }

                info = ctx.Bot.guilds[ctx.Guild.Id].Lavalink.SongQueue.First(x => x.VideoTitle.ToLower() == selection.ToLower());
            }

            if (info is null)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`There is no such song queued.`",
                }.SetError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].Lavalink.SongQueue.Remove(info);

            await RespondOrEdit(embed: new DiscordEmbedBuilder
            {
                Description = $"`Removed` [`{info.VideoTitle}`]({info.Url}) `from the current queue.`",
            }.SetSuccess(ctx));
        });
    }
}