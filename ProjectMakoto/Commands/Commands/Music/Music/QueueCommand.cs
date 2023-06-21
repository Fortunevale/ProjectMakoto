// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal sealed class QueueCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.NotSameChannel, true),
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
                DiscordButtonComponent Refresh = new(ButtonStyle.Primary, "Refresh", GetString(this.t.Common.Refresh), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔁")));

                DiscordButtonComponent NextPage = new(ButtonStyle.Primary, "NextPage", GetString(this.t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
                DiscordButtonComponent PreviousPage = new(ButtonStyle.Primary, "PreviousPage", GetString(this.t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));

                LastInt = CurrentPage * 10;

                var TotalTimespan = TimeSpan.Zero;

                for (int i = 0; i < ctx.DbGuild.MusicModule.SongQueue.Count; i++)
                {
                    TotalTimespan = TotalTimespan.Add(ctx.DbGuild.MusicModule.SongQueue[i].Length);
                }

                var Description = $"{GetString(this.t.Commands.Music.Queue.QueueCount, true, new TVar("Count", ctx.DbGuild.MusicModule.SongQueue.Count), new TVar("Timespan", TotalTimespan.GetHumanReadable())).Bold()}\n\n";
                Description += $"{string.Join("\n", ctx.DbGuild.MusicModule.SongQueue.Skip(CurrentPage * 10).Take(10).Select(x => $"**{GetInt()}**. `{x.Length.GetShortHumanReadable(TimeFormat.HOURS)}` {GetString(this.t.Commands.Music.Queue.Track, new TVar("Video", $"[`{x.VideoTitle}`]({x.Url})"), new TVar("Requester", x.user.Mention))}"))}\n\n";

                if (ctx.DbGuild.MusicModule.SongQueue.Count > 0)
                    Description += $"`{GetString(this.t.Common.Page)} {CurrentPage + 1}/{Math.Ceiling(ctx.DbGuild.MusicModule.SongQueue.Count / 10.0)}`\n\n";

                Description += $"`{GetString(this.t.Commands.Music.Queue.CurrentlyPlaying)}:` [`{(conn.CurrentState.CurrentTrack is not null ? conn.CurrentState.CurrentTrack.Title : GetString(this.t.Commands.Music.Queue.NoSong))}`]({(conn.CurrentState.CurrentTrack is not null ? conn.CurrentState.CurrentTrack.Uri.ToString() : "")})\n";
                Description += $"{(ctx.DbGuild.MusicModule.Repeat ? "🔁" : ctx.Bot.status.LoadedConfig.Emojis.DisabledRepeat)}";
                Description += $"{(ctx.DbGuild.MusicModule.Shuffle ? "🔀" : ctx.Bot.status.LoadedConfig.Emojis.DisabledShuffle)}";
                Description += $" `|` {(ctx.DbGuild.MusicModule.IsPaused ? ctx.Bot.status.LoadedConfig.Emojis.Paused : $"{(conn.CurrentState.CurrentTrack is not null ? "▶" : ctx.Bot.status.LoadedConfig.Emojis.DisabledPlay)} ")}";

                if (conn.CurrentState.CurrentTrack is not null)
                {
                    Description += $"`[{((long)Math.Round(conn.CurrentState.PlaybackPosition.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.MINUTES)}/{((long)Math.Round(conn.CurrentState.CurrentTrack.Length.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.MINUTES)}]` ";
                    Description += $"`{StringTools.GenerateASCIIProgressbar(Math.Round(conn.CurrentState.PlaybackPosition.TotalSeconds, 0), Math.Round(conn.CurrentState.CurrentTrack.Length.TotalSeconds, 0))}`";
                }

                if (CurrentPage <= 0)
                    PreviousPage = PreviousPage.Disable();

                if ((CurrentPage * 10) + 10 >= ctx.DbGuild.MusicModule.SongQueue.Count)
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

                        switch (e.GetCustomId())
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
                }).Add(ctx.Bot, ctx);
            }
        });
    }
}