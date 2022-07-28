namespace ProjectIchigo.ApplicationCommands;
internal class ConfigurationAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("join", "Allows you to review and change settings in the event somebody joins the server.")]
    public class Join : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows to review the currently used settings in the event somebody joins the server.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings in the event somebody joins the server.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("experience", "Allows you to review and change settings related to experience.")]
    public class Experience : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to experience.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to experience.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("levelrewards", "Allows you to review, add and change Level Rewards.")]
    public class LevelRewards : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently defined Level Rewards.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to add, remove and modify currently defined Level Rewards.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("phishing", "Allows you to review and change settings related to phishing link protection.")]
    public class PhishingSettings : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to phshing link protection.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.PhishingCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to phishing link protection.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.PhishingCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("bumpreminder", "Allows you to review, set up and change settings related to the Bump Reminder.")]
    public class BumpReminder : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to the Bump Reminder.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.BumpReminderCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to the Bump Reminder.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.BumpReminderCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("actionlog", "Allows you to review and change settings related to the actionlog.")]
    public class ActionLog : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to the actionlog.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ActionLogCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to the actionlog.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ActionLogCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("autocrosspost", "Allows you to review and change settings related to automatic crossposting.")]
    public class AutoCrosspost : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to automatic crossposting.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoCrosspostCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to automatic crossposting.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoCrosspostCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [SlashCommandGroup("reactionroles", "Allows you to review and change settings related to Reaction Roles.")]
    public class ReactionRoles : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently registered Reaction Roles.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to add and remove registered Reaction Roles.", (long)Permissions.Administrator)]
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

    [SlashCommandGroup("invoiceprivacy", "Allows you to review and change settings related to In-Voice Text Channel Privacy.")]
    public class InVoiceTextPrivacy : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to In-Voice Text Channel Privacy.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InVoicePrivacyCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to In-Voice Text Channel Privacy.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InVoicePrivacyCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("invitetracker", "Allows you to review and change settings related to Invite Tracking.")]
    public class InviteTracker : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to Invite Tracking.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InviteTrackerCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to Invite Tracking.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InviteTrackerCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("namenormalizer", "Allows you to review and change settings related to automatic name normalization.")]
    public class NameNormalizer : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to name normalization.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.NameNormalizerCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to name normalization.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.NameNormalizerCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("autounarchive", "Allows you to review and change settings related to automatic thread unarchiving.")]
    public class AutoUnarchive : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to automatic thread unarchiving.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoUnarchiveCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to automatic thread unarchiving.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoUnarchiveCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
    
    [SlashCommandGroup("embedmessages", "Allows you to review and change settings related to automatic message embedding.")]
    public class MessageEmbedding : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to automatic message embedding.", (long)Permissions.Administrator)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.EmbedMessageCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to automatic message embedding.", (long)Permissions.Administrator)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.EmbedMessageCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }
}
