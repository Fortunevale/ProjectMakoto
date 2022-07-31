namespace ProjectIchigo.Commands;

internal class FollowUpdatesCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageWebhooks) && await CheckOwnPermissions(Permissions.ManageWebhooks));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var channel = (FollowChannel)arguments["channel"];

            switch (channel)
            {
                case FollowChannel.GithubUpdates:
                {
                    var b = await ctx.Client.GetChannelAsync(ctx.Bot._status.LoadedConfig.GithubLogChannelId);
                    await b.FollowAsync(ctx.Channel);

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Author = new()
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Color = EmbedColors.Info,
                        Description = "✅ `Successfully followed the github updates channel.`",
                        Footer = ctx.GenerateUsedByFooter()
                    });
                    break;
                }
                case FollowChannel.GlobalBans:
                {
                    var b = await ctx.Client.GetChannelAsync(ctx.Bot._status.LoadedConfig.GlobalBanAnnouncementsChannelId);
                    await b.FollowAsync(ctx.Channel);

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Author = new()
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Color = EmbedColors.Info,
                        Description = "✅ `Successfully followed the global bans channel.`",
                        Footer = ctx.GenerateUsedByFooter()
                    });
                    break;
                }
                case FollowChannel.News:
                {
                    var b = await ctx.Client.GetChannelAsync(ctx.Bot._status.LoadedConfig.NewsChannelId);
                    await b.FollowAsync(ctx.Channel);

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Author = new()
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Color = EmbedColors.Info,
                        Description = "✅ `Successfully followed the news channel.`",
                        Footer = ctx.GenerateUsedByFooter()
                    });
                    break;
                }
            }
        });
    }
}