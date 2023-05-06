namespace ProjectMakoto.Commands.Playlists;

internal class ManageCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx))
                return;

            var countInt = 0;

            int GetCount()
            {
                countInt++;
                return countInt;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"{(ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count > 0 ? string.Join("\n", ctx.Bot.users[ctx.Member.Id].UserPlaylists.Select(x => $"**{GetCount()}**. `{x.PlaylistName.SanitizeForCode()}`: `{x.List.Count} track(s)`")) : $"`No playlist created yet.`")}"
            }.AsAwaitingInput(ctx, GetString(t.Commands.Music.Playlists.Title));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var AddToQueue = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.Manage.AddToQueueButton), (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📤")));
            var SharePlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.Manage.ShareButton), (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📎")));
            var ExportPlaylist = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.Manage.ExportButton), (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📋")));

            var ImportPlaylist = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.Manage.ImportButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📥")));
            var SaveCurrent = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.Manage.SaveCurrentButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💾")));
            var NewPlaylist = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.Manage.CreateNewButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var ModifyPlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.Manage.ModifyButton), (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚙")));
            var DeletePlaylist = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.Manage.DeleteButton), (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));

            await RespondOrEdit(builder
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
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(1));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == AddToQueue.CustomId)
            {
                List<DiscordStringSelectComponentOption> Playlists = ctx.Bot.users[ctx.Member.Id].UserPlaylists.Select(x => new DiscordStringSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                var PlaylistResult = await PromptCustomSelection(Playlists);

                if (PlaylistResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new AddToQueueCommand().ExecuteCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });
                return;
            }
            else if (e.GetCustomId() == SharePlaylist.CustomId)
            {
                List<DiscordStringSelectComponentOption> Playlists = ctx.Bot.users[ctx.Member.Id].UserPlaylists.Select(x => new DiscordStringSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                var PlaylistResult = await PromptCustomSelection(Playlists);

                if (PlaylistResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new ShareCommand().ExecuteCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });
                return;
            }
            else if (e.GetCustomId() == ExportPlaylist.CustomId)
            {
                List<DiscordStringSelectComponentOption> Playlists = ctx.Bot.users[ctx.Member.Id].UserPlaylists.Select(x => new DiscordStringSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                var PlaylistResult = await PromptCustomSelection(Playlists);

                if (PlaylistResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new ExportCommand().ExecuteCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });
                return;
            }
            else if (e.GetCustomId() == NewPlaylist.CustomId)
            {
                await new NewPlaylistCommand().ExecuteCommand(ctx, null);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == SaveCurrent.CustomId)
            {
                await new SaveCurrentCommand().ExecuteCommand(ctx, null);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ImportPlaylist.CustomId)
            {
                await new ImportCommand().ExecuteCommand(ctx, null);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ModifyPlaylist.CustomId)
            {
                embed = new DiscordEmbedBuilder()
                {
                    Description = $"`What playlist do you want to modify?`"
                }.AsAwaitingInput(ctx, GetString(t.Commands.Music.Playlists.Title));

                await RespondOrEdit(embed);

                List<DiscordStringSelectComponentOption> Playlists = ctx.Bot.users[ctx.Member.Id].UserPlaylists.Select(x => new DiscordStringSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                var PlaylistResult = await PromptCustomSelection(Playlists);

                if (PlaylistResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new ModifyCommand().ExecuteCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == DeletePlaylist.CustomId)
            {
                List<DiscordStringSelectComponentOption> Playlists = ctx.Bot.users[ctx.Member.Id].UserPlaylists.Select(x => new DiscordStringSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                var PlaylistResult = await PromptCustomSelection(Playlists);

                if (PlaylistResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (PlaylistResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (PlaylistResult.Errored)
                {
                    throw PlaylistResult.Exception;
                }

                await new DeleteCommand().ExecuteCommand(ctx, new Dictionary<string, object>
                {
                    { "id", PlaylistResult.Result }
                });

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else
            {
                DeleteOrInvalidate();
            }
        });
    }
}