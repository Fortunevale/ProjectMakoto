// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;

[ModulePriority(994)]
public sealed class ConfigurationAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("join", "Allows you to review and change settings in the event somebody joins the server.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class Join : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows to review the currently used settings in the event somebody joins the server.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.JoinCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change the currently used settings in the event somebody joins the server.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.JoinCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("experience", "Allows you to review and change settings related to experience.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class Experience : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to experience.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.ExperienceCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change the currently used settings related to experience.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.ExperienceCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("levelrewards", "Allows you to review, add and change Level Rewards.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class LevelRewards : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently defined Level Rewards.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.LevelRewardsCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to add, remove and modify currently defined Level Rewards.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.LevelRewardsCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("phishing", "Allows you to review and change settings related to phishing link protection.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class PhishingSettings : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to phshing link protection.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.PhishingCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change the currently used settings related to phishing link protection.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.PhishingCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("bumpreminder", "Allows you to review, set up and change settings related to the Bump Reminder.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class BumpReminder : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to the Bump Reminder.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.BumpReminderCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change currently used settings related to the Bump Reminder.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.BumpReminderCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("actionlog", "Allows you to review and change settings related to the actionlog.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class ActionLog : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to the actionlog.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.ActionLogCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change currently used settings related to the actionlog.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.ActionLogCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("autocrosspost", "Allows you to review and change settings related to automatic crossposting.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class AutoCrosspost : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to automatic crossposting.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.AutoCrosspostCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change currently used settings related to automatic crossposting.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.AutoCrosspostCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("reactionroles", "Allows you to review and change settings related to Reaction Roles.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class ReactionRoles : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently registered Reaction Roles.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.ReactionRolesCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to add and remove registered Reaction Roles.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.ReactionRolesCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [PrefixCommandAlternative("reactionroles add")]
    [ContextMenu(ApplicationCommandType.Message, "Add a Reaction Role", (long)Permissions.Administrator, dmPermission: false)]
    public async Task Add(ContextMenuContext ctx)
        => _ = new Commands.ReactionRolesCommand.AddCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "message", ctx.TargetMessage }, });

    [PrefixCommandAlternative("reactionroles remove")]
    [ContextMenu(ApplicationCommandType.Message, "Remove a Reaction Role", (long)Permissions.Administrator, dmPermission: false)]
    public async Task Remove(ContextMenuContext ctx)
        => _ = new Commands.ReactionRolesCommand.RemoveCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "message", ctx.TargetMessage }, });

    [PrefixCommandAlternative("reactionroles removeall")]
    [ContextMenu(ApplicationCommandType.Message, "Remove all Reaction Roles", (long)Permissions.Administrator, dmPermission: false)]
    public async Task RemoveAll(ContextMenuContext ctx)
        => _ = new Commands.ReactionRolesCommand.RemoveAllCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "message", ctx.TargetMessage }, });

    [SlashCommandGroup("invoiceprivacy", "Allows you to review and change settings related to In-Voice Text Channel Privacy.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class InVoiceTextPrivacy : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to In-Voice Text Channel Privacy.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.InVoicePrivacyCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change currently used settings related to In-Voice Text Channel Privacy.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.InVoicePrivacyCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("invitetracker", "Allows you to review and change settings related to Invite Tracking.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class InviteTracker : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to Invite Tracking.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.InviteTrackerCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change the currently used settings related to Invite Tracking.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.InviteTrackerCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("namenormalizer", "Allows you to review and change settings related to automatic name normalization.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class NameNormalizer : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to name normalization.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.NameNormalizerCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change the currently used settings related to name normalization.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.NameNormalizerCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("autounarchive", "Allows you to review and change settings related to automatic thread unarchiving.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class AutoUnarchive : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to automatic thread unarchiving.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.AutoUnarchiveCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change the currently used settings related to automatic thread unarchiving.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.AutoUnarchiveCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("embedmessages", "Allows you to review and change settings related to automatic message embedding.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class MessageEmbedding : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to automatic message embedding.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.EmbedMessageCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change the currently used settings related to automatic message embedding.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.EmbedMessageCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("tokendetection", "Allows you to review and change settings related to automatic token invalidation.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class TokenDetection : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review the currently used settings related to automatic token invalidation.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.TokenDetectionCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change the currently used settings related to automatic token invalidation.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.TokenDetectionCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("invitenotes", "Allows you to add notes to invite codes.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class InviteNotes : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently set up invite notes.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.InviteNotesCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to add and remove currently set up invite notes.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.InviteNotesCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("vccreator", "Allows you to review and change settings related to the Voice Channel Creator.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class VcCreator : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to the Voice Channel Creator.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.VcCreatorCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change currently used settings related to the Voice Channel Creator.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.VcCreatorCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("guild-language", "Allows you to review and change settings related to the guild's selected language.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class GuildLanguage : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review currently used settings related to the guild's selected language.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.GuildLanguage.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change currently used settings related to the guild's selected language.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.GuildLanguage.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("guild-prefix", "Allows you to review and change settings related to the guild's prefix.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class GuildPrefix : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("review", "Allows you to review settings related to the guild's prefix.")]
        public async Task Review(InteractionContext ctx)
            => _ = new Commands.PrefixCommand.ReviewCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("config", "Allows you to change settings related to the guild's prefix.")]
        public async Task Config(InteractionContext ctx)
            => _ = new Commands.PrefixCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);
    }
}
