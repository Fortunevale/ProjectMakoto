namespace ProjectIchigo.ApplicationCommands;
internal class User : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("user-info", "Shows information about the mentioned user")]
    public async Task UserInfoCommand(InteractionContext ctx, [Option("User", "The User")]DiscordUser victim)
    {
        Task.Run(async () =>
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

            await new UserInfo().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", await victim.GetFromApiAsync() }
            });
        }).Add(_bot._watcher, ctx);
    }
}
