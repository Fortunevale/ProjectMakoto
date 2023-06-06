// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;
public sealed class SocialAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("afk", "Allows you to set yourself AFK. Users who ping you will be notified that you're unavailable.", dmPermission: false)]
    public async Task UserInfo(InteractionContext ctx, [Option("reason", "The reason")] string reason = "-") 
        => new AfkCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
        {
            { "reason", reason }
        }).Add(_bot.watcher, ctx);

    [SlashCommand("cuddle", "Cuddle with another user.", dmPermission: false)]
    public async Task Cuddle(InteractionContext ctx, [Option("user", "The user")] DiscordUser user) 
        => new CuddleCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object> 
        {
            { "user", user }
        }, false, true).Add(_bot.watcher, ctx);



    [SlashCommand("kiss", "Kiss another user.", dmPermission: false)]
    public async Task Kiss(InteractionContext ctx, [Option("user", "The user")] DiscordUser user) 
        => new KissCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object> 
        {
            { "user", user } 
        }, false, true).Add(_bot.watcher, ctx);



    [SlashCommand("slap", "Slap another user.", dmPermission: false)]
    public async Task Slap(InteractionContext ctx, [Option("user", "The user")] DiscordUser user) 
        => new SlapCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object> 
        {
            { "user", user } 
        }, false, true).Add(_bot.watcher, ctx);



    [SlashCommand("kill", "Kill another user..?", dmPermission: false)]
    public async Task Kill(InteractionContext ctx, [Option("user", "The user")] DiscordUser user) 
        => new KillCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object> 
        {
            { "user", user } 
        }, false, true).Add(_bot.watcher, ctx);



    [SlashCommand("boop", "Give another user a boop!", dmPermission: false)]
    public async Task Boop(InteractionContext ctx, [Option("user", "The user")] DiscordUser user) 
        => new BoopCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object> 
        {
            { "user", user } 
        }, false, true).Add(_bot.watcher, ctx);



    [SlashCommand("highfive", "Give a high five!", dmPermission: false)]
    public async Task Highfive(InteractionContext ctx, [Option("user", "The user")] DiscordUser user) 
        => new HighFiveCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object> 
        {
            { "user", user }
        }, false, true).Add(_bot.watcher, ctx);



    [SlashCommand("hug", "Hug another user!", dmPermission: false)]
    public async Task Hug(InteractionContext ctx, [Option("user", "The user")] DiscordUser user) 
        => new HugCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object> 
        {
            { "user", user } 
        }, false, true).Add(_bot.watcher, ctx);



    [SlashCommand("pat", "Give someone some headpats!", dmPermission: false)]
    public async Task Pat(InteractionContext ctx, [Option("user", "The user")] DiscordUser user) 
        => new PatCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object> 
        {
            { "user", user }
        }, false, true).Add(_bot.watcher, ctx);
}
