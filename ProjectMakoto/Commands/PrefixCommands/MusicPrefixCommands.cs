// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.PrefixCommands;

public sealed class MusicPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }


    [Group("music"),
     Aliases("m"),
    Description("Allows to play music and change the current playback settings.")]
    public sealed class MusicCommands : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => _ = PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "music");

        [Command("join"), Aliases("connect"), Description("The bot will join your channel if it's not already being used in this server.")]
        public async Task Join(CommandContext ctx)
            => _ = new Commands.Music.JoinCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "announce", true }
            });

        [Command("disconnect"), Aliases("dc", "leave"), Description("Starts a voting to disconnect the bot.")]
        public async Task Disconnect(CommandContext ctx)
            => _ = new Commands.Music.DisconnectCommand().ExecuteCommand(ctx, this._bot);

        [Command("forcedisconnect"), Aliases("fdc", "forceleave", "fleave", "stop"), Description("Forces the bot to disconnect. `DJ` role or Administrator permissions required.")]
        public async Task ForceDisconnect(CommandContext ctx)
            => _ = new Commands.Music.ForceDisconnectCommand().ExecuteCommand(ctx, this._bot);

        [Command("play"), Description("Searches for a video and adds it to the queue. If given a direct url, adds it to the queue.")]
        public async Task Play(CommandContext ctx, [Description("Search Query/Url")][RemainingText] string search)
            => _ = new Commands.Music.PlayCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "search", search }
            });

        [Command("pause"), Aliases("resume"), Description("Pause or unpause the current song.")]
        public async Task Pause(CommandContext ctx)
            => _ = new Commands.Music.PauseCommand().ExecuteCommand(ctx, this._bot);

        [Command("queue"), Description("Displays the current queue.")]
        public async Task Queue(CommandContext ctx)
            => _ = new Commands.Music.QueueCommand().ExecuteCommand(ctx, this._bot);

        [Command("removequeue"), Aliases("rq"), Description("Removes a song from the queue.")]
        public async Task RemoveQueue(CommandContext ctx, [Description("Index/Video Title")][RemainingText] string selection)
            => _ = _ = new Commands.Music.RemoveQueueCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "selection", selection }
            });

        [Command("skip"), Description("Starts a voting to skip the current song.")]
        public async Task Skip(CommandContext ctx)
            => _ = new Commands.Music.SkipCommand().ExecuteCommand(ctx, this._bot);

        [Command("forceskip"), Aliases("fs", "fskip"), Description("Forces skipping of the current song. `DJ` role or Administrator permissions required.")]
        public async Task ForceSkip(CommandContext ctx)
            => _ = new Commands.Music.ForceSkipCommand().ExecuteCommand(ctx, this._bot);

        [Command("clearqueue"), Aliases("cq"), Description("Starts a voting to clear the current queue.")]
        public async Task ClearQueue(CommandContext ctx)
            => _ = new Commands.Music.ClearQueueCommand().ExecuteCommand(ctx, this._bot);

        [Command("forceclearqueue"), Aliases("fcq"), Description("Forces clearing the current queue. `DJ` role or Administrator permissions required.")]
        public async Task ForceClearQueue(CommandContext ctx)
            => _ = new Commands.Music.ForceClearQueueCommand().ExecuteCommand(ctx, this._bot);

        [Command("shuffle"), Description("Toggles shuffling of the current queue.")]
        public async Task Shuffle(CommandContext ctx)
            => _ = new Commands.Music.ShuffleCommand().ExecuteCommand(ctx, this._bot);

        [Command("repeat"), Description("Toggles repeating of the current queue.")]
        public async Task Repeat(CommandContext ctx)
            => _ = new Commands.Music.RepeatCommand().ExecuteCommand(ctx, this._bot);
    }

    [Group("playlists"), Aliases("playlist", "pl"),
    
    Description("Allows you to manage your personal playlists.")]
    public sealed class Playlists : BaseCommandModule
    {
        public Bot _bot { private get; set; }


        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => _ = PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "playlists");

        [Command("manage"), Description("Allows you to use and manage your playlists.")]
        public async Task Manage(CommandContext ctx)
            => _ = new Commands.Playlists.ManageCommand().ExecuteCommand(ctx, this._bot);

        [Command("create-new"), Description("Create a new playlist from scratch.")]
        public async Task CreateNew(CommandContext ctx)
            => _ = new Commands.Playlists.NewPlaylistCommand().ExecuteCommand(ctx, this._bot);

        [Command("save-queue"), Description("Save the current queue as playlist.")]
        public async Task SaveQueue(CommandContext ctx)
            => _ = new Commands.Playlists.SaveCurrentCommand().ExecuteCommand(ctx, this._bot);

        [Command("import"), Description("Import a playlists from another platform or from a previously exported playlist.")]
        public async Task Import(CommandContext ctx)
            => _ = new Commands.Playlists.ImportCommand().ExecuteCommand(ctx, this._bot);

        [Command("load-share"), Description("Loads a playlist share.")]
        public async Task LoadShare(CommandContext ctx, [Description("User")] ulong userid, [Description("Id")] string id)
            => _ = new Commands.Playlists.LoadShareCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "userid", userid },
                { "id", id },
            });
    }
}
