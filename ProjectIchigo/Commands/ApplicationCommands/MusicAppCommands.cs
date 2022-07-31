﻿namespace ProjectIchigo.ApplicationCommands;
internal class MusicAppCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("music", "Allows to play music and change the current playback settings.", dmPermission: false)]
    public class MusicCommands : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("join", "The bot will join your channel if it's not already being used in this server.", dmPermission: false)]
        public async Task Join(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.JoinCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "announce", true }
                });
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("disconnect", "Starts a voting to disconnect the bot.", dmPermission: false)]
        public async Task Disconnect(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.DisconnectCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("forcedisconnect", "Forces the bot to disconnect. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceDisconnect(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceDisconnectCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
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
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("pause", "Pause or unpause the current song.", dmPermission: false)]
        public async Task Pause(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.PauseCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("queue", "Displays the current queue.", dmPermission: false)]
        public async Task Queue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.QueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("removequeue", "Remove a song from the queue.", dmPermission: false)]
        public async Task RemoveQueue(InteractionContext ctx, [Option("video", "The Index or Video Title")] string selection)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.RemoveQueueCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "selection", selection }
                });
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("skip", "Starts a voting to skip the current song.", dmPermission: false)]
        public async Task Skip(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.SkipCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("forceskip", "Forces skipping of the current song. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceSkip(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceSkipCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("clearqueue", "Starts a voting to clear the current queue.", dmPermission: false)]
        public async Task ClearQueue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ClearQueueCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("forceclearqueue", "Forces clearing the current queue. `DJ` role or Administrator permissions required.", dmPermission: false)]
        public async Task ForceClearQueue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceClearQueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("shuffle", "Toggles shuffling of the current queue.", dmPermission: false)]
        public async Task Shuffle(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ShuffleCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("repeat", "Toggles repeating the current queue.", dmPermission: false)]
        public async Task Repeat(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.RepeatCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
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
            }).Add(_bot._watcher, ctx);
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
            }).Add(_bot._watcher, ctx);
        }
    }
}
