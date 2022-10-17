namespace ProjectIchigo.ApplicationCommands;
public class ConfigurationAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("join", "Allows you to review and change settings in the event somebody joins the server.", dmPermission: false)]
    public class Join : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows to review the currently used settings in the event somebody joins the server.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings in the event somebody joins the server.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.JoinCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("experience", "Allows you to review and change settings related to experience.", dmPermission: false)]
    public class Experience : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to experience.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to experience.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ExperienceCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("levelrewards", "Allows you to review, add and change Level Rewards.", dmPermission: false)]
    public class LevelRewards : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently defined Level Rewards.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to add, remove and modify currently defined Level Rewards.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.LevelRewardsCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("phishing", "Allows you to review and change settings related to phishing link protection.", dmPermission: false)]
    public class PhishingSettings : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to phshing link protection.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.PhishingCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to phishing link protection.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.PhishingCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("bumpreminder", "Allows you to review, set up and change settings related to the Bump Reminder.", dmPermission: false)]
    public class BumpReminder : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to the Bump Reminder.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.BumpReminderCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to the Bump Reminder.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.BumpReminderCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("actionlog", "Allows you to review and change settings related to the actionlog.", dmPermission: false)]
    public class ActionLog : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to the actionlog.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ActionLogCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to the actionlog.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ActionLogCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("autocrosspost", "Allows you to review and change settings related to automatic crossposting.", dmPermission: false)]
    public class AutoCrosspost : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to automatic crossposting.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoCrosspostCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to automatic crossposting.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoCrosspostCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }

    [SlashCommandGroup("reactionroles", "Allows you to review and change settings related to Reaction Roles.", dmPermission: false)]
    public class ReactionRoles : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently registered Reaction Roles.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to add and remove registered Reaction Roles.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.ReactionRolesCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
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
        }).Add(_bot.watcher, ctx);
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
        }).Add(_bot.watcher, ctx);
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
        }).Add(_bot.watcher, ctx);
    }

    [SlashCommandGroup("invoiceprivacy", "Allows you to review and change settings related to In-Voice Text Channel Privacy.", dmPermission: false)]
    public class InVoiceTextPrivacy : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to In-Voice Text Channel Privacy.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InVoicePrivacyCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to In-Voice Text Channel Privacy.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InVoicePrivacyCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("invitetracker", "Allows you to review and change settings related to Invite Tracking.", dmPermission: false)]
    public class InviteTracker : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to Invite Tracking.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InviteTrackerCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to Invite Tracking.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InviteTrackerCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("namenormalizer", "Allows you to review and change settings related to automatic name normalization.", dmPermission: false)]
    public class NameNormalizer : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to name normalization.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.NameNormalizerCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to name normalization.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.NameNormalizerCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("autounarchive", "Allows you to review and change settings related to automatic thread unarchiving.", dmPermission: false)]
    public class AutoUnarchive : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to automatic thread unarchiving.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoUnarchiveCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to automatic thread unarchiving.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.AutoUnarchiveCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
    
    [SlashCommandGroup("embedmessages", "Allows you to review and change settings related to automatic message embedding.", dmPermission: false)]
    public class MessageEmbedding : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to automatic message embedding.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.EmbedMessageCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to automatic message embedding.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.EmbedMessageCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }

    [SlashCommandGroup("tokendetection", "Allows you to review and change settings related to automatic token invalidation.", dmPermission: false)]
    public class TokenDetection : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to automatic token invalidation.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.TokenDetectionCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change the currently used settings related to automatic token invalidation.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.TokenDetectionCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }

    [SlashCommandGroup("invitenotes", "Allows you to add notes to invite codes.", dmPermission: false)]
    public class InviteNotes : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently set up invite notes.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InviteNotesCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to add and remove currently set up invite notes.", (long)Permissions.Administrator, dmPermission: false)]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.InviteNotesCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }

    [SlashCommandGroup("vccreator", "Allows you to review and change settings related to the Voice Channel Creator.")]
    public class VcCreator : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to the Voice Channel Creator.")]
        public async Task Review(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreatorCommand.ReviewCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("config", "Allows you to change currently used settings related to the Voice Channel Creator.")]
        public async Task Config(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.VcCreatorCommand.ConfigCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }
}
