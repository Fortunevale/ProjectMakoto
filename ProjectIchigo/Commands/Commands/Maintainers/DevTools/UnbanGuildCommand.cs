namespace ProjectIchigo.Commands;

internal class UnbanGuildCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            ulong guild = (ulong)arguments["guild"];

            if (!ctx.Bot.bannedGuilds.ContainsKey(guild))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Guild '{guild}' is not banned from using the bot.`").SetError(ctx));
                return;
            }

            ctx.Bot.bannedGuilds.Remove(guild);
            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Guild '{guild}' was unbanned from using the bot.`").SetSuccess(ctx));
        });
    }
}