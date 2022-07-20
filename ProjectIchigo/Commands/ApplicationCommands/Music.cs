namespace ProjectIchigo.ApplicationCommands;
internal class Music : ApplicationCommandsModule
{
    [SlashCommandGroup("music", "Allows to play music and change the current playback settings")]
    public class MusicCommands : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("join", "Project Ichigo will join your channel if it's not already being used in the server")]
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

        [SlashCommand("disconnect", "Starts a voting to disconnect the bot")]
        public async Task Disconnect(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.DisconnectCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("forcedisconnect", "Forces the bot to disconnect from the current channel")]
        public async Task ForceDisconnect(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceDisconnectCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("play", "Searches for a video and adds it to the queue. If given a direct url, adds it to the queue.")]
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

        [SlashCommand("pause", "Pause or unpause the current song")]
        public async Task Pause(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.PauseCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("queue", "Displays the current queue")]
        public async Task Queue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.QueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("removequeue", "Remove a song from the queue")]
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

        [SlashCommand("skip", "Starts a voting to skip the current song")]
        public async Task Skip(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.SkipCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("forceskip", "Forces skipping of the current song. You need to be an Administrator or have a role called `DJ`.")]
        public async Task ForceSkip(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceSkipCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("clearqueue", "Starts a voting to clear the current queue")]
        public async Task ClearQueue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ClearQueueCommand().ExecuteCommand(ctx, _bot, null, false);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("forceclearqueue", "Forces clearing the current queue. You need to be an Administrator or have a role called `DJ`.")]
        public async Task ForceClearQueue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceClearQueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("shuffle", "Toggles shuffling of the current queue")]
        public async Task Shuffle(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ShuffleCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("repeat", "Toggles repeating the current queue")]
        public async Task Repeat(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.RepeatCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }
    }

    [SlashCommandGroup("playlists", "Allows managing your personal playlists")]
    public class Playlists : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("manage", "Allows to review and manage your playlists")]
        public async Task Manage(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Playlists.ManageCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("load-share", "Loads a playlist share")]
        public async Task LoadShare(InteractionContext ctx, [Option("user", "The user")] ulong userid, [Option("Id", "The id")] string id)
        {
            Task.Run(async () =>
            {
                await new Commands.Playlists.LoadShareCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "userid", userid },
                    { "id", id },
                });
            }).Add(_bot._watcher, ctx);
        }
    }
}
