namespace ProjectIchigo.ApplicationCommands;
internal class Admin : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("join", "Allows to review and change settings in the event somebody joins the server")]
    public class JoinCommand : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows the currently used settings")]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modification of the currently used settings")]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
}
