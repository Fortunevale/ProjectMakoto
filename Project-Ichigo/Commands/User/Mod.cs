namespace Project_Ichigo.Commands.User;
internal class Mod : BaseCommandModule
{
    public Status _status { private get; set; }
    public TaskWatcher.TaskWatcher _watcher { private get; set; }



    [Command("emoji"), Aliases("emote"),
    CommandModule("mod"),
    Description("Steals emojis of the message that this command was replied to.")]
    public async Task EmojiStealer(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers))
            {
                _ = ctx.SendPermissionError(Permissions.ManageEmojisAndStickers);
                return;
            }

            ulong messageid;

            if (ctx.Message.ReferencedMessage is not null)
            {
                messageid = ctx.Message.ReferencedMessage.Id;
            }
            else
            {
                _ = ctx.SendSyntaxError();
                return;
            }

            var PerformingActionEmbed = new DiscordEmbedBuilder
            {
                Title = "",
                Description = $"`Downloading emotes of this message..`",
                Color = DiscordColor.Orange,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Message.ReferencedMessage.RespondAsync(embed: PerformingActionEmbed);

            HttpClient client = new();

            try
            {
                var bMessage = await ctx.Channel.GetMessageAsync(Convert.ToUInt64(messageid));

                List<string> EmoteList = new();
                Dictionary<ulong, EmojiStealer> SanitizedEmoteList = new();

                List<string> Emotes = bMessage.Content.GetEmotes();
                List<string> AnimatedEmotes = bMessage.Content.GetAnimatedEmotes();

                if (Emotes is not null || AnimatedEmotes is not null)
                {
                    if (Emotes is not null)
                        foreach (var b in Emotes)
                            if (!SanitizedEmoteList.ContainsKey(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value)))
                                SanitizedEmoteList.Add(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value), new EmojiStealer { Animated = false, Name = Regex.Match(b, @"((?<=:)[^ ]*(?=:))").Value });

                    if (AnimatedEmotes is not null)
                        foreach (var b in AnimatedEmotes)
                            if (!SanitizedEmoteList.ContainsKey(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value)))
                                SanitizedEmoteList.Add(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value), new EmojiStealer { Animated = true, Name = Regex.Match(b, @"((?<=:)[^ ]*(?=:))").Value });

                    PerformingActionEmbed.Description = $"`Downloading {SanitizedEmoteList.Count} emotes of this message..`";
                    await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                    string guid = Guid.NewGuid().ToString().MakeValidFileName();

                    if (Directory.Exists($"emotes-{guid}"))
                        Directory.Delete($"emotes-{guid}", true);

                    Directory.CreateDirectory($"emotes-{guid}");

                    foreach (var b in SanitizedEmoteList.ToList())
                    {
                        LogDebug($"Downloading '{b.Value.Name}'..");
                        if (!b.Value.Animated)
                        {
                            try
                            {
                                var EmoteStream = await client.GetStreamAsync($"https://cdn.discordapp.com/emojis/{b.Key}.png");

                                string FileExists = "";
                                int FileExistsInt = 1;

                                while (File.Exists($"emotes-{guid}/{b.Value.Name}{FileExists}.png"))
                                {
                                    FileExistsInt++;
                                    FileExists = $" ({FileExistsInt})";
                                }

                                using (var fileStream = File.Create($"emotes-{guid}/{b.Value.Name}{FileExists}.png"))
                                {
                                    EmoteStream.CopyTo(fileStream);
                                    await fileStream.FlushAsync();
                                }

                                LogDebug($"{b.Value.Name} ({b.Key}) is located at 'emotes-{guid}/{b.Value.Name}{FileExists}.png'");
                                SanitizedEmoteList[b.Key].Path = $"emotes-{guid}/{b.Value.Name}{FileExists}.png";
                            }
                            catch (Exception ex)
                            {
                                LogError($"{ex}");
                                SanitizedEmoteList.Remove(b.Key);
                            }
                        }
                        else
                        {
                            try
                            {
                                var EmoteStream = await client.GetStreamAsync($"https://cdn.discordapp.com/emojis/{b.Key}.gif");

                                string FileExists = "";
                                int FileExistsInt = 1;

                                while (File.Exists($"emotes-{guid}/{b.Value.Name}{FileExists}.gif"))
                                {
                                    FileExistsInt++;
                                    FileExists = $" ({FileExistsInt})";
                                }

                                using (var fileStream = File.Create($"emotes-{guid}/{b.Value.Name}{FileExists}.gif"))
                                {
                                    EmoteStream.CopyTo(fileStream);
                                    await fileStream.FlushAsync();
                                }

                                LogDebug($"{b.Value.Name} ({b.Key}) is located at 'emotes-{guid}/{b.Value.Name}{FileExists}.gif'");
                                SanitizedEmoteList[b.Key].Path = $"emotes-{guid}/{b.Value.Name}{FileExists}.gif";
                            }
                            catch (Exception ex)
                            {
                                LogError($"{ex}");
                                SanitizedEmoteList.Remove(b.Key);
                            }
                        }
                    }

                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Description = $"`Do you want to add {SanitizedEmoteList.Count} emotes to this server?`\n" +
                                                        $"`Selecting ❌ will let you choose how to receive the emotes.`";
                    await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                    await msg1.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    await msg1.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));

                    var bWait = ctx.Client.GetInteractivity();
                    await bWait.WaitForReactionAsync(message: msg1, user: ctx.User, TimeSpan.FromSeconds(30));

                    IReadOnlyList<DiscordUser> Accept = await msg1.GetReactionsAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    IReadOnlyList<DiscordUser> Deny = await msg1.GetReactionsAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_right:"));

                    await msg1.DeleteAllReactionsAsync();

                    if (Accept.Count >= 2)
                    {
                        bool Pinned = false;
                        bool DiscordWarning = false;

                        PerformingActionEmbed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                        PerformingActionEmbed.Description = $"`Added 0/{SanitizedEmoteList.Count} emotes to this server..`";
                        await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                        for (int i = 0; i < SanitizedEmoteList.Count; i++)
                        {
                            try
                            {
                                Task task;

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
                                        PerformingActionEmbed.Description = $"`Added 0/{SanitizedEmoteList.Count} emotes to this server..`\n\n" +
                                                                            $"_The bot most likely hit a rate limit. This can take up to an hour to continue._\n" +
                                                                            $"_If you want this to change, go scream at Discord. There's nothing i can do._";
                                        await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                                        DiscordWarning = true;

                                        try
                                        {
                                            await msg1.PinAsync();
                                            Pinned = true;
                                        }
                                        catch { }
                                    }
                                    await Task.Delay(1000);
                                }


                                if (Pinned)
                                    try
                                    {
                                        await msg1.UnpinAsync();
                                        Pinned = false;
                                    }
                                    catch { }

                                PerformingActionEmbed.Description = $"`Added {i}/{SanitizedEmoteList.Count} emotes to this server..`";
                                await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                            }
                            catch (Exception ex)
                            {
                                LogError($"{ex}");
                            }
                        }

                        PerformingActionEmbed.Thumbnail = null;
                        PerformingActionEmbed.Color = DiscordColor.Green;
                        PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                        PerformingActionEmbed.Description = $":white_check_mark: `Downloaded and added {SanitizedEmoteList.Count} emotes to the server.`";
                        await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                        _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                        return;
                    }
                    else
                    {
                        PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                        PerformingActionEmbed.Description = $"`How do you want to receive the emotes?`\n" +
                                                            $":mailbox_with_mail: - `Via DMs, one message for one emote (Recommended for mobile)`\n" +
                                                            $":file_folder: - `Via DMs, in a zip file (Recommended for desktop)`\n" +
                                                            $":card_box: - `In a zip file in this chat`";
                        await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                        await msg1.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":mailbox_with_mail:"));
                        await msg1.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":file_folder:"));
                        await msg1.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":card_box:"));

                        await bWait.WaitForReactionAsync(message: msg1, user: ctx.User, TimeSpan.FromSeconds(30));

                        IReadOnlyList<DiscordUser> OneMessageForOneEmote = await msg1.GetReactionsAsync(DiscordEmoji.FromName(ctx.Client, ":mailbox_with_mail:"));
                        IReadOnlyList<DiscordUser> ZipFileInDMs = await msg1.GetReactionsAsync(DiscordEmoji.FromName(ctx.Client, ":file_folder:"));
                        IReadOnlyList<DiscordUser> ZipFileInChat = await msg1.GetReactionsAsync(DiscordEmoji.FromName(ctx.Client, ":card_box:"));

                        await msg1.DeleteAllReactionsAsync();

                        if (OneMessageForOneEmote.Count >= 2)
                        {
                            PerformingActionEmbed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            PerformingActionEmbed.Description = $"`Sending the emotes in your DMs..`";
                            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                            try
                            {
                                for (int i = 0; i < SanitizedEmoteList.Count; i++)
                                {
                                    using (var fileStream = File.OpenRead(SanitizedEmoteList.ElementAt(i).Value.Path))
                                    {
                                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile($"{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}", fileStream));
                                    }

                                    await Task.Delay(1000);
                                }
                            }
                            catch (Exception)
                            {
                                var errorembed = new DiscordEmbedBuilder
                                {
                                    Author = new DiscordEmbedBuilder.EmbedAuthor
                                    {
                                        Name = ctx.Guild.Name,
                                        IconUrl = ctx.Guild.IconUrl
                                    },
                                    Description = ":x: `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                                        IconUrl = ctx.Member.AvatarUrl
                                    },
                                    Timestamp = DateTime.UtcNow,
                                    Color = DiscordColor.DarkRed,
                                    ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                                };

                                if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                                    errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                                await msg1.ModifyAsync(embed: errorembed.Build());
                                _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                return;
                            }

                            PerformingActionEmbed.Thumbnail = null;
                            PerformingActionEmbed.Color = DiscordColor.Green;
                            PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                            PerformingActionEmbed.Description = $":white_check_mark: `Downloaded and sent {SanitizedEmoteList.Count} emotes to your DMs.`";
                            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (ZipFileInDMs.Count >= 2 || ZipFileInChat.Count >= 2)
                        {
                            PerformingActionEmbed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            PerformingActionEmbed.Description = $"`Preparing your Zip File..`";
                            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                            if (Directory.Exists($"zipfile-{guid}"))
                                Directory.Delete($"zipfile-{guid}", true);

                            Directory.CreateDirectory($"zipfile-{guid}");

                            for (int i = 0; i < SanitizedEmoteList.Count; i++)
                            {
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

                            if (ZipFileInDMs.Count >= 2)
                            {
                                PerformingActionEmbed.Description = $"`Sending your Zip File in DMs..`";
                                await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                                try
                                {
                                    using (var fileStream = File.OpenRead($"Emotes-{guid}.zip"))
                                    {
                                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emotes.zip", fileStream));
                                    }
                                }
                                catch (Exception)
                                {
                                    var errorembed = new DiscordEmbedBuilder
                                    {
                                        Author = new DiscordEmbedBuilder.EmbedAuthor
                                        {
                                            Name = ctx.Guild.Name,
                                            IconUrl = ctx.Guild.IconUrl
                                        },
                                        Description = ":x: `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                                        Footer = new DiscordEmbedBuilder.EmbedFooter
                                        {
                                            Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                                            IconUrl = ctx.Member.AvatarUrl
                                        },
                                        Timestamp = DateTime.UtcNow,
                                        Color = DiscordColor.DarkRed,
                                        ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                                    };

                                    if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                                        errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                                    await msg1.ModifyAsync(embed: errorembed.Build());

                                    _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                    return;
                                }

                                PerformingActionEmbed.Thumbnail = null;
                                PerformingActionEmbed.Color = DiscordColor.Green;
                                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                                PerformingActionEmbed.Description = $":white_check_mark: `Downloaded and sent {SanitizedEmoteList.Count} emotes to your DMs.`";
                                await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                            }
                            else if (ZipFileInChat.Count >= 2)
                            {
                                PerformingActionEmbed.Description = $"`Sending your Zip File..`";
                                await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                                PerformingActionEmbed.Thumbnail = null;
                                PerformingActionEmbed.Color = DiscordColor.Green;
                                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                                PerformingActionEmbed.Description = $":white_check_mark: `Downloaded {SanitizedEmoteList.Count} emotes. Attached is a Zip File containing them.`";

                                using (var fileStream = File.OpenRead($"Emotes-{guid}.zip"))
                                {
                                    await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emotes.zip", fileStream).WithEmbed(PerformingActionEmbed.Build()));
                                }

                                await msg1.DeleteAsync();
                            }
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                    }
                }
                else
                {
                    PerformingActionEmbed.Description = $"`This message doesn't contain any emotes.`\n\n" +
                    $"```{bMessage.Content}```";
                    PerformingActionEmbed.Color = DiscordColor.DarkRed;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());

                    return;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error occured downloading emotes: {ex}");
            }
        }).Add(_watcher, ctx);
    }



    [Command("purge"), Aliases("clear"),
    CommandModule("mod"),
    Description("Deletes the specified amount of messages.")]
    public async Task Purge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser user = null)
    {
        Task.Run(async () =>
        {
            try
            {
                _ = ctx.Message.DeleteAsync();
            }
            catch { }

            if (!ctx.Member.Permissions.HasPermission(Permissions.ManageMessages))
            {
                _ = ctx.SendPermissionError(Permissions.ManageMessages);
                return;
            }

            if (number is > 2000 or < 1)
            {
                _ = ctx.SendSyntaxError();
                return;
            }

            try
            {
                int FailedToDeleteAmount = 0;

                if (number > 100)
                {
                    var PerformingActionEmbed = new DiscordEmbedBuilder
                    {
                        Description = $"`Fetching {number} messages..`",
                        Color = DiscordColor.Orange,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                        },
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                            IconUrl = ctx.Member.AvatarUrl
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

                    List<DiscordMessage> fetchedMessages = (await ctx.Channel.GetMessagesAsync(100)).ToList();

                    if (fetchedMessages.Any(x => x.Id == ctx.Message.Id))
                        fetchedMessages.Remove(fetchedMessages.First(x => x.Id == ctx.Message.Id));

                    if (fetchedMessages.Any(x => x.Id == msg1.Id))
                        fetchedMessages.Remove(fetchedMessages.First(x => x.Id == msg1.Id));

                    while (fetchedMessages.Count <= number)
                    {
                        IReadOnlyList<DiscordMessage> fetch;

                        if (fetchedMessages.Count + 100 <= number)
                            fetch = await ctx.Channel.GetMessagesBeforeAsync(fetchedMessages.Last().Id, 100);
                        else
                            fetch = await ctx.Channel.GetMessagesBeforeAsync(fetchedMessages.Last().Id, number - fetchedMessages.Count);

                        if (fetch.Any())
                            fetchedMessages.AddRange(fetch);
                        else
                            break;
                    }

                    if (user is not null)
                    {
                        foreach (var b in fetchedMessages.Where(x => x.Author.Id != user.Id).ToList())
                        {
                            fetchedMessages.Remove(b);
                        }
                    }

                    int failed_deleted = 0;

                    foreach (var b in fetchedMessages.Where(x => x.CreationTimestamp < DateTime.UtcNow.AddDays(-14)).ToList())
                    {
                        fetchedMessages.Remove(b);
                        FailedToDeleteAmount++;
                        failed_deleted++;
                    }

                    if (fetchedMessages.Count > 0)
                    {
                        PerformingActionEmbed.Description = $"`Fetched {fetchedMessages.Count} messages. Deleting..`";
                        await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                    }
                    else
                    {
                        PerformingActionEmbed.Description = $":x: `No messages were found with the specified filter.`";
                        PerformingActionEmbed.Color = DiscordColor.Red;
                        PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                        await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                    }

                    int total = fetchedMessages.Count;
                    int deleted = 0;
                    
                    List<Task> deletionOperations = new();

                    try
                    {
                        while (fetchedMessages.Any())
                        {
                            var current_deletion = fetchedMessages.Take(100);

                            deletionOperations.Add(ctx.Channel.DeleteMessagesAsync(current_deletion).ContinueWith(task =>
                            {
                                if (task.IsCompletedSuccessfully)
                                    deleted += current_deletion.Count();
                                else
                                    failed_deleted += current_deletion.Count();
                            }));

                            foreach (var b in current_deletion.ToList())
                                fetchedMessages.Remove(b);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to delete messages: {ex}");
                        PerformingActionEmbed.Description = $":x: `An error occured trying to delete the specified messages. The error has been reported, please try again in a few hours.`";
                        PerformingActionEmbed.Color = DiscordColor.Red;
                        PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                        await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                        return;
                    }

                    while (!deletionOperations.All(x => x.IsCompleted))
                    {
                        await Task.Delay(1000);
                        PerformingActionEmbed.Description = $"`Deleted {deleted}/{total} messages..`";
                        _ = msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                    }

                    PerformingActionEmbed.Description = $":white_check_mark: `Successfully deleted {total - failed_deleted} messages`";

                    if (FailedToDeleteAmount > 0)
                        PerformingActionEmbed.Description += $"\n`Failed to delete {failed_deleted} messages because they we're more than 14 days old`";

                    PerformingActionEmbed.Color = DiscordColor.Green;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Footer.Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator} • This message will auto-delete in 5 seconds";

                    _ = msg1.ModifyAsync(embed: PerformingActionEmbed.Build()).ContinueWith(_ =>
                    {
                        _ = Task.Delay(5000).ContinueWith(e =>
                        {
                            try
                            {
                                _ = msg1.DeleteAsync();
                            }
                            catch { }
                        });
                    });
                    return;
                }
                else
                {
                    List<DiscordMessage> bMessages = (await ctx.Channel.GetMessagesAsync(number)).ToList();

                    if (user is not null)
                    {
                        foreach (var b in bMessages.Where(x => x.Author.Id != user.Id).ToList())
                        {
                            bMessages.Remove(b);
                        }
                    }

                    foreach (var b in bMessages.Where(x => x.CreationTimestamp < DateTime.UtcNow.AddDays(-14)).ToList())
                    {
                        bMessages.Remove(b);
                        FailedToDeleteAmount++;
                    }

                    if (bMessages.Count > 0)
                        await ctx.Channel.DeleteMessagesAsync(bMessages);
                }

                if (FailedToDeleteAmount > 0)
                {
                    var PerformingActionEmbed = new DiscordEmbedBuilder
                    {
                        Title = "",
                        Description = $":x: `Failed to delete {FailedToDeleteAmount} messages because they we're more than 14 days old.`",
                        Color = DiscordColor.Red,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator} • This message will auto-delete in 5 seconds",
                            IconUrl = ctx.Member.AvatarUrl
                        },
                        Timestamp = DateTime.UtcNow
                    };
                    var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);
                    await Task.Delay(5000);
                    try
                    {
                        await msg1.DeleteAsync();
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error while trying to delete {number} messages\n\n{ex}");
            }
        }).Add(_watcher, ctx);
    }



    [Command("guild-purge"), Aliases("guild-clear", "server-purge", "server-clear"),
    CommandModule("mod"),
    Description("Scans the specified amount of messages for the given user's messages and deletes them. Similar to the `purge` command's behaviour.")]
    public async Task GuildPurge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.ManageMessages))
            {
                _ = ctx.SendPermissionError(Permissions.ManageMessages);
                return;
            }

            if (number is > 2000 or < 1)
            {
                _ = ctx.SendSyntaxError();
                return;
            }

            var status_embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Server Purge • {ctx.Guild.Name}" },
                Color = ColorHelper.Warning,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.Now,
                Description = $"`Scanning all channels for messages sent by '{user.UsernameWithDiscriminator}' ({user.Id})..`"
            };

            var status_message = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(status_embed));

            int current_prog = 0;
            int max_prog = ctx.Guild.Channels.Count;

            int all_msg = 0;
            Dictionary<ulong, List<DiscordMessage>> messages = new();

            foreach (var channel in ctx.Guild.Channels.Where(x => x.Value.Type is ChannelType.Text or ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.News))
            {
                all_msg = 0;
                foreach (var b in messages)
                    all_msg += b.Value.Count;

                current_prog++;

                status_embed.Description = $"`Scanning all channels for messages sent by '{user.UsernameWithDiscriminator}' ({user.Id})..`\n\n" +
                                            $"`Current Channel`: `({current_prog}/{max_prog})` {channel.Value.Mention} `({channel.Value.Id})`\n" +
                                            $"`Found Messages `: `{all_msg}`";
                await status_message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(status_embed));

                int MessageInt = number;

                List<DiscordMessage> requested_messages = new();

                var pre_request = await channel.Value.GetMessagesAsync(1);

                if (pre_request.Count > 0)
                {
                    requested_messages.Add(pre_request[0]);
                    MessageInt -= 1;
                }

                while (true)
                {
                    if (pre_request.Count == 0)
                        break;

                    if (MessageInt <= 0)
                        break;

                    if (MessageInt > 100)
                    {
                        var current_request = await channel.Value.GetMessagesBeforeAsync(requested_messages.Last().Id, 100);

                        if (current_request.Count == 0)
                            break;

                        foreach (var b in current_request)
                            requested_messages.Add(b);

                        MessageInt -= 100;
                    }
                    else
                    {
                        var current_request = await channel.Value.GetMessagesBeforeAsync(requested_messages.Last().Id, MessageInt);

                        if (current_request.Count == 0)
                            break;

                        foreach (var b in current_request)
                            requested_messages.Add(b);

                        MessageInt -= MessageInt;
                    }
                }

                if (requested_messages.Count > 0)
                    foreach (var b in requested_messages.ToList())
                    {
                        if (b.Author.Id == user.Id && b.CreationTimestamp.AddDays(14) > DateTime.UtcNow)
                        {
                            if (!messages.ContainsKey(channel.Key))
                                messages.Add(channel.Key, new List<DiscordMessage>());

                            messages[channel.Key].Add(b);
                        }
                    }
            }

            status_embed.Description = $"`Found {all_msg} messages sent by '{user.UsernameWithDiscriminator}' ({user.Id}). Deleting..`";
            await status_message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(status_embed));

            current_prog = 0;
            max_prog = messages.Count;

            foreach (var channel in messages)
            {
                current_prog++;
                status_embed.Description = $"`Found {all_msg} messages sent by '{user.UsernameWithDiscriminator}' ({user.Id}). Deleting..`\n\n" +
                                            $"`Current Channel`: `({current_prog}/{max_prog})` <#{channel.Key}> `({channel.Key})`\n" +
                                            $"`Found Messages `: `{all_msg}`";
                await status_message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(status_embed));

                await ctx.Guild.GetChannel(channel.Key).DeleteMessagesAsync(channel.Value);
            }

            status_embed.Description = $"`Finished operation.`";
            status_embed.Color = ColorHelper.Success;
            status_embed.Author.IconUrl = Resources.LogIcons.Info;
            await status_message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(status_embed));
        }).Add(_watcher, ctx);
    }



    [Command("timeout"), Aliases("time-out"),
    CommandModule("mod"),
    Description("Times the user for the specified amount of time out.")]
    public async Task Timeout(CommandContext ctx, DiscordMember victim, [Description("Duration")] string duration)
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.ModerateMembers))
            {
                _ = ctx.SendPermissionError(Permissions.ModerateMembers);
                return;
            }

            var PerformingActionEmbed = new DiscordEmbedBuilder
            {
                Title = "",
                Description = $"`Timing {victim.Username}#{victim.Discriminator} ({victim.Id}) out..`",
                Color = DiscordColor.Orange,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                if (!DateTime.TryParse(duration, out DateTime until))
                {
                    switch (duration[^1..])
                    {
                        case "Y":
                            until = DateTime.UtcNow.AddYears(Convert.ToInt32(duration.Replace("Y", "")));
                            break;
                        case "M":
                            until = DateTime.UtcNow.AddMonths(Convert.ToInt32(duration.Replace("M", "")));
                            break;
                        case "d":
                            until = DateTime.UtcNow.AddDays(Convert.ToInt32(duration.Replace("d", "")));
                            break;
                        case "h":
                            until = DateTime.UtcNow.AddHours(Convert.ToInt32(duration.Replace("h", "")));
                            break;
                        case "m":
                            until = DateTime.UtcNow.AddMinutes(Convert.ToInt32(duration.Replace("m", "")));
                            break;
                        case "s":
                            until = DateTime.UtcNow.AddSeconds(Convert.ToInt32(duration.Replace("s", "")));
                            break;
                        default:
                            until = DateTime.UtcNow.AddMinutes(Convert.ToInt32(duration));
                            return;
                    }
                }


                if (victim.IsProtected(_status))
                {
                    PerformingActionEmbed.Color = DiscordColor.Red;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Description = $"{DiscordEmoji.FromName(ctx.Client, ":x:")} `{victim.Username}#{victim.Discriminator} ({victim.Id}) couldn't be timed out.`";
                    await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                    return;
                }

                try
                {
                    await victim.TimeoutAsync(until);
                    PerformingActionEmbed.Color = DiscordColor.Green;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Description = $"{DiscordEmoji.FromName(ctx.Client, ":white_check_mark:")} `{victim.Username}#{victim.Discriminator} ({victim.Id}) was timed out for {until.GetTotalSecondsUntil().GetHumanReadable(TimeFormat.HOURS)}.`";
                }
                catch (Exception)
                {
                    PerformingActionEmbed.Color = DiscordColor.Red;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Description = $"{DiscordEmoji.FromName(ctx.Client, ":x:")} `{victim.Username}#{victim.Discriminator} ({victim.Id}) couldn't be timed out.`";
                }

                await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
            }
            catch (Exception)
            {
                _ = ctx.SendSyntaxError();
                return;
            }
        }).Add(_watcher, ctx);
    }



    [Command("remove-timeout"), Aliases("rm-timeout", "rmtimeout", "removetimeout"),
    CommandModule("mod"),
    Description("Removes the timeout for the specified user.")]
    public async Task RemoveTimeout(CommandContext ctx, DiscordMember victim)
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.ModerateMembers))
            {
                _ = ctx.SendPermissionError(Permissions.ModerateMembers);
                return;
            }

            var PerformingActionEmbed = new DiscordEmbedBuilder
            {
                Title = "",
                Description = $"`Removing timeout for {victim.Username}#{victim.Discriminator} ({victim.Id})..`",
                Color = DiscordColor.Orange,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                await victim.RemoveTimeoutAsync();
                PerformingActionEmbed.Color = DiscordColor.Green;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"{DiscordEmoji.FromName(ctx.Client, ":white_check_mark:")} `Removed timeout for {victim.Username}#{victim.Discriminator} ({victim.Id}).`";
            }
            catch (Exception)
            {
                PerformingActionEmbed.Color = DiscordColor.Red;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"{DiscordEmoji.FromName(ctx.Client, ":x:")} `Couldn't remove timeout for {victim.Username}#{victim.Discriminator} ({victim.Id}).`";
            }

            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
        }).Add(_watcher, ctx);
    }



    [Command("kick"),
    CommandModule("mod"),
    Description("Kicks the specified user.")]
    public async Task Kick(CommandContext ctx, DiscordMember victim, [Description("Reason")][RemainingText] string reason = "")
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.KickMembers))
            {
                _ = ctx.SendPermissionError(Permissions.KickMembers);
                return;
            }

            if (reason is null or "")
                reason = "-";

            var PerformingActionEmbed = new DiscordEmbedBuilder
            {
                Title = "",
                Description = $"`Kicking {victim.Username}#{victim.Discriminator} ({victim.Id})..`",
                Color = DiscordColor.Orange,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                await victim.RemoveAsync(reason);

                PerformingActionEmbed.Color = DiscordColor.Red;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"<@{victim.Id}> `{victim.Username}#{victim.Discriminator}` was kicked.\n\n" +
                                                        $"Reason: `{reason}`\n" +
                                                        $"Kicked by: {ctx.Member.Mention} `{ctx.Member.Username}#{ctx.Member.Discriminator}` (`{ctx.Member.Id}`)";
            }
            catch (Exception)
            {
                PerformingActionEmbed.Color = DiscordColor.Red;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $":x: Encountered an exception while trying to kick <@{victim.Id}> `{victim.Username}#{victim.Discriminator}`";
            }

            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
        }).Add(_watcher, ctx);
    }



    [Command("ban"),
    CommandModule("mod"),
    Description("Bans the specified user.")]
    public async Task Ban(CommandContext ctx, DiscordUser victim, [Description("Reason")][RemainingText] string reason = "")
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.BanMembers))
            {
                _ = ctx.SendPermissionError(Permissions.BanMembers);
                return;
            }

            if (reason is null or "")
                reason = "-";

            var PerformingActionEmbed = new DiscordEmbedBuilder
            {
                Title = "",
                Description = $"`Banning {victim.Username}#{victim.Discriminator} ({victim.Id})..`",
                Color = DiscordColor.Orange,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                await ctx.Guild.BanMemberAsync(victim.Id, 7, reason);

                PerformingActionEmbed.Color = DiscordColor.Red;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"<@{victim.Id}> `{victim.Username}#{victim.Discriminator}` was banned.\n\n" +
                                                        $"Reason: `{reason}`\n" +
                                                        $"Banned by: {ctx.Member.Mention} `{ctx.Member.Username}#{ctx.Member.Discriminator}` (`{ctx.Member.Id}`)";
            }
            catch (Exception)
            {
                PerformingActionEmbed.Color = DiscordColor.Red;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $":x: Encountered an exception while trying to ban <@{victim.Id}> `{victim.Username}#{victim.Discriminator}`";
            }

            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
        }).Add(_watcher, ctx);
    }



    [Command("unban"),
    CommandModule("mod"),
    Description("Unbans the specified user.")]
    public async Task Unban(CommandContext ctx, DiscordUser victim)
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.BanMembers))
            {
                _ = ctx.SendPermissionError(Permissions.BanMembers);
                return;
            }

            var PerformingActionEmbed = new DiscordEmbedBuilder
            {
                Title = "",
                Description = $"`Unbanning {victim.Username}#{victim.Discriminator} ({victim.Id})..`",
                Color = DiscordColor.Orange,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                    IconUrl = ctx.Member.AvatarUrl
                },
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                await ctx.Guild.UnbanMemberAsync(victim);

                PerformingActionEmbed.Color = DiscordColor.Green;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"<@{victim.Id}> `{victim.Username}#{victim.Discriminator}` was unbanned.";
            }
            catch (Exception)
            {
                PerformingActionEmbed.Color = DiscordColor.Red;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"<@{victim.Id}> `{victim.Username}#{victim.Discriminator}` **could not** be unbanned.";
            }

            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
        }).Add(_watcher, ctx);
    }
}
