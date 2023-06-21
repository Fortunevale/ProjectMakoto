// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class ModifyCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            string playlistId = (string)arguments["id"];

            if (!ctx.DbUser.UserPlaylists.Any(x => x.PlaylistId == playlistId))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.Playlists.NoPlaylist, true),
                }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title)));
                return;
            }

            UserPlaylist SelectedPlaylist = ctx.DbUser.UserPlaylists.First(x => x.PlaylistId == playlistId);

            var embed = new DiscordEmbedBuilder().AsInfo(ctx);

            int LastInt = 0;
            int GetInt()
            {
                LastInt++;
                return LastInt;
            }

            int CurrentPage = 0;

            async Task UpdateMessage()
            {
                LastInt = CurrentPage * 10;

                var CurrentTracks = SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10);

                DiscordButtonComponent NextPage = new(ButtonStyle.Primary, "NextPage", GetString(this.t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
                DiscordButtonComponent PreviousPage = new(ButtonStyle.Primary, "PreviousPage", GetString(this.t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));

                DiscordButtonComponent PlaylistName = new(ButtonStyle.Success, "ChangePlaylistName", GetString(this.t.Commands.Music.Playlists.Modify.ChangeName), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

                DiscordButtonComponent ChangePlaylistColor = new(ButtonStyle.Secondary, "ChangeColor", GetString(this.t.Commands.Music.Playlists.Modify.ChangeColor), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎨")));
                DiscordButtonComponent ChangePlaylistThumbnail = new(ButtonStyle.Secondary, "ChangeThumbnail", GetString(this.t.Commands.Music.Playlists.Modify.ChangeThumbnail), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖼")));

                DiscordButtonComponent AddSong = new(ButtonStyle.Success, "AddSong", GetString(this.t.Commands.Music.Playlists.Modify.AddTracks), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                DiscordButtonComponent RemoveSong = new(ButtonStyle.Danger, "DeleteSong", GetString(this.t.Commands.Music.Playlists.Modify.RemoveTracks), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
                DiscordButtonComponent RemoveDuplicates = new(ButtonStyle.Secondary, "RemoveDuplicates", GetString(this.t.Commands.Music.Playlists.Modify.RemoveDuplicates), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("♻")));

                var TotalTimespan = TimeSpan.Zero;

                for (int i = 0; i < SelectedPlaylist.List.Count; i++)
                {
                    TotalTimespan = TotalTimespan.Add(SelectedPlaylist.List[i].Length.Value);
                }

                var Description = $"**`{GetString(this.t.Commands.Music.Playlists.Modify.CurrentTrackCount, new TVar("Count", SelectedPlaylist.List.Count), new TVar("Timespan", TotalTimespan.GetHumanReadable()))}`**\n\n";
                Description += $"{string.Join("\n", CurrentTracks.Select(x => $"**{GetInt()}**. `{x.Length.Value.GetShortHumanReadable(TimeFormat.HOURS)}` {GetString(this.t.Commands.Music.Playlists.Modify.Track, new TVar("Track", $"**[`{x.Title}`]({x.Url})**"), new TVar("Timestamp", Formatter.Timestamp(x.AddedTime)))}"))}";

                if (SelectedPlaylist.List.Count > 0)
                    Description += $"\n\n`{GetString(this.t.Common.Page)} {CurrentPage + 1}/{Math.Ceiling(SelectedPlaylist.List.Count / 10.0)}`";

                if (CurrentPage <= 0)
                    PreviousPage = PreviousPage.Disable();

                if ((CurrentPage * 10) + 10 >= SelectedPlaylist.List.Count)
                    NextPage = NextPage.Disable();

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Color = (SelectedPlaylist.PlaylistColor is "#FFFFFF" or null or "" ? EmbedColors.Info : new DiscordColor(SelectedPlaylist.PlaylistColor.IsValidHexColor()));
                embed.Title = $"{GetString(this.t.Commands.Music.Playlists.Modify.ModifyingPlaylist)}: `{SelectedPlaylist.PlaylistName}`";
                embed.Description = Description;
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = (SelectedPlaylist.PlaylistThumbnail.IsNullOrWhiteSpace() ? "" : SelectedPlaylist.PlaylistThumbnail) };
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { PreviousPage, NextPage })
                    .AddComponents(new List<DiscordComponent> { AddSong, RemoveSong, RemoveDuplicates })
                    .AddComponents(new List<DiscordComponent> { PlaylistName, ChangePlaylistColor, ChangePlaylistThumbnail })
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                return;
            }

            await UpdateMessage();

            CancellationTokenSource tokenSource = new();

            _ = Task.Delay(120000, tokenSource.Token).ContinueWith(x =>
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
                        tokenSource.Cancel();
                        tokenSource = new();

                        _ = Task.Delay(120000, tokenSource.Token).ContinueWith(x =>
                        {
                            if (x.IsCompletedSuccessfully)
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                ModifyToTimedOut();
                            }
                        });

                        switch (e.GetCustomId())
                        {
                            case "AddSong":
                            {
                                if (SelectedPlaylist.List.Count >= 250)
                                {
                                    embed.Description = GetString(this.t.Commands.Music.Playlists.Modify.TrackLimit, true);
                                    embed.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title));
                                    await RespondOrEdit(embed.Build());
                                    _ = Task.Delay(5000).ContinueWith(async x =>
                                    {
                                        await UpdateMessage();
                                    });
                                    return;
                                }

                                var modal = new DiscordInteractionModalBuilder(GetString(this.t.Commands.Music.Playlists.Modify.AddSong), Guid.NewGuid().ToString())
                                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "query", GetString(this.t.Commands.Music.Playlists.CreatePlaylist.SupportedAddType), "", 1, 100, true));

                                var ModalResult = await PromptModalWithRetry(e.Interaction, modal, false);

                                if (ModalResult.TimedOut)
                                {
                                    ModifyToTimedOut(true);
                                    return;
                                }
                                else if (ModalResult.Cancelled)
                                {
                                    await UpdateMessage();
                                    break;
                                }
                                else if (ModalResult.Errored)
                                {
                                    throw ModalResult.Exception;
                                }

                                var (Tracks, oriResult, Continue) = await MusicModuleAbstractions.GetLoadResult(ctx, ModalResult.Result.Interaction.GetModalValueByCustomId("query"));

                                if (!Continue)
                                {
                                    await UpdateMessage();
                                    break;
                                }

                                if (SelectedPlaylist.List.Count >= 250)
                                {
                                    embed.Description = GetString(this.t.Commands.Music.Playlists.Modify.TrackLimit, true);
                                    embed.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title));
                                    await RespondOrEdit(embed.Build());
                                    _ = Task.Delay(5000).ContinueWith(async x =>
                                    {
                                        await UpdateMessage();
                                    });
                                    return;
                                }

                                SelectedPlaylist.List.AddRange(Tracks.Take(250 - SelectedPlaylist.List.Count).Select(x => new PlaylistEntry { Title = x.Title, Url = x.Uri.ToString(), Length = x.Length }));

                                await UpdateMessage();
                                break;
                            }
                            case "ChangeThumbnail":
                            {
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                try
                                {
                                    embed = new DiscordEmbedBuilder
                                    {
                                        Description = $"{GetString(this.t.Commands.Music.Playlists.Modify.UploadThumbnail, true, new TVar("Command", $"{ctx.Prefix}upload"))}\n\n" +
                                            $"⚠ {GetString(this.t.Commands.Music.Playlists.ThumbnailModerationNote, true)}",
                                    }.AsAwaitingInput(ctx, GetString(this.t.Commands.Music.Playlists.Title));

                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                    (Stream stream, int fileSize) stream;

                                    try
                                    {
                                        stream = await PromptForFileUpload();
                                    }
                                    catch (AlreadyAppliedException)
                                    {
                                        return;
                                    }
                                    catch (ArgumentException)
                                    {
                                        ModifyToTimedOut();
                                        return;
                                    }

                                    embed.Description = GetString(this.t.Commands.Music.Playlists.Modify.ImportingThumbnail, true);
                                    embed.AsLoading(ctx, GetString(this.t.Commands.Music.Playlists.Title));
                                    await RespondOrEdit(embed.Build());

                                    if (stream.fileSize > ctx.Bot.status.LoadedConfig.Discord.MaxUploadSize)
                                    {
                                        embed.Description = GetString(this.t.Commands.Music.Playlists.Modify.ThumbnailSizeError, true, new TVar("Size", ctx.Bot.status.LoadedConfig.Discord.MaxUploadSize.FileSizeToHumanReadable()));
                                        embed.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title));
                                        await RespondOrEdit(embed.Build());
                                        _ = Task.Delay(5000).ContinueWith(async x =>
                                        {
                                            await UpdateMessage();
                                        });
                                        return;
                                    }

                                    var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.PlaylistAssets)).SendMessageAsync(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} `{ctx.User.GetUsernameWithIdentifier()} ({ctx.User.Id})`\n`{SelectedPlaylist.PlaylistName}`").WithFile($"{Guid.NewGuid()}.png", stream.stream));

                                    SelectedPlaylist.PlaylistThumbnail = asset.Attachments[0].Url;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError("An exception occurred while trying to import thumbnail", ex);

                                    embed.Description = GetString(this.t.Commands.Music.Playlists.Modify.ThumbnailError, true);
                                    embed.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title));
                                    await RespondOrEdit(embed.Build());
                                    _ = Task.Delay(5000).ContinueWith(async x =>
                                    {
                                        await UpdateMessage();
                                    });
                                    return;
                                }

                                await UpdateMessage();
                                break;
                            }
                            case "ChangeColor":
                            {
                                var modal = new DiscordInteractionModalBuilder(GetString(this.t.Commands.Music.Playlists.Modify.NewPlaylistColor), Guid.NewGuid().ToString())
                                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "color", GetString(this.t.Commands.Music.Playlists.Modify.NewPlaylistColor), "#FF0000", 1, 100, true, SelectedPlaylist.PlaylistColor));

                                var ModalResult = await PromptModalWithRetry(e.Interaction, modal, new DiscordEmbedBuilder
                                {
                                    Description = GetString(this.t.Commands.Music.Playlists.Modify.NewPlaylistColorPrompt, true,
                                        new TVar("Hex", "#FF0000"),
                                        new TVar("HelpUrl", $"` [`{GetString(this.t.Commands.Music.Playlists.Modify.HexHelp)}`](https://g.co/kgs/jDHPp6)")),
                                }.AsAwaitingInput(ctx, GetString(this.t.Commands.Music.Playlists.Title)), false);

                                if (ModalResult.TimedOut)
                                {
                                    ModifyToTimedOut(true);
                                    return;
                                }
                                else if (ModalResult.Cancelled)
                                {
                                    await UpdateMessage();
                                    break;
                                }
                                else if (ModalResult.Errored)
                                {
                                    throw ModalResult.Exception;
                                }

                                SelectedPlaylist.PlaylistColor = ModalResult.Result.Interaction.GetModalValueByCustomId("color");

                                await UpdateMessage();
                                break;
                            }
                            case "ChangePlaylistName":
                            {
                                var modal = new DiscordInteractionModalBuilder(GetString(this.t.Commands.Music.Playlists.CreatePlaylist.SetPlaylistName), Guid.NewGuid().ToString())
                                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "name", GetString(this.t.Commands.Music.Playlists.CreatePlaylist.PlaylistName), "Playlist", 1, 100, true, SelectedPlaylist.PlaylistName));

                                var ModalResult = await PromptModalWithRetry(e.Interaction, modal, new DiscordEmbedBuilder
                                {
                                    Description = $"⚠ {GetString(this.t.Commands.Music.Playlists.NameModerationNote, true)}",
                                }.AsAwaitingInput(ctx, GetString(this.t.Commands.Music.Playlists.Title)), false);

                                if (ModalResult.TimedOut)
                                {
                                    ModifyToTimedOut(true);
                                    return;
                                }
                                else if (ModalResult.Cancelled)
                                {
                                    await UpdateMessage();
                                    break;
                                }
                                else if (ModalResult.Errored)
                                {
                                    throw ModalResult.Exception;
                                }

                                SelectedPlaylist.PlaylistName = ModalResult.Result.Interaction.GetModalValueByCustomId("name");

                                await UpdateMessage();
                                break;
                            }
                            case "RemoveDuplicates":
                            {
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                CurrentPage = 0;
                                SelectedPlaylist.List = SelectedPlaylist.List.GroupBy(x => x.Url).Select(y => y.FirstOrDefault()).ToList();
                                await UpdateMessage();
                                break;
                            }
                            case "DeleteSong":
                            {
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                List<DiscordStringSelectComponentOption> TrackList = SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10).Select(x => new DiscordStringSelectComponentOption($"{x.Title}", x.Url.MakeValidFileName(), $"Added {x.AddedTime.GetTimespanSince().GetHumanReadable()} ago")).ToList();

                                DiscordStringSelectComponent Tracks = new(GetString(this.t.Commands.Music.Playlists.Modify.DeleteNote), TrackList, Guid.NewGuid().ToString(), 1, TrackList.Count);

                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(Tracks));

                                var Response = await s.GetInteractivity().WaitForSelectAsync(ctx.ResponseMessage, x => x.User.Id == ctx.User.Id, ComponentType.StringSelect);

                                if (Response.TimedOut)
                                {
                                    ModifyToTimedOut();
                                    return;
                                }

                                _ = Response.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                foreach (var b in Response.Result.Values.Select(x => SelectedPlaylist.List.First(y => y.Url.MakeValidFileName() == x)))
                                {
                                    SelectedPlaylist.List.Remove(b);
                                }

                                if (SelectedPlaylist.List.Count <= 0)
                                {
                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                    {
                                        Description = GetString(this.t.Commands.Music.Playlists.Delete.Deleted, true, new TVar("Name", SelectedPlaylist.PlaylistName)),
                                    }.AsSuccess(ctx, GetString(this.t.Commands.Music.Playlists.Title))));

                                    ctx.DbUser.UserPlaylists.Remove(SelectedPlaylist);

                                    await Task.Delay(5000);
                                    return;
                                }

                                if (!SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10).Any())
                                    CurrentPage--;

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
                            case "cancel":
                            {
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                if (!ctx.Transferred)
                                    DeleteOrInvalidate();
                                else
                                    _ = new ManageCommand().TransferCommand(ctx, null);
                                return;
                            }
                        }
                    }
                }).Add(ctx.Bot, ctx);
            }
            return;
        });
    }
}