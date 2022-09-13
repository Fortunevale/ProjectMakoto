namespace ProjectIchigo.Commands.Music;

internal class ForceDisconnectCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
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

            if (!ctx.Member.IsDJ(ctx.Bot.status))
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`You need Administrator Permissions or a role called 'DJ' to utilize this command.`",
                }.SetError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].Lavalink.Dispose(ctx.Bot, ctx.Guild.Id, "Graceful Disconnect");
            ctx.Bot.guilds[ctx.Guild.Id].Lavalink = new(ctx.Bot.guilds[ctx.Guild.Id]);

            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

            await RespondOrEdit(embed: new DiscordEmbedBuilder
            {
                Description = $"`The bot was force disconnected.`",
            }.SetSuccess(ctx));
        });
    }
}