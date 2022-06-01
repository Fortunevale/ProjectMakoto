namespace Project_Ichigo.Commands.User;

internal class Music : BaseCommandModule
{
    public Bot _bot { private get; set; }


    [Group("music"),
    CommandModule("music"), Aliases("m"),
    Description("Allows to play music and change the current playback settings")]
    public class MusicCommands : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (ctx.Member.VoiceState is null)
            {
                _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Description = $"❌ `You aren't in a voice channel.`",
                    Color = ColorHelper.Error,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
                throw new CancelCommandException("User is not in a voice channel", ctx);
            }
        }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                if (ctx.Command.Parent is not null)
                    await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
                else
                    await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
            }).Add(_bot._watcher, ctx);
        }

        [Command("join"), Aliases("connect"), Description("Project Ichigo will join your channel if it's not already being used in the server")]
        public async Task Join(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    if (!lava.ConnectedNodes.Any())
                    {
                        throw new Exception("Lavalink connection isn't established.");
                    }

                    conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                    return;
                }

                if (conn.Channel.Users.Count >= 2 && !(ctx.Member.VoiceState.Channel.Id == conn.Channel.Id))
                {
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is already in use.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }.Build());
                    return;
                }

                if (ctx.Member.VoiceState.Channel.Id != conn.Channel.Id)
                {
                    await conn.DisconnectAsync();
                    conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                }
            }).Add(_bot._watcher, ctx);
        }

        [Command("disconnect"), Aliases("dc", "leave"), Description("Starts a voting to disconnect the bot")]
        public async Task Disconnect(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You aren't in the same channel as the bot.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Contains(ctx.User.Id))
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already voted to disconnect the bot.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Add(ctx.User.Id);

                if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                {
                    _bot._guilds.List[ctx.Guild.Id].Lavalink = new();

                    await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                    await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"✅ `The bot was disconnected.`",
                        Color = ColorHelper.Success,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Description = $"❓ `You voted to disconnect the bot. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
                    Color = ColorHelper.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                DiscordButtonComponent DisconnectVote = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Vote to disconnect", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⛔")));
                builder.AddComponents(DisconnectVote);

                var msg = await ctx.Channel.SendMessageAsync(builder);

                _ = Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        msg.ModifyToTimedOut();
                    }
                });

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Contains(e.User.Id))
                            {
                                _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ `You already voted to disconnect the bot.`").AsEphemeral());
                                return;
                            }

                            var member = await e.User.ConvertToMember(ctx.Guild);

                            if (member.VoiceState is null || member.VoiceState.Channel.Id != conn.Channel.Id)
                            {
                                _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("❌ `You aren't in the same channel as the bot.`").AsEphemeral());
                                return;
                            }

                            _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Add(e.User.Id);

                            if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                            {
                                _bot._guilds.List[ctx.Guild.Id].Lavalink = new();

                                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                {
                                    Description = $"✅ `The bot was disconnected.`",
                                    Color = ColorHelper.Success,
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

                            embed.Description = $"❓ `You voted to disconnect the bot. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                            _ = msg.ModifyAsync(embed.Build());
                        }
                    }).Add(_bot._watcher);
                }
            }).Add(_bot._watcher, ctx);
        }

        [Command("play"), Description("Searches for a video and adds it to the queue. If given a direct url, adds it to the queue.")]
        public async Task Play(CommandContext ctx, [Description("Search Query/Url")][RemainingText]string search)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                if (Regex.IsMatch(search, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
                    throw new Exception();

                var embed = new DiscordEmbedBuilder
                {
                    Description = $":arrows_counterclockwise: `Preparing connection..`",
                    Color = ColorHelper.Processing,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                };
                var msg = await ctx.Channel.SendMessageAsync(embed);

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    if (!lava.ConnectedNodes.Any())
                    {
                        throw new Exception("Lavalink connection isn't established.");
                    }

                    conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                }

                if (conn.Channel.Users.Count >= 2 && !(ctx.Member.VoiceState.Channel.Id == conn.Channel.Id))
                {
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is already in use.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }.Build());
                    return;
                }

                if (ctx.Member.VoiceState.Channel.Id != conn.Channel.Id)
                {
                    await conn.DisconnectAsync();
                    conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                }

                embed.Description = $":arrows_counterclockwise: `Looking for '{search}'..`";
                await msg.ModifyAsync(embed.Build());

                embed.Author.IconUrl = ctx.Guild.IconUrl;

                LavalinkLoadResult loadResult;

                if (Regex.IsMatch(search, Resources.Regex.YouTubeUrl))
                    loadResult = await node.Rest.GetTracksAsync(search, LavalinkSearchType.Plain);
                else
                    loadResult = await node.Rest.GetTracksAsync(search);

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
                {
                    embed.Description = $"❌ `Failed to load '{search}'.`";
                    embed.Color = ColorHelper.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    _ = Task.Delay(5000).ContinueWith(x =>
                    {
                        _ = msg.DeleteAsync();
                    });
                    return;
                }
                else if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    embed.Description = $"❌ `No matches found for '{search}'.`";
                    embed.Color = ColorHelper.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    _ = Task.Delay(5000).ContinueWith(x =>
                    {
                        _ = msg.DeleteAsync();
                    });
                    return;
                }
                else if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
                {
                    int added = 0;

                    foreach (var b in loadResult.Tracks)
                    {
                        added++;
                        _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Add(new(b.Title, b.Uri.ToString(), ctx.Guild, ctx.User));
                    }

                    embed.Description = $"✅ `Queued {added} songs from '{loadResult.PlaylistInfo.Name}'.`";

                    embed.AddField(new DiscordEmbedField($"📜 Queue positions", $"{(_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count - added + 1)} - {_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}", true));

                    embed.Color = ColorHelper.Success;
                    _ = msg.ModifyAsync(embed.Build());
                }
                else if (loadResult.LoadResultType == LavalinkLoadResultType.TrackLoaded)
                {
                    LavalinkTrack track = loadResult.Tracks.First();

                    _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Add(new(track.Title, track.Uri.ToString(), ctx.Guild, ctx.User));

                    embed.Description = $"✅ `Queued '{track.Title}'.`";

                    embed.AddField(new DiscordEmbedField($"📜 Queue position", $"{_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}", true));
                    embed.AddField(new DiscordEmbedField($"🔼 Uploaded by", $"{track.Author}", true));
                    embed.AddField(new DiscordEmbedField($"🕒 Duration", $"{track.Length.GetHumanReadable(TimeFormat.MINUTES)}", true));

                    embed.Color = ColorHelper.Success;
                    _ = msg.ModifyAsync(embed.Build());
                }
                else
                {
                    throw new Exception("Unknown Load Result Type.");
                }

            }).Add(_bot._watcher, ctx);
        }
        
        [Command("queue"), Description("Displays the current queue")]
        public async Task Queue(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You aren't in the same channel as the bot.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                int LastInt = 0;
                int GetInt()
                {
                    LastInt++;
                    return LastInt;
                }

                var Description = $"**`There's currently {_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count} song(s) queued.`**\n\n";
                Description += $"{string.Join("\n", _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Select(x => $"**{GetInt()}**. `{x.VideoTitle}` requested by {x.user.Mention}"))}\n\n";
                Description += $"`Currently playing:` `{(conn.CurrentState.CurrentTrack is not null ? conn.CurrentState.CurrentTrack.Title : "No song is playing")}`\n";
                Description += $"{(_bot._guilds.List[ctx.Guild.Id].Lavalink.Repeat ? "🔁" : "<:disabledrepeat:981594645165408286>")}";
                Description += $"{(_bot._guilds.List[ctx.Guild.Id].Lavalink.Shuffle ? "🔀" : "<:disabledshuffle:981594650018209863>")}";
                Description += $" `|` {(_bot._guilds.List[ctx.Guild.Id].Lavalink.IsPaused ? "<a:paused:981594656435490836>" : $"{(conn.CurrentState.CurrentTrack is not null ? "▶" : "<:disabledplay:981594639440154744>")} ")}";

                if (conn.CurrentState.CurrentTrack is not null)
                {
                    Description += $"`[{((long)Math.Round(conn.CurrentState.PlaybackPosition.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.MINUTES)}/{((long)Math.Round(conn.CurrentState.CurrentTrack.Length.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.MINUTES)}]` ";
                    Description += $"`{GenerateASCIIProgressbar(Math.Round(conn.CurrentState.PlaybackPosition.TotalSeconds, 0), Math.Round(conn.CurrentState.CurrentTrack.Length.TotalSeconds, 0))}`"; 
                }
                
                
                var msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Aquamarine,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = Description,
                });

                
            }).Add(_bot._watcher, ctx);
        }

        [Command("skip"), Description("Starts a voting to skip the current song")]
        public async Task Skip(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You aren't in the same channel as the bot.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Contains(ctx.User.Id))
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already voted to skip the current song.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Add(ctx.User.Id);

                if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                {
                    await conn.StopAsync();

                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"✅ `The song was skipped.`",
                        Color = ColorHelper.Success,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Description = $"❓ `You voted to skip the current song. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
                    Color = ColorHelper.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                DiscordButtonComponent SkipSongVote = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Vote to skip the current song", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏩")));
                builder.AddComponents(SkipSongVote);

                var msg = await ctx.Channel.SendMessageAsync(builder);

                _ = Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        msg.ModifyToTimedOut();
                    }
                });

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Contains(e.User.Id))
                            {
                                _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ `You already voted to skip the current song.`").AsEphemeral());
                                return;
                            }

                            var member = await e.User.ConvertToMember(ctx.Guild);

                            if (member.VoiceState is null || member.VoiceState.Channel.Id != conn.Channel.Id)
                            {
                                _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("❌ `You aren't in the same channel as the bot.`").AsEphemeral());
                                return;
                            }

                            _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Add(e.User.Id);

                            if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                            {
                                _bot._guilds.List[ctx.Guild.Id].Lavalink = new();

                                await conn.StopAsync();

                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                {
                                    Description = $"✅ `The song was skipped.`",
                                    Color = ColorHelper.Success,
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

                            embed.Description = $"❓ `You voted to skip the current song. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                            _ = msg.ModifyAsync(embed.Build());
                        }
                    }).Add(_bot._watcher);
                }
            }).Add(_bot._watcher, ctx);
        }

        [Command("clearqueue"), Aliases("cq"), Description("Starts a voting to clear the current queue")]
        public async Task ClearQueue(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You aren't in the same channel as the bot.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Contains(ctx.User.Id))
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You already voted to clear the current queue.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Add(ctx.User.Id);

                if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                {
                    _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Clear();
                    _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Clear();

                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"✅ `The queue was cleared.`",
                        Color = ColorHelper.Success,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Description = $"❓ `You voted to clear the current queue. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
                    Color = ColorHelper.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                DiscordButtonComponent DisconnectVote = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Vote to clear the current queue", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
                builder.AddComponents(DisconnectVote);

                var msg = await ctx.Channel.SendMessageAsync(builder);

                _ = Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        msg.ModifyToTimedOut();
                    }
                });

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Message.Id == msg.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Contains(e.User.Id))
                            {
                                _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ `You already voted to clear the current queue.`").AsEphemeral());
                                return;
                            }

                            var member = await e.User.ConvertToMember(ctx.Guild);

                            if (member.VoiceState is null || member.VoiceState.Channel.Id != conn.Channel.Id)
                            {
                                _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("❌ `You aren't in the same channel as the bot.`").AsEphemeral());
                                return;
                            }

                            _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Add(e.User.Id);

                            if (_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                            {
                                _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Clear();
                                _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Clear();

                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                {
                                    Description = $"✅ `The queue was cleared.`",
                                    Color = ColorHelper.Success,
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

                            embed.Description = $"❓ `You voted to clear the current queue. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                            _ = msg.ModifyAsync(embed.Build());
                        }
                    }).Add(_bot._watcher);
                }
            }).Add(_bot._watcher, ctx);
        }

        [Command("shuffle"), Description("Toggles shuffling of the current queue")]
        public async Task Shuffle(CommandContext ctx)
        {
            _ = Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You aren't in the same channel as the bot.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                _bot._guilds.List[ctx.User.Id].Lavalink.Shuffle = !_bot._guilds.List[ctx.User.Id].Lavalink.Shuffle;

                _ = ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = (_bot._guilds.List[ctx.User.Id].Lavalink.Shuffle ? "✅ `The queue now shuffles.`" : "✅ `The queue no longer shuffles.`"),
                    Color = ColorHelper.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                })
                .ContinueWith(msg =>
                {
                    if (msg.IsCompletedSuccessfully)
                        _ = Task.Delay(5000).ContinueWith(_ => { _ = msg.Result.DeleteAsync(); });
                });
            });
        }

        [Command("repeat"), Description("Toggles repeating the current queue")]
        public async Task Repeat(CommandContext ctx)
        {
            _ = Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You aren't in the same channel as the bot.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                _bot._guilds.List[ctx.User.Id].Lavalink.Repeat = !_bot._guilds.List[ctx.User.Id].Lavalink.Repeat;

                _ = ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = (_bot._guilds.List[ctx.User.Id].Lavalink.Shuffle ? "✅ `The queue now repeats itself.`" : "✅ `The queue no longer repeats itself.`"),
                    Color = ColorHelper.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                })
                .ContinueWith(msg =>
                {
                    if (msg.IsCompletedSuccessfully)
                        _ = Task.Delay(5000).ContinueWith(_ => { _ = msg.Result.DeleteAsync(); });
                });
            });
        }
        
        [Command("pause"), Description("Pause or unpause the current song")]
        public async Task Pause(CommandContext ctx)
        {
            _ = Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You aren't in the same channel as the bot.`",
                        Color = ColorHelper.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                _bot._guilds.List[ctx.User.Id].Lavalink.IsPaused = !_bot._guilds.List[ctx.User.Id].Lavalink.IsPaused;

                if (_bot._guilds.List[ctx.User.Id].Lavalink.IsPaused)
                    _ = conn.PauseAsync();
                else
                    _ = conn.ResumeAsync();

                _ = ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = (_bot._guilds.List[ctx.User.Id].Lavalink.IsPaused ? "✅ `Paused playback.`" : "✅ `Resumed playback.`"),
                    Color = ColorHelper.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                })
                .ContinueWith(msg =>
                {
                    if (msg.IsCompletedSuccessfully)
                        _ = Task.Delay(5000).ContinueWith(_ => { _ = msg.Result.DeleteAsync(); });
                });
            });
        }
    }
}
