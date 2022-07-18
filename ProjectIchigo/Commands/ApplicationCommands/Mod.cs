namespace ProjectIchigo.ApplicationCommands;
internal class Mod : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("purge", "Deletes the specified amount of messages", (long)Permissions.ManageMessages) ]
    public async Task Purge(InteractionContext ctx, [Option("number", "1-2000"), MinimumValue(1), MaximumValue(2000)] int number, [Option("user", "Only delete messages by this user")] DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new PurgeCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "number", number },
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }
}
