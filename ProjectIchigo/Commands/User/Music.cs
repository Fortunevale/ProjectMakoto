namespace ProjectIchigo.Commands.User;

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
                    Color = EmbedColors.Error,
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
                    _bot._guilds.List[ctx.Guild.Id].Lavalink.QueueHandler(_bot, ctx.Client, node, conn);
                    return;
                }

                if (conn.Channel.Users.Count >= 2 && !(ctx.Member.VoiceState.Channel.Id == conn.Channel.Id))
                {
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is already in use.`",
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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
                    _bot._guilds.List[ctx.Guild.Id].Lavalink.Dispose(_bot, ctx.Guild.Id);
                    _bot._guilds.List[ctx.Guild.Id].Lavalink = new();

                    await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                    await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"✅ `The bot was disconnected.`",
                        Color = EmbedColors.Success,
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

                DiscordEmbedBuilder embed = new()
                {
                    Description = $"❓ `You voted to disconnect the bot. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
                    Color = EmbedColors.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                DiscordButtonComponent DisconnectVote = new(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Vote to disconnect", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⛔")));
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
                                _bot._guilds.List[ctx.Guild.Id].Lavalink.Dispose(_bot, ctx.Guild.Id);
                                _bot._guilds.List[ctx.Guild.Id].Lavalink = new();

                                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                {
                                    Description = $"✅ `The bot was disconnected.`",
                                    Color = EmbedColors.Success,
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

        [Command("forcedisconnect"), Aliases("fdc", "forceleave", "fleave", "stop"), Description("Forces the bot to disconnect from the current channel")]
        public async Task ForceDisconnect(CommandContext ctx)
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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

                if (!ctx.Member.IsDJ(_bot._status))
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You need Administrator Permissions or a role called 'DJ' to utilize this command.`",
                        Color = EmbedColors.Error,
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

                _bot._guilds.List[ctx.Guild.Id].Lavalink.Dispose(_bot, ctx.Guild.Id);
                _bot._guilds.List[ctx.Guild.Id].Lavalink = new();

                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Description = $"✅ `The bot was force disconnected.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("play"), Description("Searches for a video and adds it to the queue. If given a direct url, adds it to the queue.")]
        public async Task Play(CommandContext ctx, [Description("Search Query/Url")][RemainingText]string search)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                if (string.IsNullOrWhiteSpace(search))
                {
                    _ = ctx.SendSyntaxError();
                    return;
                }

                if (Regex.IsMatch(search, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
                    throw new Exception();

                var embed = new DiscordEmbedBuilder
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
                    _bot._guilds.List[ctx.Guild.Id].Lavalink.QueueHandler(_bot, ctx.Client, node, conn);
                }

                if (conn.Channel.Users.Count >= 2 && !(ctx.Member.VoiceState.Channel.Id == conn.Channel.Id))
                {
                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is already in use.`",
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
                    embed.Color = EmbedColors.Error;
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
                    embed.Color = EmbedColors.Error;
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

                    embed.Color = EmbedColors.Success;
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

                    embed.Color = EmbedColors.Success;
                    _ = msg.ModifyAsync(embed.Build());
                }
                else if (loadResult.LoadResultType == LavalinkLoadResultType.SearchResult)
                {
                    embed.Description = $"❓ `Found {loadResult.Tracks.Count()} search results. Please select the song you want to add below.`";
                    await msg.ModifyAsync(embed.Build());

                    string SelectedUri;

                    try
                    {
                        SelectedUri = await GenericSelectors.PromptCustomSelection(_bot, loadResult.Tracks.Select(x => new DiscordSelectComponentOption(x.Title, x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList(), ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut();
                        return;
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    LavalinkTrack track = loadResult.Tracks.First(x => x.Uri.ToString() == SelectedUri);

                    _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Add(new(track.Title, track.Uri.ToString(), ctx.Guild, ctx.User));

                    embed.Description = $"✅ `Queued '{track.Title}'.`";

                    embed.AddField(new DiscordEmbedField($"📜 Queue position", $"{_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}", true));
                    embed.AddField(new DiscordEmbedField($"🔼 Uploaded by", $"{track.Author}", true));
                    embed.AddField(new DiscordEmbedField($"🕒 Duration", $"{track.Length.GetHumanReadable(TimeFormat.MINUTES)}", true));

                    embed.Color = EmbedColors.Success;
                    _ = msg.ModifyAsync(embed.Build());
                }
                else
                {
                    throw new Exception($"Unknown Load Result Type: {loadResult.LoadResultType}");
                }

            }).Add(_bot._watcher, ctx);
        }

        [Command("pause"), Description("Pause or unpause the current song")]
        public async Task Pause(CommandContext ctx)
        {
            _ = Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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
                    Color = EmbedColors.Success,
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

        [Command("queue"), Description("Displays the current queue")]
        public async Task Queue(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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

                int CurrentPage = 0;

                async Task<DiscordMessage> UpdateMessage(DiscordMessage msg)
                {
                    DiscordButtonComponent Refresh = new(ButtonStyle.Primary, "Refresh", "Refresh", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔁")));

                    DiscordButtonComponent NextPage = new(ButtonStyle.Primary, "NextPage", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
                    DiscordButtonComponent PreviousPage = new(ButtonStyle.Primary, "PreviousPage", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));

                    if (msg is null)
                    {
                        msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Color = EmbedColors.Success,
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = ctx.Guild.Name,
                                IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                            },
                            Footer = ctx.GenerateUsedByFooter(),
                            Timestamp = DateTime.UtcNow,
                            Description = "`Loading..`"
                        }));
                    }

                    LastInt = CurrentPage * 10;

                    var Description = $"**`There's currently {_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count} song(s) queued.`**\n\n";
                    Description += $"{string.Join("\n", _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Skip(CurrentPage * 10).Take(10).Select(x => $"**{GetInt()}**. `{x.VideoTitle}` requested by {x.user.Mention}"))}\n\n";
                    
                    if (_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count > 0)
                        Description += $"`Page {CurrentPage + 1}/{Math.Ceiling(_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count / 10.0)}`\n\n";
                    
                    Description += $"`Currently playing:` `{(conn.CurrentState.CurrentTrack is not null ? conn.CurrentState.CurrentTrack.Title : "No song is playing")}`\n";
                    Description += $"{(_bot._guilds.List[ctx.Guild.Id].Lavalink.Repeat ? "🔁" : "<:disabledrepeat:981594645165408286>")}";
                    Description += $"{(_bot._guilds.List[ctx.Guild.Id].Lavalink.Shuffle ? "🔀" : "<:disabledshuffle:981594650018209863>")}";
                    Description += $" `|` {(_bot._guilds.List[ctx.Guild.Id].Lavalink.IsPaused ? "<a:paused:981594656435490836>" : $"{(conn.CurrentState.CurrentTrack is not null ? "▶" : "<:disabledplay:981594639440154744>")} ")}";

                    if (conn.CurrentState.CurrentTrack is not null)
                    {
                        Description += $"`[{((long)Math.Round(conn.CurrentState.PlaybackPosition.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.MINUTES)}/{((long)Math.Round(conn.CurrentState.CurrentTrack.Length.TotalSeconds, 0)).GetShortHumanReadable(TimeFormat.MINUTES)}]` ";
                        Description += $"`{GenerateASCIIProgressbar(Math.Round(conn.CurrentState.PlaybackPosition.TotalSeconds, 0), Math.Round(conn.CurrentState.CurrentTrack.Length.TotalSeconds, 0))}`";
                    }

                    if (CurrentPage <= 0)
                        PreviousPage = PreviousPage.Disable();

                    if ((CurrentPage * 10) + 10 >= _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count)
                        NextPage = NextPage.Disable();

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Color = EmbedColors.Success,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow,
                        Description = Description
                    }).AddComponents(Refresh).AddComponents(new List<DiscordComponent> { PreviousPage, NextPage }));

                    return msg;
                }

                var msg = await UpdateMessage(null);

                _ = Task.Delay(120000).ContinueWith(x =>
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
                        if (e.Message?.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            switch (e.Interaction.Data.CustomId)
                            {
                                case "Refresh":
                                {
                                    msg = await UpdateMessage(msg);
                                    break;
                                }                                
                                case "NextPage":
                                {
                                    CurrentPage++;
                                    msg = await UpdateMessage(msg);
                                    break;
                                }                                
                                case "PreviousPage":
                                {
                                    CurrentPage--;
                                    msg = await UpdateMessage(msg);
                                    break;
                                }
                            }
                        }
                    }).Add(_bot._watcher, ctx);
                }
            }).Add(_bot._watcher, ctx);
        }
        
        [Command("removequeue"), Aliases("rq"), Description("Remove a song from the queue")]
        public async Task RemoveQueue(CommandContext ctx, [Description("Index/Video Title")][RemainingText]string selection)
        {
            Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(selection))
                {
                    _ = ctx.SendSyntaxError();
                    return;
                }

                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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

                Lavalink.QueueInfo info = null;

                if (selection.IsDigitsOnly())
                {
                    int Index = Convert.ToInt32(selection) - 1;

                    if (Index < 0 || Index >= _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count)
                    {
                        _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Description = $"❌ `Your value is out of range. Currently, the range is 1-{_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}.`",
                            Color = EmbedColors.Error,
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

                    info = _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue[Index];
                }
                else
                {
                    if (!_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Any(x => x.VideoTitle.ToLower() == selection.ToLower()))
                    {
                        _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Description = $"❌ `There is no such song queued.`",
                            Color = EmbedColors.Error,
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

                    info = _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.First(x => x.VideoTitle.ToLower() == selection.ToLower());
                }

                if (info is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `There is no such song queued.`",
                        Color = EmbedColors.Error,
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

                _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Remove(info);

                _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Description = $"✅ `Removed '{info.VideoTitle}' from the current queue.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Success,
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

                DiscordEmbedBuilder embed = new()
                {
                    Description = $"❓ `You voted to skip the current song. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
                    Color = EmbedColors.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                DiscordButtonComponent SkipSongVote = new(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Vote to skip the current song", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏩")));
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
                                await conn.StopAsync();

                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                {
                                    Description = $"✅ `The song was skipped.`",
                                    Color = EmbedColors.Success,
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

                            embed.Description = $"❓ `You voted to skip the current song. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedSkips.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                            _ = msg.ModifyAsync(embed.Build());
                        }
                    }).Add(_bot._watcher);
                }
            }).Add(_bot._watcher, ctx);
        }

        [Command("forceskip"), Aliases("fs", "fskip"), Description("Forces skipping of the current song. You need to be an Administrator or have a role called `DJ`.")]
        public async Task ForceSkip(CommandContext ctx)
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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

                if (!ctx.Member.IsDJ(_bot._status))
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You need Administrator Permissions or a role called 'DJ' to utilize this command.`",
                        Color = EmbedColors.Error,
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

                await conn.StopAsync();

                _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Description = $"✅ `The song was force skipped.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("clearqueue"), Aliases("cq"), Description("Starts a voting to clear the current queue")]
        public async Task ClearQueue(CommandContext ctx)
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Success,
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

                DiscordEmbedBuilder embed = new()
                {
                    Description = $"❓ `You voted to clear the current queue. ({_bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
                    Color = EmbedColors.AwaitingInput,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter()
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                DiscordButtonComponent DisconnectVote = new(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Vote to clear the current queue", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
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
                                    Color = EmbedColors.Success,
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
        
        [Command("forceclearqueue"), Aliases("fcq"), Description("Forces clearing the current queue. You need to be an Administrator or have a role called `DJ`.")]
        public async Task ForceClearQueue(CommandContext ctx)
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
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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

                if (!ctx.Member.IsDJ(_bot._status))
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `You need Administrator Permissions or a role called 'DJ' to utilize this command.`",
                        Color = EmbedColors.Error,
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

                _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Clear();
                _bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Clear();

                _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Description = $"✅ `The queue was force cleared.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
            }).Add(_bot._watcher, ctx);
        }

        [Command("shuffle"), Description("Toggles shuffling of the current queue")]
        public async Task Shuffle(CommandContext ctx)
        {
            _ = Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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

                _bot._guilds.List[ctx.Guild.Id].Lavalink.Shuffle = !_bot._guilds.List[ctx.Guild.Id].Lavalink.Shuffle;

                _ = ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = (_bot._guilds.List[ctx.Guild.Id].Lavalink.Shuffle ? "✅ `The queue now shuffles.`" : "✅ `The queue no longer shuffles.`"),
                    Color = EmbedColors.Success,
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
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First();
                var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

                if (conn is null)
                {
                    _ = ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Description = $"❌ `The bot is not in a voice channel.`",
                        Color = EmbedColors.Error,
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
                        Color = EmbedColors.Error,
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

                _bot._guilds.List[ctx.Guild.Id].Lavalink.Repeat = !_bot._guilds.List[ctx.Guild.Id].Lavalink.Repeat;

                _ = ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = (_bot._guilds.List[ctx.Guild.Id].Lavalink.Repeat ? "✅ `The queue now repeats itself.`" : "✅ `The queue no longer repeats itself.`"),
                    Color = EmbedColors.Success,
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

    [Group("playlists"), Aliases("playlist", "pl"),
    CommandModule("music"), 
    Description("Allows managing your personal playlists")]
    public class Playlists : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        //[GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        //public async Task Help(CommandContext ctx)
        //{
        //    Task.Run(async () =>
        //    {
        //        if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
        //            return;

        //        if (ctx.Command.Parent is not null)
        //            await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx);
        //        else
        //            await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx);
        //    }).Add(_bot._watcher, ctx);
        //}

        [GroupCommand, Command("manage"), Description("Allows to review and manage your playlists")]
        public async Task Manage(CommandContext ctx)
        {
            Task.Run(async () =>
            {
                if (!_bot._users.List.ContainsKey(ctx.User.Id))
                    _bot._users.List.Add(ctx.User.Id, new Users.Info(_bot));

                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                    return;

                DiscordEmbedBuilder embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Playlists • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"{(_bot._users.List[ctx.Member.Id].UserPlaylists.Count > 0 ? string.Join("\n", _bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => $"`{x.PlaylistName.SanitizeForCodeBlock()}`: `{x.List.Count} track(s)`")) : $"`No playlist created yet.`")}"
                };

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                var AddToQueue = new DiscordButtonComponent(ButtonStyle.Success , Guid.NewGuid().ToString(), "Add a playlist to the current queue", (_bot._users.List[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📤")));

                var ImportPlaylist = new DiscordButtonComponent(ButtonStyle.Success , Guid.NewGuid().ToString(), "Import a playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📥")));
                var SaveCurrent = new DiscordButtonComponent(ButtonStyle.Success , Guid.NewGuid().ToString(), "Save current queue as playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💾")));
                var NewPlaylist = new DiscordButtonComponent(ButtonStyle.Success , Guid.NewGuid().ToString(), "Create new playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var ModifyPlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Select a playlist to modify", (_bot._users.List[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚙")));
                var DeletePlaylist = new DiscordButtonComponent(ButtonStyle.Danger , Guid.NewGuid().ToString(), "Select a playlist to delete", (_bot._users.List[ctx.Member.Id].UserPlaylists.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));

                var msg = await ctx.Channel.SendMessageAsync(builder
                .AddComponents(AddToQueue)
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

                var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(1));

                if (e.TimedOut)
                {
                    msg.ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Result.Interaction.Data.CustomId == AddToQueue.CustomId)
                {
                    List<DiscordSelectComponentOption> Playlists = _bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => new DiscordSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                    string SelectedPlaylistId;
                    UserPlaylist SelectedPlaylist;

                    try
                    {
                        SelectedPlaylistId = await GenericSelectors.PromptCustomSelection(_bot, Playlists, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                        SelectedPlaylist = _bot._users.List[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == SelectedPlaylistId);
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut();
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
                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

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
                        _bot._guilds.List[ctx.Guild.Id].Lavalink.QueueHandler(_bot, ctx.Client, node, conn);
                    }

                    if (conn.Channel.Users.Count >= 2 && !(ctx.Member.VoiceState.Channel.Id == conn.Channel.Id))
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"❌ `The bot is already in use.`",
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

                    if (ctx.Member.VoiceState.Channel.Id != conn.Channel.Id)
                    {
                        await conn.DisconnectAsync();
                        conn = await node.ConnectAsync(ctx.Member.VoiceState.Channel);
                    }

                    embed.Description = $":arrows_counterclockwise: `Adding '{SelectedPlaylist.PlaylistName}' with {SelectedPlaylist.List.Count} track(s) to the queue..`";
                    await msg.ModifyAsync(embed.Build());

                    _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.AddRange(SelectedPlaylist.List.Select(x => new Lavalink.QueueInfo(x.Title, x.Url, ctx.Guild, ctx.User)));

                    embed.Description = $"✅ `Queued {SelectedPlaylist.List.Count} songs from your personal playlist '{SelectedPlaylist.PlaylistName}'.`";

                    embed.AddField(new DiscordEmbedField($"📜 Queue positions", $"{(_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count - SelectedPlaylist.List.Count + 1)} - {_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count}", true));

                    embed.Color = EmbedColors.Success;
                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                    _ = msg.ModifyAsync(embed.Build());
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == NewPlaylist.CustomId)
                {
                    if (_bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                    var PlaylistName = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                    if (PlaylistName.TimedOut)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    _ = Task.Delay(2000).ContinueWith(_ =>
                    {
                        _ = PlaylistName.Result.DeleteAsync();
                    });

                    await Task.Delay(1000);

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed.WithDescription($"`What track(s) do you want to add first to your playlist?`")));

                    var FirstTrack = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                    if (FirstTrack.TimedOut)
                    {
                        msg.ModifyToTimedOut(true);
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

                    await msg.ModifyAsync(new DiscordMessageBuilder()
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
                        _ = msg.ModifyAsync(embed.Build());
                        _ = Task.Delay(5000).ContinueWith(x =>
                        {
                            _ = msg.DeleteAsync();
                        });
                        return;
                    }
                    else if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                    {
                        embed.Description = $"❌ `No matches found for '{FirstTrack.Result.Content}'.`";
                        embed.Color = EmbedColors.Error;
                        _ = msg.ModifyAsync(embed.Build());
                        _ = Task.Delay(5000).ContinueWith(x =>
                        {
                            _ = msg.DeleteAsync();
                        });
                        return;
                    }
                    else if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded || loadResult.LoadResultType == LavalinkLoadResultType.TrackLoaded)
                    {
                        Tracks.AddRange(loadResult.Tracks.Select(x => new PlaylistItem { Title = x.Title, Url = x.Uri.ToString() }).Take(250));
                    }
                    else if (loadResult.LoadResultType == LavalinkLoadResultType.SearchResult)
                    {
                        embed.Author.IconUrl = ctx.Guild.IconUrl;
                        embed.Description = $"❓ `Found {loadResult.Tracks.Count()} search results. Please select the song you want to add below.`";
                        await msg.ModifyAsync(embed.Build());

                        string SelectedUri;

                        try
                        {
                            SelectedUri = await GenericSelectors.PromptCustomSelection(_bot, loadResult.Tracks.Select(x => new DiscordSelectComponentOption(x.Title, x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList(), ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                        }
                        catch (ArgumentException)
                        {
                            msg.ModifyToTimedOut();
                            return;
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        LavalinkTrack track = loadResult.Tracks.First(x => x.Uri.ToString() == SelectedUri);

                        Tracks.Add(new PlaylistItem { Title = track.Title, Url = track.Uri.ToString() });
                    }

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    if (_bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    _bot._users.List[ctx.Member.Id].UserPlaylists.Add(new UserPlaylist
                    {
                        PlaylistName = PlaylistName.Result.Content,
                        List = Tracks
                    });

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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
                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == SaveCurrent.CustomId)
                {
                    if (ctx.Member.VoiceState is null || ctx.Member.VoiceState.Channel.Id != (await ctx.Client.CurrentUser.ConvertToMember(ctx.Guild)).VoiceState?.Channel?.Id)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    if (_bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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
                    
                    if (_bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Count <= 0)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    var Tracks = _bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Select(x => new PlaylistItem { Title = x.VideoTitle, Url = x.Url }).Take(250).ToList();

                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    _ = Task.Delay(2000).ContinueWith(_ =>
                    {
                        _ = PlaylistName.Result.DeleteAsync();
                    });

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    if (_bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    _bot._users.List[ctx.Member.Id].UserPlaylists.Add(new UserPlaylist
                    {
                        PlaylistName = PlaylistName.Result.Content,
                        List = Tracks
                    });

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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
                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                    return;
                }
                else if (e.Result.Interaction.Data.CustomId == ImportPlaylist.CustomId)
                {
                    if (_bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`What is the URL from the playlist you want to import?`",
                        Color = EmbedColors.AwaitingInput,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow
                    }));

                    var lava = ctx.Client.GetLavalink();
                    var node = lava.ConnectedNodes.Values.First();

                    var search = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                    if (search.TimedOut)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    _ = Task.Delay(2000).ContinueWith(_ =>
                    {
                        _ = search.Result.DeleteAsync();
                    });

                    if (Regex.IsMatch(search.Result.Content, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
                        throw new Exception();

                    LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(search.Result.Content, LavalinkSearchType.Plain);

                    if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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
                        var Tracks = loadResult.Tracks.Select(x => new PlaylistItem { Title = x.Title, Url = x.Uri.ToString() }).Take(250).ToList();

                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                        if (_bot._users.List[ctx.Member.Id].UserPlaylists.Count >= 10)
                        {
                            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                        _bot._users.List[ctx.Member.Id].UserPlaylists.Add(new UserPlaylist
                        {
                            PlaylistName = loadResult.PlaylistInfo.Name,
                            List = Tracks
                        });

                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`Your playlist '{loadResult.PlaylistInfo.Name}' has been created with {Tracks.Count} entries.`",
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
                        _ = msg.DeleteAsync();
                        _ = ctx.Command.ExecuteAsync(ctx);
                        return;
                    }
                    else
                    {
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    msg = await msg.ModifyAsync(embed.Build());

                    List<DiscordSelectComponentOption> Playlists = _bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => new DiscordSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                    string SelectedPlaylistId;
                    UserPlaylist SelectedPlaylist;

                    try
                    {
                        SelectedPlaylistId = await GenericSelectors.PromptCustomSelection(_bot, Playlists, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                        SelectedPlaylist = _bot._users.List[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == SelectedPlaylistId);
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut();
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

                    async Task<DiscordMessage> UpdateMessage(DiscordMessage msg)
                    {
                        LastInt = CurrentPage * 10;

                        var CurrentTracks = SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10);

                        DiscordButtonComponent NextPage = new(ButtonStyle.Primary, "NextPage", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));
                        DiscordButtonComponent PreviousPage = new(ButtonStyle.Primary, "PreviousPage", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));

                        DiscordButtonComponent PlaylistName = new(ButtonStyle.Success, "ChangePlaylistName", "Change the name of this playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

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
                        embed.Color = EmbedColors.Info;
                        embed.Title = $"Modifying your playlist: `{SelectedPlaylist.PlaylistName}`";
                        embed.Description = Description;
                        msg = await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { PreviousPage, NextPage }).AddComponents(new List<DiscordComponent> { PlaylistName, AddSong, RemoveSong, RemoveDuplicates }).AddComponents(Resources.CancelButton));

                        return msg;
                    }

                    await UpdateMessage(msg);

                    CancellationTokenSource tokenSource = new();

                    _ = Task.Delay(120000, tokenSource.Token).ContinueWith(x =>
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
                            if (e.Message?.Id == msg.Id && e.User.Id == ctx.User.Id)
                            {
                                tokenSource.Cancel();
                                tokenSource = new();

                                _ = Task.Delay(120000, tokenSource.Token).ContinueWith(x =>
                                {
                                    if (x.IsCompletedSuccessfully)
                                    {
                                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                        msg.ModifyToTimedOut();
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
                                            _ = msg.ModifyAsync(embed.Build());
                                            _ = Task.Delay(5000).ContinueWith(async x =>
                                            {
                                                msg = await UpdateMessage(msg);
                                            });
                                            return;
                                        }

                                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed.WithDescription($"`Please send a link to the track or playlist you want to add to this playlist.`")));

                                        var FirstTrack = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                                        if (FirstTrack.TimedOut)
                                        {
                                            msg.ModifyToTimedOut(true);
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

                                        await msg.ModifyAsync(new DiscordMessageBuilder()
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
                                                _ = msg.ModifyAsync(embed.Build());
                                                _ = Task.Delay(5000).ContinueWith(async x =>
                                                {
                                                    msg = await UpdateMessage(msg);
                                                });
                                                return;
                                            }

                                            case LavalinkLoadResultType.NoMatches:
                                            {
                                                embed.Description = $"❌ `No matches found for '{FirstTrack.Result.Content}'.`";
                                                embed.Color = EmbedColors.Error;
                                                _ = msg.ModifyAsync(embed.Build());
                                                _ = Task.Delay(5000).ContinueWith(async x =>
                                                {
                                                    msg = await UpdateMessage(msg);
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
                                                await msg.ModifyAsync(embed.Build());

                                                string SelectedUri;

                                                try
                                                {
                                                    SelectedUri = await GenericSelectors.PromptCustomSelection(_bot, loadResult.Tracks.Select(x => new DiscordSelectComponentOption(x.Title, x.Uri.ToString(), $"🔼 {x.Author} | 🕒 {x.Length.GetHumanReadable(TimeFormat.MINUTES)}")).ToList(), ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                                                }
                                                catch (ArgumentException)
                                                {
                                                    msg.ModifyToTimedOut();
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
                                            _ = msg.ModifyAsync(embed.Build());
                                            _ = Task.Delay(5000).ContinueWith(async x =>
                                            {
                                                msg = await UpdateMessage(msg);
                                            });
                                            return;
                                        }

                                        SelectedPlaylist.List.AddRange(Tracks.Take(250 - SelectedPlaylist.List.Count));

                                        msg = await UpdateMessage(msg);
                                        break;
                                    }
                                    case "ChangePlaylistName":
                                    {
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

                                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                        var PlaylistName = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                                        if (PlaylistName.TimedOut)
                                        {
                                            msg.ModifyToTimedOut(true);
                                            return;
                                        }

                                        _ = Task.Delay(2000).ContinueWith(_ =>
                                        {
                                            _ = PlaylistName.Result.DeleteAsync();
                                        });

                                        SelectedPlaylist.PlaylistName = PlaylistName.Result.Content;

                                        msg = await UpdateMessage(msg);
                                        break;
                                    }
                                    case "RemoveDuplicates":
                                    {
                                        CurrentPage = 0;
                                        SelectedPlaylist.List = SelectedPlaylist.List.GroupBy(x => x.Url).Select(y => y.FirstOrDefault()).ToList();
                                        msg = await UpdateMessage(msg);
                                        break;
                                    }
                                    case "DeleteSong":
                                    {
                                        List<DiscordSelectComponentOption> TrackList = SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10).Select(x => new DiscordSelectComponentOption($"{x.Title}", x.Url.MakeValidFileName(), $"Added {x.AddedTime.GetTimespanSince().GetHumanReadable()} ago")).ToList();

                                        string SelectedTrackId;
                                        PlaylistItem SelectedTrack;

                                        try
                                        {
                                            SelectedTrackId = await GenericSelectors.PromptCustomSelection(_bot, TrackList, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                                            SelectedTrack = SelectedPlaylist.List.First(x => x.Url.MakeValidFileName() == SelectedTrackId);
                                        }
                                        catch (ArgumentException)
                                        {
                                            msg.ModifyToTimedOut();
                                            return;
                                        }
                                        catch (Exception)
                                        {
                                            throw;
                                        }
                                        SelectedPlaylist.List.Remove(SelectedTrack);

                                        if (SelectedPlaylist.List.Count <= 0)
                                        {
                                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                                            _bot._users.List[ctx.Member.Id].UserPlaylists.Remove(SelectedPlaylist);

                                            await Task.Delay(5000);
                                            _ = msg.DeleteAsync();
                                            _ = ctx.Command.ExecuteAsync(ctx);
                                            return;
                                        }

                                        if (SelectedPlaylist.List.Skip(CurrentPage * 10).Take(10).Count() <= 0)
                                            CurrentPage--;

                                        msg = await UpdateMessage(msg);
                                        break;
                                    }
                                    case "NextPage":
                                    {
                                        CurrentPage++;
                                        msg = await UpdateMessage(msg);
                                        break;
                                    }
                                    case "PreviousPage":
                                    {
                                        CurrentPage--;
                                        msg = await UpdateMessage(msg);
                                        break;
                                    }
                                    case "cancel":
                                    {
                                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                        _ = msg.DeleteAsync();
                                        _ = ctx.Command.ExecuteAsync(ctx);
                                        return;
                                    }
                                }
                            }
                        }).Add(_bot._watcher, ctx);
                    }
                }
                else if (e.Result.Interaction.Data.CustomId == DeletePlaylist.CustomId)
                {
                    List<DiscordSelectComponentOption> Playlists = _bot._users.List[ctx.Member.Id].UserPlaylists.Select(x => new DiscordSelectComponentOption($"{x.PlaylistName}", x.PlaylistId, $"{x.List.Count} track(s)")).ToList();

                    string SelectedPlaylistId;
                    UserPlaylist SelectedPlaylist;

                    try
                    {
                        SelectedPlaylistId = await GenericSelectors.PromptCustomSelection(_bot, Playlists, ctx.Client, ctx.Guild, ctx.Channel, ctx.Member, msg);
                        SelectedPlaylist = _bot._users.List[ctx.Member.Id].UserPlaylists.First(x => x.PlaylistId == SelectedPlaylistId);
                    }
                    catch (ArgumentException)
                    {
                        msg.ModifyToTimedOut();
                        return;
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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

                    _bot._users.List[ctx.Member.Id].UserPlaylists.Remove(SelectedPlaylist);

                    await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
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
                    _ = msg.DeleteAsync();
                    _ = ctx.Command.ExecuteAsync(ctx);
                    return;
                }
                else
                {
                    _ = msg.DeleteAsync();
                }
            }).Add(_bot._watcher, ctx);
        }
    }
}
