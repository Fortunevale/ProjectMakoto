namespace ProjectIchigo.Commands.Playlists;

internal class ManageCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (!ctx.Bot._users.List.ContainsKey(ctx.User.Id))
                ctx.Bot._users.List.Add(ctx.User.Id, new Users.Info(ctx.Bot));

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            var countInt = 0;

            int GetCount()
            {
                countInt++;
                return countInt;
            }

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Playlists • {ctx.Guild.Name}" },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"{(ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count > 0 ? string.Join("\n", ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => $"**{GetCount()}**. `{x.PlaylistName.SanitizeForCodeBlock()}`: `{x.List.Count} track(s)`")) : $"`No playlist created yet.`")}"
            };

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var AddToQueue = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Add a playlist to the current queue", (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📤")));
            var SharePlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Share a playlist", (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📎")));
            var ExportPlaylist = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Export a playlist", (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📋")));

            var ImportPlaylist = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Import a playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📥")));
            var SaveCurrent = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Save current queue as playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💾")));
            var NewPlaylist = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Create new playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var ModifyPlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Select a playlist to modify", (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚙")));
            var DeletePlaylist = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Select a playlist to delete", (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));

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
            .AddComponents(Resources.CancelButton));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(1));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == AddToQueue.CustomId)
            {
                List<DiscordSelectComponentOption> Playlists = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => new DiscordSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                string SelectedPlaylistId;
                UserPlaylist SelectedPlaylist;

                try
                {
                    SelectedPlaylistId = await PromptCustomSelection(Playlists);
                    SelectedPlaylist = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == SelectedPlaylistId);
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }
                catch (Exception)
                {
                    throw;
                }

                embed = new DiscordEmbedBuilder
                {
                    Description = $":arrows_counterclockwise: `Preparing connection..`",
                    Color = EmbedColors.Processing,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                };
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                try
                {
                    await new Commands.Music.JoinCommand().ExecuteCommand(ctx, null);
                }
                catch (CancelCommandException)
                {
                    return;
                }

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                embed.Description = $":arrows_counterclockwise: `Adding '{SelectedPlaylist.PlaylistName}' with {SelectedPlaylist.List.Count} track(s) to the queue..`";
                await RespondOrEdit(embed.Build());

                ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.AddRange(SelectedPlaylist.List.Select(x => new Lavalink.QueueInfo(x.Title, x.Url, ctx.Guild, ctx.User)));

                embed.Description = $"✅ `Queued {SelectedPlaylist.List.Count} songs from your personal playlist '{SelectedPlaylist.PlaylistName}'.`";

                embed.AddField(new DiscordEmbedField($"📜 Queue positions", $"{(ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count - SelectedPlaylist.List.Count + 1)} - {ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}", true));

                embed.Color = EmbedColors.Success;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                await RespondOrEdit(embed.Build());
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == SharePlaylist.CustomId)
            {
                List<DiscordSelectComponentOption> Playlists = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => new DiscordSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                string SelectedPlaylistId;
                UserPlaylist SelectedPlaylist;

                try
                {
                    SelectedPlaylistId = await PromptCustomSelection(Playlists);
                    SelectedPlaylist = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == SelectedPlaylistId);
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }
                catch (Exception)
                {
                    throw;
                }

                string ShareCode = $"{Guid.NewGuid()}";

                if (!Directory.Exists("PlaylistShares"))
                    Directory.CreateDirectory("PlaylistShares");

                if (!Directory.Exists($"PlaylistShares/{ctx.User.Id}"))
                    Directory.CreateDirectory($"PlaylistShares/{ctx.User.Id}");

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Playlists • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Your amazing playlist is ready for sharing!` ✨\n\n" +
                                  $"`For others to use your playlist, instruct them to run:`\n`{ctx.Prefix}playlists load-share {ctx.User.Id} {ShareCode}`"
                }));

                File.WriteAllText($"PlaylistShares/{ctx.User.Id}/{ShareCode}.json", JsonConvert.SerializeObject(SelectedPlaylist, Formatting.Indented));
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ExportPlaylist.CustomId)
            {
                List<DiscordSelectComponentOption> Playlists = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => new DiscordSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                string SelectedPlaylistId;
                UserPlaylist SelectedPlaylist;

                try
                {
                    SelectedPlaylistId = await PromptCustomSelection(Playlists);
                    SelectedPlaylist = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == SelectedPlaylistId);
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }
                catch (Exception)
                {
                    throw;
                }

                string FileName = $"{Guid.NewGuid()}.json";
                File.WriteAllText(FileName, JsonConvert.SerializeObject(SelectedPlaylist, Formatting.Indented));
                using (FileStream fileStream = new(FileName, FileMode.Open))
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Playlists • {ctx.Guild.Name}" },
                        Color = EmbedColors.Info,
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow,
                        Description = $"`Exported your playlist '{SelectedPlaylist.PlaylistName}' to json. Please download the attached file.`"
                    }).WithFile(FileName, fileStream));
                }

                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            File.Delete(FileName);
                            return;
                        }
                        catch { }
                        await Task.Delay(1000);
                    }
                });
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == NewPlaylist.CustomId)
            {
                if (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already have 10 Playlists stored. Please delete one to create a new one.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                embed = new DiscordEmbedBuilder
                {
                    Description = $"`What do you want to name this playlist?`",
                    Color = EmbedColors.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                };

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                var PlaylistName = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                if (PlaylistName.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                _ = Task.Delay(2000).ContinueWith(_ =>
                {
                    _ = PlaylistName.Result.DeleteAsync();
                });

                await Task.Delay(1000);

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription($"`What track(s) do you want to add first to your playlist?`")));

                var FirstTrack = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                if (FirstTrack.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                _ = Task.Delay(2000).ContinueWith(_ =>
                {
                    _ = FirstTrack.Result.DeleteAsync();
                });

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();

                if (Regex.IsMatch(FirstTrack.Result.Content, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
                    throw new Exception();

                LavalinkLoadResult loadResult;

                await RespondOrEdit(new DiscordMessageBuilder()
                    .WithEmbed(embed
                        .WithDescription($":arrows_counterclockwise: `Looking for '{FirstTrack.Result.Content}'..`")
                        .WithAuthor(ctx.Guild.Name, null, Resources.StatusIndicators.DiscordCircleLoading)));

                if (Regex.IsMatch(FirstTrack.Result.Content, Resources.Regex.YouTubeUrl))
                    loadResult = await node.Rest.GetTracksAsync(FirstTrack.Result.Content, LavalinkSearchType.Plain);
                else
                    loadResult = await node.Rest.GetTracksAsync(FirstTrack.Result.Content);

                List<PlaylistItem> Tracks = new();

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
                {
                    embed.Description = $"❌ `Failed to load '{FirstTrack.Result.Content}'.`";
                    embed.Color = EmbedColors.Error;
                    await RespondOrEdit(embed.Build());
                    return;
                }
                else if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    embed.Description = $"❌ `No matches found for '{FirstTrack.Result.Content}'.`";
                    embed.Color = EmbedColors.Error;
                    await RespondOrEdit(embed.Build());
                    return;
                }
                else if (loadResult.LoadResultType is LavalinkLoadResultType.PlaylistLoaded or LavalinkLoadResultType.TrackLoaded)
                {
                    Tracks.AddRange(loadResult.Tracks.Select(x => new PlaylistItem { Title = x.Title, Url = x.Uri.ToString() }).Take(250));
                }
                else if (loadResult.LoadResultType == LavalinkLoadResultType.SearchResult)
                {
                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                    embed.Description = $"❓ `Found {loadResult.Tracks.Count()} search results. Please select the song you want to add below.`";
                    await RespondOrEdit(embed.Build());

                    string SelectedUri;

                    try
                    {
                        SelectedUri = await PromptCustomSelection(loadResult.Tracks.Select(x => new DiscordSelectComponentOption(x.Title, x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList());
                    }
                    catch (ArgumentException)
                    {
                        ModifyToTimedOut();
                        return;
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    LavalinkTrack track = loadResult.Tracks.First(x => x.Uri.ToString() == SelectedUri);

                    Tracks.Add(new PlaylistItem { Title = track.Title, Url = track.Uri.ToString() });
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Creating your playlist..`",
                    Color = EmbedColors.Loading,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));

                if (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already have 10 Playlists stored. Please delete one to create a new one.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Add(new UserPlaylist
                {
                    PlaylistName = PlaylistName.Result.Content,
                    List = Tracks
                });

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Your playlist '{PlaylistName.Result.Content}' has been created with {Tracks.Count} entries.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));
                await Task.Delay(5000);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == SaveCurrent.CustomId)
            {
                if (ctx.Member.VoiceState is null || ctx.Member.VoiceState.Channel.Id != (await ctx.Client.CurrentUser.ConvertToMember(ctx.Guild)).VoiceState?.Channel?.Id)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You aren't in the same channel as the bot.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                if (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already have 10 Playlists stored. Please delete one to create a new one.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                if (ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count <= 0)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `There is no song currently queued.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                var Tracks = ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Select(x => new PlaylistItem { Title = x.VideoTitle, Url = x.Url }).Take(250).ToList();

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`What do you want to name this playlist?`",
                    Color = EmbedColors.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));

                var PlaylistName = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                if (PlaylistName.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                _ = Task.Delay(2000).ContinueWith(_ =>
                {
                    _ = PlaylistName.Result.DeleteAsync();
                });

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Creating your playlist..`",
                    Color = EmbedColors.Loading,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));

                if (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already have 10 Playlists stored. Please delete one to create a new one.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Add(new UserPlaylist
                {
                    PlaylistName = PlaylistName.Result.Content,
                    List = Tracks
                });

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Your playlist '{PlaylistName.Result.Content}' has been created with {Tracks.Count} entries.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));
                await Task.Delay(5000);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ImportPlaylist.CustomId)
            {
                if (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already have 10 Playlists stored. Please delete one to create a new one.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Please link the playlist you want to import. Alternatively, upload an exported playlist as attachment.`",
                    Color = EmbedColors.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));

                string PlaylistName;
                string PlaylistColor = "#FFFFFF";
                List<PlaylistItem> Tracks;

                var search = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                if (search.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                _ = Task.Delay(2000).ContinueWith(_ =>
                {
                    _ = search.Result.DeleteAsync();
                });

                if (!search.Result.Attachments.Any(x => x.FileName.EndsWith(".json")))
                {
                    if (search.Result.Content.IsNullOrWhiteSpace())
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"❌ `Your message did not contain a json file or link to a playlist.`",
                            Color = EmbedColors.Error,
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = ctx.Guild.Name,
                                IconUrl = ctx.Guild.IconUrl
                            },
                            Footer = ctx.GenerateUsedByFooter(),
                            Timestamp = DateTime.UtcNow
                        }));
                        return;
                    }

                    var lava = ctx.Client.GetLavalink();
                    var node = lava.ConnectedNodes.Values.First();

                    if (Regex.IsMatch(search.Result.Content, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
                        throw new Exception();

                    LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(search.Result.Content, LavalinkSearchType.Plain);

                    if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"❌ `Couldn't load a playlist from this url.`",
                            Color = EmbedColors.Error,
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = ctx.Guild.Name,
                                IconUrl = ctx.Guild.IconUrl
                            },
                            Footer = ctx.GenerateUsedByFooter(),
                            Timestamp = DateTime.UtcNow
                        }));
                        return;
                    }
                    else if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
                    {
                        Tracks = loadResult.Tracks.Select(x => new PlaylistItem { Title = x.Title, Url = x.Uri.ToString() }).Take(250).ToList();
                        PlaylistName = loadResult.PlaylistInfo.Name;
                    }
                    else
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"❌ `The specified url doesn't lead to a playlist.`",
                            Color = EmbedColors.Error,
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = ctx.Guild.Name,
                                IconUrl = ctx.Guild.IconUrl
                            },
                            Footer = ctx.GenerateUsedByFooter(),
                            Timestamp = DateTime.UtcNow
                        }));
                        return;
                    }
                }
                else
                {
                    try
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`Importing your attachment..`",
                            Color = EmbedColors.Loading,
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = ctx.Guild.Name,
                                IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                            },
                            Footer = ctx.GenerateUsedByFooter(),
                            Timestamp = DateTime.UtcNow
                        }));

                        var attachment = search.Result.Attachments.First(x => x.FileName.EndsWith(".json"));

                        if (attachment.FileSize > 8000000)
                            throw new Exception();

                        var rawJson = await new HttpClient().GetStringAsync(attachment.Url);

                        var ImportJson = JsonConvert.DeserializeObject<UserPlaylist>((rawJson is null or "null" or "" ? "[]" : rawJson), new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });

                        ImportJson.List = ImportJson.List.Where(x => Regex.IsMatch(x.Url, Resources.Regex.Url)).Select(x => new PlaylistItem { Title = x.Title, Url = x.Url }).Take(250).ToList();

                        if (!ImportJson.List.Any())
                            throw new Exception();

                        PlaylistName = ImportJson.PlaylistName;
                        Tracks = ImportJson.List;
                        PlaylistColor = ImportJson.PlaylistColor;
                    }
                    catch (Exception ex)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"❌ `Failed to import your attachment. Is this a valid playlist?`",
                            Color = EmbedColors.Error,
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = ctx.Guild.Name,
                                IconUrl = ctx.Guild.IconUrl
                            },
                            Footer = ctx.GenerateUsedByFooter(),
                            Timestamp = DateTime.UtcNow
                        }));

                        _logger.LogError("Failed to import a playlist", ex);

                        return;
                    }
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Creating your playlist..`",
                    Color = EmbedColors.Loading,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));

                if (ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already have 10 Playlists stored. Please delete one to create a new one.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));
                    return;
                }

                ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Add(new UserPlaylist
                {
                    PlaylistName = PlaylistName,
                    List = Tracks,
                    PlaylistColor = PlaylistColor
                });

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Your playlist '{PlaylistName}' has been created with {Tracks.Count} entries.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));
                await Task.Delay(5000);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ModifyPlaylist.CustomId)
            {
                embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Playlists • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"`What playlist do you want to modify?`"
                };

                await RespondOrEdit(embed.Build());

                List<DiscordSelectComponentOption> Playlists = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => new DiscordSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                string SelectedPlaylistId;
                UserPlaylist SelectedPlaylist;

                try
                {
                    SelectedPlaylistId = await PromptCustomSelection(Playlists);
                    SelectedPlaylist = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == SelectedPlaylistId);
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }
                catch (Exception)
                {
                    throw;
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
                    LastInt = CurrentPage * 10;

                    var CurrentTracks = SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10);

                    DiscordButtonComponent NextPage = new(ButtonStyle.Primary, "NextPage", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
                    DiscordButtonComponent PreviousPage = new(ButtonStyle.Primary, "PreviousPage", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));

                    DiscordButtonComponent PlaylistName = new(ButtonStyle.Success, "ChangePlaylistName", "Change the name of this playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

                    DiscordButtonComponent ChangePlaylistColor = new(ButtonStyle.Secondary, "ChangeColor", "Change playlist color", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎨")));
                    DiscordButtonComponent ChangePlaylistThumbnail = new(ButtonStyle.Secondary, "ChangeThumbnail", "Change playlist thumbnail", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖼")));

                    DiscordButtonComponent AddSong = new(ButtonStyle.Success, "AddSong", "Add a song", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                    DiscordButtonComponent RemoveSong = new(ButtonStyle.Danger, "DeleteSong", "Remove a song", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
                    DiscordButtonComponent RemoveDuplicates = new(ButtonStyle.Secondary, "RemoveDuplicates", "Remove all duplicates", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("♻")));

                    var Description = $"**`There's currently {SelectedPlaylist.List.Count} tracks(s) in this playlist.`**\n\n";
                    Description += $"{string.Join("\n", CurrentTracks.Select(x => $"**{GetInt()}**. **[`{x.Title}`]({x.Url})** added {Formatter.Timestamp(x.AddedTime)}"))}";

                    if (SelectedPlaylist.List.Count > 0)
                        Description += $"\n\n`Page {CurrentPage + 1}/{Math.Ceiling(SelectedPlaylist.List.Count / 10.0)}`";

                    if (CurrentPage <= 0)
                        PreviousPage = PreviousPage.Disable();

                    if ((CurrentPage * 10) + 10 >= SelectedPlaylist.List.Count)
                        NextPage = NextPage.Disable();

                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                    embed.Color = (SelectedPlaylist.PlaylistColor is "#FFFFFF" or null or "" ? EmbedColors.Info : new DiscordColor(SelectedPlaylist.PlaylistColor.IsValidHexColor()));
                    embed.Title = $"Modifying your playlist: `{SelectedPlaylist.PlaylistName}`";
                    embed.Description = Description;
                    embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = (SelectedPlaylist.PlaylistThumbnail.IsNullOrWhiteSpace() ? "" : SelectedPlaylist.PlaylistThumbnail) };
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                        .AddComponents(new List<DiscordComponent> { PreviousPage, NextPage })
                        .AddComponents(new List<DiscordComponent> { AddSong, RemoveSong, RemoveDuplicates })
                        .AddComponents(new List<DiscordComponent> { PlaylistName, ChangePlaylistColor, ChangePlaylistThumbnail })
                        .AddComponents(Resources.CancelButton));

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

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            switch (e.Interaction.Data.CustomId)
                            {
                                case "AddSong":
                                {
                                    if (SelectedPlaylist.List.Count >= 250)
                                    {
                                        embed.Description = $"❌ `You already have 250 Tracks stored in this playlist. Please delete one to add a new one.`";
                                        embed.Color = EmbedColors.Error;
                                        await RespondOrEdit(embed.Build());
                                        _ = Task.Delay(5000).ContinueWith(async x =>
                                        {
                                            await UpdateMessage();
                                        });
                                        return;
                                    }

                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription($"`Please send a link to the track or playlist you want to add to this playlist.`")));

                                    var FirstTrack = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                                    if (FirstTrack.TimedOut)
                                    {
                                        ModifyToTimedOut(true);
                                        return;
                                    }

                                    _ = Task.Delay(2000).ContinueWith(_ =>
                                    {
                                        _ = FirstTrack.Result.DeleteAsync();
                                    });

                                    var lava = ctx.Client.GetLavalink();
                                    var node = lava.ConnectedNodes.Values.First();

                                    if (Regex.IsMatch(FirstTrack.Result.Content, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
                                        throw new Exception();

                                    LavalinkLoadResult loadResult;

                                    await RespondOrEdit(new DiscordMessageBuilder()
                                        .WithEmbed(embed
                                            .WithDescription($":arrows_counterclockwise: `Looking for '{FirstTrack.Result.Content}'..`")
                                            .WithAuthor(ctx.Guild.Name, null, Resources.StatusIndicators.DiscordCircleLoading)));

                                    if (Regex.IsMatch(FirstTrack.Result.Content, Resources.Regex.YouTubeUrl))
                                        loadResult = await node.Rest.GetTracksAsync(FirstTrack.Result.Content, LavalinkSearchType.Plain);
                                    else
                                        loadResult = await node.Rest.GetTracksAsync(FirstTrack.Result.Content);

                                    List<PlaylistItem> Tracks = new();

                                    switch (loadResult.LoadResultType)
                                    {
                                        case LavalinkLoadResultType.LoadFailed:
                                        {
                                            embed.Description = $"❌ `Failed to load '{FirstTrack.Result.Content}'.`";
                                            embed.Color = EmbedColors.Error;
                                            await RespondOrEdit(embed.Build());
                                            _ = Task.Delay(5000).ContinueWith(async x =>
                                            {
                                                await UpdateMessage();
                                            });
                                            return;
                                        }

                                        case LavalinkLoadResultType.NoMatches:
                                        {
                                            embed.Description = $"❌ `No matches found for '{FirstTrack.Result.Content}'.`";
                                            embed.Color = EmbedColors.Error;
                                            await RespondOrEdit(embed.Build());
                                            _ = Task.Delay(5000).ContinueWith(async x =>
                                            {
                                                await UpdateMessage();
                                            });
                                            return;
                                        }

                                        case LavalinkLoadResultType.PlaylistLoaded:
                                        case LavalinkLoadResultType.TrackLoaded:
                                        {
                                            Tracks.AddRange(loadResult.Tracks.Select(x => new PlaylistItem { Title = x.Title, Url = x.Uri.ToString() }));
                                            break;
                                        }

                                        case LavalinkLoadResultType.SearchResult:
                                        {
                                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                                            embed.Description = $"❓ `Found {loadResult.Tracks.Count()} search results. Please select the song you want to add below.`";
                                            await RespondOrEdit(embed.Build());

                                            string SelectedUri;

                                            try
                                            {
                                                SelectedUri = await PromptCustomSelection(loadResult.Tracks.Select(x => new DiscordSelectComponentOption(x.Title, x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList());
                                            }
                                            catch (ArgumentException)
                                            {
                                                ModifyToTimedOut();
                                                return;
                                            }
                                            catch (Exception)
                                            {
                                                throw;
                                            }

                                            LavalinkTrack track = loadResult.Tracks.First(x => x.Uri.ToString() == SelectedUri);

                                            Tracks.Add(new PlaylistItem { Title = track.Title, Url = track.Uri.ToString() });
                                            break;
                                        }
                                    }

                                    if (SelectedPlaylist.List.Count >= 250)
                                    {
                                        embed.Description = $"❌ `You already have 250 Tracks stored in this playlist. Please delete one to add a new one.`";
                                        embed.Color = EmbedColors.Error;
                                        await RespondOrEdit(embed.Build());
                                        _ = Task.Delay(5000).ContinueWith(async x =>
                                        {
                                            await UpdateMessage();
                                        });
                                        return;
                                    }

                                    SelectedPlaylist.List.AddRange(Tracks.Take(250 - SelectedPlaylist.List.Count));

                                    await UpdateMessage();
                                    break;
                                }
                                case "ChangeThumbnail":
                                {
                                    try
                                    {
                                        embed = new DiscordEmbedBuilder
                                        {
                                            Description = $"`Please upload a new thumbnail for your playlist.`\n\n" +
                                    $"⚠ `Please note: Playlist thumbnails are being moderated. If your thumbnail is determined to be inappropriate or otherwise harming it will be removed and you'll lose access to the entirety of Project Ichigo. This includes the bot being removed from guilds you own or manage. Please keep it safe. ♥`",
                                            Color = EmbedColors.AwaitingInput,
                                            Author = new DiscordEmbedBuilder.EmbedAuthor
                                            {
                                                Name = ctx.Guild.Name,
                                                IconUrl = ctx.Guild.IconUrl
                                            },
                                            Footer = ctx.GenerateUsedByFooter(),
                                            Timestamp = DateTime.UtcNow
                                        };

                                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                        var NewThumbnail = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                                        if (NewThumbnail.TimedOut)
                                        {
                                            ModifyToTimedOut(true);
                                            return;
                                        }

                                        embed.Description = $"`Importing your thumbnail..`";
                                        embed.Color = EmbedColors.Loading;
                                        await RespondOrEdit(embed.Build());

                                        _ = Task.Delay(8000).ContinueWith(_ =>
                                        {
                                            _ = NewThumbnail.Result.DeleteAsync();
                                        });

                                        if (!NewThumbnail.Result.Attachments.Any(x => x.FileName.EndsWith(".png") || x.FileName.EndsWith(".jpeg") || x.FileName.EndsWith(".jpg")))
                                        {
                                            embed.Description = $"❌ `Please attach an image.`";
                                            embed.Color = EmbedColors.Error;
                                            await RespondOrEdit(embed.Build());
                                            _ = Task.Delay(5000).ContinueWith(async x =>
                                            {
                                                await UpdateMessage();
                                            });
                                            return;
                                        }

                                        var attachment = NewThumbnail.Result.Attachments.First(x => x.FileName.EndsWith(".png") || x.FileName.EndsWith(".jpeg") || x.FileName.EndsWith(".jpg"));

                                        if (attachment.FileSize > 8000000)
                                        {
                                            embed.Description = $"❌ `Please attach an image below 8mb.`";
                                            embed.Color = EmbedColors.Error;
                                            await RespondOrEdit(embed.Build());
                                            _ = Task.Delay(5000).ContinueWith(async x =>
                                            {
                                                await UpdateMessage();
                                            });
                                            return;
                                        }

                                        var rawFile = await new HttpClient().GetStreamAsync(attachment.Url);

                                        var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot._status.LoadedConfig.PlaylistAssetsChannelId)).SendMessageAsync(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`\n`{SelectedPlaylist.PlaylistName}`").WithFile($"{Guid.NewGuid()}{attachment.Url.Remove(0, attachment.Url.LastIndexOf("."))}", rawFile));
                                        string url = asset.Attachments[0].Url;

                                        SelectedPlaylist.PlaylistThumbnail = url;
                                        _ = NewThumbnail.Result.DeleteAsync();
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"Failed to upload thumbnail", ex);
                                        embed.Description = $"❌ `Something went wrong while trying to upload your thumbnail. Please try again.`";
                                        embed.Color = EmbedColors.Error;
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
                                    embed = new DiscordEmbedBuilder
                                    {
                                        Description = $"`What color should this playlist be? (e.g. #FF0000)` [`Need help with hex color codes?`](https://g.co/kgs/jDHPp6)",
                                        Color = EmbedColors.AwaitingInput,
                                        Author = new DiscordEmbedBuilder.EmbedAuthor
                                        {
                                            Name = ctx.Guild.Name,
                                            IconUrl = ctx.Guild.IconUrl
                                        },
                                        Footer = ctx.GenerateUsedByFooter(),
                                        Timestamp = DateTime.UtcNow
                                    };

                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                    var ColorCode = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                                    if (ColorCode.TimedOut)
                                    {
                                        ModifyToTimedOut(true);
                                        return;
                                    }

                                    _ = Task.Delay(2000).ContinueWith(_ =>
                                    {
                                        _ = ColorCode.Result.DeleteAsync();
                                    });

                                    SelectedPlaylist.PlaylistColor = ColorCode.Result.Content;

                                    await UpdateMessage();
                                    break;
                                }
                                case "ChangePlaylistName":
                                {
                                    embed = new DiscordEmbedBuilder
                                    {
                                        Description = $"`What do you want to name this playlist?`\n\n" +
                                        $"⚠ `Please note: Playlist Names are being moderated. If your playlist name is determined to be inappropriate or otherwise harming it will be removed and you'll lose access to the entirety of Project Ichigo. This includes the bot being removed from guilds you own or manage. Please keep it safe. ♥`",
                                        Color = EmbedColors.AwaitingInput,
                                        Author = new DiscordEmbedBuilder.EmbedAuthor
                                        {
                                            Name = ctx.Guild.Name,
                                            IconUrl = ctx.Guild.IconUrl
                                        },
                                        Footer = ctx.GenerateUsedByFooter(),
                                        Timestamp = DateTime.UtcNow
                                    };

                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                    var PlaylistName = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                                    if (PlaylistName.TimedOut)
                                    {
                                        ModifyToTimedOut(true);
                                        return;
                                    }

                                    _ = Task.Delay(2000).ContinueWith(_ =>
                                    {
                                        _ = PlaylistName.Result.DeleteAsync();
                                    });

                                    SelectedPlaylist.PlaylistName = PlaylistName.Result.Content;

                                    await UpdateMessage();
                                    break;
                                }
                                case "RemoveDuplicates":
                                {
                                    CurrentPage = 0;
                                    SelectedPlaylist.List = SelectedPlaylist.List.GroupBy(x => x.Url).Select(y => y.FirstOrDefault()).ToList();
                                    await UpdateMessage();
                                    break;
                                }
                                case "DeleteSong":
                                {
                                    List<DiscordSelectComponentOption> TrackList = SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10).Select(x => new DiscordSelectComponentOption($"{x.Title}", x.Url.MakeValidFileName(), $"Added {x.AddedTime.GetTimespanSince().GetHumanReadable()} ago")).ToList();

                                    string SelectedTrackId;
                                    PlaylistItem SelectedTrack;

                                    try
                                    {
                                        SelectedTrackId = await PromptCustomSelection(TrackList);
                                        SelectedTrack = SelectedPlaylist.List.First(x => x.Url.MakeValidFileName() == SelectedTrackId);
                                    }
                                    catch (ArgumentException)
                                    {
                                        ModifyToTimedOut();
                                        return;
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                    SelectedPlaylist.List.Remove(SelectedTrack);

                                    if (SelectedPlaylist.List.Count <= 0)
                                    {
                                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                        {
                                            Description = $"`Your playlist '{SelectedPlaylist.PlaylistName}' has been deleted.`",
                                            Color = EmbedColors.Success,
                                            Author = new DiscordEmbedBuilder.EmbedAuthor
                                            {
                                                Name = ctx.Guild.Name,
                                                IconUrl = ctx.Guild.IconUrl
                                            },
                                            Footer = ctx.GenerateUsedByFooter(),
                                            Timestamp = DateTime.UtcNow
                                        }));

                                        ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Remove(SelectedPlaylist);

                                        await Task.Delay(5000);
                                        await ExecuteCommand(ctx, arguments);
                                        return;
                                    }

                                    if (!SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10).Any())
                                        CurrentPage--;

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
                                case "cancel":
                                {
                                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                    await ExecuteCommand(ctx, arguments);
                                    return;
                                }
                            }
                        }
                    }).Add(ctx.Bot._watcher, ctx);
                }
            }
            else if (e.Result.Interaction.Data.CustomId == DeletePlaylist.CustomId)
            {
                List<DiscordSelectComponentOption> Playlists = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => new DiscordSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                string SelectedPlaylistId;
                UserPlaylist SelectedPlaylist;

                try
                {
                    SelectedPlaylistId = await PromptCustomSelection(Playlists);
                    SelectedPlaylist = ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == SelectedPlaylistId);
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }
                catch (Exception)
                {
                    throw;
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Deleting your playlist '{SelectedPlaylist.PlaylistName}'..`",
                    Color = EmbedColors.Loading,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));

                ctx.Bot._users.List[ctx.Member.Id].UserPlaylists.Remove(SelectedPlaylist);

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Your playlist '{SelectedPlaylist.PlaylistName}' has been deleted.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));
                await Task.Delay(5000);
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