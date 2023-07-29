// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;

[ModulePriority(998)]
public sealed class SocialAppCommands : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommand("afk", "Allows you to set yourself AFK. Users who ping you will be notified that you're unavailable.", dmPermission: false)]
    public async Task UserInfo(InteractionContext ctx, [Option("reason", "The reason")] string reason = "-")
        => _ = new AfkCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "reason", reason }
        });

    [SlashCommand("cuddle", "Cuddle with another user.", dmPermission: false)]
    public async Task Cuddle(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        => _ = new CuddleCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }, false, true);



    [SlashCommand("kiss", "Kiss another user.", dmPermission: false)]
    public async Task Kiss(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        => _ = new KissCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }, false, true);



    [SlashCommand("slap", "Slap another user.", dmPermission: false)]
    public async Task Slap(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        => _ = new SlapCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }, false, true);



    [SlashCommand("kill", "Kill another user..?", dmPermission: false)]
    public async Task Kill(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        => _ = new KillCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }, false, true);



    [SlashCommand("boop", "Give another user a boop!", dmPermission: false)]
    public async Task Boop(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        => _ = new BoopCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }, false, true);



    [SlashCommand("highfive", "Give a high five!", dmPermission: false)]
    public async Task Highfive(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        => _ = new HighFiveCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }, false, true);



    [SlashCommand("hug", "Hug another user!", dmPermission: false)]
    public async Task Hug(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        => _ = new HugCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }, false, true);



    [SlashCommand("pat", "Give someone some headpats!", dmPermission: false)]
    public async Task Pat(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        => _ = new PatCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        }, false, true);
}
