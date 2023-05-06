// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;
public class MusicAppCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("music", "Allows to play music and change the current playback settings.", dmPermission: false)]
    public class MusicCommands : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        public class SongQueueAutocompleteProvider : IAutocompleteProvider
        {
            public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
            {
                try
                {
                    if ((ctx.Member?.VoiceState?.Channel?.Id ?? 0) != (ctx.Client.CurrentUser.ConvertToMember(ctx.Guild).Result.VoiceState?.Channel?.Id ?? 1))
                        return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();

                    IEnumerable<Lavalink.QueueInfo> Queue = ((Bot)ctx.Services.GetService(typeof(Bot))).guilds[ctx.Guild.Id].MusicModule.SongQueue
                        .Where(x => x.VideoTitle.StartsWith(ctx.FocusedOption.Value.ToString(), StringComparison.InvariantCultureIgnoreCase)).Take(25);

                    List<DiscordApplicationCommandAutocompleteChoice> options = Queue.Select(x => new DiscordApplicationCommandAutocompleteChoice(x.VideoTitle, x.VideoTitle)).ToList();
                    return options.AsEnumerable();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to provide autocomplete for song queue", ex);
                    return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
                }
            }
        }

        [SlashCommand("join", "The bot will join your channel if it's not already being used in this server.", dmPermission: false)]
        public async Task Join(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.JoinCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "announce", true }
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("disconnect", "Starts a voting to disconnect the bot.", dmPermission: false)]
        public async Task Disconnect(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.DisconnectCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("forcedisconnect", "Forces the bot to disconnect. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceDisconnect(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceDisconnectCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("play", "Searches for a video and adds it to the queue. If given a direct url, adds it to the queue.", dmPermission: false)]
        public async Task Play(InteractionContext ctx, [Option("search", "Search Query/Url")] string search)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.PlayCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "search", search }
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("pause", "Pause or unpause the current song.", dmPermission: false)]
        public async Task Pause(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.PauseCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("queue", "Displays the current queue.", dmPermission: false)]
        public async Task Queue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.QueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("removequeue", "Remove a song from the queue.", dmPermission: false)]
        public async Task RemoveQueue(InteractionContext ctx, [Autocomplete(typeof(SongQueueAutocompleteProvider))] [Option("video", "The Index or Video Title", true)] string selection)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.RemoveQueueCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "selection", selection }
                });
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("skip", "Starts a voting to skip the current song.", dmPermission: false)]
        public async Task Skip(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.SkipCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("forceskip", "Forces skipping of the current song. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceSkip(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceSkipCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("clearqueue", "Starts a voting to clear the current queue.", dmPermission: false)]
        public async Task ClearQueue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ClearQueueCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("forceclearqueue", "Forces clearing the current queue. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceClearQueue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceClearQueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("shuffle", "Toggles shuffling of the current queue.", dmPermission: false)]
        public async Task Shuffle(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ShuffleCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("repeat", "Toggles repeating the current queue.", dmPermission: false)]
        public async Task Repeat(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.RepeatCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }

    [SlashCommandGroup("playlists", "Allows you to manage your personal playlists.", dmPermission: false)]
    public class Playlists : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("manage", "Allows you to use and manage your playlists.", dmPermission: false)]
        public async Task Manage(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Playlists.ManageCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [SlashCommand("load-share", "Loads a playlist share.", dmPermission: false)]
        public async Task LoadShare(InteractionContext ctx, [Option("user", "The user")] DiscordUser userid, [Option("Id", "The Id")] string id)
        {
            Task.Run(async () =>
            {
                await new Commands.Playlists.LoadShareCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "userid", userid.Id },
                    { "id", id },
                });
            }).Add(_bot.watcher, ctx);
        }
    }
}
