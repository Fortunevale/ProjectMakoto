// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;

[ModulePriority(997)]
public sealed class MusicAppCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("music", "Allows to play music and change the current playback settings.", dmPermission: false)]
    public sealed class MusicCommands : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        public sealed class SongQueueAutocompleteProvider : IAutocompleteProvider
        {
            public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
            {
                try
                {
                    if ((ctx.Member?.VoiceState?.Channel?.Id ?? 0) != ((await ctx.Client.CurrentUser.ConvertToMember(ctx.Guild)).VoiceState?.Channel?.Id ?? 1))
                        return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();

                    var Queue = ((Bot)ctx.Services.GetService(typeof(Bot))).Guilds[ctx.Guild.Id].MusicModule.SongQueue
                        .Where(x => x.VideoTitle.StartsWith(ctx.FocusedOption.Value.ToString(), StringComparison.InvariantCultureIgnoreCase)).Take(25);

                    var options = Queue.Select(x => new DiscordApplicationCommandAutocompleteChoice(x.VideoTitle, x.VideoTitle)).ToList();
                    return options.AsEnumerable();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to provide autocomplete for song queue");
                    return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
                }
            }
        }

        [SlashCommand("join", "The bot will join your channel if it's not already being used in this server.", dmPermission: false)]
        public async Task Join(InteractionContext ctx)
            => _ = new Commands.Music.JoinCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "announce", true }
            });

        [SlashCommand("disconnect", "Starts a voting to disconnect the bot.", dmPermission: false)]
        public async Task Disconnect(InteractionContext ctx)
            => _ = new Commands.Music.DisconnectCommand().ExecuteCommand(ctx, this._bot, null, false);

        [SlashCommand("forcedisconnect", "Forces the bot to disconnect. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceDisconnect(InteractionContext ctx)
            => _ = new Commands.Music.ForceDisconnectCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("play", "Searches for a video and adds it to the queue. If given a direct url, adds it to the queue.", dmPermission: false)]
        public async Task Play(InteractionContext ctx, [Option("search", "Search Query/Url")] string search)
            => _ = new Commands.Music.PlayCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "search", search }
            });

        [SlashCommand("pause", "Pause or unpause the current song.", dmPermission: false)]
        public async Task Pause(InteractionContext ctx)
            => _ = new Commands.Music.PauseCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("queue", "Displays the current queue.", dmPermission: false)]
        public async Task Queue(InteractionContext ctx)
            => _ = new Commands.Music.QueueCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("removequeue", "Remove a song from the queue.", dmPermission: false)]
        public async Task RemoveQueue(InteractionContext ctx, [Autocomplete(typeof(SongQueueAutocompleteProvider))][Option("video", "The Index or Video Title", true)] string selection)
            => _ = new Commands.Music.RemoveQueueCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "selection", selection }
            });

        [SlashCommand("skip", "Starts a voting to skip the current song.", dmPermission: false)]
        public async Task Skip(InteractionContext ctx)
            => _ = new Commands.Music.SkipCommand().ExecuteCommand(ctx, this._bot, null, false);

        [SlashCommand("forceskip", "Forces skipping of the current song. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceSkip(InteractionContext ctx)
            => _ = new Commands.Music.ForceSkipCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("clearqueue", "Starts a voting to clear the current queue.", dmPermission: false)]
        public async Task ClearQueue(InteractionContext ctx)
            => _ = new Commands.Music.ClearQueueCommand().ExecuteCommand(ctx, this._bot, null, false);

        [SlashCommand("forceclearqueue", "Forces clearing the current queue. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceClearQueue(InteractionContext ctx)
            => _ = new Commands.Music.ForceClearQueueCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("shuffle", "Toggles shuffling of the current queue.", dmPermission: false)]
        public async Task Shuffle(InteractionContext ctx)
            => _ = new Commands.Music.ShuffleCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("repeat", "Toggles repeating the current queue.", dmPermission: false)]
        public async Task Repeat(InteractionContext ctx)
            => _ = new Commands.Music.RepeatCommand().ExecuteCommand(ctx, this._bot);
    }

    [SlashCommandGroup("playlists", "Allows you to manage your personal playlists.", dmPermission: false)]
    public sealed class Playlists : ApplicationCommandsModule
    {
        public sealed class PlaylistsAutoCompleteProvider : IAutocompleteProvider
        {
            public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
            {
                try
                {
                    var bot = ((Bot)ctx.Services.GetService(typeof(Bot)));
                    var Queue = bot.Users[ctx.User.Id].UserPlaylists
                        .Where(x => x.PlaylistName.Contains(ctx.FocusedOption.Value.ToString(), StringComparison.InvariantCultureIgnoreCase)).Take(25);

                    var options = Queue.Select(x => new DiscordApplicationCommandAutocompleteChoice($"{x.PlaylistName} ({x.List.Length} {bot.LoadedTranslations.Commands.Music.Playlists.Tracks.Get(bot.Users[ctx.User.Id]).Build()})", x.PlaylistId)).ToList();
                    return options.AsEnumerable();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to provide autocomplete for playlists");
                    return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
                }
            }
        }

        public Bot _bot { private get; set; }

        [SlashCommand("manage", "Allows you to use and manage your playlists.", dmPermission: false)]
        public async Task Manage(InteractionContext ctx)
            => _ = new Commands.Playlists.ManageCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("add-to-queue", "Adds a playlist to the current song queue.", dmPermission: false)]
        public async Task AddToQueue(InteractionContext ctx, [Autocomplete(typeof(PlaylistsAutoCompleteProvider))][Option("playlist", "The Playlist Id", true)] string id)
            => _ = new Commands.Playlists.AddToQueueCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "id", id },
            });

        [SlashCommand("share", "Share one of your playlists.", dmPermission: false)]
        public async Task Share(InteractionContext ctx, [Autocomplete(typeof(PlaylistsAutoCompleteProvider))][Option("playlist", "The Playlist Id", true)] string id)
            => _ = new Commands.Playlists.ShareCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "id", id },
            });

        [SlashCommand("export", "Export one of your playlists.", dmPermission: false)]
        public async Task Export(InteractionContext ctx, [Autocomplete(typeof(PlaylistsAutoCompleteProvider))][Option("playlist", "The Playlist Id", true)] string id)
            => _ = new Commands.Playlists.ExportCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "id", id },
            });

        [SlashCommand("modify", "Modify one of your playlists.", dmPermission: false)]
        public async Task Modify(InteractionContext ctx, [Autocomplete(typeof(PlaylistsAutoCompleteProvider))][Option("playlist", "The Playlist Id", true)] string id)
            => _ = new Commands.Playlists.ModifyCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "id", id },
            });

        [SlashCommand("delete", "Delete one of your playlists.", dmPermission: false)]
        public async Task Delete(InteractionContext ctx, [Autocomplete(typeof(PlaylistsAutoCompleteProvider))][Option("playlist", "The Playlist Id", true)] string id)
            => _ = new Commands.Playlists.DeleteCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "id", id },
            });

        [SlashCommand("create-new", "Create a new playlist from scratch.", dmPermission: false)]
        public async Task CreateNew(InteractionContext ctx)
            => _ = new Commands.Playlists.NewPlaylistCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("save-queue", "Save the current queue as playlist.", dmPermission: false)]
        public async Task SaveQueue(InteractionContext ctx)
            => _ = new Commands.Playlists.SaveCurrentCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("import", "Import a playlists from another platform or from a previously exported playlist.", dmPermission: false)]
        public async Task Import(InteractionContext ctx)
            => _ = new Commands.Playlists.ImportCommand().ExecuteCommand(ctx, this._bot);

        [SlashCommand("load-share", "Loads a playlist share.", dmPermission: false)]
        public async Task LoadShare(InteractionContext ctx, [Option("user", "The user")] DiscordUser userid, [Option("Id", "The Id")] string id)
            => _ = new Commands.Playlists.LoadShareCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "userid", userid.Id },
                { "id", id },
            });
    }
}
