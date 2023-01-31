namespace ProjectMakoto.Commands.Music;

internal class PauseCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`The bot is not in a voice channel.`",
                }.AsError(ctx));
                return;
            }

            if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`You aren't in the same channel as the bot.`",
                }.AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.IsPaused = !ctx.Bot.guilds[ctx.Guild.Id].MusicModule.IsPaused;

            if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.IsPaused)
                _ = conn.PauseAsync();
            else
                _ = conn.ResumeAsync();

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.IsPaused ? "`Paused playback.`" : "`Resumed playback.`"),
            }.AsSuccess(ctx));
        });
    }
}