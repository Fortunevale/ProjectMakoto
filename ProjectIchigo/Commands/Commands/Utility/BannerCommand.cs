namespace ProjectIchigo.Commands;
internal class BannerCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            if (victim is null)
                victim = ctx.User;

            victim = await victim.GetFromApiAsync();

            var embed = new DiscordEmbedBuilder
            {
                ImageUrl = victim.BannerUrl,
            }.SetInfo(ctx, $"{victim.UsernameWithDiscriminator}'s Banner");

            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

            await RespondOrEdit(builder);
        });
    }
}
