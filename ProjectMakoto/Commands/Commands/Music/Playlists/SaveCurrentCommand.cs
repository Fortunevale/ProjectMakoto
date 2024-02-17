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
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.NotSameChannel, true)
                }.AsError(ctx)));
                return;
            }

            var SelectedPlaylistName = "";
            var SelectedTracks = ctx.DbGuild.MusicModule.SongQueue.Select(x => new PlaylistEntry { Title = x.VideoTitle, Url = x.Url, Length = x.Length }).Take(250).ToArray();

            while (true)
            {
                if (ctx.DbUser.UserPlaylists.Length >= 10)
                {
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                    }.AsError(ctx, this.GetString(this.t.Commands.Music.Playlists.Title))));
                    await Task.Delay(5000);
                    return;
                }

                var SelectName = new DiscordButtonComponent((SelectedPlaylistName.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.ChangeName), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.CreatePlaylist), (SelectedPlaylistName.IsNullOrWhiteSpace()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, this.t.Commands.Music.Playlists.CreatePlaylist.PlaylistName, this.t.Commands.Music.Playlists.CreatePlaylist.FirstTracks);

                var embed = new DiscordEmbedBuilder
                {
                    Description = $"`{this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.PlaylistName).PadRight(pad)}`: `{(SelectedPlaylistName.IsNullOrWhiteSpace() ? this.GetString(this.t.Common.NotSelected) : SelectedPlaylistName)}`\n" +
                                  $"`{this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.FirstTracks).PadRight(pad)}`: {(SelectedTracks.IsNotNullAndNotEmpty() ? (SelectedTracks.Length > 1 ? $"`{SelectedTracks.Length} {this.GetString(this.t.Commands.Music.Playlists.Tracks)}`" : $"[`{SelectedTracks[0].Title}`]({SelectedTracks[0].Url})") : this.GetString(this.t.Common.NotSelected, true))}"
                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title));

                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { SelectName, Finish })
                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                var Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }

                if (Menu.GetCustomId() == SelectName.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder(this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.SetPlaylistName), Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "name", this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.PlaylistName), this.GetString(this.t.Commands.Music.Playlists.Title), 1, 100, true, (SelectedPlaylistName.IsNullOrWhiteSpace() ? "New Playlist" : SelectedPlaylistName)));


                    var ModalResult = await this.PromptModalWithRetry(Menu.Result.Interaction, modal, new DiscordEmbedBuilder
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

                    if (ctx.DbUser.UserPlaylists.Length >= 10)
                    {
                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = this.GetString(this.t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                        }.AsError(ctx, this.GetString(this.t.Commands.Music.Playlists.Title))));
                        await Task.Delay(5000);
                        return;
                    }

                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.Creating, true),
                    }.AsLoading(ctx, this.GetString(this.t.Commands.Music.Playlists.Title))));

                    var v = new UserPlaylist
                    {
                        PlaylistName = SelectedPlaylistName,
                        List = SelectedTracks
                    };

                    ctx.DbUser.UserPlaylists = ctx.DbUser.UserPlaylists.Add(v);

                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Music.Playlists.CreatePlaylist.Created, true,
                            new TVar("Playlist", v.PlaylistName),
                            new TVar("Count", v.List.Length)),
                    }.AsSuccess(ctx, this.GetString(this.t.Commands.Music.Playlists.Title))));
                    await Task.Delay(2000);
                    await new ModifyCommand().TransferCommand(ctx, new Dictionary<string, object>
                    {
                        { "id", v.PlaylistId }
                    });
                    return;
                }
                else if (Menu.GetCustomId() == MessageComponents.CancelButtonId)
                {
                    if (!ctx.Transferred)
                        this.DeleteOrInvalidate();

                    return;
                }

                return;
            }
        });
    }
}