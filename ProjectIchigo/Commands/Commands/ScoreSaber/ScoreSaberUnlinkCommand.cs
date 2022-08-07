namespace ProjectIchigo.Commands;

internal class ScoreSaberUnlinkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            if (!ctx.Bot._users.ContainsKey(ctx.User.Id))
                ctx.Bot._users.Add(ctx.User.Id, new User(ctx.Bot, ctx.User.Id));

            if (ctx.Bot._users[ctx.User.Id].ScoreSaber.Id != 0)
            {
                ctx.Bot._users[ctx.User.Id].ScoreSaber.Id = 0;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"{ctx.User.Mention} `Unlinked your Score Saber Profile from your Discord Account`"
                }.SetSuccess(ctx, "Score Saber")));
            }
            else
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"{ctx.User.Mention} `There is no Score Saber Profile linked to your Discord Account.`"
                }.SetError(ctx, "Score Saber")));
            }
        });
    }
}