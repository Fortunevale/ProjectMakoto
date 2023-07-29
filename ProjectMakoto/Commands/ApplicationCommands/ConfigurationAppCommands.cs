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

    [SlashCommandGroup("config", "Allows you to configure Makoto.", (long)Permissions.Administrator, dmPermission: false)]
    public sealed class Configuration : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("join", "Allows you to review and change settings in the event somebody joins the server.")]
        public async Task Join(InteractionContext ctx)
            => _ = new JoinCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("experience", "Allows you to review and change settings related to experience.")]
        public async Task Experience(InteractionContext ctx)
            => _ = new ExperienceCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("levelrewards", "Allows you to review, add and change Level Rewards.")]
        public async Task LevelRewards(InteractionContext ctx)
            => _ = new LevelRewardsCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("phishing", "Allows you to review and change settings related to phishing link protection.")]
        public async Task Phishing(InteractionContext ctx)
            => _ = new PhishingCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("bumpreminder", "Allows you to review, set up and change settings related to the Bump Reminder.")]
        public async Task BumpReminder(InteractionContext ctx)
            => _ = new BumpReminderCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("actionlog", "Allows you to review and change settings related to the actionlog.")]
        public async Task ActionLog(InteractionContext ctx)
            => _ = new ActionLogCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("autocrosspost", "Allows you to review and change settings related to automatic crossposting.")]
        public async Task AutoCrosspost(InteractionContext ctx)
            => _ = new AutoCrosspostCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("reactionroles", "Allows you to review and change settings related to Reaction Roles.")]
        public async Task ReactionRoles(InteractionContext ctx)
            => _ = new Commands.ReactionRolesCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("invoiceprivacy", "Allows you to review and change settings related to In-Voice Text Channel Privacy.")]
        public async Task InVoicePrivacy(InteractionContext ctx)
            => _ = new InVoicePrivacyCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("invitetracker", "Allows you to review and change settings related to Invite Tracking.")]
        public async Task InviteTracker(InteractionContext ctx)
            => _ = new InviteTrackerCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("namenormalizer", "Allows you to review and change settings related to automatic name normalization.")]
        public async Task NameNormalizer(InteractionContext ctx)
            => _ = new NameNormalizerCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("autounarchive", "Allows you to review and change settings related to automatic thread unarchiving.")]
        public async Task AutoUnarchive(InteractionContext ctx)
            => _ = new AutoUnarchiveCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("embedmessages", "Allows you to review and change settings related to automatic message embedding.")]
        public async Task EmbedMessages(InteractionContext ctx)
            => _ = new EmbedMessageCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("tokendetection", "Allows you to review and change settings related to automatic token invalidation.")]
        public async Task TokenDetection(InteractionContext ctx)
            => _ = new TokenDetectionCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("invitenotes", "Allows you to add notes to invite codes.")]
        public async Task InviteNotes(InteractionContext ctx)
            => _ = new InviteNotesCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("vccreator", "Allows you to review and change settings related to the Voice Channel Creator.")]
        public async Task VcCreator(InteractionContext ctx)
            => _ = new VcCreatorCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("guild-language", "Allows you to review and change settings related to the guild's selected language.")]
        public async Task GuildLanguage(InteractionContext ctx)
            => _ = new GuildLanguageCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("guild-prefix", "Allows you to review and change settings related to the guild's prefix.")]
        public async Task GuildPrefix(InteractionContext ctx)
            => _ = new PrefixCommand().ExecuteCommand(ctx, this._bot);
    }

    [PrefixCommandAlternative("config reactionroles add")]
    [ContextMenu(ApplicationCommandType.Message, "Add a Reaction Role", (long)Permissions.Administrator, dmPermission: false)]
    public async Task Add(ContextMenuContext ctx)
    => _ = new Commands.ReactionRolesCommand.AddCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "message", ctx.TargetMessage }, });

    [PrefixCommandAlternative("config reactionroles remove")]
    [ContextMenu(ApplicationCommandType.Message, "Remove a Reaction Role", (long)Permissions.Administrator, dmPermission: false)]
    public async Task Remove(ContextMenuContext ctx)
        => _ = new Commands.ReactionRolesCommand.RemoveCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "message", ctx.TargetMessage }, });

    [PrefixCommandAlternative("config reactionroles removeall")]
    [ContextMenu(ApplicationCommandType.Message, "Remove all Reaction Roles", (long)Permissions.Administrator, dmPermission: false)]
    public async Task RemoveAll(ContextMenuContext ctx)
        => _ = new Commands.ReactionRolesCommand.RemoveAllCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "message", ctx.TargetMessage }, });
}
