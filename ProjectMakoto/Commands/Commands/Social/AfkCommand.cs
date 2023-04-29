namespace ProjectMakoto.Commands;

internal class AfkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string reason = (string)arguments["reason"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx))
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
                Description = $"{ctx.User.Mention} `{GetString(t.Commands.Afk.SetAfk)}`"
            }.AsSuccess(ctx, GetString(t.Commands.Afk.Title)));
            await Task.Delay(10000);
            _ = ctx.ResponseMessage.DeleteAsync();
        });
    }
}