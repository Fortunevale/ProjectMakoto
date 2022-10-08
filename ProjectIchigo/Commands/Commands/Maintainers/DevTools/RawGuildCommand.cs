namespace ProjectIchigo.Commands;

internal class RawGuildCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            ulong? guild = (ulong?)arguments["guild"];
            guild ??= ctx.Guild.Id;

            await RespondOrEdit(new DiscordMessageBuilder().WithFile("guild.json", JsonConvert.SerializeObject(ctx.Bot.guilds[guild.Value], Formatting.Indented).ToStream()));
        });
    }
}