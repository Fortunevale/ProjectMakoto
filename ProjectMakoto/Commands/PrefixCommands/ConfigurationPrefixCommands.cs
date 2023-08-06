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

    [Group("config"), Description("Allows you to configure Makoto.")]
    public sealed class Configuration : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => _ = PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "config");


        [Command("join"), Description("Allows you to review and change settings in the event somebody joins the server.")]
        public async Task Join(CommandContext ctx)
            => _ = new JoinCommand().ExecuteCommand(ctx, this._bot);

        [Command("experience"), Description("Allows you to review and change settings related to experience.")]
        public async Task Experience(CommandContext ctx)
            => _ = new ExperienceCommand().ExecuteCommand(ctx, this._bot);

        [Command("levelrewards"), Description("Allows you to review, add and change Level Rewards.")]
        public async Task LevelRewards(CommandContext ctx)
            => _ = new LevelRewardsCommand().ExecuteCommand(ctx, this._bot);

        [Command("phishing"), Description("Allows you to review and change settings related to phishing link protection.")]
        public async Task Phishing(CommandContext ctx)
            => _ = new PhishingCommand().ExecuteCommand(ctx, this._bot);

        [Command("bumpreminder"), Description("Allows you to review, set up and change settings related to the Bump Reminder.")]
        public async Task BumpReminder(CommandContext ctx)
            => _ = new BumpReminderCommand().ExecuteCommand(ctx, this._bot);

        [Command("actionlog"), Description("Allows you to review and change settings related to the actionlog.")]
        public async Task ActionLog(CommandContext ctx)
            => _ = new ActionLogCommand().ExecuteCommand(ctx, this._bot);

        [Command("autocrosspost"), Description("Allows you to review and change settings related to automatic crossposting.")]
        public async Task AutoCrosspost(CommandContext ctx)
            => _ = new AutoCrosspostCommand().ExecuteCommand(ctx, this._bot);

        [Group("reactionroles"), Description("Allows you to review and change settings related to Reaction Roles.")]
        public class ReactionRoles : BaseCommandModule
        {
            public Bot _bot { private get; set; }

            [Command("help"), Description("Sends a list of available sub-commands")]
            public async Task Help(CommandContext ctx)
                => _ = PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "reactionroles");

            [GroupCommand, Command("manage"), Description("Allows you to review and change settings related to Reaction Roles.")]
            public async Task Manage(CommandContext ctx)
            => _ = new Commands.ReactionRolesCommand.ConfigCommand().ExecuteCommand(ctx, this._bot);

            [Command("add"), Description("Allows you to add a reaction role to a message directly, skipping the lengthy questioning. Reply to a message with this command to select the message."), Priority(0)]
            public async Task Add(CommandContext ctx, DiscordEmoji emoji_parameter, DiscordRole role_parameter)
                => _ = new Commands.ReactionRolesCommand.AddCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "emoji_parameter", emoji_parameter }, { "role_parameter", role_parameter }, });

            [Command("add"), Description("Allows you to add a reaction role to a message directly, skipping the lengthy questioning. Reply to a message with this command to select the message."), Priority(1)]
            public async Task Add2(CommandContext ctx, DiscordRole role_parameter, DiscordEmoji emoji_parameter)
                => await this.Add(ctx, emoji_parameter, role_parameter);

            [Command("remove"), Description("Allows you to remove a specific reaction role from a message directly, skipping the lengthy questioning. Reply to a message with this command to select the message.")]
            public async Task Remove(CommandContext ctx, DiscordEmoji emoji_parameter)
                => _ = new Commands.ReactionRolesCommand.RemoveCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> { { "emoji_parameter", emoji_parameter }, });

            [Command("removeall"), Description("Allows you to remove all reaction roles from a message directly, skipping the lengthy questioning. Reply to a message with this command to select the message.")]
            public async Task RemoveAll(CommandContext ctx)
                => _ = new Commands.ReactionRolesCommand.RemoveAllCommand().ExecuteCommand(ctx, this._bot);
        }

        [Command("invoiceprivacy"), Description("Allows you to review and change settings related to In-Voice Text Channel Privacy.")]
        public async Task InVoicePrivacy(CommandContext ctx)
            => _ = new InVoicePrivacyCommand().ExecuteCommand(ctx, this._bot);

        [Command("invitetracker"), Description("Allows you to review and change settings related to Invite Tracking.")]
        public async Task InviteTracker(CommandContext ctx)
            => _ = new InviteTrackerCommand().ExecuteCommand(ctx, this._bot);

        [Command("namenormalizer"), Description("Allows you to review and change settings related to automatic name normalization.")]
        public async Task NameNormalizer(CommandContext ctx)
            => _ = new NameNormalizerCommand().ExecuteCommand(ctx, this._bot);

        [Command("autounarchive"), Description("Allows you to review and change settings related to automatic thread unarchiving.")]
        public async Task AutoUnarchive(CommandContext ctx)
            => _ = new AutoUnarchiveCommand().ExecuteCommand(ctx, this._bot);

        [Command("embedmessages"), Description("Allows you to review and change settings related to automatic message embedding.")]
        public async Task EmbedMessages(CommandContext ctx)
            => _ = new EmbedMessageCommand().ExecuteCommand(ctx, this._bot);

        [Command("tokendetection"), Description("Allows you to review and change settings related to automatic token invalidation.")]
        public async Task TokenDetection(CommandContext ctx)
            => _ = new TokenDetectionCommand().ExecuteCommand(ctx, this._bot);

        [Command("invitenotes"), Description("Allows you to add notes to invite codes.")]
        public async Task InviteNotes(CommandContext ctx)
            => _ = new InviteNotesCommand().ExecuteCommand(ctx, this._bot);

        [Command("vccreator"), Description("Allows you to review and change settings related to the Voice Channel Creator.")]
        public async Task VcCreator(CommandContext ctx)
            => _ = new VcCreatorCommand().ExecuteCommand(ctx, this._bot);

        [Command("guild-language"), Description("Allows you to review and change settings related to the guild's selected language.")]
        public async Task GuildLanguage(CommandContext ctx)
            => _ = new GuildLanguageCommand().ExecuteCommand(ctx, this._bot);

        [Command("guild-prefix"), Description("Allows you to review and change settings related to the guild's prefix.")]
        public async Task GuildPrefix(CommandContext ctx)
            => _ = new PrefixCommand().ExecuteCommand(ctx, this._bot);
    }
}
