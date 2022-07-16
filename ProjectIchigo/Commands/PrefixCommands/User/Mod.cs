namespace ProjectIchigo.PrefixCommands;
internal class Mod : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("purge"), Aliases("clear"),
    CommandModule("mod"),
    Description("Deletes the specified amount of messages")]
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
                        Color = EmbedColors.Processing,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                        },
                        Footer = ctx.GenerateUsedByFooter(),
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
                        PerformingActionEmbed.Description = $"❌ `No messages were found with the specified filter.`";
                        PerformingActionEmbed.Color = EmbedColors.Error;
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
                        _logger.LogError($"Failed to delete messages", ex);
                        PerformingActionEmbed.Description = $"❌ `An error occured trying to delete the specified messages. The error has been reported, please try again in a few hours.`";
                        PerformingActionEmbed.Color = EmbedColors.Error;
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

                    PerformingActionEmbed.Description = $"✅ `Successfully deleted {total - failed_deleted} messages`";

                    if (FailedToDeleteAmount > 0)
                        PerformingActionEmbed.Description += $"\n`Failed to delete {failed_deleted} messages because they we're more than 14 days old`";

                    PerformingActionEmbed.Color = EmbedColors.Success;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Footer = ctx.GenerateUsedByFooter("This message will auto-delete in 5 seconds");

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
                        Description = $"❌ `Failed to delete {FailedToDeleteAmount} messages because they we're more than 14 days old.`",
                        Color = EmbedColors.Error,
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Footer = ctx.GenerateUsedByFooter("This message will auto-delete in 5 seconds"),
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
                _logger.LogError($"Exception occured while trying to deleted {number} messages", ex);
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("guild-purge"), Aliases("guild-clear", "server-purge", "server-clear"),
    CommandModule("mod"),
    Description("Scans the specified amount of messages for the given user's messages and deletes them. Similar to the `purge` command's behaviour.")]
    public async Task GuildPurge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ ctx.Member.Id ].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
                return;

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
                Color = EmbedColors.Processing,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
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
            status_embed.Color = EmbedColors.Success;
            status_embed.Author.IconUrl = Resources.LogIcons.Info;
            await status_message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(status_embed));
        }).Add(_bot._watcher, ctx);
    }



    [Command("clearbackup"), Aliases("clearroles", "clearrole", "clearbackuproles", "clearbackuprole"),
    CommandModule("mod"),
    Description($"Clears the stored roles of a user.")]
    public async Task ClearBackup(CommandContext ctx, DiscordUser victim)
    {
        Task.Run(async () =>
        {
            if (!ctx.Member.Permissions.HasPermission(Permissions.ManageRoles))
            {
                _ = ctx.SendPermissionError(Permissions.ManageRoles);
                return;
            }

            if ((await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == victim.Id))
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"`{victim.Username}#{victim.Discriminator} ({victim.Id}) is on the server and therefor their stored nickname and roles cannot be cleared.`",
                    Color = EmbedColors.Error,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = victim.AvatarUrl
                    },
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

            if (!_bot._guilds.List.ContainsKey(ctx.Guild.Id))
                _bot._guilds.List.Add(ctx.Guild.Id, new Guilds.ServerSettings());

            if (!_bot._guilds.List[ctx.Guild.Id].Members.ContainsKey(victim.Id))
                _bot._guilds.List[ctx.Guild.Id].Members.Add(victim.Id, new());

            _bot._guilds.List[ctx.Guild.Id].Members[victim.Id].MemberRoles.Clear();
            _bot._guilds.List[ctx.Guild.Id].Members[victim.Id].SavedNickname = "";

            _ = ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Description = $"`Deleted stored nickname and roles for {victim.Username}#{victim.Discriminator} ({victim.Id}).`",
                Color = EmbedColors.StrongPunishment,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
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



    [Command("timeout"), Aliases("time-out", "mute"),
    CommandModule("mod"),
    Description("Times the user for the specified amount of time out")]
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
                Color = EmbedColors.Processing,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            if (string.IsNullOrWhiteSpace(duration))
                duration = "30m";

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

                if (DateTime.UtcNow > until)
                {
                    _ = ctx.SendSyntaxError();
                    return;
                }

                if (victim.IsProtected(_bot._status))
                {
                    PerformingActionEmbed.Color = EmbedColors.Error;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Description = $"❌ `{victim.Username}#{victim.Discriminator} ({victim.Id}) couldn't be timed out.`";
                    await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
                    return;
                }

                try
                {
                    await victim.TimeoutAsync(until);
                    PerformingActionEmbed.Color = EmbedColors.Success;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Description = $"✅ `{victim.Username}#{victim.Discriminator} ({victim.Id}) was timed out for {until.GetTotalSecondsUntil().GetHumanReadable(TimeFormat.HOURS)}.`";
                }
                catch (Exception)
                {
                    PerformingActionEmbed.Color = EmbedColors.Error;
                    PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                    PerformingActionEmbed.Description = $"❌ `{victim.Username}#{victim.Discriminator} ({victim.Id}) couldn't be timed out.`";
                }

                await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
            }
            catch (Exception)
            {
                _ = ctx.SendSyntaxError();
                return;
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("remove-timeout"), Aliases("rm-timeout", "rmtimeout", "removetimeout", "unmute"),
    CommandModule("mod"),
    Description("Removes the timeout for the specified user")]
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
                Color = EmbedColors.Processing,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                await victim.RemoveTimeoutAsync();
                PerformingActionEmbed.Color = EmbedColors.Success;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"✅ `Removed timeout for {victim.Username}#{victim.Discriminator} ({victim.Id}).`";
            }
            catch (Exception)
            {
                PerformingActionEmbed.Color = EmbedColors.Error;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"❌ `Couldn't remove timeout for {victim.Username}#{victim.Discriminator} ({victim.Id}).`";
            }

            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
        }).Add(_bot._watcher, ctx);
    }



    [Command("kick"),
    CommandModule("mod"),
    Description("Kicks the specified user")]
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
                Color = EmbedColors.Processing,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                await victim.RemoveAsync(reason);

                PerformingActionEmbed.Color = EmbedColors.Success;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"<@{victim.Id}> `{victim.Username}#{victim.Discriminator}` was kicked.\n\n" +
                                                        $"Reason: `{reason}`\n" +
                                                        $"Kicked by: {ctx.Member.Mention} `{ctx.Member.Username}#{ctx.Member.Discriminator}` (`{ctx.Member.Id}`)";
            }
            catch (Exception)
            {
                PerformingActionEmbed.Color = EmbedColors.Error;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"❌ Encountered an exception while trying to kick <@{victim.Id}> `{victim.Username}#{victim.Discriminator}`";
            }

            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
        }).Add(_bot._watcher, ctx);
    }



    [Command("ban"),
    CommandModule("mod"),
    Description("Bans the specified user")]
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
                Color = EmbedColors.Processing,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                await ctx.Guild.BanMemberAsync(victim.Id, 7, reason);

                PerformingActionEmbed.Color = EmbedColors.Success;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"<@{victim.Id}> `{victim.Username}#{victim.Discriminator}` was banned.\n\n" +
                                                        $"Reason: `{reason}`\n" +
                                                        $"Banned by: {ctx.Member.Mention} `{ctx.Member.Username}#{ctx.Member.Discriminator}` (`{ctx.Member.Id}`)";
            }
            catch (Exception)
            {
                PerformingActionEmbed.Color = EmbedColors.Error;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"❌ Encountered an exception while trying to ban <@{victim.Id}> `{victim.Username}#{victim.Discriminator}`";
            }

            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
        }).Add(_bot._watcher, ctx);
    }



    [Command("unban"),
    CommandModule("mod"),
    Description("Unbans the specified user")]
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
                Color = EmbedColors.Processing,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            var msg1 = await ctx.Channel.SendMessageAsync(embed: PerformingActionEmbed);

            try
            {
                await ctx.Guild.UnbanMemberAsync(victim);

                PerformingActionEmbed.Color = EmbedColors.Success;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"<@{victim.Id}> `{victim.Username}#{victim.Discriminator}` was unbanned.";
            }
            catch (Exception)
            {
                PerformingActionEmbed.Color = EmbedColors.Error;
                PerformingActionEmbed.Author.IconUrl = ctx.Guild.IconUrl;
                PerformingActionEmbed.Description = $"<@{victim.Id}> `{victim.Username}#{victim.Discriminator}` **could not** be unbanned.";
            }

            await msg1.ModifyAsync(embed: PerformingActionEmbed.Build());
        }).Add(_bot._watcher, ctx);
    }
}
