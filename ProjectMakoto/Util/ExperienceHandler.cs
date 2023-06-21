// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

internal sealed class ExperienceHandler : RequiresBotReference
{
    internal ExperienceHandler(Bot _bot) : base(_bot)
    {
    }

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

        if (RegexTemplates.Url.IsMatch(message.Content))
        {
            string ModifiedString = RegexTemplates.Url.Replace(message.Content, "");

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

        if (!this.Bot.Guilds[guild.Id].Experience.UseExperience)
            return;

        if (this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Points is > (long.MaxValue - 10000) or < (long.MinValue + 10000))
        {
            _logger.LogWarn("Member '{User}' on '{Guild}' is within 10000 points of the experience limit. Resetting.", user.Id, guild.Id);
            this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Points = 1;
        }

        this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Points += Amount;

        long PreviousLevel = this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level;

        CheckExperience(user.Id, guild);

        if (this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level != PreviousLevel && channel != null && channel.Type is ChannelType.Text or ChannelType.PublicThread or ChannelType.PrivateThread)
        {
            DiscordEmbedBuilder embed = null;

            if (this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level > PreviousLevel)
            {
                string build = $":stars: Congrats, {user.Mention}! You gained {(this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level - PreviousLevel is 1 ? $"{this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level - PreviousLevel} level" : $"{this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level - PreviousLevel} levels")}.\n\n" +
                                $"You're now on Level {this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level}.";

                int delete_delay = 10000;

                if (this.Bot.Guilds[guild.Id].LevelRewards.Any(x => x.Level <= this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level))
                {
                    build += "\n\n";

                    foreach (var reward in this.Bot.Guilds[guild.Id].LevelRewards.ToList().Where(x => x.Level <= this.Bot.Guilds[guild.Id].Members[user.Id].Experience.Level))
                    {
                        if (!guild.Roles.ContainsKey(reward.RoleId))
                        {
                            this.Bot.Guilds[guild.Id].LevelRewards.Remove(reward);
                            continue;
                        }

                        if (user.Roles.Any(x => x.Id == reward.RoleId))
                            continue;

                        delete_delay = 20000;

                        await user.GrantRoleAsync(guild.GetRole(reward.RoleId));

                        build += $"`{reward.Message.Replace("##Role##", $"{guild.GetRole(reward.RoleId).Name}").SanitizeForCode()}`\n";
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
                                this.Bot.DiscordClient.ComponentInteractionCreated -= RunInteraction;

                                this.Bot.Users[user.Id].ExperienceUser.DirectMessageOptOut = true;

                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                await (await user.CreateDmChannelAsync()).SendMessageAsync($"Alright, i will no longer send you any level up notifications via DM. If you wan't to re-enable this, run `;;levelrewards-optin` on any _bot._guilds with {this.Bot.DiscordClient.CurrentUser.Mention}.");
                            }
                        }).Add(this.Bot);
                    }

                    IEnumerable<DiscordComponent> discordComponents = new List<DiscordComponent>
                    {
                        { new DiscordButtonComponent(ButtonStyle.Secondary, "opt-out-experience-dm", "Disable Direct Message Experience Notifications", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⛔"))) },
                    };

                    msg = await (await user.CreateDmChannelAsync()).SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(discordComponents));

                    this.Bot.DiscordClient.ComponentInteractionCreated += RunInteraction;

                    try
                    {
                        await Task.Delay(3600000);
                        embed.Footer.Text += " • Interaction timed out";
                        await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                        this.Bot.DiscordClient.ComponentInteractionCreated -= RunInteraction;
                    }
                    catch { }
                }
            }
        }
    }

    internal void CheckExperience(ulong user, DiscordGuild guild)
    {
        long PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(this.Bot.Guilds[guild.Id].Members[user].Experience.Level - 1);
        long RequiredRepuationForNextLevel = CalculateLevelRequirement(this.Bot.Guilds[guild.Id].Members[user].Experience.Level);

        while (RequiredRepuationForNextLevel <= this.Bot.Guilds[guild.Id].Members[user].Experience.Points)
        {
            this.Bot.Guilds[guild.Id].Members[user].Experience.Level++;

            PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(this.Bot.Guilds[guild.Id].Members[user].Experience.Level - 1);
            RequiredRepuationForNextLevel = CalculateLevelRequirement(this.Bot.Guilds[guild.Id].Members[user].Experience.Level);
        }

        while (PreviousRequiredRepuationForNextLevel >= this.Bot.Guilds[guild.Id].Members[user].Experience.Points)
        {
            this.Bot.Guilds[guild.Id].Members[user].Experience.Level--;

            PreviousRequiredRepuationForNextLevel = CalculateLevelRequirement(this.Bot.Guilds[guild.Id].Members[user].Experience.Level - 1);
        }
    }

    internal long CalculateLevelRequirement(long Level)
    {
        if (!this.LevelCache.ContainsKey(Level))
        {
            long v = (long)Math.Ceiling(Math.Pow((double)Level, 1.60) * 92);
            this.LevelCache.Add(Level, v);
        }

        return this.LevelCache[Level];
    }
}
