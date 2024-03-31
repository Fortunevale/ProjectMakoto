// Project Makoto
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
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The User")),
                    new MakotoCommand("guild-info", "Displays information this or the mentioned guild.", typeof(GuildInfoCommand),
                        new MakotoCommandOverload(typeof(string), "guild", "The Guild")),
                    new MakotoCommand("reminders", "Allows you to manage your reminders.", typeof(RemindersCommand)),
                    new MakotoCommand("avatar", "Displays your or the mentioned user's avatar as an embedded image.", typeof(AvatarCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The User")),
                    new MakotoCommand("banner", "Displays your or the mentioned user's banner as an embedded image.", typeof(BannerCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The User")),
                    new MakotoCommand("rank", "Shows your or the mentioned user's rank and rank progress.", typeof(RankCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The User")),
                    new MakotoCommand("leaderboard", "Displays the current experience rankings on this server.", typeof(LeaderboardCommand),
                        new MakotoCommandOverload(typeof(int), "amount", "The amount of rankings to show")
                            .WithMinimumValue(3)
                            .WithMaximumValue(50)),
                    new MakotoCommand("report-host", "Allows you to contribute a new malicious host to our database.", typeof(ReportHostCommand),
                        new MakotoCommandOverload(typeof(string), "url", "The host")),
                    new MakotoCommand("report-translation", "Allows you to report missing, invalid or incorrect translations in Makoto.", typeof(ReportHostCommand),
                        new MakotoCommandOverload(typeof(ReportTranslationType), "affected_type", "The type of module that is affected"),
                        new MakotoCommandOverload(typeof(string), "component", "The affected component")
                            .WithAutoComplete(typeof(AutocompleteProviders.ReportTranslationAutoComplete)),
                        new MakotoCommandOverload(typeof(ReportTranslationReason), "report_type", "What type of issue you're reporting"),
                        new MakotoCommandOverload(typeof(string), "additional_information", "Any additional information you can give us", false)),
                    new MakotoCommand("upload", "Upload a file to the bot. Only use when instructed to.", typeof(UploadCommand),
                        new MakotoCommandOverload(typeof(DiscordAttachment), "file", "The file you want to upload.")),
                    new MakotoCommand("urban-dictionary", "Look up a term on Urban Dictionary.", typeof(UrbanDictionaryCommand),
                        new MakotoCommandOverload(typeof(string), "term", "The term you want to look up.")),
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
                            new MakotoCommandOverload(typeof(string), "name", "The name")),
                        new MakotoCommand("limit", "Changes the user limit of your channel.", typeof(VcCreator.LimitCommand),
                            new MakotoCommandOverload(typeof(int), "limit", "The limit")
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
                        new MakotoCommand("change-owner", "Sets a new person to be the owner of your channel.", typeof(VcCreator.UnbanCommand),
                            new MakotoCommandOverload(typeof(DiscordUser), "user", "User"))),
                    new MakotoCommand(ApplicationCommandType.Message, "Steal Emojis", "h", typeof(EmojiStealerCommand), "emoji"),
                ]).WithPriority(999)
        ];
}
