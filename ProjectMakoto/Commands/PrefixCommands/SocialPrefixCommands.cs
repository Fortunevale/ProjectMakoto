// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.PrefixCommands;

public sealed class SocialPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }

    [Command("afk"),
    CommandModule("social"),
    Description("Allows you to set yourself AFK. Users who ping you will be notified that you're unavailable.")]
    public async Task Afk(CommandContext ctx, [RemainingText][Description("Text (<128 characters)")] string reason = "-")
        => new AfkCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "reason", reason }
        }).Add(this._bot.watcher, ctx);

    [Command("cuddle"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Cuddle with another user.")]
    public async Task Cuddle(CommandContext ctx, DiscordUser user)
        => new CuddleCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }).Add(this._bot.watcher, ctx);



    [Command("kiss"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Kiss another user.")]
    public async Task Kiss(CommandContext ctx, DiscordUser user)
        => new KissCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }).Add(this._bot.watcher, ctx);



    [Command("slap"), Aliases("bonk", "punch"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Slap another user.")]
    public async Task Slap(CommandContext ctx, DiscordUser user)
        => new SlapCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }).Add(this._bot.watcher, ctx);



    [Command("kill"), Aliases("waste"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Kill another user..?")]
    public async Task Kill(CommandContext ctx, DiscordUser user)
        => new KillCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }).Add(this._bot.watcher, ctx);



    [Command("boop"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give another user a boop!")]
    public async Task Boop(CommandContext ctx, DiscordUser user)
        => new BoopCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }).Add(this._bot.watcher, ctx);



    [Command("highfive"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give a high five!")]
    public async Task Highfive(CommandContext ctx, DiscordUser user)
        => new HighFiveCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }).Add(this._bot.watcher, ctx);



    [Command("hug"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Hug another user!")]
    public async Task Hug(CommandContext ctx, DiscordUser user)
        => new HugCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }).Add(this._bot.watcher, ctx);



    [Command("pat"), Aliases("pet", "headpat", "headpet"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give someone some headpats!")]
    public async Task Pat(CommandContext ctx, DiscordUser user)
        => new PatCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }).Add(this._bot.watcher, ctx);
}
