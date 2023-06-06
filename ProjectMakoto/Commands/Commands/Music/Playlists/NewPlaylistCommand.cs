// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class NewPlaylistCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            string SelectedPlaylistName = "";
            List<PlaylistEntry> SelectedTracks = null;

            while (true)
            {
                if (ctx.DbUser.UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                    }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                    await Task.Delay(5000);
                    return;
                }

                var SelectName = new DiscordButtonComponent((SelectedPlaylistName.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(this.t.Commands.Music.Playlists.CreatePlaylist.ChangeName), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                var SelectFirstTracks = new DiscordButtonComponent((SelectedTracks is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(this.t.Commands.Music.Playlists.CreatePlaylist.ChangeTracks), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎵")));
                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(this.t.Commands.Music.Playlists.CreatePlaylist.CreatePlaylist), (SelectedPlaylistName.IsNullOrWhiteSpace()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, this.t.Commands.Music.Playlists.CreatePlaylist.PlaylistName, this.t.Commands.Music.Playlists.CreatePlaylist.FirstTracks);

                var embed = new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(this.t.Commands.Music.Playlists.CreatePlaylist.PlaylistName).PadRight(pad)}`: `{(SelectedPlaylistName.IsNullOrWhiteSpace() ? GetString(this.t.Common.NotSelected) : SelectedPlaylistName)}`\n" +
                                  $"`{GetString(this.t.Commands.Music.Playlists.CreatePlaylist.FirstTracks).PadRight(pad)}`: {(SelectedTracks.IsNotNullAndNotEmpty() ? (SelectedTracks.Count > 1 ? $"`{SelectedTracks.Count} {GetString(this.t.Commands.Music.Playlists.Tracks)}`" : $"[`{SelectedTracks[0].Title}`]({SelectedTracks[0].Url})") : GetString(this.t.Common.NotSelected, true))}"
                }.AsAwaitingInput(ctx, GetString(this.t.Commands.Music.Playlists.Title));

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { SelectName, SelectFirstTracks, Finish })
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                var Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                if (Menu.GetCustomId() == SelectName.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder(GetString(this.t.Commands.Music.Playlists.CreatePlaylist.SetPlaylistName), Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "name", GetString(this.t.Commands.Music.Playlists.CreatePlaylist.PlaylistName), GetString(this.t.Commands.Music.Playlists.Title), 1, 100, true, (SelectedPlaylistName.IsNullOrWhiteSpace() ? "New Playlist" : SelectedPlaylistName)));

                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, new DiscordEmbedBuilder
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
                        continue;
                    }
                    else if (ModalResult.Errored)
                    {
                        throw ModalResult.Exception;
                    }

                    SelectedPlaylistName = ModalResult.Result.Interaction.GetModalValueByCustomId("name");
                    continue;
                }
                else if (Menu.GetCustomId() == SelectFirstTracks.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder(GetString(this.t.Commands.Music.Playlists.CreatePlaylist.SetFirstTracks), Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "query", GetString(this.t.Commands.Music.Playlists.CreatePlaylist.SupportedAddType), "Url", 1, 100, true));


                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, false);

                    if (ModalResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (ModalResult.Cancelled)
                    {
                        continue;
                    }
                    else if (ModalResult.Errored)
                    {
                        throw ModalResult.Exception;
                    }

                    var query = ModalResult.Result.Interaction.GetModalValueByCustomId("query");

                    var (Tracks, oriResult, Continue) = await MusicModuleAbstractions.GetLoadResult(ctx, query);

                    if (!Continue || !Tracks.IsNotNullAndNotEmpty())
                        continue;

                    SelectedTracks = Tracks.Select(x => new PlaylistEntry
                    {
                        Title = x.Title,
                        Url = x.Uri.ToString(),
                        Length = x.Length
                    }).ToList();
                    continue;
                }
                else if (Menu.GetCustomId() == Finish.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    if (ctx.DbUser.UserPlaylists.Count >= 10)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = GetString(this.t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                        }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                        await Task.Delay(5000);
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.CreatePlaylist.Creating, true),
                    }.AsLoading(ctx, GetString(this.t.Commands.Music.Playlists.Title))));

                    var v = new UserPlaylist
                    {
                        PlaylistName = SelectedPlaylistName,
                        List = SelectedTracks
                    };

                    ctx.DbUser.UserPlaylists.Add(v);

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.CreatePlaylist.Created, true,
                            new TVar("Playlist", v.PlaylistName),
                            new TVar("Count", v.List.Count)),
                    }.AsSuccess(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                    await Task.Delay(2000);
                    await new ModifyCommand().TransferCommand(ctx, new Dictionary<string, object>
                    {
                        { "id", v.PlaylistId }
                    });
                    return;
                }
                else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
                {
                    if (!ctx.Transferred)
                        DeleteOrInvalidate();

                    return;
                }

                return;
            }
        });
    }
}