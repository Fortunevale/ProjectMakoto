// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.PrefixCommands;

public sealed class ConfigurationPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }


    [Group("join"), Aliases("joinsettings", "join-settings"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings in the event somebody joins the server.")]
    public sealed class JoinSettings : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "join").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows to review the currently used settings in the event somebody joins the server.")]
        public async Task Review(CommandContext ctx)
            => new Commands.JoinCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change the currently used settings in the event somebody joins the server.")]
        public async Task Config(CommandContext ctx)
            => new Commands.JoinCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("experience"), Aliases("experiencesettings", "experience-settings"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to experience.")]
    public sealed class ExperienceSettings : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "experience").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review the currently used settings related to experience.")]
        public async Task Review(CommandContext ctx)
            => new Commands.ExperienceCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change the currently used settings related to experience.")]
        public async Task Config(CommandContext ctx)
            => new Commands.ExperienceCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("levelrewards"), Aliases("level-rewards", "rewards"),
    CommandModule("configuration"),
    Description("Allows you to review, add and change Level Rewards.")]
    public sealed class LevelRewards : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "levelrewards").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review currently defined Level Rewards.")]
        public async Task Review(CommandContext ctx)
            => new Commands.LevelRewardsCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to add, remove and modify currently defined Level Rewards.")]
        public async Task Config(CommandContext ctx)
            => new Commands.LevelRewardsCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("phishing"), Aliases("phishingsettings", "phishing-settings"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to phishing link protection.")]
    public sealed class PhishingSettings : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "phishing").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review the currently used settings related to phshing link protection.")]
        public async Task Review(CommandContext ctx)
            => new Commands.PhishingCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change the currently used settings related to phishing link protection.")]
        public async Task Config(CommandContext ctx)
            => new Commands.PhishingCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("bumpreminder"), Aliases("bump-reminder"),
    CommandModule("configuration"),
    Description("Allows you to review, set up and change settings related to the Bump Reminder.")]
    public sealed class BumpReminder : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "bumpreminder").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review the currently used settings related to the Bump Reminder.")]
        public async Task Review(CommandContext ctx)
            => new Commands.BumpReminderCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change currently used settings related to the Bump Reminder.")]
        public async Task Config(CommandContext ctx)
            => new Commands.BumpReminderCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("actionlog"), Aliases("action-log"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to the actionlog.")]
    public sealed class ActionLog : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "actionlog").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review currently used settings related to the actionlog.")]
        public async Task Review(CommandContext ctx)
            => new Commands.ActionLogCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);


        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change currently used settings related to the actionlog.")]
        public async Task Config(CommandContext ctx)
            => new Commands.ActionLogCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("autocrosspost"), Aliases("auto-crosspost", "crosspost"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to automatic crossposting.")]
    public sealed class AutoCrosspost : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "autocrosspost").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review currently used settings related to automatic crossposting.")]
        public async Task Review(CommandContext ctx)
            => new Commands.AutoCrosspostCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change currently used settings related to automatic crossposting.")]
        public async Task Config(CommandContext ctx)
            => new Commands.AutoCrosspostCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }


    [Group("reactionroles"), Aliases("reactionrole", "reaction-roles", "reaction-role"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to Reaction Roles.")]
    public sealed class ReactionRoles : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "reactionroles").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review currently registered Reaction Roles.")]
        public async Task Review(CommandContext ctx)
            => new Commands.ReactionRolesCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to add and remove registered Reaction Roles.")]
        public async Task Config(CommandContext ctx)
            => new Commands.ReactionRolesCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("add"), Description("Allows you to add a reaction role to a message directly, skipping the lengthy questioning. Reply to a message with this command to select the message."), Priority(0)]
        public async Task Add(CommandContext ctx, DiscordEmoji emoji_parameter, DiscordRole role_parameter)
            => new Commands.ReactionRolesCommand.AddCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "emoji_parameter", emoji_parameter }, { "role_parameter", role_parameter }, }).Add(this._bot.watcher, ctx);

        [Command("add"), Description("Allows you to add a reaction role to a message directly, skipping the lengthy questioning. Reply to a message with this command to select the message."), Priority(1)]
        public async Task Add2(CommandContext ctx, DiscordRole role_parameter, DiscordEmoji emoji_parameter)
            => await Add(ctx, emoji_parameter, role_parameter);

        [Command("remove"), Description("Allows you to remove a specific reaction role from a message directly, skipping the lengthy questioning. Reply to a message with this command to select the message.")]
        public async Task Remove(CommandContext ctx, DiscordEmoji emoji_parameter)
            => new Commands.ReactionRolesCommand.RemoveCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "emoji_parameter", emoji_parameter }, }).Add(this._bot.watcher, ctx);

        [Command("removeall"), Description("Allows you to remove all reaction roles from a message directly, skipping the lengthy questioning. Reply to a message with this command to select the message.")]
        public async Task RemoveAll(CommandContext ctx)
            => new Commands.ReactionRolesCommand.RemoveAllCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }


    [Group("invoiceprivacy"), Aliases("in-voice-privacy", "vc-privacy", "vcprivacy"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to In-Voice Text Channel Privacy.")]
    public sealed class InVoiceTextPrivacy : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "invoiceprivacy").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review currently used settings related to In-Voice Text Channel Privacy.")]
        public async Task Review(CommandContext ctx)
            => new Commands.InVoicePrivacyCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change currently used settings related to In-Voice Text Channel Privacy.")]
        public async Task Config(CommandContext ctx)
            => new Commands.InVoicePrivacyCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }


    [Group("invitetracker"), Aliases("invite-tracker", "invitetracking", "invite-tracking"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to Invite Tracking.")]
    public sealed class InviteTracker : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "invitetracker").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review the currently used settings related to Invite Tracking.")]
        public async Task Review(CommandContext ctx)
            => new Commands.InviteTrackerCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change the currently used settings related to Invite Tracking.")]
        public async Task Config(CommandContext ctx)
            => new Commands.InviteTrackerCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("namenormalizer"), Aliases("name-normalizer"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to automatic name normalization.")]
    public sealed class NameNormalizer : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "namenormalizer").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review the currently used settings related to name normalization.")]
        public async Task Review(CommandContext ctx)
            => new Commands.NameNormalizerCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change the currently used settings related to name normalization.")]
        public async Task Config(CommandContext ctx)
            => new Commands.NameNormalizerCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("autounarchive"), Aliases("auto-unarchive"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to automatic thread unarchiving.")]
    public sealed class AutoUnarchive : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "tokendetection", "\n\nThis module allows you to automatically unarchive threads of certain channels. **You will need to lock threads to actually archive them.**", "", "Auto Thread Unarchiver").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review the currently used settings related to automatic thread unarchiving.")]
        public async Task Review(CommandContext ctx)
            => new Commands.AutoUnarchiveCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change the currently used settings related to automatic thread unarchiving.")]
        public async Task Config(CommandContext ctx)
            => new Commands.AutoUnarchiveCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("embedmessages"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to automatic message embedding.")]
    public sealed class MessageEmbedding : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "embedmessages").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review the currently used settings related to automatic message embedding.")]
        public async Task Review(CommandContext ctx)
            => new Commands.EmbedMessageCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change the currently used settings related to automatic message embedding.")]
        public async Task Config(CommandContext ctx)
            => new Commands.EmbedMessageCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("tokendetection"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to automatic token invalidation.")]
    public sealed class TokenDetection : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "tokendetection").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review the currently used settings related to automatic token invalidation.")]
        public async Task Review(CommandContext ctx)
            => new Commands.TokenDetectionCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change the currently used settings related to automatic token invalidation.")]
        public async Task Config(CommandContext ctx)
            => new Commands.TokenDetectionCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("invitenotes"),
    CommandModule("configuration"),
    Description("Allows you to add notes to invite codes.")]
    public sealed class InviteNotes : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "invitenotes").Add(this._bot.watcher, ctx);

        [Command("review"), Description("Allows you to review currently set up invite notes.")]
        public async Task Review(CommandContext ctx)
            => new Commands.InviteNotesCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Description("Allows you to add and remove currently set up invite notes.")]
        public async Task Config(CommandContext ctx)
            => new Commands.InviteNotesCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("vccreator"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to the Voice Channel Creator.")]
    public sealed class VcCreator : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "vccreator").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review currently used settings related to the Voice Channel Creator.")]
        public async Task Review(CommandContext ctx)
            => new Commands.VcCreatorCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change currently used settings related to the Voice Channel Creator.")]
        public async Task Config(CommandContext ctx)
            => new Commands.VcCreatorCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("guild-prefix"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to the guild's prefix.")]
    public sealed class GuildPrefix : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "guild-prefix").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review settings related to the guild's prefix.")]
        public async Task Review(CommandContext ctx)
            => new Commands.PrefixCommand.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change settings related to the guild's prefix.")]
        public async Task Config(CommandContext ctx)
            => new Commands.PrefixCommand.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [Group("guild-language"),
    CommandModule("configuration"),
    Description("Allows you to review and change settings related to the guild's selected language.")]
    public sealed class GuildLanguage : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "guild-language").Add(this._bot.watcher, ctx);

        [Command("review"), Aliases("list"),
        Description("Allows you to review currently used settings related to the guild's selected language.")]
        public async Task Review(CommandContext ctx)
            => new Commands.GuildLanguage.ReviewCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [Command("config"), Aliases("configure", "settings", "list", "modify"),
        Description("Allows you to change currently used settings related to the guild's selected language.")]
        public async Task Config(CommandContext ctx)
            => new Commands.GuildLanguage.ConfigCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }
}
