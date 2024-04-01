// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal sealed class QueueCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var session = lava.ConnectedSessions.Values.First(x => x.IsConnected);
            var conn = session.GetGuildPlayer(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.NotSameChannel, true),
                }.AsError(ctx));
                return;
            }

            var LastInt = 0;
            int GetInt()
            {
                LastInt++;
                return LastInt;
            }

            var CurrentPage = 0;

            async Task UpdateMessage()
            {
                DiscordButtonComponent Refresh = new(ButtonStyle.Primary, "Refresh", this.GetString(this.t.Common.Refresh), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔁")));

                DiscordButtonComponent NextPage = new(ButtonStyle.Primary, "NextPage", this.GetString(this.t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
                DiscordButtonComponent PreviousPage = new(ButtonStyle.Primary, "PreviousPage", this.GetString(this.t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));

                DiscordButtonComponent PlayPause = new(ButtonStyle.Secondary, "Playback", this.GetString(this.t.Commands.Music.Queue.Play), conn.Player.Track is null, (ctx.DbGuild.MusicModule.IsPaused ? DiscordEmoji.FromGuildEmote(ctx.Client, ctx.Bot.status.LoadedConfig.Emojis.Paused) : (conn.Player.Track is not null ? "▶".UnicodeToEmoji() : DiscordEmoji.FromGuildEmote(ctx.Client, ctx.Bot.status.LoadedConfig.Emojis.DisabledPlay))).ToComponent());
                DiscordButtonComponent Repeat = new(ButtonStyle.Secondary, "Repeat", this.GetString(this.t.Commands.Music.Queue.Repeat), conn.Player.Track is null, (ctx.DbGuild.MusicModule.Repeat ? "🔁".UnicodeToEmoji() : DiscordEmoji.FromGuildEmote(ctx.Client, ctx.Bot.status.LoadedConfig.Emojis.DisabledRepeat)).ToComponent());
                DiscordButtonComponent Shuffle = new(ButtonStyle.Secondary, "Shuffle", this.GetString(this.t.Commands.Music.Queue.Shuffle), conn.Player.Track is null, (ctx.DbGuild.MusicModule.Shuffle ? "🔀".UnicodeToEmoji() : DiscordEmoji.FromGuildEmote(ctx.Client, ctx.Bot.status.LoadedConfig.Emojis.DisabledShuffle)).ToComponent());

                LastInt = CurrentPage * 10;

                var TotalTimespan = TimeSpan.Zero;

                for (var i = 0; i < ctx.DbGuild.MusicModule.SongQueue.Length; i++)
                {
                    TotalTimespan = TotalTimespan.Add(ctx.DbGuild.MusicModule.SongQueue[i].Length);
                }

                var Description = $"{this.GetString(this.t.Commands.Music.Queue.QueueCount, true, new TVar("Count", ctx.DbGuild.MusicModule.SongQueue.Length), new TVar("Timespan", TotalTimespan.GetHumanReadable())).Bold()}\n\n";
                Description += $"{string.Join("\n", ctx.DbGuild.MusicModule.SongQueue.Skip(CurrentPage * 10).Take(10).Select(x => $"**{GetInt()}**. `{x.Length.GetShortHumanReadable(TimeFormat.Hours)}` {this.GetString(this.t.Commands.Music.Queue.Track, new TVar("Video", $"[`{x.VideoTitle}`]({x.Url})"), new TVar("Requester", $"<@{x.UserId}>"))}"))}\n\n";

                if (ctx.DbGuild.MusicModule.SongQueue.Length > 0)
                    Description += $"`{this.GetString(this.t.Common.Page)} {CurrentPage + 1}/{Math.Ceiling(ctx.DbGuild.MusicModule.SongQueue.Length / 10.0)}`\n\n";

                Description += $"`{this.GetString(this.t.Commands.Music.Queue.CurrentlyPlaying)}:` [`{(conn.Player.Track is not null ? conn.Player.Track.Info.Title : this.GetString(this.t.Commands.Music.Queue.NoSong))}`]({(conn.Player.Track is not null ? conn.Player.Track.Info.Uri.ToString() : "")})\n";
                Description += $"{(ctx.DbGuild.MusicModule.Repeat ? "🔁".UnicodeToEmoji() : DiscordEmoji.FromGuildEmote(ctx.Client, ctx.Bot.status.LoadedConfig.Emojis.DisabledRepeat))}";
                Description += $"{(ctx.DbGuild.MusicModule.Shuffle ? "🔀".UnicodeToEmoji() : DiscordEmoji.FromGuildEmote(ctx.Client, ctx.Bot.status.LoadedConfig.Emojis.DisabledShuffle))}";
                Description += $" `|` {(ctx.DbGuild.MusicModule.IsPaused ? DiscordEmoji.FromGuildEmote(ctx.Client, ctx.Bot.status.LoadedConfig.Emojis.Paused) : $"{(conn.Player.Track is not null ? "▶".UnicodeToEmoji() : DiscordEmoji.FromGuildEmote(ctx.Client, ctx.Bot.status.LoadedConfig.Emojis.DisabledPlay))} ")}";

                if (conn.CurrentTrack is not null)
                {
                    Description += $"`[{((long)Math.Round(conn.Player.PlayerState.Position.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.Minutes)}/{((long)Math.Round(conn.Player.Track.Info.Length.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.Minutes)}]` ";
                    Description += $"`{StringTools.GenerateASCIIProgressbar(Math.Round(conn.Player.PlayerState.Position.TotalSeconds, 0), Math.Round(conn.Player.Track.Info.Length.TotalSeconds, 0))}`";
                }

                if (CurrentPage <= 0)
                    PreviousPage = PreviousPage.Disable();

                if ((CurrentPage * 10) + 10 >= ctx.DbGuild.MusicModule.SongQueue.Length)
                    NextPage = NextPage.Disable();

                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = Description
                }.AsInfo(ctx))
                .AddComponents(PreviousPage, NextPage, Refresh)
                .AddComponents(PlayPause, Repeat, Shuffle));

                return;
            }

            await UpdateMessage();

            var sw = Stopwatch.StartNew();
            _ = Task.Run(async () =>
            {
                while (sw.ElapsedMilliseconds < 120000)
                {
                    await UpdateMessage();
                    Thread.Sleep(10000);
                }
            });

            _ = Task.Run(() =>
            {
                while (sw.ElapsedMilliseconds < 120000)
                    Thread.Sleep(1000);

                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                this.ModifyToTimedOut();
            });

            ctx.Client.ComponentInteractionCreated += RunInteraction;
            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        sw.Restart();

                        switch (e.GetCustomId())
                        {
                            case "Playback":
                            {
                                await new Music.PauseCommand().ExecuteCommand(e, s, "pause", ctx.Bot).Add(ctx.Bot);

                                await UpdateMessage();
                                break;
                            }
                            case "Repeat":
                            {
                                await new Music.RepeatCommand().ExecuteCommand(e, s, "repeat", ctx.Bot).Add(ctx.Bot);

                                await UpdateMessage();
                                break;
                            }
                            case "Shuffle":
                            {
                                await new Music.ShuffleCommand().ExecuteCommand(e, s, "shuffle", ctx.Bot).Add(ctx.Bot);

                                await UpdateMessage();
                                break;
                            }
                            case "Refresh":
                            {
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                await UpdateMessage();
                                break;
                            }
                            case "NextPage":
                            {
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                CurrentPage++;
                                await UpdateMessage();
                                break;
                            }
                            case "PreviousPage":
                            {
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                CurrentPage--;
                                await UpdateMessage();
                                break;
                            }
                        }
                    }
                }).Add(ctx.Bot, ctx);
            }
        });
    }
}