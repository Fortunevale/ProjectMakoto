// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class ImportCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            if (ctx.DbUser.UserPlaylists.Count >= 10)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                return;
            }

            var Link = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(this.t.Commands.Music.Playlists.Import.Link), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("↘")));
            var ExportedPlaylist = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(this.t.Commands.Music.Playlists.Import.ExportedPlaylist), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📂")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = GetString(this.t.Commands.Music.Playlists.Import.ImportMethod, true),
            }.AsAwaitingInput(ctx, GetString(this.t.Commands.Music.Playlists.Title)))
            .AddComponents(new List<DiscordComponent> { Link, ExportedPlaylist })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var Menu = await ctx.WaitForButtonAsync();

            if (Menu.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            if (Menu.GetCustomId() == Link.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder(GetString(this.t.Commands.Music.Playlists.Import.ImportPlaylist), Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "query", GetString(this.t.Commands.Music.Playlists.Import.PlaylistUrl), "", 1, 100, true));

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
                        Description = GetString(this.t.Commands.Music.Playlists.Import.NotLoaded, true),
                    }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                    return;
                }
                else if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
                {
                    if (ctx.DbUser.UserPlaylists.Count >= 10)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = GetString(this.t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                        }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.Import.Creating, true),
                    }.AsLoading(ctx, GetString(this.t.Commands.Music.Playlists.Title))));

                    var v = new UserPlaylist
                    {
                        PlaylistName = loadResult.PlaylistInfo.Name,
                        List = loadResult.Tracks.Select(x => new PlaylistEntry { Title = x.Title, Url = x.Uri.ToString(), Length = x.Length }).Take(250).ToList()
                    };

                    ctx.DbUser.UserPlaylists.Add(v);

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.Import.Created, true,
                        new TVar("Name", v.PlaylistName),
                        new TVar("Count", v.List.Count)),
                    }.AsSuccess(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                    await Task.Delay(5000);
                    return;
                }
                else
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.Import.NotLoaded, true),
                    }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
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
                        Description = GetString(this.t.Commands.Music.Playlists.Import.UploadExport, true, new TVar("Command", $"{ctx.Prefix}upload")),
                    }.AsAwaitingInput(ctx, GetString(this.t.Commands.Music.Playlists.Title))));

                    Stream stream;

                    try
                    {
                        stream = (await PromptForFileUpload()).stream;
                    }
                    catch (AlreadyAppliedException)
                    {
                        return;
                    }
                    catch (ArgumentException)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.Import.Importing, true),
                    }.AsLoading(ctx, GetString(this.t.Commands.Music.Playlists.Title))));

                    var rawJson = new StreamReader(stream).ReadToEnd();

                    var ImportJson = JsonConvert.DeserializeObject<UserPlaylist>((rawJson is null or "null" or "" ? "[]" : rawJson), new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });

                    ImportJson.List = ImportJson.List.Where(x => RegexTemplates.Url.IsMatch(x.Url)).Select(x => new PlaylistEntry { Title = x.Title, Url = x.Url, Length = x.Length }).Take(250).ToList();

                    if (!ImportJson.List.Any())
                        throw new Exception();

                    if (ctx.DbUser.UserPlaylists.Count >= 10)
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = GetString(this.t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                        }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                        return;
                    }

                    var v = new UserPlaylist
                    {
                        PlaylistName = ImportJson.PlaylistName,
                        List = ImportJson.List,
                        PlaylistColor = ImportJson.PlaylistColor
                    };

                    ctx.DbUser.UserPlaylists.Add(v);

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.Import.Created, true,
                        new TVar("Name", v.PlaylistName),
                        new TVar("Count", v.List.Count)),
                    }.AsSuccess(ctx, GetString(this.t.Commands.Music.Playlists.Title))));
                    await Task.Delay(5000);
                    return;
                }
                catch (Exception ex)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(this.t.Commands.Music.Playlists.Import.ImportFailed, true),
                    }.AsError(ctx, GetString(this.t.Commands.Music.Playlists.Title))));

                    _logger.LogError("Failed to import a playlist", ex);

                    return;
                }
            }
            else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                return;
            }
        });
    }
}