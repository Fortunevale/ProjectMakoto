namespace ProjectIchigo.Commands;

internal class UnbanUserCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (!ctx.Bot.bannedUsers.ContainsKey(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.UsernameWithDiscriminator}' is not banned from using the bot.`").SetError(ctx));
                return;
            }

            ctx.Bot.bannedUsers.Remove(victim.Id);
            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.UsernameWithDiscriminator}' was unbanned from using the bot.`").SetSuccess(ctx));
        });
    }
}