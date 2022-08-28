namespace ProjectIchigo.PrefixCommands;

public class MusicPrefixCommands : BaseCommandModule
{
    public Bot _bot { private get; set; }


    [Group("music"),
    CommandModule("music"), Aliases("m"),
    Description("Allows to play music and change the current playback settings.")]
    public class MusicCommands : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot.users[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot.watcher, ctx);
        }

        [Command("join"), Aliases("connect"), Description("The bot will join your channel if it's not already being used in this server.")]
        public async Task Join(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.JoinCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "announce", true }
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("disconnect"), Aliases("dc", "leave"), Description("Starts a voting to disconnect the bot.")]
        public async Task Disconnect(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.DisconnectCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("forcedisconnect"), Aliases("fdc", "forceleave", "fleave", "stop"), Description("Forces the bot to disconnect. `DJ` role or Administrator permissions required.")]
        public async Task ForceDisconnect(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceDisconnectCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("play"), Description("Searches for a video and adds it to the queue. If given a direct url, adds it to the queue.")]
        public async Task Play(CommandContext ctx, [Description("Search Query/Url")][RemainingText]string search)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.PlayCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "search", search }
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("pause"), Aliases("resume"), Description("Pause or unpause the current song.")]
        public async Task Pause(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.PauseCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("queue"), Description("Displays the current queue.")]
        public async Task Queue(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.QueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("removequeue"), Aliases("rq"), Description("Removes a song from the queue.")]
        public async Task RemoveQueue(CommandContext ctx, [Description("Index/Video Title")][RemainingText]string selection)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.RemoveQueueCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "selection", selection }
                });
            }).Add(_bot.watcher, ctx);
        }

        [Command("skip"), Description("Starts a voting to skip the current song.")]
        public async Task Skip(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.SkipCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("forceskip"), Aliases("fs", "fskip"), Description("Forces skipping of the current song. `DJ` role or Administrator permissions required.")]
        public async Task ForceSkip(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceSkipCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("clearqueue"), Aliases("cq"), Description("Starts a voting to clear the current queue.")]
        public async Task ClearQueue(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ClearQueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
        
        [Command("forceclearqueue"), Aliases("fcq"), Description("Forces clearing the current queue. `DJ` role or Administrator permissions required.")]
        public async Task ForceClearQueue(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ForceClearQueueCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("shuffle"), Description("Toggles shuffling of the current queue.")]
        public async Task Shuffle(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.ShuffleCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("repeat"), Description("Toggles repeating of the current queue.")]
        public async Task Repeat(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Music.RepeatCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }
    }

    [Group("playlists"), Aliases("playlist", "pl"),
    CommandModule("music"), 
    Description("Allows you to manage your personal playlists.")]
    public class Playlists : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        //[GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        //public async Task Help(CommandContext ctx)
        //{
        //    Task.Run(async () =>
        //    {
        //        if (await _bot._users[ ctx.Member.Id ].Cooldown.WaitForLight(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
        //            return;

        //        if (ctx.Command.Parent is not null)
        //            await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
        //        else
        //            await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
        //    }).Add(_bot._watcher, ctx);
        //}

        [GroupCommand, Command("manage"), Description("Allows you to use and manage your playlists.")]
        public async Task Manage(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                await new Commands.Playlists.ManageCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot.watcher, ctx);
        }

        [Command("load-share"), Description("Loads a playlist share.")]
        public async Task LoadShare(CommandContext ctx, [Description("User")]ulong userid, [Description("Id")]string id)
        {
            Task.Run(async () =>
            {
                await new Commands.Playlists.LoadShareCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "userid", userid },
                    { "id", id },
                });
            }).Add(_bot.watcher, ctx);
        }
    }
}
