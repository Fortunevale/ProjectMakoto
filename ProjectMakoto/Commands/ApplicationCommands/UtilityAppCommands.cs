﻿// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;
public class UtilityAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }


    public class HelpAutoComplete : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
        {
            try
            {
                Bot bot = ((Bot)ctx.Services.GetService(typeof(Bot)));

                IEnumerable<DiscordApplicationCommand> filteredCommands = bot.discordClient.GetCommandList(bot)
                    .Where(x => x.Name.Contains(ctx.FocusedOption.Value.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    .Where(x => !x.DefaultMemberPermissions.HasValue || ctx.Member.Permissions.HasPermission(x.DefaultMemberPermissions.Value))
                    .Where(x => x.Type == ApplicationCommandType.ChatInput)
                    .Take(25);

                List<DiscordApplicationCommandAutocompleteChoice> options = filteredCommands
                    .Select(x => new DiscordApplicationCommandAutocompleteChoice(string.Join("-", x.Name.Split(new string[] { "-", "_" }, StringSplitOptions.None)
                        .Select(x => x.FirstLetterToUpper())), x.Name))
                    .ToList();
                return options.AsEnumerable();
            }
            catch (Exception)
            {
                return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
            }
        }
    }

    [SlashCommand("help", "Sends you a list of all available commands, their usage and their description.", dmPermission: false)]
    public async Task Help(InteractionContext ctx, [Option("command", "The command to show help for", true)][Autocomplete(typeof(HelpAutoComplete))] string command = "") 
        => new HelpCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        { 
            { "command", command }
        }).Add(this._bot.watcher, ctx);

    [SlashCommand("user-info", "Displays information the bot knows about you or the mentioned user.", dmPermission: false)]
    public async Task UserInfo(InteractionContext ctx, [Option("User", "The User")] DiscordUser victim = null) 
        => new UserInfoCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        {
            { "victim", victim } 
        }).Add(this._bot.watcher, ctx);

    [SlashCommand("guild-info", "Displays information this or the mentioned guild.", dmPermission: false)]
    public async Task GuildInfo(InteractionContext ctx, [Option("Guild", "The Guild")] string guildId = null) 
        => Task.Run(async () =>
        {
            if (guildId != null && !guildId.IsDigitsOnly())
            {
                _ = ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("❌ `Please use digits.`").AsEphemeral());
                return;
            }

            await new GuildInfoCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "guildId", guildId.ToUInt64() }
            });
        }).Add(this._bot.watcher, ctx);

    [SlashCommand("reminders", "Allows you to manage your reminders.", dmPermission: false)]
    public async Task Reminders(InteractionContext ctx) 
        => new RemindersCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

    [SlashCommand("avatar", "Displays your or the mentioned user's avatar as an embedded image.", dmPermission: false)]
    public async Task Avatar(InteractionContext ctx, [Option("User", "The User")] DiscordUser victim = null) 
        => new AvatarCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        {
            { "victim", victim } 
        }).Add(this._bot.watcher, ctx);

    [SlashCommand("banner", "Displays your or the mentioned user's banner as an embedded image.", dmPermission: false)]
    public async Task Banner(InteractionContext ctx, [Option("User", "The User")] DiscordUser victim = null) 
        => new BannerCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        { { "victim", victim }
        }).Add(this._bot.watcher, ctx);

    [SlashCommand("rank", "Shows your or the mentioned user's rank and rank progress.", dmPermission: false)]
    public async Task Rank(InteractionContext ctx, [Option("User", "The User")] DiscordUser victim = null) 
        => new RankCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        { 
            { "victim", victim } 
        }).Add(this._bot.watcher, ctx);

    [SlashCommand("leaderboard", "Displays the current experience rankings on this server.", dmPermission: false)]
    public async Task Leaderboard(InteractionContext ctx, [Option("amount", "The amount of rankings to show"), MinimumValue(3), MaximumValue(50)] int ShowAmount = 10) 
        => new LeaderboardCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        {
            { "ShowAmount", ShowAmount } 
        }).Add(this._bot.watcher, ctx);

    [SlashCommand("report-host", "Allows you to contribute a new malicious host to our database.", dmPermission: false)]
    public async Task ReportHost(InteractionContext ctx, [Option("url", "The host")] string url) 
        => new ReportHostCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        {
            { "url", url } }
        ).Add(this._bot.watcher, ctx);

    [SlashCommand("upload", "Upload a file to the bot. Only use when instructed to.", dmPermission: false)]
    public async Task Upload(InteractionContext ctx, [Option("file", "The file you want to upload.")] DiscordAttachment attachment) 
        => new UploadCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        { 
            { "stream", await new HttpClient().GetStreamAsync(attachment.Url) }, 
            { "filesize", attachment.FileSize } 
        }).Add(this._bot.watcher, ctx);

    [SlashCommand("urban-dictionary", "Look up a term on Urban Dictionary.", dmPermission: false)]
    public async Task UrbanDictionary(InteractionContext ctx, [Option("term", "The term you want to look up.")] string term) 
        => new UrbanDictionaryCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        {
            { "term", term } 
        }).Add(this._bot.watcher, ctx);

    [SlashCommandGroup("data", "Allows you to request or manage your user data.", dmPermission: false)]
    public class Data : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("request", "Allows you to request your user data.", dmPermission: false)]
        public async Task Request(InteractionContext ctx) 
            => new Commands.Data.RequestCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [SlashCommand("delete", "Allows you to delete your user data and stop Makoto from further processing of your user data.", dmPermission: false)]
        public async Task Delete(InteractionContext ctx) 
            => new Commands.Data.DeleteCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [SlashCommand("policy", "Allows you to view how Makoto processes your data.", dmPermission: false)]
        public async Task Info(InteractionContext ctx) 
            => new Commands.Data.InfoCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }

    [SlashCommand("language", "Change the language Makoto uses.", dmPermission: false)]
    public async Task Language(InteractionContext ctx) 
        => new LanguageCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

    [SlashCommand("credits", "Allows you to view who contributed the bot.", dmPermission: false)]
    public async Task Credits(InteractionContext ctx) 
        => new CreditsCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

    [SlashCommandGroup("vcc", "Allows you to modify your own voice channel.", dmPermission: false)]
    public class VcCreatorManagement : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("open", "Opens your channel so new users can freely join.")]
        public async Task Open(InteractionContext ctx) 
            => new Commands.VcCreator.OpenCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [SlashCommand("close", "Closes your channel. You have to invite people for them to join.")]
        public async Task Close(InteractionContext ctx) 
            => new Commands.VcCreator.CloseCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);

        [SlashCommand("name", "Changes the name of your channel.")]
        public async Task Name(InteractionContext ctx, [Option("name", "Name")] string newName = "") 
            => new Commands.VcCreator.NameCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
            {
                { "newName", newName },
            }).Add(this._bot.watcher, ctx);

        [SlashCommand("limit", "Changes the user limit of your channel.")]
        public async Task Limit(InteractionContext ctx, [Option("limit", "Limit"), MaximumValue(99), MinimumValue(0)] int newLimit) 
            => new Commands.VcCreator.LimitCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
            {
                { "newLimit", newLimit.ToUInt32() },
            }).Add(this._bot.watcher, ctx);

        [SlashCommand("invite", "Invites a new person to your channel.")]
        public async Task Invite(InteractionContext ctx, [Option("user", "User")] DiscordUser victim)
            => new Commands.VcCreator.InviteCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "victim", await victim.ConvertToMember(ctx.Guild) },
            }).Add(this._bot.watcher, ctx);

        [SlashCommand("kick", "Kicks person from your channel.")]
        public async Task Kick(InteractionContext ctx, [Option("user", "User")] DiscordUser victim) 
            => new Commands.VcCreator.KickCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
            {
                { "victim", await victim.ConvertToMember(ctx.Guild) },
            }).Add(this._bot.watcher, ctx);

        [SlashCommand("ban", "Bans person from your channel.")]
        public async Task Ban(InteractionContext ctx, [Option("user", "User")] DiscordUser victim)
            => new Commands.VcCreator.BanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
            {
                { "victim", await victim.ConvertToMember(ctx.Guild) },
            }).Add(this._bot.watcher, ctx);

        [SlashCommand("unban", "Unbans person from your channel.")]
        public async Task Unban(InteractionContext ctx, [Option("user", "User")] DiscordUser victim) 
            => new Commands.VcCreator.UnbanCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
            {
                { "victim", await victim.ConvertToMember(ctx.Guild) },
            }).Add(this._bot.watcher, ctx);


        [SlashCommand("change-owner", "Sets a new person to be the owner of your channel.")]
        public async Task ChangeOwner(InteractionContext ctx, [Option("user", "User")] DiscordUser victim) 
            => new Commands.VcCreator.ChangeOwnerCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "victim", await victim.ConvertToMember(ctx.Guild) },
            }).Add(this._bot.watcher, ctx);
    }

    [ContextMenu(ApplicationCommandType.Message, "Steal Emojis", dmPermission: false)]
    public async Task EmojiStealer(ContextMenuContext ctx)
        => new EmojiStealerCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        {
            { "message", ctx.TargetMessage } 
        }).Add(this._bot.watcher, ctx);

    [ContextMenu(ApplicationCommandType.Message, "Translate Message", dmPermission: false)]
    public async Task Translate(ContextMenuContext ctx)
        => new TranslateCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object> 
        {
            { "message", ctx.TargetMessage }
        }).Add(this._bot.watcher, ctx);
}
