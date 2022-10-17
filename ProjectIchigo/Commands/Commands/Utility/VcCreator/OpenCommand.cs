namespace ProjectIchigo.Commands.VcCreator;

internal class OpenCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (!ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels.ContainsKey(ctx.Member.VoiceState.Channel.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You're not in a Channel created by the Voice Channel Creator.`").AsError(ctx));
                return;
            }
        });
    }
}