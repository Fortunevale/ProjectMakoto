// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Users;

namespace ProjectMakoto.Commands.Playlists;

internal sealed class ModifyCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            var playlistId = (string)arguments["playlist"];

            if (!ctx.DbUser.UserPlaylists.Any(x => x.PlaylistId == playlistId))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Playlists.NoPlaylist, true),
                }.AsError(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));
                return;
            }

            var SelectedPlaylist = ctx.DbUser.UserPlaylists.First(x => x.PlaylistId == playlistId);

            var embed = new DiscordEmbedBuilder().AsInfo(ctx);

            var LastInt = 0;
            int GetInt()
            {
                LastInt++;
                return LastInt;
            }

            var CurrentPage = 0;

            async Task UpdateMessage()
            {
                LastInt = CurrentPage * 10;

                var CurrentTracks = SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10);

                DiscordButtonComponent NextPage = new(ButtonStyle.Primary, "NextPage", this.GetString(this.t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
                DiscordButtonComponent PreviousPage = new(ButtonStyle.Primary, "PreviousPage", this.GetString(this.t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));

                DiscordButtonComponent PlaylistName = new(ButtonStyle.Success, "ChangePlaylistName", this.GetString(this.t.Commands.Music.Playlists.Modify.ChangeName), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

                DiscordButtonComponent ChangePlaylistColor = new(ButtonStyle.Secondary, "ChangeColor", this.GetString(this.t.Commands.Music.Playlists.Modify.ChangeColor), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎨")));
                DiscordButtonComponent ChangePlaylistThumbnail = new(ButtonStyle.Secondary, "ChangeThumbnail", this.GetString(this.t.Commands.Music.Playlists.Modify.ChangeThumbnail), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖼")));

                DiscordButtonComponent AddSong = new(ButtonStyle.Success, "AddSong", this.GetString(this.t.Commands.Music.Playlists.Modify.AddTracks), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                DiscordButtonComponent RemoveSong = new(ButtonStyle.Danger, "DeleteSong", this.GetString(this.t.Commands.Music.Playlists.Modify.RemoveTracks), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
                DiscordButtonComponent RemoveDuplicates = new(ButtonStyle.Secondary, "RemoveDuplicates", this.GetString(this.t.Commands.Music.Playlists.Modify.RemoveDuplicates), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("♻")));

                var TotalTimespan = TimeSpan.Zero;

                for (var i = 0; i < SelectedPlaylist.List.Length; i++)
                {
                    TotalTimespan = TotalTimespan.Add(SelectedPlaylist.List[i].Length.Value);
                }

                var Description = $"**`{this.GetString(this.t.Commands.Music.Playlists.Modify.CurrentTrackCount, new TVar("Count", SelectedPlaylist.List.Length), new TVar("Timespan", TotalTimespan.GetHumanReadable()))}`**\n\n";
                Description += $"{string.Join("\n", CurrentTracks.Select(x => $"**{GetInt()}**. `{x.Length.Value.GetShortHumanReadable(TimeFormat.Hours)}` {this.GetString(this.t.Commands.Music.Playlists.Modify.Track, new TVar("Track", $"**[`{x.Title}`]({x.Url})**"), new TVar("Timestamp", Formatter.Timestamp(x.AddedTime)))}"))}";

                if (SelectedPlaylist.List.Length > 0)
                    Description += $"\n\n`{this.GetString(this.t.Common.Page)} {CurrentPage + 1}/{Math.Ceiling(SelectedPlaylist.List.Length / 10.0)}`";

                if (CurrentPage <= 0)
                    PreviousPage = PreviousPage.Disable();

                if ((CurrentPage * 10) + 10 >= SelectedPlaylist.List.Length)
                    NextPage = NextPage.Disable();

                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Color = (SelectedPlaylist.PlaylistColor is "#FFFFFF" or null or "" ? EmbedColors.Info : new DiscordColor(SelectedPlaylist.PlaylistColor.IsValidHexColor()));
                embed.Title = $"{this.GetString(this.t.Commands.Music.Playlists.Modify.ModifyingPlaylist)}: `{SelectedPlaylist.PlaylistName}`";
                embed.Description = Description;
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = (SelectedPlaylist.PlaylistThumbnail.IsNullOrWhiteSpace() ? "" : SelectedPlaylist.PlaylistThumbnail) };
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
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
                    this.ModifyToTimedOut();
                }
            });

            ctx.Client.ComponentInteractionCreated += RunInteraction;
            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
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
                                this.ModifyToTimedOut();
                            }
                        });

                        switch (e.GetCustomId())
                        {
                            case "AddSong":
                            {
                                if (SelectedPlaylist.List.Length >= 250)
                                {
                                    embed.Description = this.GetString(this.t.Commands.Music.Playlists.Modify.TrackLimit, true);
                                    _ = embed.AsError(ctx, this.GetString(this.t.Commands.Music.Playlists.Title));
                                    _ = await this.RespondOrEdit(embed.Build());
                                    _ = Task.Delay(5000).ContinueWith(async x =>
                                    {
                                        await UpdateMessage();
                                    });
                                    return;
                                }

                                var modal = new DiscordInteractionModalBuilder(this.GetString(this.t.Commands.Music.Playlists.Modify.AddSong), Guid.NewGuid().ToString())
                                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "query", this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.SupportedAddType), "", 1, 100, true));

                                var ModalResult = await this.PromptModalWithRetry(e.Interaction, modal, false);

                                if (ModalResult.TimedOut)
                                {
                                    this.ModifyToTimedOut(true);
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

                                if (SelectedPlaylist.List.Length >= 250)
                                {
                                    embed.Description = this.GetString(this.t.Commands.Music.Playlists.Modify.TrackLimit, true);
                                    _ = embed.AsError(ctx, this.GetString(this.t.Commands.Music.Playlists.Title));
                                    _ = await this.RespondOrEdit(embed.Build());
                                    _ = Task.Delay(5000).ContinueWith(async x =>
                                    {
                                        await UpdateMessage();
                                    });
                                    return;
                                }

                                SelectedPlaylist.List = SelectedPlaylist.List.AddRange(Tracks.Take(250 - SelectedPlaylist.List.Length).Select(x => new PlaylistEntry { Title = x.Info.Title, Url = x.Info.Uri.ToString(), Length = x.Info.Length }));

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
                                        Description = $"{this.GetString(this.t.Commands.Music.Playlists.Modify.UploadThumbnail, true, new TVar("Command", $"{ctx.Prefix}upload"))}\n\n" +
                                            $"⚠ {this.GetString(this.t.Commands.Music.Playlists.ThumbnailModerationNote, true)}",
                                    }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title));

                                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                    (Stream stream, int fileSize) stream;

                                    try
                                    {
                                        stream = await this.PromptForFileUpload();
                                    }
                                    catch (AlreadyAppliedException)
                                    {
                                        return;
                                    }
                                    catch (ArgumentException)
                                    {
                                        this.ModifyToTimedOut();
                                        return;
                                    }

                                    embed.Description = this.GetString(this.t.Commands.Music.Playlists.Modify.ImportingThumbnail, true);
                                    _ = embed.AsLoading(ctx, this.GetString(this.t.Commands.Music.Playlists.Title));
                                    _ = await this.RespondOrEdit(embed.Build());

                                    if (stream.fileSize > ctx.Bot.status.LoadedConfig.Discord.MaxUploadSize)
                                    {
                                        embed.Description = this.GetString(this.t.Commands.Music.Playlists.Modify.ThumbnailSizeError, true, new TVar("Size", ctx.Bot.status.LoadedConfig.Discord.MaxUploadSize.FileSizeToHumanReadable()));
                                        _ = embed.AsError(ctx, this.GetString(this.t.Commands.Music.Playlists.Title));
                                        _ = await this.RespondOrEdit(embed.Build());
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
                                    Log.Error(ex, "An exception occurred while trying to import thumbnail");

                                    embed.Description = this.GetString(this.t.Commands.Music.Playlists.Modify.ThumbnailError, true);
                                    _ = embed.AsError(ctx, this.GetString(this.t.Commands.Music.Playlists.Title));
                                    _ = await this.RespondOrEdit(embed.Build());
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
                                var modal = new DiscordInteractionModalBuilder(this.GetString(this.t.Commands.Music.Playlists.Modify.NewPlaylistColor), Guid.NewGuid().ToString())
                                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "color", this.GetString(this.t.Commands.Music.Playlists.Modify.NewPlaylistColor), "#FF0000", 1, 100, true, SelectedPlaylist.PlaylistColor));

                                var ModalResult = await this.PromptModalWithRetry(e.Interaction, modal, new DiscordEmbedBuilder
                                {
                                    Description = this.GetString(this.t.Commands.Music.Playlists.Modify.NewPlaylistColorPrompt, true,
                                        new TVar("Hex", "#FF0000"),
                                        new TVar("HelpUrl", $"` [`{this.GetString(this.t.Commands.Music.Playlists.Modify.HexHelp)}`](https://g.co/kgs/jDHPp6)")),
                                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)), false);

                                if (ModalResult.TimedOut)
                                {
                                    this.ModifyToTimedOut(true);
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
                                var modal = new DiscordInteractionModalBuilder(this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.SetPlaylistName), Guid.NewGuid().ToString())
                                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "name", this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.PlaylistName), "Playlist", 1, 100, true, SelectedPlaylist.PlaylistName));

                                var ModalResult = await this.PromptModalWithRetry(e.Interaction, modal, new DiscordEmbedBuilder
                                {
                                    Description = $"⚠ {this.GetString(this.t.Commands.Music.Playlists.NameModerationNote, true)}",
                                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)), false);

                                if (ModalResult.TimedOut)
                                {
                                    this.ModifyToTimedOut(true);
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
                                SelectedPlaylist.List = SelectedPlaylist.List.GroupBy(x => x.Url).Select(y => y.FirstOrDefault()).ToArray();
                                await UpdateMessage();
                                break;
                            }
                            case "DeleteSong":
                            {
                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                var TrackList = SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10).Select(x => new DiscordStringSelectComponentOption($"{x.Title}", x.Url.MakeValidFileName(), $"Added {x.AddedTime.GetTimespanSince().GetHumanReadable()} ago")).ToList();

                                DiscordStringSelectComponent Tracks = new(this.GetString(this.t.Commands.Music.Playlists.Modify.DeleteNote), TrackList, Guid.NewGuid().ToString(), 1, TrackList.Count);

                                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(Tracks));

                                var Response = await s.GetInteractivity().WaitForSelectAsync(ctx.ResponseMessage, x => x.User.Id == ctx.User.Id, ComponentType.StringSelect);

                                if (Response.TimedOut)
                                {
                                    this.ModifyToTimedOut();
                                    return;
                                }

                                _ = Response.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                foreach (var b in Response.Result.Values.Select(x => SelectedPlaylist.List.First(y => y.Url.MakeValidFileName() == x)))
                                {
                                    SelectedPlaylist.List = SelectedPlaylist.List.Remove(x => x.Url, b);
                                }

                                if (SelectedPlaylist.List.Length <= 0)
                                {
                                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                    {
                                        Description = this.GetString(this.t.Commands.Music.Playlists.Delete.Deleted, true, new TVar("Name", SelectedPlaylist.PlaylistName)),
                                    }.AsSuccess(ctx, this.GetString(this.t.Commands.Music.Playlists.Title))));

                                    ctx.DbUser.UserPlaylists = ctx.DbUser.UserPlaylists.Remove(x => x.PlaylistId, SelectedPlaylist);

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
                                    this.DeleteOrInvalidate();
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