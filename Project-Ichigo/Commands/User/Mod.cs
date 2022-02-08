namespace Project_Ichigo.Commands.User;
internal class Mod : BaseCommandModule
{
    [Command("emoji"), Aliases("emote"),
    CommandModule("mod"),
    Description("Steals emojis of the message that this command was replied to.")]
    public async Task EmojiStealCommand(CommandContext ctx)
    {
        _ = Task.Run(async () =>
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
        });
    }
}
