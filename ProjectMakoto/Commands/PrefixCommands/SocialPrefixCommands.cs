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
    
    Description("Allows you to set yourself AFK. Users who ping you will be notified that you're unavailable.")]
    public async Task Afk(CommandContext ctx, [RemainingText][Description("Text (<128 characters)")] string reason = "-")
        => _ = new AfkCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "reason", reason }
        });

    [Command("cuddle"), PreventCommandDeletion,
    
    Description("Cuddle with another user.")]
    public async Task Cuddle(CommandContext ctx, DiscordUser user)
        => _ = new CuddleCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        });



    [Command("kiss"), PreventCommandDeletion,
    
    Description("Kiss another user.")]
    public async Task Kiss(CommandContext ctx, DiscordUser user)
        => _ = new KissCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        });



    [Command("slap"), Aliases("bonk", "punch"), PreventCommandDeletion,
    
    Description("Slap another user.")]
    public async Task Slap(CommandContext ctx, DiscordUser user)
        => _ = new SlapCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        });



    [Command("kill"), Aliases("waste"), PreventCommandDeletion,
    
    Description("Kill another user..?")]
    public async Task Kill(CommandContext ctx, DiscordUser user)
        => _ = new KillCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        });



    [Command("boop"), PreventCommandDeletion,
    
    Description("Give another user a boop!")]
    public async Task Boop(CommandContext ctx, DiscordUser user)
        => _ = new BoopCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        });



    [Command("highfive"), PreventCommandDeletion,
    
    Description("Give a high five!")]
    public async Task Highfive(CommandContext ctx, DiscordUser user)
        => _ = new HighFiveCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        });



    [Command("hug"), PreventCommandDeletion,
    
    Description("Hug another user!")]
    public async Task Hug(CommandContext ctx, DiscordUser user)
        => _ = new HugCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        });



    [Command("pat"), Aliases("pet", "headpat", "headpet"), PreventCommandDeletion,
    
    Description("Give someone some headpats!")]
    public async Task Pat(CommandContext ctx, DiscordUser user)
        => _ = new PatCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
        {
            { "user", user }
        });
}
