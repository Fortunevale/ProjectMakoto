namespace ProjectIchigo.Commands;

internal class AfkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string reason = (string)arguments["reason"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            if (reason.Length > 128)
            {
                SendSyntaxError();
                return;
            }

            ctx.Bot.users[ctx.User.Id].AfkStatus.Reason = reason.FullSanitize();
            ctx.Bot.users[ctx.User.Id].AfkStatus.TimeStamp = DateTime.UtcNow;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"{ctx.User.Mention} `You're now set to be afk. Next time you send a message, your afk status will be removed.`"
            }.AsSuccess(ctx, "Afk Status"));
            await Task.Delay(10000);
            _ = ctx.ResponseMessage.DeleteAsync();
        });
    }
}