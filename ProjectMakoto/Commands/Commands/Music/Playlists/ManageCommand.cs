// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class ManageCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            var countInt = 0;

            int GetCount()
            {
                countInt++;
                return countInt;
            }

            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
            {
                Description = $"{(ctx.DbUser.UserPlaylists.Count > 0 ? string.Join("\n", ctx.DbUser.UserPlaylists.Select(x => $"**{GetCount()}**. `{x.PlaylistName.SanitizeForCode()}`: `{x.List.Count} {this.GetString(this.t.Commands.Music.Playlists.Tracks)}`")) : this.GetString(this.t.Commands.Music.Playlists.Manage.NoPlaylists, true))}"
            }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));

            var AddToQueue = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.Manage.AddToQueueButton), (ctx.DbUser.UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📤")));
            var SharePlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.Manage.ShareButton), (ctx.DbUser.UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📎")));
            var ExportPlaylist = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.Manage.ExportButton), (ctx.DbUser.UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📋")));

            var ImportPlaylist = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.Manage.ImportButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📥")));
            var SaveCurrent = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.Manage.SaveCurrentButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💾")));
            var NewPlaylist = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.Manage.CreateNewButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var ModifyPlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.Manage.ModifyButton), (ctx.DbUser.UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚙")));
            var DeletePlaylist = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Music.Playlists.Manage.DeleteButton), (ctx.DbUser.UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));

            _ = await this.RespondOrEdit(builder
            .AddComponents(new List<DiscordComponent> {
                AddToQueue,
                SharePlaylist,
                ExportPlaylist
            })
            .AddComponents(new List<DiscordComponent>
            {
                ImportPlaylist,
                SaveCurrent,
                NewPlaylist
            })
            .AddComponents(new List<DiscordComponent>
            {
                ModifyPlaylist,
                DeletePlaylist
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(1));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == AddToQueue.CustomId)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Playlists.Manage.PlaylistSelectorQueue, true)
                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));

                var PlaylistResult = await this.PromptCustomSelection(GetPlaylistOptions());

                if (PlaylistResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    this.DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new AddToQueueCommand().TransferCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });
                return;
            }
            else if (e.GetCustomId() == SharePlaylist.CustomId)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Playlists.Manage.PlaylistSelectorShare, true)
                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));

                var PlaylistResult = await this.PromptCustomSelection(GetPlaylistOptions());

                if (PlaylistResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    this.DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new ShareCommand().TransferCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });
                return;
            }
            else if (e.GetCustomId() == ExportPlaylist.CustomId)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Playlists.Manage.PlaylistSelectorExport, true)
                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));

                var PlaylistResult = await this.PromptCustomSelection(GetPlaylistOptions());

                if (PlaylistResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    this.DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new ExportCommand().TransferCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });
                return;
            }
            else if (e.GetCustomId() == NewPlaylist.CustomId)
            {
                await new NewPlaylistCommand().TransferCommand(ctx, null);
                return;
            }
            else if (e.GetCustomId() == SaveCurrent.CustomId)
            {
                await new SaveCurrentCommand().TransferCommand(ctx, null);
                return;
            }
            else if (e.GetCustomId() == ImportPlaylist.CustomId)
            {
                await new ImportCommand().TransferCommand(ctx, null);

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ModifyPlaylist.CustomId)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Playlists.Manage.PlaylistSelectorModify, true)
                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));

                var PlaylistResult = await this.PromptCustomSelection(GetPlaylistOptions());

                if (PlaylistResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    this.DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new ModifyCommand().TransferCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });
                return;
            }
            else if (e.GetCustomId() == DeletePlaylist.CustomId)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Playlists.Manage.PlaylistSelectorDelete, true)
                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Music.Playlists.Title)));

                var PlaylistResult = await this.PromptCustomSelection(GetPlaylistOptions());

                if (PlaylistResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    this.DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new DeleteCommand().TransferCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else
            {
                this.DeleteOrInvalidate();
            }

            List<DiscordStringSelectComponentOption> GetPlaylistOptions()
            => ctx.DbUser.UserPlaylists.Select(x => new DiscordStringSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} {this.GetString(this.t.Commands.Music.Playlists.Tracks)}")).ToList();
        });
    }
}