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

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            HttpClient client = new();

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Downloading emojis of this message..`",
                Color = EmbedColors.Processing,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            await RespondOrEdit(embed);

            Dictionary<ulong, EmojiStealer> SanitizedEmoteList = new();

            var Emotes = bMessage.Content.GetEmotes();

            foreach (var b in Emotes)
                SanitizedEmoteList.Add(b.Item1, new EmojiStealer
                {
                    Name = b.Item2,
                    Animated = b.Item3
                });

            if (!Emotes.Any() && (bMessage.Stickers is null || bMessage.Stickers.Count == 0))
            {
                embed.Description = $"`This message doesn't contain any emojis or stickers.`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                await RespondOrEdit(embed);

                return;
            }

            bool ContainsStickers = bMessage.Stickers.Count > 0;

            string guid = Guid.NewGuid().ToString().MakeValidFileName();

            if (Directory.Exists($"emotes-{guid}"))
                Directory.Delete($"emotes-{guid}", true);

            Directory.CreateDirectory($"emotes-{guid}");

            if (SanitizedEmoteList.Count > 0)
            {
                embed.Description = $"`Downloading {SanitizedEmoteList.Count} emojis of this message..`";
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
                        _logger.LogError($"Failed to download an emote", ex);
                        SanitizedEmoteList.Remove(b.Key);
                    }
                }
            }

            if (ContainsStickers)
            {
                embed.Description = $"`Downloading {bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()).Count()} stickers of this message..`";
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

                    SanitizedEmoteList.Add(b.Id, new EmojiStealer { Animated = false, Name = b.Name, Path = $"emotes-{guid}/{fileName}", Type = EmojiType.STICKER });
                }
            }

            if (SanitizedEmoteList.Count == 0)
            {
                embed.Description = $"`Couldn't download any emojis or stickers from this message.`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                await RespondOrEdit(embed);

                return;
            }

            string emojiText = "";

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                emojiText += "emojis";

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                emojiText += $"{(emojiText.Length > 0 ? " and stickers" : "stickers")}";

            embed.Author.IconUrl = ctx.Guild.IconUrl;
            embed.Description = $"`Select how you want to receive the downloaded {emojiText}.`";

            bool IncludeStickers = false;

            if (!SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                IncludeStickers = true;

            var IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", "Include Stickers", !SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI), new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));

            var AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", "Add Emoji(s) to Server", (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || IncludeStickers), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var ZipPrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "ZipPrivateMessage", "Direct Message as Zip File", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖥")));
            var SinglePrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "SinglePrivateMessage", "Direct Message as Single Files", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📱")));

            var SendHereButton = new DiscordButtonComponent(ButtonStyle.Secondary, "SendHere", "In this chat as Zip File", !(ctx.Member.Permissions.HasPermission(Permissions.AttachFiles)), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

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

                        if (e.Interaction.Data.CustomId == AddToServerButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            cancellationTokenSource.Cancel();

                            if (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers))
                            {
                                SendPermissionError(Permissions.ManageEmojisAndStickers);
                                return;
                            }

                            if (IncludeStickers)
                            {
                                embed.Description = $"`You cannot add any emoji(s) to the server while including stickers.`";
                                embed.Color = EmbedColors.Error;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                await RespondOrEdit(embed);

                                return;
                            }

                            bool DiscordWarning = false;

                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Added 0/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`";
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
                                            embed.Description = $"`Added {i}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`\n\n" +
                                                                                $"_The bot most likely hit a rate limit. This can take up to an hour to continue._\n" +
                                                                                $"_If you want this to change, go scream at Discord. There's nothing i can do._";
                                            await RespondOrEdit(embed);

                                            DiscordWarning = true;
                                        }
                                        await Task.Delay(1000);
                                    }

                                    embed.Description = $"`Added {i}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`";
                                    await RespondOrEdit(embed);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to add an emote to guild", ex);
                                }
                            }

                            embed.Thumbnail = null;
                            embed.Color = EmbedColors.Success;
                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.Description = $"✅ `Downloaded and added {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to the server.`";
                            await RespondOrEdit(embed);
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == SinglePrivateMessageButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            cancellationTokenSource.Cancel();

                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Sending the {emojiText} in your DMs..`";
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

                                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent($"Heyho! Here's the {emojiText} you requested."));
                            }
                            catch (DisCatSharp.Exceptions.UnauthorizedException)
                            {
                                var errorembed = new DiscordEmbedBuilder
                                {
                                    Author = new DiscordEmbedBuilder.EmbedAuthor
                                    {
                                        Name = ctx.Guild.Name,
                                        IconUrl = ctx.Guild.IconUrl
                                    },
                                    Description = "❌ `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                                    Footer = ctx.GenerateUsedByFooter(),
                                    Timestamp = DateTime.UtcNow,
                                    Color = EmbedColors.Error,
                                    ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                                };

                                if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                                    errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                                await RespondOrEdit(errorembed);
                                _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }

                            embed.Thumbnail = null;
                            embed.Color = EmbedColors.Success;
                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.Description = $"✅ `Downloaded and sent {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText} to your DMs.`";
                            await RespondOrEdit(embed);
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == ZipPrivateMessageButton.CustomId || e.Interaction.Data.CustomId == SendHereButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            cancellationTokenSource.Cancel();

                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Preparing your Zip File..`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

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

                            if (e.Interaction.Data.CustomId == ZipPrivateMessageButton.CustomId)
                            {
                                embed.Description = $"`Sending your Zip File in DMs..`";
                                await RespondOrEdit(embed);

                                try
                                {
                                    using (var fileStream = File.OpenRead($"Emotes-{guid}.zip"))
                                    {
                                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emojis.zip", fileStream).WithContent("Heyho! Here's the emojis you requested."));
                                    }
                                }
                                catch (DisCatSharp.Exceptions.UnauthorizedException)
                                {
                                    var errorembed = new DiscordEmbedBuilder
                                    {
                                        Author = new DiscordEmbedBuilder.EmbedAuthor
                                        {
                                            Name = ctx.Guild.Name,
                                            IconUrl = ctx.Guild.IconUrl
                                        },
                                        Description = "❌ `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                                        Footer = ctx.GenerateUsedByFooter(),
                                        Timestamp = DateTime.UtcNow,
                                        Color = EmbedColors.Error,
                                        ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                                    };

                                    if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                                        errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                                    await RespondOrEdit(errorembed);
                                    _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                    return;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }

                                embed.Thumbnail = null;
                                embed.Color = EmbedColors.Success;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                embed.Description = $"✅ `Downloaded and sent {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText} to your DMs.`";
                                await RespondOrEdit(embed);
                            }
                            else if (e.Interaction.Data.CustomId == SendHereButton.CustomId)
                            {
                                if (!ctx.Member.Permissions.HasPermission(Permissions.AttachFiles))
                                {
                                    SendPermissionError(Permissions.AttachFiles);
                                    return;
                                }

                                embed.Description = $"`Sending your Zip File..`";
                                await RespondOrEdit(embed);

                                embed.Thumbnail = null;
                                embed.Color = EmbedColors.Success;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                embed.Description = $"✅ `Downloaded {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText}. Attached is a Zip File containing them.`";

                                using (var fileStream = File.OpenRead($"Emotes-{guid}.zip"))
                                {
                                    await RespondOrEdit(new DiscordMessageBuilder().WithFile($"Emotes.zip", fileStream).WithEmbed(embed));
                                }
                            }
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == IncludeStickersButton.CustomId)
                        {
                            IncludeStickers = !IncludeStickers;

                            if (!IncludeStickers)
                            {
                                if (!SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                                    IncludeStickers = true;
                            }

                            IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", "Include Stickers", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));
                            AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", "Add Emoji(s) to Server", (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || IncludeStickers), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));

                            var builder = new DiscordMessageBuilder().WithEmbed(embed);

                            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                                builder.AddComponents(IncludeStickersButton);

                            builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

                            await RespondOrEdit(builder);
                        }
                    }

                }).Add(ctx.Bot._watcher, ctx);
            }
        });
    }
}