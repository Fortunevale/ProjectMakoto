namespace ProjectIchigo.Commands;

internal class KillCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            string gif = await SocialCommandAbstractions.GetGif(new string[] { "kill", "wasted" }.SelectRandom());

            string[] phrases =
            {
                $"%1 kills %2! That looks like it hurt.. {ctx.Bot._status.LoadedConfig.SlapEmoji}",
                $"%1 kills %2! Ouch.. {ctx.Bot._status.LoadedConfig.SlapEmoji}",
            };

            string[] self_phrases =
            {
                "Come on, %1. There's no need to be so hard on yourself!",
                "Come on, %1.. This isn't a solution is it?"
            };

            if (ctx.Member.Id == user.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.SelectRandom().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = Formatter.Bold(phrases.SelectRandom().Replace("%1", ctx.User.Mention).Replace("%2", user.Mention)),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }).WithAllowedMention(UserMention.All));
        });
    }
}