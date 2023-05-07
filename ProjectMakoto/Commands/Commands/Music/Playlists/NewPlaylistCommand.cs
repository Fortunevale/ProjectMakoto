// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal class NewPlaylistCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx))
                return;

            string SelectedPlaylistName = "";
            List<PlaylistEntry> SelectedTracks = null;

            while (true)
            {
                if (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                    }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                    await Task.Delay(5000);
                    return;
                }

                var SelectName = new DiscordButtonComponent((SelectedPlaylistName.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Change Playlist Name", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                var SelectFirstTracks = new DiscordButtonComponent((SelectedTracks is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Change First Tracks", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎵")));
                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Create Playlist", (SelectedPlaylistName.IsNullOrWhiteSpace()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));


                var embed = new DiscordEmbedBuilder
                {
                    Description = $"`Playlist Name `: `{(SelectedPlaylistName.IsNullOrWhiteSpace() ? "Not yet selected." : SelectedPlaylistName)}`\n" +
                                  $"`First Track(s)`: {(SelectedTracks.IsNotNullAndNotEmpty() ? (SelectedTracks.Count > 1 ? $"`{SelectedTracks.Count} Tracks`" : $"[`{SelectedTracks[0].Title}`]({SelectedTracks[0].Url})") : "`Not yet selected.`")}"
                }.AsAwaitingInput(ctx, GetString(t.Commands.Music.Playlists.Title));

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { SelectName, SelectFirstTracks, Finish })
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

                var Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                if (Menu.GetCustomId() == SelectName.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder("Set a playlist name", Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "name", "Playlist Name", "Playlist", 1, 100, true, (SelectedPlaylistName.IsNullOrWhiteSpace() ? "New Playlist" : SelectedPlaylistName)));

                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, new DiscordEmbedBuilder
                    {
                        Description = $"⚠ `Please note: Playlist Names are being moderated. If your playlist name is determined to be inappropriate or otherwise harming it will be removed and you'll lose access to the entirety of Makoto. This includes the bot being removed from guilds you own or manage. Please keep it safe. ♥`",
                    }.AsAwaitingInput(ctx, GetString(t.Commands.Music.Playlists.Title)), false);

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
                    var modal = new DiscordInteractionModalBuilder("Set first track(s) for your Playlist", Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "query", "Song Url, Playlist Url or Search Query", "Url", 1, 100, true));


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

                    if (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count >= 10)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = GetString(t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                        }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                        await Task.Delay(5000);
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Creating your playlist..`",
                    }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title))));

                    var v = new UserPlaylist
                    {
                        PlaylistName = SelectedPlaylistName,
                        List = SelectedTracks
                    };

                    ctx.Bot.users[ctx.Member.Id].UserPlaylists.Add(v);

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Your playlist '{v.PlaylistName}' has been created with {v.List.Count} entries.`",
                    }.AsSuccess(ctx, GetString(t.Commands.Music.Playlists.Title))));
                    await Task.Delay(2000);
                    await new ModifyCommand().TransferCommand(ctx, new Dictionary<string, object>
                    {
                        { "id", v.PlaylistId }
                    });
                    return;
                }
                else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
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