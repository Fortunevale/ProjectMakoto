namespace ProjectIchigo.Commands;

internal class KillCommand : BaseCommand
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

            string gif = await SocialCommandAbstractions.GetGif(new string[] { "kill", "wasted" }.OrderBy(x => Guid.NewGuid()).First());

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