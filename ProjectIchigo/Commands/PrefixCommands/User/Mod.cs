namespace ProjectIchigo.PrefixCommands;
internal class Mod : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("purge"), Aliases("clear"),
    CommandModule("mod"),
    Description("Deletes the specified amount of messages")]
    public async Task Purge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new PurgeCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "number", number },
                { "victim", victim },
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("guild-purge"), Aliases("guild-clear", "server-purge", "server-clear"),
    CommandModule("mod"),
    Description("Scans the specified amount of messages for the given user's messages and deletes them. Similar to the `purge` command's behaviour.")]
    public async Task GuildPurge(CommandContext ctx, [Description("1-2000")] int number, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
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
