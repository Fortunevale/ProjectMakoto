namespace ProjectIchigo.Commands;

internal class AfkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string reason = (string)arguments["reason"];

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            if (!ctx.Bot._users.ContainsKey(ctx.User.Id))
                ctx.Bot._users.Add(ctx.User.Id, new User(ctx.Bot, ctx.User.Id));

            if (reason.Length > 128)
            {
                SendSyntaxError();
                return;
            }

            ctx.Bot._users[ctx.User.Id].AfkStatus.Reason = reason.Sanitize();
            ctx.Bot._users[ctx.User.Id].AfkStatus.TimeStamp = DateTime.UtcNow;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Afk Status • {ctx.Guild.Name}" },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"{ctx.User.Mention} `You're now set to be afk. Next time you send a message, your afk status will be removed.`"
            });
            await Task.Delay(10000);
            _ = ctx.ResponseMessage.DeleteAsync();
        });
    }
}