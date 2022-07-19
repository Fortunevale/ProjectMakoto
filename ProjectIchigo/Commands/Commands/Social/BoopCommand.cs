﻿namespace ProjectIchigo.Commands;

internal class BoopCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser user = (DiscordUser)arguments["user"];

            if (ctx.CommandType == Enums.CommandType.ApplicationCommand)
                _ = ctx.OriginalInteractionContext.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            string gif = await SocialCommandAbstractions.GetGif("boop");

            string[] phrases =
            {
                "%1 boops %2! Adorable..",
                "%1 boops %2! So cute!",
            };

            string[] self_phrases =
            {
                "%1, i don't think that's how it works..",
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }));
        });
    }
}