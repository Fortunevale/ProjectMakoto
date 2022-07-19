namespace ProjectIchigo.ApplicationCommands;
internal class Admin : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("join", "Allows to review and change settings in the event somebody joins the server")]
    public class Join : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows the currently used settings", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modification of the currently used settings", (long)Permissions.Administrator)]
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

        [SlashCommand("review", "Shows the currently used settings", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modification of the currently used settings", (long)Permissions.Administrator)]
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

        [SlashCommand("review", "Shows a list of all currently defined Level Rewards", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows adding, removing and modifying currently defined Level Rewards", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("phishing", "Allows to review, add, remove and modify Level Rewards")]
    public class PhishingSettings : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows a list of all currently defined Phishing Protection settings", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.PhishingCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modifying currently used Phishing Protection settings", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.PhishingCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("bumpreminder", "Allows to review, set up and change settings for the Bump Reminder")]
    public class BumpReminder : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows a list of all currently defined Bump Reminder settings", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.BumpReminderCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modifying currently used Bump Reminder settings", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.BumpReminderCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("actionlog", "Allows to review and change settings for the actionlog")]
    public class ActionLog : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows a list of all currently defined Actionlog Settings", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ActionLogCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modifying currently used Actionlog settings", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ActionLogCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("autocrosspost", "Allows to review and change settings for the automatic crossposts")]
    public class AutoCrosspost : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows a list of all currently defined Auto Crosspost Channels", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoCrosspostCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modifying currently defined Auto Crosspost Channels and settings related to it", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoCrosspostCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [SlashCommandGroup("reactionroles", "Allows to review and change settings for Reaction Roles")]
    public class ReactionRoles : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows a list of all currently defined Auto Crosspost Channels", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to add, delete and modify reaction roles", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [ContextMenu(ApplicationCommandType.Message, "Add a Reaction Role", (long)Permissions.Administrator)]
    public async Task Add(ContextMenuContext ctx)
    {
        Task.Run(async () =>
        {
            await new Commands.ReactionRolesCommand.AddCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "message", ctx.TargetMessage },
                });
        }).Add(_bot._watcher, ctx);
    }

    [ContextMenu(ApplicationCommandType.Message, "Remove a Reaction Role", (long)Permissions.Administrator)]
    public async Task Remove(ContextMenuContext ctx)
    {
        Task.Run(async () =>
        {
            await new Commands.ReactionRolesCommand.RemoveCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "message", ctx.TargetMessage },
                });
        }).Add(_bot._watcher, ctx);
    }

    [ContextMenu(ApplicationCommandType.Message, "Remove all Reaction Roles", (long)Permissions.Administrator)]
    public async Task RemoveAll(ContextMenuContext ctx)
    {
        Task.Run(async () =>
        {
            await new Commands.ReactionRolesCommand.RemoveAllCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "message", ctx.TargetMessage },
                });
        }).Add(_bot._watcher, ctx);
    }

    [SlashCommandGroup("invoiceprivacy", "Allows to review and change In-Voice Text Channel Privacy Settings")]
    public class InVoiceTextPrivacy : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows currently defined settings for In-Voice Text Channel Privacy", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InVoicePrivacyCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modifying currently defined In-Voice Text Channel Privacy Settings", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InVoicePrivacyCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("invitetracker", "Allows to review and change Invite Tracker Settings")]
    public class InviteTracker : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Shows currently defined settings for Invite Tracking Settings", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InviteTrackerCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows modifying currently defined Invite Tracking Settings", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InviteTrackerCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
}
