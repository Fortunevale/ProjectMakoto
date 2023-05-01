namespace ProjectMakoto.Commands;
internal class BannerCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx))
                return;

            victim ??= ctx.User;

            victim = await victim.GetFromApiAsync();

            var embed = new DiscordEmbedBuilder
            {
                ImageUrl = victim.BannerUrl,
                Description = victim.BannerUrl.IsNullOrWhiteSpace() ? GetString(t.Commands.Utility.Banner.NoBanner, true) : ""
            }.AsInfo(ctx, GetString(t.Commands.Utility.Banner.Banner, false, new TVar("User", victim.UsernameWithDiscriminator)));

            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

            await RespondOrEdit(builder);
        });
    }
}
