﻿namespace ProjectIchigo.Commands;

internal class PatCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            string[] phrases =
            {
                    "%1 gives %2 headpats!",
                };

            string[] self_phrases =
            {
                    "There, there.. I'll give you some headpats, %1 😢",
                    "I'll give you some headpats, %1.. 😢",
                    "You look lonely there, %1..",
                };

            if (ctx.Member.Id == user.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.SelectRandom().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter(),
                }));
                return;
            }

            var response = await SocialCommandAbstractions.GetGif(ctx.Bot, "pat");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = Formatter.Bold(phrases.SelectRandom().Replace("%1", ctx.User.Mention).Replace("%2", user.Mention)),
                ImageUrl = response.Item2,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter(response.Item1),
            }).WithAllowedMention(UserMention.All));
        });
    }
}