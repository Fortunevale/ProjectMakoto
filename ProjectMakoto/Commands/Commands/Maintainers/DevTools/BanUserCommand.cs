namespace ProjectMakoto.Commands;

internal class BanUserCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            string reason = (string)arguments["reason"];

            if (reason.IsNullOrWhiteSpace())
                reason = "No reason provided.";

            if (ctx.Bot.status.TeamMembers.Contains(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.GetUsername()}' is registered in the staff team.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.bannedUsers.ContainsKey(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.GetUsername()}' is already banned from using the bot.`").AsError(ctx));
                return;
            }

            ctx.Bot.bannedUsers.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            foreach (var b in ctx.Client.Guilds.Where(x => x.Value.OwnerId == victim.Id))
            {
                _logger.LogInfo("Leaving guild '{guild}'..", b.Key);
                await b.Value.LeaveAsync();
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`'{victim.GetUsername()}' was banned from using the bot.`").AsSuccess(ctx));
        });
    }
}