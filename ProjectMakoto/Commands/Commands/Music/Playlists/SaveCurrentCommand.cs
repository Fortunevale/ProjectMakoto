// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class SaveCurrentCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            if (ctx.Member.VoiceState is null || ctx.Member.VoiceState.Channel.Id != (await ctx.Client.CurrentUser.ConvertToMember(ctx.Guild)).VoiceState?.Channel?.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.NotSameChannel, true)
                }.AsError(ctx)));
                return;
            }

            string SelectedPlaylistName = "";
            List<PlaylistEntry> SelectedTracks = ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Select(x => new PlaylistEntry { Title = x.VideoTitle, Url = x.Url, Length = x.Length }).Take(250).ToList();

            while (true)
            {
                if (ctx.DbUser.UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                    }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                    await Task.Delay(5000);
                    return;
                }

                var SelectName = new DiscordButtonComponent((SelectedPlaylistName.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.CreatePlaylist.ChangeName), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.CreatePlaylist.CreatePlaylist), (SelectedPlaylistName.IsNullOrWhiteSpace()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, t.Commands.Music.Playlists.CreatePlaylist.PlaylistName, t.Commands.Music.Playlists.CreatePlaylist.FirstTracks);

                var embed = new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(t.Commands.Music.Playlists.CreatePlaylist.PlaylistName).PadRight(pad)}`: `{(SelectedPlaylistName.IsNullOrWhiteSpace() ? GetString(t.Common.NotSelected) : SelectedPlaylistName)}`\n" +
                                  $"`{GetString(t.Commands.Music.Playlists.CreatePlaylist.FirstTracks).PadRight(pad)}`: {(SelectedTracks.IsNotNullAndNotEmpty() ? (SelectedTracks.Count > 1 ? $"`{SelectedTracks.Count} {GetString(t.Commands.Music.Playlists.Tracks)}`" : $"[`{SelectedTracks[0].Title}`]({SelectedTracks[0].Url})") : GetString(t.Common.NotSelected, true))}"
                }.AsAwaitingInput(ctx, GetString(t.Commands.Music.Playlists.Title));

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { SelectName, Finish })
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                var Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                if (Menu.GetCustomId() == SelectName.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder(GetString(t.Commands.Music.Playlists.CreatePlaylist.SetPlaylistName), Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "name", GetString(t.Commands.Music.Playlists.CreatePlaylist.PlaylistName), GetString(t.Commands.Music.Playlists.Title), 1, 100, true, (SelectedPlaylistName.IsNullOrWhiteSpace() ? "New Playlist" : SelectedPlaylistName)));


                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, new DiscordEmbedBuilder
                    {
                        Description = $"⚠ {GetString(t.Commands.Music.Playlists.NameModerationNote, true)}",
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
                else if (Menu.GetCustomId() == Finish.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    if (ctx.DbUser.UserPlaylists.Count >= 10)
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
                        Description = GetString(t.Commands.Music.Playlists.CreatePlaylist.Creating, true),
                    }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title))));

                    var v = new UserPlaylist
                    {
                        PlaylistName = SelectedPlaylistName,
                        List = SelectedTracks
                    };

                    ctx.DbUser.UserPlaylists.Add(v);

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(t.Commands.Music.Playlists.CreatePlaylist.Created, true, 
                            new TVar("Playlist", v.PlaylistName),
                            new TVar("Count", v.List.Count)),
                    }.AsSuccess(ctx, GetString(t.Commands.Music.Playlists.Title))));
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