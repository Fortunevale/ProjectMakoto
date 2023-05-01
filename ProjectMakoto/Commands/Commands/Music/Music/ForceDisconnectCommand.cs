namespace ProjectMakoto.Commands.Music;

internal class ForceDisconnectCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
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

            if (!ctx.Member.IsDJ(ctx.Bot.status))
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.DjRole, true, new TVar("Role", "DJ")),
                }.AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Dispose(ctx.Bot, ctx.Guild.Id, "Graceful Disconnect");
            ctx.Bot.guilds[ctx.Guild.Id].MusicModule = new(ctx.Bot.guilds[ctx.Guild.Id]);

            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

            await RespondOrEdit(embed: new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Music.ForceDisconnect.Disconnected, true),
            }.AsSuccess(ctx));
        });
    }
}