namespace ProjectMakoto.Commands;

internal class ScoreSaberUnlinkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            if (ctx.Bot.users[ctx.User.Id].ScoreSaber.Id != 0)
            {
                ctx.Bot.users[ctx.User.Id].ScoreSaber.Id = 0;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.ScoreSaber.Unlink.Unlinked, true)
                }.AsSuccess(ctx, "Score Saber")));
            }
            else
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.ScoreSaber.Unlink.NoLink, true)
                }.AsError(ctx, "Score Saber")));
            }
        });
    }
}