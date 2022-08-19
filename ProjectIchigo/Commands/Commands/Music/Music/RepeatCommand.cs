namespace ProjectIchigo.Commands.Music;

internal class RepeatCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
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

            ctx.Bot.guilds[ctx.Guild.Id].Lavalink.Repeat = !ctx.Bot.guilds[ctx.Guild.Id].Lavalink.Repeat;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = (ctx.Bot.guilds[ctx.Guild.Id].Lavalink.Repeat ? "`The queue now repeats itself.`" : "`The queue no longer repeats itself.`"),
            }.SetSuccess(ctx));
        });
    }
}