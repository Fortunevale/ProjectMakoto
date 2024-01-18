// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.PrefixCommands;

public sealed class UtilityPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("help"),
    
    Description("Sends you a list of all available commands, their usage and their description.")]
    public async Task Help(CommandContext ctx, [Description("Command")] string command = "")
        => _ = new HelpCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "command", command }
        });



    [Command("user-info"), Aliases("userinfo"),
    
    Description("Displays information the bot knows about you or the mentioned user.")]
    public async Task UserInfo(CommandContext ctx, DiscordUser victim = null)
        => _ = new UserInfoCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim }
        });


    [Command("guild-info"),
    
    Description("Displays information this or the mentioned guild.")]
    public async Task GuildInfo(CommandContext ctx, [Description("GuildId")] ulong? guildId = null)
        => _ = new GuildInfoCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "guildId", guildId }
        });


    [Command("reminders"),
    
    Description("Allows you to manage your reminders.")]
    public async Task Reminders(CommandContext ctx)
        => _ = new RemindersCommand().ExecuteCommand(ctx, this._bot);


    [Command("avatar"), Aliases("pfp"),
    
    Description("Displays your or the mentioned user's avatar as an embedded image.")]
    public async Task Avatar(CommandContext ctx, DiscordUser victim = null)
        => _ = new AvatarCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim }
        });



    [Command("banner"),
    
    Description("Displays your or the mentioned user's banner as an embedded image.")]
    public async Task Banner(CommandContext ctx, DiscordUser victim = null)
        => _ = new BannerCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim }
        });



    [Command("rank"), Aliases("level", "lvl"),
    
    Description("Shows your or the mentioned user's rank and rank progress.")]
    public async Task Rank(CommandContext ctx, DiscordUser victim = null)
        => _ = new RankCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "victim", victim }
        });



    [Command("leaderboard"),
    
    Description("Displays the current experience rankings on this server.")]
    public async Task Leaderboard(CommandContext ctx, [Description("3-50")] int ShowAmount = 10)
        => _ = new LeaderboardCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "ShowAmount", ShowAmount }
        });



    [Command("report-host"),
    
    Description("Allows you to contribute a new malicious host to our database.")]
    public async Task ReportHost(CommandContext ctx, [Description("Host")] string url)
        => _ = new ReportHostCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "url", url }
        });



    [Command("emoji"), Aliases("emojis", "emote", "steal", "grab", "sticker", "stickers"),
    
    Description("Steals all emojis and stickers of a message. Reply to a message to select it.")]
    public async Task EmojiStealer(CommandContext ctx)
        => _ = new EmojiStealerCommand().ExecuteCommand(ctx, this._bot);



    [Command("translate"),
    
    Description("Allows you to translate a message. Reply to a message to select it.")]
    public async Task Translate(CommandContext ctx)
        => _ = new TranslateCommand().ExecuteCommand(ctx, this._bot);



    [Command("upload"),
    
    Description("Upload a file to the bot. Only use when instructed to.")]
    public async Task Upload(CommandContext ctx)
        => _ = Task.Run(async () =>
        {
            if (!ctx.Message.Attachments.Any())
            {
                _ = ctx.SendSyntaxError("<File>");
                return;
            }

            await new UploadCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "stream", await new HttpClient().GetStreamAsync(ctx.Message.Attachments[0].Url) },
                { "filesize", ctx.Message.Attachments[0].FileSize }
            });
        });



    [Command("urban-dictionary"),
    
    Description("Look up a term on Urban Dictionary.")]
    public async Task UrbanDictionary(CommandContext ctx, [RemainingText] string term)
        => _ = new UrbanDictionaryCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "term", term }
        });



    [Group("data"),
    
    Description("Allows you to request or manage your user data.")]
    public sealed class Join : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => _ = PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "data");

        [Command("request"), Description("Allows you to request your user data.")]
        public async Task Request(CommandContext ctx)
            => _ = new Commands.Data.RequestCommand().ExecuteCommand(ctx, this._bot);

        [Command("delete"), Description("Allows you to delete your user data and stop Makoto from further processing of your user data.")]
        public async Task Delete(CommandContext ctx)
            => _ = new Commands.Data.DeleteCommand().ExecuteCommand(ctx, this._bot);

        [Command("policy"), Description("Allows you to view how Makoto processes your data.")]
        public async Task Info(CommandContext ctx)
            => _ = new Commands.Data.InfoCommand().ExecuteCommand(ctx, this._bot);
    }



    [Command("language"),
    
    Description("Change the language Makoto uses.")]
    public async Task Language(CommandContext ctx)
        => _ = new LanguageCommand().ExecuteCommand(ctx, this._bot);



    [Command("credits"),
    
    Description("Allows you to view who contributed the bot.")]
    public async Task Credits(CommandContext ctx)
        => _ = new CreditsCommand().ExecuteCommand(ctx, this._bot);

    [Group("vcc"),
    
    Description("Allows you to modify your own voice channel.")]
    public sealed class VcCreatorManagement : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => _ = PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "vcc");

        [Command("open"), Description("Opens your channel so new users can freely join.")]
        public async Task Open(CommandContext ctx)
            => _ = new Commands.VcCreator.OpenCommand().ExecuteCommand(ctx, this._bot);

        [Command("close"), Description("Closes your channel. You have to invite people for them to join.")]
        public async Task Close(CommandContext ctx)
            => _ = new Commands.VcCreator.CloseCommand().ExecuteCommand(ctx, this._bot);

        [Command("name"), Description("Changes the name of your channel.")]
        public async Task Name(CommandContext ctx, [RemainingText] string newName)
            => _ = new Commands.VcCreator.NameCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "newName", newName },
            });

        [Command("limit"), Description("Changes the user limit of your channel.")]
        public async Task Limit(CommandContext ctx, uint newLimit)
            => _ = new Commands.VcCreator.LimitCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "newLimit", newLimit },
            });

        [Command("invite"), Description("Invites a new person to your channel.")]
        public async Task Invite(CommandContext ctx, DiscordMember victim)
            => _ = new Commands.VcCreator.InviteCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });

        [Command("kick"), Description("Kicks person from your channel.")]
        public async Task Kick(CommandContext ctx, DiscordMember victim)
            => _ = new Commands.VcCreator.KickCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });

        [Command("ban"), Description("Bans person from your channel.")]
        public async Task Ban(CommandContext ctx, DiscordMember victim)
            => _ = new Commands.VcCreator.BanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });

        [Command("unban"), Description("Unbans person from your channel.")]
        public async Task Unban(CommandContext ctx, DiscordMember victim)
            => _ = new Commands.VcCreator.UnbanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });

        [Command("change-owner"), Description("Sets a new person to be the owner of your channel.")]
        public async Task ChangeOwner(CommandContext ctx, DiscordMember victim)
            => _ = new Commands.VcCreator.ChangeOwnerCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "victim", victim },
            });
    }
}
