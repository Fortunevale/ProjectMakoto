namespace ProjectIchigo.Commands.Music;

internal class JoinCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckVoiceState() && await CheckOwnPermissions(Permissions.UseVoice) && await CheckOwnPermissions(Permissions.UseVoiceDetection));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            bool Announce = arguments?.ContainsKey("announce") ?? false;

            if (Announce)
                if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                    return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null)
            {
                if (!lava.ConnectedNodes.Any())
                {
                    throw new Exception("Lavalink connection isn't established.");
                }

                conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                ctx.Bot._guilds[ctx.Guild.Id].Lavalink.QueueHandler(ctx.Bot, ctx.Client, node, conn);

                if (Announce)
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"✅ `The bot joined your channel.`",
                        Color = EmbedColors.Success,
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

            if (conn.Channel.Users.Count >= 2 && !(ctx.Member.VoiceState.Channel.Id == conn.Channel.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"❌ `The bot is already in use.`",
                    Color = EmbedColors.Error,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });

                throw new CancelCommandException("", null);
            }

            if (ctx.Member.VoiceState.Channel.Id != conn.Channel.Id)
            {
                await conn.DisconnectAsync();
                conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
            }

            if (Announce)
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"✅ `The bot joined your channel.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
        });
    }
}