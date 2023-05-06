// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal class SaveCurrentCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx))
                return;

            if (ctx.Member.VoiceState is null || ctx.Member.VoiceState.Channel.Id != (await ctx.Client.CurrentUser.ConvertToMember(ctx.Guild)).VoiceState?.Channel?.Id)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`You aren't in the same channel as the bot.`"
                }.AsError(ctx)));
                return;
            }

            string SelectedPlaylistName = "";
            List<PlaylistEntry> SelectedTracks = ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Select(x => new PlaylistEntry { Title = x.VideoTitle, Url = x.Url, Length = x.Length }).Take(250).ToList();

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
                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Create Playlist", (SelectedPlaylistName.IsNullOrWhiteSpace()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));


                var embed = new DiscordEmbedBuilder
                {
                    Description = $"`Playlist Name `: `{(SelectedPlaylistName.IsNullOrWhiteSpace() ? "Not yet selected." : SelectedPlaylistName)}`\n" +
                                  $"`First Track(s)`: {(SelectedTracks.IsNotNullAndNotEmpty() ? (SelectedTracks.Count > 1 ? $"`{SelectedTracks.Count} Tracks`" : $"[`{SelectedTracks[0].Title}`]({SelectedTracks[0].Url})") : "`Not yet selected.`")}"
                }.AsAwaitingInput(ctx, GetString(t.Commands.Music.Playlists.Title));

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(new List<DiscordComponent> { SelectName, Finish })
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
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "name", "Playlist Name", "Playlist", 1, 100, true, (SelectedPlaylistName.IsNullOrWhiteSpace() ? "" : SelectedPlaylistName)));


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
                    await new ModifyCommand().ExecuteCommand(ctx, new Dictionary<string, object>
                    {
                        { "id", v.PlaylistId }
                    });
                    return;
                }
                else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                {
                    return;
                }

                return;
            }
        });
    }
}