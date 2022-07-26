﻿namespace ProjectIchigo.Commands;

internal class RegisterCommandsCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (!ctx.Bot._status.LoadedConfig.IsDev)
                throw new Exception("Not in developer mode!");

            await RespondOrEdit("Starting to register commands..");

            var appCommands = ctx.Client.GetApplicationCommands();

            appCommands.RegisterGuildCommands<ApplicationCommands.MaintainersAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.ConfigurationAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.ModerationAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.SocialAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.ScoreSaberAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.MusicAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);
            appCommands.RegisterGuildCommands<ApplicationCommands.UtilityAppCommands>(ctx.Bot._status.LoadedConfig.AssetsGuildId);

            int count = 0;

            appCommands.GuildApplicationCommandsRegistered += GuildApplicationCommandsRegistered;
            async Task GuildApplicationCommandsRegistered(ApplicationCommandsExtension sender, GuildApplicationCommandsRegisteredEventArgs e)
            {
                count = e.RegisteredCommands.Count;
            }

            try
            {
                int prevCount = 0;
                int timeOut = 0;

                while (true)
                {
                    if (prevCount != count)
                    {
                        prevCount = count;
                        timeOut = 0;

                        await RespondOrEdit($"Registered {count} commands..");
                    }

                    timeOut++;

                    if (timeOut > 60)
                    {
                        break;
                    }

                    await Task.Delay(1000);
                }

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