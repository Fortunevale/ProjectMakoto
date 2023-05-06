namespace ProjectMakoto.Commands.Playlists;

internal class ImportCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx))
                return;

            if (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count >= 10)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`You already have 10 Playlists stored. Please delete one to create a new one.`",
                }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                return;
            }

            var Link = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Link", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("↘")));
            var ExportedPlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Exported Playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📂")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"`Do you want to import a playlist via link or exported playlist?`",
            }.AsAwaitingInput(ctx, GetString(t.Commands.Music.Playlists.Title)))
            .AddComponents(new List<DiscordComponent> { Link, ExportedPlaylist })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var Menu = await ctx.WaitForButtonAsync();

            if (Menu.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            if (Menu.GetCustomId() == Link.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder("Import Playlist", Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "query", "Playlist Url, Playlist Url", "", 1, 100, true));

                var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, false);

                if (ModalResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (ModalResult.Cancelled)
                {
                    return;
                }
                else if (ModalResult.Errored)
                {
                    throw ModalResult.Exception;
                }

                var query = ModalResult.Result.Interaction.GetModalValueByCustomId("query");

                var lava = ctx.Client.GetLavalink();
                var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);

                if (Regex.IsMatch(query, "{jndi:(ldap[s]?|rmi):\\/\\/[^\n]+"))
                    throw new Exception();

                LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(query, LavalinkSearchType.Plain);

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Couldn't load a playlist from this url.`",
                    }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                    return;
                }
                else if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
                {
                    if (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count >= 10)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`You already have 10 Playlists stored. Please delete one to create a new one.`",
                        }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Creating your playlist..`",
                    }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title))));

                    var v = new UserPlaylist
                    {
                        PlaylistName = loadResult.PlaylistInfo.Name,
                        List = loadResult.Tracks.Select(x => new PlaylistEntry { Title = x.Title, Url = x.Uri.ToString(), Length = x.Length }).Take(250).ToList()
                    };

                    ctx.Bot.users[ctx.Member.Id].UserPlaylists.Add(v);

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Your playlist '{v.PlaylistName}' has been created with {v.List.Count} entries.`\nContinuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(6))}..",
                    }.AsSuccess(ctx, GetString(t.Commands.Music.Playlists.Title))));
                    await Task.Delay(5000);
                    return;
                }
                else
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`The specified url doesn't lead to a playlist.`",
                    }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                    return;
                }
            }
            else if (Menu.GetCustomId() == ExportedPlaylist.CustomId)
            {
                try
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Please upload an exported playlist via '{ctx.Prefix}upload'.`",
                    }.AsAwaitingInput(ctx, GetString(t.Commands.Music.Playlists.Title))));

                    Stream stream;

                    try
                    {
                        stream = (await PromptForFileUpload()).stream;
                    }
                    catch (AlreadyAppliedException)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`An upload interaction is already taking place. Please finish it beforehand.`",
                        }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                        return;
                    }
                    catch (ArgumentException)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Importing your attachment..`",
                    }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title))));

                    var rawJson = new StreamReader(stream).ReadToEnd();

                    var ImportJson = JsonConvert.DeserializeObject<UserPlaylist>((rawJson is null or "null" or "" ? "[]" : rawJson), new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });

                    ImportJson.List = ImportJson.List.Where(x => RegexTemplates.Url.IsMatch(x.Url)).Select(x => new PlaylistEntry { Title = x.Title, Url = x.Url, Length = x.Length }).Take(250).ToList();

                    if (!ImportJson.List.Any())
                        throw new Exception();

                    if (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count >= 10)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`You already have 10 Playlists stored. Please delete one to create a new one.`",
                        }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Creating your playlist..`",
                    }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title))));

                    var v = new UserPlaylist
                    {
                        PlaylistName = ImportJson.PlaylistName,
                        List = ImportJson.List,
                        PlaylistColor = ImportJson.PlaylistColor
                    };

                    ctx.Bot.users[ctx.Member.Id].UserPlaylists.Add(v);

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Your playlist '{v.PlaylistName}' has been created with {v.List.Count} entries.`\nContinuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(6))}..",
                    }.AsSuccess(ctx, GetString(t.Commands.Music.Playlists.Title))));
                    await Task.Delay(5000);
                    return;
                }
                catch (Exception ex)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Failed to import your attachment. Is this a valid playlist?`",
                    }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));

                    _logger.LogError("Failed to import a playlist", ex);

                    return;
                }
            }
            else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                return;
            }
        });
    }
}