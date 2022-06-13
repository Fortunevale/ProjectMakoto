namespace ProjectIchigo.Experience;

internal class ExperienceHandler
{
    internal ExperienceHandler(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }


    private Dictionary<long, long> LevelCache = new();

    internal int CalculateMessageExperience(DiscordMessage message)
    {
        if (message.Content.Length > 0)
        {
            if (Regex.IsMatch(message.Content, @"^(-|>>|;;|\$|\!|\!d|owo |/)"))
                return 0;
        }

        int Points = 1;

        if (message.ReferencedMessage is not null)
            Points += 2;

        if (message.Attachments is not null && string.IsNullOrWhiteSpace(message.Content))
            Points -= 1;

        if (Regex.IsMatch(message.Content, Resources.Regex.Url))
        {
            string ModifiedString = Regex.Replace(message.Content, Resources.Regex.Url, "");

            if (ModifiedString.Length > 10)
                Points += 1;

            if (ModifiedString.Length > 25)
                Points += 1;

            if (ModifiedString.Length > 50)
                Points += 1;

            if (ModifiedString.Length > 75)
                Points += 1;
        }
        else
        {
            if (message.Content.Length > 10)
                Points += 1;

            if (message.Content.Length > 25)
                Points += 1;

            if (message.Content.Length > 50)
                Points += 1;

            if (message.Content.Length > 75)
                Points += 1;
        }

        return Points;
    }

    internal async void ModifyExperience(ulong user, DiscordGuild guild, DiscordChannel channel, int Amount) => ModifyExperience(await guild.GetMemberAsync(user), guild, channel, Amount);
    internal async void ModifyExperience(DiscordUser user, DiscordGuild guild, DiscordChannel channel, int Amount) => ModifyExperience(await user.ConvertToMember(guild), guild, channel, Amount);

    internal async void ModifyExperience(DiscordMember user, DiscordGuild guild, DiscordChannel channel, int Amount)
    {
        if (user.IsBot)
            return;

        if (!_bot._guilds.List[guild.Id].ExperienceSettings.UseExperience)
            return;

        if (_bot._guilds.List[guild.Id].Members[user.Id].Experience is > (long.MaxValue - 10000) or < (long.MinValue + 10000))
        {
            LogWarn($"Member '{user.Id}' on '{guild.Id}' is within 10000 points of the experience limit. Resetting.");
            _bot._guilds.List[guild.Id].Members[user.Id].Experience = 1;
        }

        _bot._guilds.List[guild.Id].Members[user.Id].Experience += Amount;

        long PreviousLevel = _bot._guilds.List[guild.Id].Members[user.Id].Level;

        CheckExperience(user.Id, guild);

        if (_bot._guilds.List[guild.Id].Members[user.Id].Level != PreviousLevel && channel != null && channel.Type is ChannelType.Text or ChannelType.PublicThread or ChannelType.PrivateThread)
        {
            DiscordEmbedBuilder embed = null;

            if (_bot._guilds.List[guild.Id].Members[user.Id].Level > PreviousLevel)
            {
                string build = $":stars: Congrats, {user.Mention}! You gained {(_bot._guilds.List[guild.Id].Members[user.Id].Level - PreviousLevel is 1 ? $"{_bot._guilds.List[guild.Id].Members[user.Id].Level - PreviousLevel} level" : $"{_bot._guilds.List[guild.Id].Members[user.Id].Level - PreviousLevel} levels")}.\n\n" +
                                $"You're now on Level {_bot._guilds.List[guild.Id].Members[user.Id].Level}.";

                int delete_delay = 10000;

                if (_bot._guilds.List[guild.Id].LevelRewards.Any(x => x.Level <= _bot._guilds.List[guild.Id].Members[user.Id].Level))
                {
                    build += "\n\n";

                    foreach (var reward in _bot._guilds.List[guild.Id].LevelRewards.ToList().Where(x => x.Level <= _bot._guilds.List[guild.Id].Members[user.Id].Level))
                    {
                        if (!guild.Roles.ContainsKey(reward.RoleId))
                        {
                            _bot._guilds.List[guild.Id].LevelRewards.Remove(reward);
                            continue;
                        }

                        if (user.Roles.Any(x => x.Id == reward.RoleId))
                            continue;

                        delete_delay = 20000;

                        await user.GrantRoleAsync(guild.GetRole(reward.RoleId));

                        build += $"`{reward.Message.Replace("##Role##", $"{guild.GetRole(reward.RoleId).Name}").SanitizeForCodeBlock()}`\n";
                    }
                }

                embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = guild.IconUrl,
                        Name = guild.Name
                    },
                    Title = "",
                    Description = build,
                    Timestamp = DateTime.UtcNow,
                    Color = new DiscordColor("#4287f5"),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = user.AvatarUrl },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"This message will be automatically deleted in {delete_delay / 1000} seconds"
                    }
                };

                if (channel is not null)
                {
                    _ = channel.SendMessageAsync($"{user.Mention}", embed).ContinueWith(async x =>
                    {
                        if (!x.IsCompletedSuccessfully)
                            return;

                        await Task.Delay(delete_delay);
                        _ = x.Result.DeleteAsync();
                    });
                }
                else
                {
                    DiscordMessage msg;

                    async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                    {
                        Task.Run(async () =>
                        {
                            if (msg.Id == e.Message.Id)
                            {
                                _bot.discordClient.ComponentInteractionCreated -= RunInteraction;

                                _bot._users.List[user.Id].ExperienceUserSettings.DirectMessageOptOut = true;

                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                await (await user.CreateDmChannelAsync()).SendMessageAsync($"Alright, i will no longer send you any level up notifications via DM. If you wan't to re-enable this, run `;;levelrewards-optin` on any _bot._guilds with {_bot.discordClient.CurrentUser.Mention}.");
                            }
                        }).Add(_bot._watcher);
                    }

                    IEnumerable<DiscordComponent> discordComponents = new List<DiscordComponent>
                    {
                        { new DiscordButtonComponent(ButtonStyle.Secondary, "opt-out-experience-dm", "Disable Direct Message Experience Notifications", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⛔"))) },
                    };

                    msg = await (await user.CreateDmChannelAsync()).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(discordComponents));

                    _bot.discordClient.ComponentInteractionCreated += RunInteraction;

                    try
                    {
                        await Task.Delay(3600000);
                        embed.Footer.Text += " • Interaction timed out";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                        _bot.discordClient.ComponentInteractionCreated -= RunInteraction;
                    }
                    catch { }
                }
            }
        }
    }

    internal void CheckExperience(ulong user, DiscordGuild guild)
    {
        long PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(_bot._guilds.List[guild.Id].Members[user].Level - 1);
        long RequiredRepuationForNextLevel = CalculateLevelRequirement(_bot._guilds.List[guild.Id].Members[user].Level);

        while (RequiredRepuationForNextLevel <= _bot._guilds.List[guild.Id].Members[user].Experience)
        {
            _bot._guilds.List[guild.Id].Members[user].Level++;

            PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(_bot._guilds.List[guild.Id].Members[user].Level - 1);
            RequiredRepuationForNextLevel = CalculateLevelRequirement(_bot._guilds.List[guild.Id].Members[user].Level);
        }

        while (PreviousRequiredRepuationForNextLevel >= _bot._guilds.List[guild.Id].Members[user].Experience)
        {
            _bot._guilds.List[guild.Id].Members[user].Level--;

            PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(_bot._guilds.List[guild.Id].Members[user].Level - 1);
        }
    }

    internal long CalculateLevelRequirement(long Level)
    {
        if (!LevelCache.ContainsKey(Level))
        {
            long v = (long)Math.Ceiling(Math.Pow((double)Level, 1.60) * 92);
            LevelCache.Add(Level, v);
        }

        return LevelCache[Level];
    }
}
