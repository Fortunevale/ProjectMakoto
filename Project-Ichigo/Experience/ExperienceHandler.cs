namespace Project_Ichigo.Experience;

internal class ExperienceHandler
{
    internal ExperienceHandler(DiscordClient client, TaskWatcher.TaskWatcher watcher, ServerInfo server, Users users)
    {
        this.client = client;
        this.watcher = watcher;
        this.server = server;
        this.users = users;
    }

    DiscordClient client { get; set; }
    TaskWatcher.TaskWatcher watcher { get; set; }
    ServerInfo server { get; set; }
    Users users { get; set; }


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

        if (!server.Servers[guild.Id].ExperienceSettings.UseExperience)
            return;

        if (server.Servers[guild.Id].Members[user.Id].Experience is > (long.MaxValue - 10000) or < (long.MinValue + 10000))
        {
            LogWarn($"Member '{user.Id}' on '{guild.Id}' is within 10000 points of the experience limit. Resetting.");
            server.Servers[guild.Id].Members[user.Id].Experience = 1;
        }

        server.Servers[guild.Id].Members[user.Id].Experience += Amount;

        long PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user.Id].Level - 1);
        long RequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user.Id].Level);

        long PreviousLevel = server.Servers[guild.Id].Members[user.Id].Level;

        while (RequiredRepuationForNextLevel <= server.Servers[guild.Id].Members[user.Id].Experience)
        {
            server.Servers[guild.Id].Members[user.Id].Level++;

            PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user.Id].Level - 1);
            RequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user.Id].Level);
        }

        while (PreviousRequiredRepuationForNextLevel >= server.Servers[guild.Id].Members[user.Id].Experience)
        {
            server.Servers[guild.Id].Members[user.Id].Level--;

            PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user.Id].Level - 1);
            RequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user.Id].Level);
        }

        if (server.Servers[guild.Id].Members[user.Id].Level != PreviousLevel && channel != null && channel.Type is ChannelType.Text or ChannelType.PublicThread or ChannelType.PrivateThread)
        {
            DiscordEmbedBuilder embed = null;

            if (server.Servers[guild.Id].Members[user.Id].Level > PreviousLevel)
            {
                embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = guild.IconUrl,
                        Name = guild.Name
                    },
                    Title = "",
                    Description = $":stars: Congrats, {user.Mention}! You gained {(server.Servers[guild.Id].Members[user.Id].Level - PreviousLevel is 1 ? $"{server.Servers[guild.Id].Members[user.Id].Level - PreviousLevel} level" : $"{server.Servers[guild.Id].Members[user.Id].Level - PreviousLevel} levels")}.\n\n" +
                                  $"You're now on Level {server.Servers[guild.Id].Members[user.Id].Level}.",
                    Timestamp = DateTime.UtcNow,
                    Color = new DiscordColor("#4287f5"),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = user.AvatarUrl },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "This message will be automatically deleted in 10 seconds"
                    }
                };

                if (channel is not null)
                {
                    _ = channel.SendMessageAsync($"{user.Mention}", embed).ContinueWith(async x =>
                    {
                        if (!x.IsCompletedSuccessfully)
                            return;

                        await Task.Delay(10000);
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
                                client.ComponentInteractionCreated -= RunInteraction;

                                users.List[user.Id].ExperienceUserSettings.DirectMessageOptOut = true;

                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                await (await user.CreateDmChannelAsync()).SendMessageAsync($"Alright, i will no longer send you any level up notifications via DM. If you wan't to re-enable this, run `;;levelrewards-optin` on any server with {client.CurrentUser.Mention}.");
                            }
                        }).Add(watcher);
                    }

                    IEnumerable<DiscordComponent> discordComponents = new List<DiscordComponent>
                    {
                        { new DiscordButtonComponent(ButtonStyle.Secondary, "opt-out-experience-dm", "Disable Direct Message Experience Notifications", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":no_entry:"))) },
                    };

                    msg = await (await user.CreateDmChannelAsync()).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(discordComponents));

                    client.ComponentInteractionCreated += RunInteraction;

                    try
                    {
                        await Task.Delay(3600000);
                        embed.Footer.Text += " • Interaction timed out";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                        client.ComponentInteractionCreated -= RunInteraction;
                    }
                    catch { }
                }
            }
        }
    }

    internal void CheckExperience(ulong user, DiscordGuild guild)
    {
        long PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user].Level - 1);
        long RequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user].Level);

        while (RequiredRepuationForNextLevel <= server.Servers[guild.Id].Members[user].Experience)
        {
            server.Servers[guild.Id].Members[user].Level++;

            PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user].Level - 1);
            RequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user].Level);
        }

        while (PreviousRequiredRepuationForNextLevel >= server.Servers[guild.Id].Members[user].Experience)
        {
            server.Servers[guild.Id].Members[user].Level--;

            PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(server.Servers[guild.Id].Members[user].Level - 1);
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
