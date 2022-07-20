namespace ProjectIchigo.Commands.Music;

internal class PauseCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"❌ `The bot is not in a voice channel.`",
                    Color = EmbedColors.Error,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"❌ `You aren't in the same channel as the bot.`",
                    Color = EmbedColors.Error,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.IsPaused = !ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.IsPaused;

            if (ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.IsPaused)
                _ = conn.PauseAsync();
            else
                _ = conn.ResumeAsync();

            await RespondOrEdit(new DiscordEmbedBuilder
            {

                Description = (ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.IsPaused ? "✅ `Paused playback.`" : "✅ `Resumed playback.`"),
                Color = EmbedColors.Success,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = ctx.Guild.IconUrl
                },
                Footer = ctx.GenerateUsedByFooter()
            });
        });
    }
}