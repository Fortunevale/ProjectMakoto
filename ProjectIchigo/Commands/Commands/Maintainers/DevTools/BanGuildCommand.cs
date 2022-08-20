namespace ProjectIchigo.Commands;

internal class BanGuildCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            ulong guild = (ulong)arguments["guild"];
            string reason = (string)arguments["reason"];

            if (reason.IsNullOrWhiteSpace())
                reason = "No reason provided.";

            if (ctx.Bot.bannedGuilds.ContainsKey(guild))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Guild '{guild}' is already banned from using the bot.`").SetError(ctx));
                return;
            }

            ctx.Bot.bannedGuilds.Add(guild, new() { Reason = reason, Moderator = ctx.User.Id });

            foreach (var b in ctx.Client.Guilds.Where(x => x.Key == guild))
            {
                _logger.LogInfo($"Leaving guild '{b.Key}'..");
                await b.Value.LeaveAsync();
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Guild '{guild}' was banned from using the bot.`").SetSuccess(ctx));
        });
    }
}