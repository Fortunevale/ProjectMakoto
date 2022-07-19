namespace ProjectIchigo.ApplicationCommands;
internal class Admin : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("join", "Allows to review and change settings in the event somebody joins the server")]
    public class Join : ApplicationCommandsModule
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
    
    [SlashCommandGroup("experience", "Allows to review and change settings related to experience")]
    public class Experience : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows the currently used settings")]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modification of the currently used settings")]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("levelrewards", "Allows to review, add, remove and modify Level Rewards")]
    public class LevelRewards : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows a list of all currently defined Level Rewards")]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows adding, removing and modifying currently defined Level Rewards")]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
}
