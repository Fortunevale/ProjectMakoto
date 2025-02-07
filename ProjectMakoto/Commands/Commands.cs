﻿// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;
internal static class Commands
{
    public static List<MakotoModule> GetList() => [
            new MakotoModule("Utility", [
                    new MakotoCommand("help", "Sends you a list of all available commands, their usage and their description.", typeof(HelpCommand),
                        new MakotoCommandOverload(typeof(string), "command", "The command to show help for", false)
                            .WithAutoComplete(typeof(AutocompleteProviders.HelpAutoComplete))),

                    new MakotoCommand("user-info", "Displays information the bot knows about you or the mentioned user.", typeof(UserInfoCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The User", false))
                            .WithAliases("userinfo"),

                    new MakotoCommand("guild-info", "Displays information this or the mentioned guild.", typeof(GuildInfoCommand),
                        new MakotoCommandOverload(typeof(string), "guild", "The Guild", false))
                            .WithAliases("guildinfo"),

                    new MakotoCommand("reminders", "Allows you to manage your reminders.", typeof(RemindersCommand)),

                    new MakotoCommand("avatar", "Displays your or the mentioned user's avatar as an embedded image.", typeof(AvatarCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The User", false))
                            .WithAliases("pfp"),

                    new MakotoCommand("banner", "Displays your or the mentioned user's banner as an embedded image.", typeof(BannerCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The User", false)),

                    new MakotoCommand("rank", "Shows your or the mentioned user's rank and rank progress.", typeof(RankCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The User", false))
                            .WithAliases("level", "lvl"),

                    new MakotoCommand("leaderboard", "Displays the current experience rankings on this server.", typeof(LeaderboardCommand),
                        new MakotoCommandOverload(typeof(int), "amount", "The amount of rankings to show", false)
                            .WithMinimumValue(3)
                            .WithMaximumValue(50)),

                    new MakotoCommand("report-host", "Allows you to contribute a new malicious host to our database.", typeof(ReportHostCommand),
                        new MakotoCommandOverload(typeof(string), "url", "The host", UseRemainingString: true)),

                    new MakotoCommand("report-translation", "Allows you to report missing, invalid or incorrect translations in Makoto.", typeof(ReportTranslationCommand),
                        new MakotoCommandOverload(typeof(ReportTranslationType), "affected_type", "The type of module that is affected"),
                        new MakotoCommandOverload(typeof(string), "component", "The affected component")
                            .WithAutoComplete(typeof(AutocompleteProviders.ReportTranslationAutoComplete)),
                        new MakotoCommandOverload(typeof(ReportTranslationReason), "report_type", "What type of issue you're reporting"),
                        new MakotoCommandOverload(typeof(string), "additional_information", "Any additional information you can give us", false, true)),

                    new MakotoCommand("upload", "Upload a file to the bot. Only use when instructed to.", typeof(UploadCommand),
                        new MakotoCommandOverload(typeof(DiscordAttachment), "file", "The file you want to upload.")),

                    new MakotoCommand("urban-dictionary", "Look up a term on Urban Dictionary.", typeof(UrbanDictionaryCommand),
                        new MakotoCommandOverload(typeof(string), "term", "The term you want to look up.", UseRemainingString: true)),

                    new MakotoCommand("data", "Allows you to request or manage your user data.",
                        new MakotoCommand("request", "Allows you to request your user data.", typeof(Data.RequestCommand)),
                        new MakotoCommand("delete", "Allows you to delete your user data and stop Makoto from further processing of your user data.", typeof(Data.DeleteCommand)),
                        new MakotoCommand("policy", "Allows you to view how Makoto processes your data.", typeof(Data.InfoCommand))),

                    new MakotoCommand("language", "Change the language Makoto uses.", typeof(LanguageCommand)),

                    new MakotoCommand("credits", "Allows you to view who contributed the bot.", typeof(CreditsCommand)),

                    new MakotoCommand("vcc", "Allows you to modify your own voice channel.", 
                        new MakotoCommand("open", "Opens your channel so new users can freely join.", typeof(VcCreator.OpenCommand)),
                        new MakotoCommand("close", "Closes your channel. You have to invite people for them to join.", typeof(VcCreator.CloseCommand)),
                        new MakotoCommand("name", "Changes the name of your channel.", typeof(VcCreator.NameCommand),
                            new MakotoCommandOverload(typeof(string), "name", "The name", false, UseRemainingString: true)),
                        new MakotoCommand("limit", "Changes the user limit of your channel.", typeof(VcCreator.LimitCommand),
                            new MakotoCommandOverload(typeof(int), "limit", "The limit", false)
                                .WithMaximumValue(99)
                                .WithMinimumValue(0)),
                        new MakotoCommand("invite", "Invites a new person to your channel.", typeof(VcCreator.InviteCommand),
                            new MakotoCommandOverload(typeof(DiscordUser), "user", "User")),
                        new MakotoCommand("kick", "Kicks person from your channel.", typeof(VcCreator.KickCommand),
                            new MakotoCommandOverload(typeof(DiscordUser), "user", "User")),
                        new MakotoCommand("ban", "Bans person from your channel.", typeof(VcCreator.BanCommand),
                            new MakotoCommandOverload(typeof(DiscordUser), "user", "User")),
                        new MakotoCommand("unban", "Unbans person from your channel.", typeof(VcCreator.UnbanCommand),
                            new MakotoCommandOverload(typeof(DiscordUser), "user", "User")),
                        new MakotoCommand("change-owner", "Sets a new person to be the owner of your channel.", typeof(VcCreator.ChangeOwnerCommand),
                            new MakotoCommandOverload(typeof(DiscordUser), "user", "User"))),

                    new MakotoCommand(ApplicationCommandType.Message, "Steal Emojis", "Steals all emojis and stickers of a message. Reply to a message to select it.", typeof(EmojiStealerCommand), "emoji")
                        .WithAliases("emojis", "emote", "steal", "grab", "sticker", "stickers"),
                ]).WithPriority(999),

            new MakotoModule("Moderation", [
                    new MakotoCommand("purge", "Deletes the specified amount of messages.", typeof(PurgeCommand),
                        new MakotoCommandOverload(typeof(int), "number", "1-2000")
                            .WithMinimumValue(1)
                            .WithMaximumValue(2000),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "Only delete messages by this user", false))
                        .WithRequiredPermissions(Permissions.ManageMessages),

                    new MakotoCommand("guild-purge", "Scans all channels and deletes the specified user's messages.", typeof(GuildPurgeCommand),
                        new MakotoCommandOverload(typeof(int), "number", "1-2000")
                            .WithMinimumValue(1)
                            .WithMaximumValue(2000),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "Only delete messages by this user"))
                        .WithRequiredPermissions(Permissions.ManageMessages | Permissions.ManageChannels),

                    new MakotoCommand("clearbackup", "Clears the stored roles and nickname of a user.", typeof(ClearBackupCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "Only delete messages by this user"))
                        .WithRequiredPermissions(Permissions.ManageRoles),

                    new MakotoCommand("timeout", "Sets the specified user into a timeout.", typeof(TimeoutCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user"),
                        new MakotoCommandOverload(typeof(string), "duration", "The duration", false),
                        new MakotoCommandOverload(typeof(string), "reason", "The reason", false, true))
                        .WithRequiredPermissions(Permissions.ModerateMembers),

                    new MakotoCommand("remove-timeout", "Removes a timeout from the specified user.", typeof(RemoveTimeoutCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user"))
                        .WithRequiredPermissions(Permissions.ModerateMembers),

                    new MakotoCommand("kick", "Kicks the specified user.", typeof(KickCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user"),
                        new MakotoCommandOverload(typeof(string), "reason", "The reason", false, true))
                        .WithRequiredPermissions(Permissions.KickMembers),

                    new MakotoCommand("ban", "Bans the specified user.", typeof(BanCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user"),
                        new MakotoCommandOverload(typeof(int), "days", "Days of messages to delete")
                            .WithMinimumValue(0)
                            .WithMaximumValue(7),
                        new MakotoCommandOverload(typeof(string), "reason", "The reason", false, true))
                        .WithRequiredPermissions(Permissions.BanMembers),

                    new MakotoCommand("softban", "Soft bans the specified user.", typeof(SoftBanCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user"),
                        new MakotoCommandOverload(typeof(int), "days", "Days of messages to delete")
                            .WithMinimumValue(0)
                            .WithMaximumValue(7),
                        new MakotoCommandOverload(typeof(string), "reason", "The reason", false, true))
                        .WithRequiredPermissions(Permissions.BanMembers),

                    new MakotoCommand("unban", "Unbans the specified user.", typeof(UnbanCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user"))
                        .WithRequiredPermissions(Permissions.BanMembers),

                    new MakotoCommand("follow", "Allows you to follow an announcement channel from our support server.", typeof(FollowUpdatesCommand),
                        new MakotoCommandOverload(typeof(FollowChannel), "channel", "The channel"))
                        .WithRequiredPermissions(Permissions.ManageWebhooks),

                    new MakotoCommand("moveall", "Move all users in your Voice Channel to another Voice Channel.", typeof(MoveAllCommand),
                        new MakotoCommandOverload(typeof(DiscordChannel), "channel", "The channel to move to.")
                            .WithChannelType(ChannelType.Voice))
                        .WithRequiredPermissions(Permissions.MoveMembers),

                    new MakotoCommand("movehere", "Move all users from another Voice Channel to your Voice Channel.", typeof(MoveHereCommand),
                        new MakotoCommandOverload(typeof(DiscordChannel), "channel", "The channel to move from.")
                            .WithChannelType(ChannelType.Voice))
                        .WithRequiredPermissions(Permissions.MoveMembers),

                    new MakotoCommand("customembed", "Create an embedded message", typeof(CustomEmbedCommand))
                        .WithRequiredPermissions(Permissions.EmbedLinks | Permissions.ManageChannels),

                    new MakotoCommand("override-bump-time", "Allows fixing of the last bump in case Disboard did not properly post a message.", typeof(ManualBumpCommand))
                        .WithRequiredPermissions(Permissions.ManageChannels),
                ]).WithPriority(995),

            new MakotoModule("Configuration", [
                    new MakotoCommand("config", "Allows you to configure Makoto.",
                        new MakotoCommand("join", "Allows you to review and change settings in the event somebody joins the server.", typeof(Configuration.JoinCommand)),
                        new MakotoCommand("experience", "Allows you to review and change settings related to experience.", typeof(Configuration.ExperienceCommand)),
                        new MakotoCommand("levelrewards", "Allows you to review, add and change Level Rewards.", typeof(Configuration.LevelRewardsCommand)),
                        new MakotoCommand("phishing", "Allows you to review and change settings related to phishing link protection.", typeof(Configuration.PhishingCommand)),
                        new MakotoCommand("bumpreminder", "Allows you to review, set up and change settings related to the Bump Reminder.", typeof(Configuration.BumpReminderCommand)),
                        new MakotoCommand("actionlog", "Allows you to review and change settings related to the actionlog.", typeof(Configuration.ActionLogCommand)),
                        new MakotoCommand("autocrosspost", "Allows you to review and change settings related to automatic crossposting.", typeof(Configuration.AutoCrosspostCommand)),
                        new MakotoCommand("reactionroles", "Allows you to review and change settings related to Reaction Roles.", typeof(ReactionRolesCommand.ConfigCommand)),
                        new MakotoCommand("invoiceprivacy", "Allows you to review and change settings related to In-Voice Text Channel Privacy.", typeof(Configuration.InVoicePrivacyCommand)),
                        new MakotoCommand("invitetracker", "Allows you to review and change settings related to Invite Tracking.", typeof(Configuration.InviteTrackerCommand)),
                        new MakotoCommand("namenormalizer", "Allows you to review and change settings related to automatic name normalization.", typeof(Configuration.NameNormalizerCommand)),
                        new MakotoCommand("autounarchive", "Allows you to review and change settings related to automatic thread unarchiving.", typeof(Configuration.AutoUnarchiveCommand)),
                        new MakotoCommand("embedmessages", "Allows you to review and change settings related to automatic message embedding.", typeof(Configuration.EmbedMessageCommand)),
                        new MakotoCommand("tokendetection", "Allows you to review and change settings related to automatic token invalidation.", typeof(Configuration.TokenDetectionCommand)),
                        new MakotoCommand("invitenotes", "Allows you to add notes to invite codes.", typeof(Configuration.InviteNotesCommand)),
                        new MakotoCommand("vccreator", "Allows you to review and change settings related to the Voice Channel Creator.", typeof(Configuration.VcCreatorCommand)),
                        new MakotoCommand("guild-language", "Allows you to review and change settings related to the guild's selected language.", typeof(Configuration.GuildLanguageCommand)),
                        new MakotoCommand("guild-prefix", "Allows you to review and change settings related to the guild's prefix.", typeof(Configuration.PrefixCommand)))
                    .WithRequiredPermissions(Permissions.Administrator),

                    new MakotoCommand(ApplicationCommandType.Message, "Add a Reaction Role", "Allows you to add a reaction role to a message directly.", typeof(ReactionRolesCommand.AddCommand))
                        .WithRequiredPermissions(Permissions.Administrator),
                    new MakotoCommand(ApplicationCommandType.Message, "Remove a Reaction Role", "Allows you to remove a specific reaction role from a message directly.", typeof(ReactionRolesCommand.RemoveCommand))
                        .WithRequiredPermissions(Permissions.Administrator),
                    new MakotoCommand(ApplicationCommandType.Message, "Remove all Reaction Roles", "Allows you to remove all reaction roles from a message directly.", typeof(ReactionRolesCommand.RemoveAllCommand))
                        .WithRequiredPermissions(Permissions.Administrator),
                ]).WithPriority(994),
        ];
}
