namespace ProjectIchigo.Commands;

internal class FollowUpdatesCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageWebhooks) && await CheckOwnPermissions(Permissions.ManageWebhooks));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var channel = (FollowChannel)arguments["channel"];

            try
            {
                switch (channel)
                {
                    case FollowChannel.GithubUpdates:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.GithubLogChannelId);
                        await b.FollowAsync(ctx.Channel);
                        break;
                    }
                    case FollowChannel.GlobalBans:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.GlobalBanAnnouncementsChannelId);
                        await b.FollowAsync(ctx.Channel);
                        break;
                    }
                    case FollowChannel.News:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.NewsChannelId);
                        await b.FollowAsync(ctx.Channel);
                        break;
                    }
                }

                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Successfully followed {channel}.`",
                }.SetSuccess(ctx));
            }
            catch (Exception)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Could not follow {channel}.`",
                }.SetError(ctx));
            }
        });
    }
}