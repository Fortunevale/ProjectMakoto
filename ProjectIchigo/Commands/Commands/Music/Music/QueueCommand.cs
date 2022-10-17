namespace ProjectIchigo.Commands.Music;

internal class QueueCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`The bot is not in a voice channel.`",
                }.AsError(ctx));
                return;
            }

            if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`You aren't in the same channel as the bot.`",
                }.AsError(ctx));
                return;
            }

            int LastInt = 0;
            int GetInt()
            {
                LastInt++;
                return LastInt;
            }

            int CurrentPage = 0;

            async Task UpdateMessage()
            {
                DiscordButtonComponent Refresh = new(ButtonStyle.Primary, "Refresh", "Refresh", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔁")));

                DiscordButtonComponent NextPage = new(ButtonStyle.Primary, "NextPage", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
                DiscordButtonComponent PreviousPage = new(ButtonStyle.Primary, "PreviousPage", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));

                LastInt = CurrentPage * 10;

                var TotalTimespan = TimeSpan.Zero;

                for (int i = 0; i < ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count; i++)
                {
                    TotalTimespan = TotalTimespan.Add(ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue[i].Length);
                }

                var Description = $"**`There's currently {ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count} song(s) queued. This queue last for {TotalTimespan.GetHumanReadable()}.`**\n\n";
                Description += $"{string.Join("\n", ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Skip(CurrentPage * 10).Take(10).Select(x => $"**{GetInt()}**. `{x.Length.GetShortHumanReadable(TimeFormat.HOURS)}` [`{x.VideoTitle}`]({x.Url}) requested by {x.user.Mention}"))}\n\n";

                if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count > 0)
                    Description += $"`Page {CurrentPage + 1}/{Math.Ceiling(ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count / 10.0)}`\n\n";

                Description += $"`Currently playing:` [`{(conn.CurrentState.CurrentTrack is not null ? conn.CurrentState.CurrentTrack.Title : "No song is playing")}`]({(conn.CurrentState.CurrentTrack is not null ? conn.CurrentState.CurrentTrack.Uri.ToString() : "")})\n";
                Description += $"{(ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Repeat ? "🔁" : ctx.Bot.status.LoadedConfig.Emojis.DisabledRepeat)}";
                Description += $"{(ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Shuffle ? "🔀" : ctx.Bot.status.LoadedConfig.Emojis.DisabledShuffle)}";
                Description += $" `|` {(ctx.Bot.guilds[ctx.Guild.Id].MusicModule.IsPaused ? ctx.Bot.status.LoadedConfig.Emojis.Paused : $"{(conn.CurrentState.CurrentTrack is not null ? "▶" : ctx.Bot.status.LoadedConfig.Emojis.DisabledPlay)} ")}";

                if (conn.CurrentState.CurrentTrack is not null)
                {
                    Description += $"`[{((long)Math.Round(conn.CurrentState.PlaybackPosition.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.MINUTES)}/{((long)Math.Round(conn.CurrentState.CurrentTrack.Length.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.MINUTES)}]` ";
                    Description += $"`{GenerateASCIIProgressbar(Math.Round(conn.CurrentState.PlaybackPosition.TotalSeconds, 0), Math.Round(conn.CurrentState.CurrentTrack.Length.TotalSeconds, 0))}`";
                }

                if (CurrentPage <= 0)
                    PreviousPage = PreviousPage.Disable();

                if ((CurrentPage * 10) + 10 >= ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count)
                    NextPage = NextPage.Disable();

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = Description
                }.AsInfo(ctx)).AddComponents(Refresh).AddComponents(new List<DiscordComponent> { PreviousPage, NextPage }));

                return;
            }

            await UpdateMessage();

            _ = Task.Delay(120000).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                    ModifyToTimedOut();
                }
            });

            ctx.Client.ComponentInteractionCreated += RunInteraction;
            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        switch (e.Interaction.Data.CustomId)
                        {
                            case "Refresh":
                            {
                                await UpdateMessage();
                                break;
                            }
                            case "NextPage":
                            {
                                CurrentPage++;
                                await UpdateMessage();
                                break;
                            }
                            case "PreviousPage":
                            {
                                CurrentPage--;
                                await UpdateMessage();
                                break;
                            }
                        }
                    }
                }).Add(ctx.Bot.watcher, ctx);
            }
        });
    }
}