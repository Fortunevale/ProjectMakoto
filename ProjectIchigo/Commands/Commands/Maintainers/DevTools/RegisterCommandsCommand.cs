namespace ProjectIchigo.Commands;

internal class RegisterCommandsCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (!ctx.Bot._status.LoadedConfig.IsDev)
                throw new Exception("Not in developer mode!");

            var appCommands = ctx.Client.GetApplicationCommands();

            if (appCommands.RegisteredCommands.Count > 0)
                throw new Exception("Commands already registered!");

            await RespondOrEdit("Registering commands. This may take a moment..");

            appCommands.RegisterGuildCommands<ApplicationCommands.MaintainersAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.ConfigurationAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.ModerationAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.SocialAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.ScoreSaberAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.MusicAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.UtilityAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);

            await ctx.Client.ReconnectAsync(true);

            int count = 0;

            appCommands.GuildApplicationCommandsRegistered += GuildApplicationCommandsRegistered;
            async Task GuildApplicationCommandsRegistered(ApplicationCommandsExtension sender, GuildApplicationCommandsRegisteredEventArgs e)
            {
                count = e.RegisteredCommands.Count;
            }

            try
            {
                await Task.Delay(90000);
                await RespondOrEdit($"Registered {count} commands.");

                appCommands.GuildApplicationCommandsRegistered -= GuildApplicationCommandsRegistered;
            }
            catch (Exception)
            {
                appCommands.GuildApplicationCommandsRegistered -= GuildApplicationCommandsRegistered;

                await RespondOrEdit("An exception occured while trying to register slash commands.");
                throw;
            }
        });
    }
}