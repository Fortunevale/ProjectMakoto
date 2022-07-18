namespace ProjectIchigo.Commands;
internal class BannerCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            if (victim is null)
                victim = ctx.User;

            victim = await victim.GetFromApiAsync();

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{victim.UsernameWithDiscriminator}'s Banner",
                    Url = victim.AvatarUrl
                },
                ImageUrl = victim.BannerUrl,
                Description = (victim.BannerUrl.IsNullOrWhiteSpace() ? "`This user has no banner.`" : ""),
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Color = EmbedColors.Info
            };

            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

            await RespondOrEdit(builder);
        });
    }
}
