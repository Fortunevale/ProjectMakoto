namespace ProjectIchigo.Commands;

internal class EmojiStealerCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordMessage bMessage;

            if (arguments?.ContainsKey("message") ?? false)
            {
                bMessage = (DiscordMessage)arguments["message"];
            }
            else
            {
                switch (ctx.CommandType)
                {
                    case Enums.CommandType.PrefixCommand:
                    {
                        if (ctx.OriginalCommandContext.Message.ReferencedMessage is not null)
                        {
                            bMessage = ctx.OriginalCommandContext.Message.ReferencedMessage;
                        }
                        else
                        {
                            SendSyntaxError();
                            return;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Message expected");
                }
            }

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            HttpClient client = new();

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`{GetString(t.commands.emojistealer.downloadingpre)}`",
            }.AsLoading(ctx);
            await RespondOrEdit(embed);

            Dictionary<ulong, EmojiEntry> SanitizedEmoteList = new();

            var Emotes = bMessage.Content.GetEmotes();

            foreach (var b in Emotes)
                SanitizedEmoteList.Add(b.Item1, new EmojiEntry
                {
                    Name = b.Item2,
                    Animated = b.Item3,
                    Type = EmojiType.EMOJI
                });

            if (!Emotes.Any() && (bMessage.Stickers is null || bMessage.Stickers.Count == 0))
            {
                embed.Description = $"`{GetString(t.commands.emojistealer.noemojis)}`";
                await RespondOrEdit(embed.AsError(ctx));
                return;
            }

            bool ContainsStickers = bMessage.Stickers.Count > 0;

            string guid = Guid.NewGuid().ToString().MakeValidFileName();

            if (Directory.Exists($"emotes-{guid}"))
                Directory.Delete($"emotes-{guid}", true);

            Directory.CreateDirectory($"emotes-{guid}");

            if (SanitizedEmoteList.Count > 0)
            {
                embed.Description = $"`{GetString(t.commands.emojistealer.downloadingemojis).Replace("{Count}", SanitizedEmoteList.Count)}`";
                await RespondOrEdit(embed);

                foreach (var b in SanitizedEmoteList.ToList())
                {
                    try
                    {
                        var EmoteStream = await client.GetStreamAsync($"https://cdn.discordapp.com/emojis/{b.Key}.{(b.Value.Animated ? "gif" : "png")}");

                        string FileExists = "";
                        int FileExistsInt = 1;

                        string fileName = $"{b.Value.Name}{FileExists}.{(b.Value.Animated ? "gif" : "png")}".MakeValidFileName('_');

                        while (File.Exists($"emotes-{guid}/{fileName}"))
                        {
                            FileExistsInt++;
                            FileExists = $" ({FileExistsInt})";

                            fileName = $"{b.Value.Name}{FileExists}.{(b.Value.Animated ? "gif" : "png")}".MakeValidFileName('_');
                        }

                        using (var fileStream = File.Create($"emotes-{guid}/{fileName}"))
                        {
                            EmoteStream.CopyTo(fileStream);
                            await fileStream.FlushAsync();
                        }

                        SanitizedEmoteList[b.Key].Path = $"emotes-{guid}/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to download an emote", ex);
                        SanitizedEmoteList.Remove(b.Key);
                    }
                }
            }

            if (ContainsStickers)
            {
                embed.Description = $"`{GetString(t.commands.emojistealer.downloadingemojis).Replace("{Count}", bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()).Count())}`";
                await RespondOrEdit(embed);

                foreach (var b in bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()))
                {
                    var StickerStream = await client.GetStreamAsync(b.Url);

                    string FileExists = "";
                    int FileExistsInt = 1;

                    string fileName = $"{b.Name}{FileExists}.png".MakeValidFileName('_');

                    while (File.Exists($"emotes-{guid}/{fileName}"))
                    {
                        FileExistsInt++;
                        FileExists = $" ({FileExistsInt})";

                        fileName = $"{b.Name}{FileExists}.png".MakeValidFileName('_');
                    }

                    using (var fileStream = File.Create($"emotes-{guid}/{fileName}"))
                    {
                        StickerStream.CopyTo(fileStream);
                        await fileStream.FlushAsync();
                    }

                    SanitizedEmoteList.Add(b.Id, new EmojiEntry { Animated = false, Name = b.Name, Path = $"emotes-{guid}/{fileName}", Type = EmojiType.STICKER });
                }
            }

            if (SanitizedEmoteList.Count == 0)
            {
                embed.Description = $"`{GetString(t.commands.emojistealer.nosuccessfuldownload)}`";
                await RespondOrEdit(embed.AsError(ctx));

                return;
            }

            string emojiText = "";

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                emojiText += GetString(t.commands.emojistealer.emoji);

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                emojiText += $"{(emojiText.Length > 0 ? $" & {GetString(t.commands.emojistealer.sticker)}" : GetString(t.commands.emojistealer.sticker))}";

            embed.Description = $"`{GetString(t.commands.emojistealer.receiveprompt).Replace("{Type}", emojiText)}`";
            embed.AsAwaitingInput(ctx);

            bool IncludeStickers = false;

            if (!SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                IncludeStickers = true;

            var IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", GetString(t.commands.emojistealer.togglestickers), !SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI), new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));

            var AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", GetString(t.commands.emojistealer.addtoserver), (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || !ctx.CurrentMember.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || IncludeStickers), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var ZipPrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "ZipPrivateMessage", GetString(t.commands.emojistealer.directmessagezip), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖥")));
            var SinglePrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "SinglePrivateMessage", GetString(t.commands.emojistealer.directmessagesingle), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📱")));

            var SendHereButton = new DiscordButtonComponent(ButtonStyle.Secondary, "SendHere", GetString(t.commands.emojistealer.currentchatzip), !(ctx.Member.Permissions.HasPermission(Permissions.AttachFiles)), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                builder.AddComponents(IncludeStickersButton);

            builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

            await RespondOrEdit(builder);

            CancellationTokenSource cancellationTokenSource = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            _ = Task.Delay(60000, cancellationTokenSource.Token).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ctx.Client.ComponentInteractionCreated -= RunInteraction;

                    _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                    ModifyToTimedOut();
                }
            });

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        _ = Task.Delay(60000, cancellationTokenSource.Token).ContinueWith(x =>
                        {
                            if (x.IsCompletedSuccessfully)
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                ModifyToTimedOut();
                            }
                        });

                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.GetCustomId() == AddToServerButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            cancellationTokenSource.Cancel();

                            if (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers))
                            {
                                SendPermissionError(Permissions.ManageEmojisAndStickers);
                                return;
                            }
                            
                            if (!ctx.CurrentMember.Permissions.HasPermission(Permissions.ManageEmojisAndStickers))
                            {
                                SendOwnPermissionError(Permissions.ManageEmojisAndStickers);
                                return;
                            }

                            if (IncludeStickers)
                            {
                                embed.Description = $"`{GetString(t.commands.emojistealer.addtoserverstickererror)}`";
                                embed.AsError(ctx);
                                await RespondOrEdit(embed);

                                return;
                            }

                            bool DiscordWarning = false;

                            embed.Description = $"`{GetString(t.commands.emojistealer.addtoserverloading).Replace("{Min}", "0").Replace("{Max}", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count()))}`";
                            embed.AsLoading(ctx);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                            for (int i = 0; i < SanitizedEmoteList.Count; i++)
                            {
                                try
                                {
                                    Task task;

                                    if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                        continue;

                                    using (var fileStream = File.OpenRead(SanitizedEmoteList.ElementAt(i).Value.Path))
                                    {
                                        task = ctx.Guild.CreateEmojiAsync(SanitizedEmoteList.ElementAt(i).Value.Name, fileStream);
                                    }

                                    int WaitSeconds = 0;

                                    while (task.Status == TaskStatus.WaitingForActivation)
                                    {
                                        WaitSeconds++;

                                        if (WaitSeconds > 10 && !DiscordWarning)
                                        {
                                            embed.Description = $"`{GetString(t.commands.emojistealer.addtoserverloading).Replace("{Min}", i).Replace("{Max}", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count()))}`\n\n" +
                                                                $"{GetString(t.commands.emojistealer.addtoserverloadingnotice)}";
                                            await RespondOrEdit(embed);

                                            DiscordWarning = true;
                                        }
                                        await Task.Delay(1000);
                                    }

                                    if (task.IsFaulted)
                                        throw task.Exception.InnerException;

                                    embed.Description = $"`{GetString(t.commands.emojistealer.addtoserverloading).Replace("{Min}", i).Replace("{Max}", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count()))}`";
                                    embed.AsSuccess(ctx);
                                    await RespondOrEdit(embed);
                                }
                                catch (DisCatSharp.Exceptions.BadRequestException ex)
                                {
                                    var regex = Regex.Match(ex.WebResponse.Response.Replace("\\", ""), "((\"code\": )(\\d*))");

                                    if (regex.Groups[3].Value == "30008")
                                    {
                                        embed.Description = $"`{GetString(t.commands.emojistealer.nomoreroom).Replace("{Count}", i)}`";
                                        embed.AsError(ctx);
                                        await RespondOrEdit(embed);
                                        _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                        return;
                                    }
                                    else
                                        throw;
                                }
                            }

                            embed.Description = $"`{GetString(t.commands.emojistealer.addedtoserver).Replace("{Count}", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count()))}`";
                            embed.AsSuccess(ctx);
                            await RespondOrEdit(embed);
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.GetCustomId() == SinglePrivateMessageButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            cancellationTokenSource.Cancel();

                            embed.Description = $"`{GetString(t.commands.emojistealer.sendingdm).Replace("{Type}", emojiText)}`";
                            embed.AsLoading(ctx);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                            try
                            {
                                for (int i = 0; i < SanitizedEmoteList.Count; i++)
                                {
                                    if (!IncludeStickers)
                                        if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                            continue;

                                    using (var fileStream = File.OpenRead(SanitizedEmoteList.ElementAt(i).Value.Path))
                                    {
                                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent($"`{i + 1}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())}` `{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}`").WithFile($"{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}", fileStream));
                                    }

                                    await Task.Delay(1000);
                                }

                                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent(GetString(t.commands.emojistealer.successdm).Replace("{Type}", emojiText)));
                            }
                            catch (DisCatSharp.Exceptions.UnauthorizedException)
                            {
                                SendDmError();
                                _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }

                            embed.Description = $"`{GetString(t.commands.emojistealer.successdmori).Replace("{Count}", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())).Replace("{Type}", emojiText)}`";
                            await RespondOrEdit(embed.AsSuccess(ctx));
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.GetCustomId() == ZipPrivateMessageButton.CustomId || e.GetCustomId() == SendHereButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            cancellationTokenSource.Cancel();

                            embed.Description = $"`{GetString(t.commands.emojistealer.preparingzip)}`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsLoading(ctx)));

                            if (Directory.Exists($"zipfile-{guid}"))
                                Directory.Delete($"zipfile-{guid}", true);

                            Directory.CreateDirectory($"zipfile-{guid}");

                            for (int i = 0; i < SanitizedEmoteList.Count; i++)
                            {
                                if (!IncludeStickers)
                                    if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                        continue;

                                string NewFilename = $"{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}";

                                int FileExistsInt = 1;

                                while (File.Exists($"zipfile-{guid}/{NewFilename}"))
                                {
                                    FileExistsInt++;
                                    NewFilename = $"{SanitizedEmoteList.ElementAt(i).Value.Name}_{FileExistsInt}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}";
                                }

                                File.Copy(SanitizedEmoteList.ElementAt(i).Value.Path, $"zipfile-{guid}/{NewFilename}");
                            }

                            ZipFile.CreateFromDirectory($"zipfile-{guid}", $"Emotes-{guid}.zip");

                            if (e.GetCustomId() == ZipPrivateMessageButton.CustomId)
                            {
                                embed.Description = $"`{GetString(t.commands.emojistealer.sendingzipdm)}`";
                                await RespondOrEdit(embed);

                                try
                                {
                                    using var fileStream = File.OpenRead($"Emotes-{guid}.zip");
                                    await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emojis.zip", fileStream).WithContent(GetString(t.commands.emojistealer.successdm).Replace("{Type}", emojiText)));
                                }
                                catch (DisCatSharp.Exceptions.UnauthorizedException)
                                {
                                    SendDmError();
                                    _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                    return;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }

                                embed.Description = $"`{GetString(t.commands.emojistealer.successdmori).Replace("{Count}", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())).Replace("{Type}", emojiText)}`";
                                await RespondOrEdit(embed.AsSuccess(ctx));
                            }
                            else if (e.GetCustomId() == SendHereButton.CustomId)
                            {
                                if (!ctx.Member.Permissions.HasPermission(Permissions.AttachFiles))
                                {
                                    SendPermissionError(Permissions.AttachFiles);
                                    return;
                                }

                                embed.Description = $"`{GetString(t.commands.emojistealer.sendingzipchat)}`";
                                await RespondOrEdit(embed);

                                embed.Description = $"`{GetString(t.commands.emojistealer.successchat).Replace("{Count}", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())).Replace("{Type}", emojiText)}`";

                                using var fileStream = File.OpenRead($"Emotes-{guid}.zip");
                                await RespondOrEdit(new DiscordMessageBuilder().WithFile($"Emotes.zip", fileStream).WithEmbed(embed.AsSuccess(ctx)));
                            }
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.GetCustomId() == IncludeStickersButton.CustomId)
                        {
                            IncludeStickers = !IncludeStickers;

                            if (!IncludeStickers)
                            {
                                if (!SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                                    IncludeStickers = true;
                            }

                            IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", GetString(t.commands.emojistealer.togglestickers), !SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI), new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));
                            AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", GetString(t.commands.emojistealer.addtoserver), (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || !ctx.CurrentMember.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || IncludeStickers), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));

                            var builder = new DiscordMessageBuilder().WithEmbed(embed);

                            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                                builder.AddComponents(IncludeStickersButton);

                            builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

                            await RespondOrEdit(builder);
                        }
                    }

                }).Add(ctx.Bot.watcher, ctx);
            }
        });
    }
}