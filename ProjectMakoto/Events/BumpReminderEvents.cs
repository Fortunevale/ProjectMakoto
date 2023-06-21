// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;

internal sealed class BumpReminderEvents : RequiresTranslation
{
    public BumpReminderEvents(Bot bot) : base(bot)
    {
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Guild is null || e.Channel is null || e.Channel.IsPrivate || !this.Bot.Guilds[e.Guild.Id].BumpReminder.Enabled || e.Channel.Id != this.Bot.Guilds[e.Guild.Id].BumpReminder.ChannelId)
            return;

        DiscordUser bUser = null;

        var getuser = sender.GetUserAsync(this.Bot.Guilds[e.Guild.Id].BumpReminder.LastUserId, true).ContinueWith(x =>
        {
            if (x.IsCompletedSuccessfully)
                bUser = x.Result;
        });

        getuser.Wait(10000);

        if (!(e.Author.Id == sender.CurrentUser.Id && e.Message.Embeds.Any()))
            this.Bot.BumpReminder.SendPersistentMessage(sender, e.Channel, bUser);

        if (e.Author.Id != this.Bot.status.LoadedConfig.Accounts.Disboard || !e.Message.Embeds.Any())
            return;

        if (e.Message.Embeds[0].Description.ToLower().Contains(":thumbsup:"))
        {
            this.Bot.Guilds[e.Guild.Id].BumpReminder.LastBump = DateTime.UtcNow;
            this.Bot.Guilds[e.Guild.Id].BumpReminder.LastReminder = DateTime.UtcNow;
            this.Bot.Guilds[e.Guild.Id].BumpReminder.BumpsMissed = 0;

            try
            {
                DiscordMember _bumper;

                if (e.Message.MessageType is MessageType.ChatInputCommand)
                {
                    _bumper = await e.Message.Interaction.User.ConvertToMember(e.Guild);
                }
                else
                {
                    List<string> Mentions = e.Message.Embeds[0].Description.ToLower().GetMentions();

                    if (Mentions is null || Mentions.Count is 0)
                        throw new Exception("No mentions in message");

                    _bumper = await e.Guild.GetMemberAsync(Convert.ToUInt64(Regex.Match(Mentions.First(), @"\d+").Value));
                }

                this.Bot.Guilds[e.Guild.Id].BumpReminder.LastUserId = _bumper.Id;

                _ = e.Channel.SendMessageAsync($"**{_bumper.Mention} Thanks a lot for supporting the server!**\n\n" +
                                            $"_**You can subscribe and unsubscribe to the bump reminder notifications at any time by reacting to the pinned message!**_");

                try
                {
                    if (this.Bot.Guilds[e.Guild.Id].Experience.UseExperience && this.Bot.Guilds[e.Guild.Id].Experience.BoostXpForBumpReminder)
                        this.Bot.ExperienceHandler.ModifyExperience(_bumper, e.Guild, e.Channel, 50);
                }
                catch { }

                this.Bot.BumpReminder.ScheduleBump(sender, e.Guild.Id);
            }
            catch (Exception)
            {
                this.Bot.Guilds[e.Guild.Id].BumpReminder.LastUserId = 0;
                this.Bot.BumpReminder.ScheduleBump(sender, e.Guild.Id);

                throw;
            }
        }
        else
        {
            if (this.Bot.Guilds[e.Guild.Id].BumpReminder.LastBump < DateTime.UtcNow.AddHours(-2))
            {
                if (e.Message.Embeds[0].Description.ToLower().Contains("please wait another"))
                {
                    string _embedDescription = e.Message.Embeds[0].Description.ToLower();

                    try
                    {
                        _embedDescription = _embedDescription.Remove(0, _embedDescription.IndexOf(">"));
                        int _minutes = Int32.Parse(Regex.Match(_embedDescription, @"\d+").Value);

                        this.Bot.Guilds[e.Guild.Id].BumpReminder.LastBump = DateTime.UtcNow.AddMinutes(_minutes - 120);
                        this.Bot.Guilds[e.Guild.Id].BumpReminder.LastReminder = DateTime.UtcNow.AddMinutes(_minutes - 120);
                        this.Bot.Guilds[e.Guild.Id].BumpReminder.LastUserId = 0;

                        e.Channel.SendMessageAsync($"⚠ It seems the last bump was not registered properly.\n" +
                            $"The last time the server was bumped was determined to be around {Formatter.Timestamp(this.Bot.Guilds[e.Guild.Id].BumpReminder.LastBump, TimestampFormat.LongDateTime)}.").Add(this.Bot);

                        this.Bot.BumpReminder.ScheduleBump(sender, e.Guild.Id);
                    }
                    catch (Exception ex) { _logger.LogDebug(ex.ToString()); }
                }
            }
        }
    }

    internal async Task MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
    {
        if (e.Guild == null || e.Channel.IsPrivate || !this.Bot.Guilds[e.Guild.Id].BumpReminder.Enabled || e.Channel.Id != this.Bot.Guilds[e.Guild.Id].BumpReminder.ChannelId)
            return;

        if (e.Message.Id == this.Bot.Guilds[e.Guild.Id].BumpReminder.PersistentMessageId)
        {
            DiscordUser bUser = null;

            var getuser = sender.GetUserAsync(this.Bot.Guilds[e.Guild.Id].BumpReminder.LastUserId, true).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                    bUser = x.Result;
            });

            getuser.Wait(10000);

            this.Bot.BumpReminder.SendPersistentMessage(sender, e.Channel, bUser);
        }
    }

    internal async Task ReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        if (e.Guild == null || e.Channel.IsPrivate || !this.Bot.Guilds[e.Guild.Id].BumpReminder.Enabled || e.Channel.Id != this.Bot.Guilds[e.Guild.Id].BumpReminder.ChannelId)
            return;

        if (e.Message.Id == this.Bot.Guilds[e.Guild.Id].BumpReminder.MessageId && e.Emoji.ToString() == "✅")
        {
            var member = await e.Guild.GetMemberAsync(e.User.Id);

            await member.GrantRoleAsync(e.Guild.GetRole(this.Bot.Guilds[e.Guild.Id].BumpReminder.RoleId));
        }
    }

    internal async Task ReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        if (e.Guild == null || e.Channel.IsPrivate || !this.Bot.Guilds[e.Guild.Id].BumpReminder.Enabled || e.Channel.Id != this.Bot.Guilds[e.Guild.Id].BumpReminder.ChannelId)
            return;

        if (e.Message.Id == this.Bot.Guilds[e.Guild.Id].BumpReminder.MessageId && e.Emoji.ToString() == "✅")
        {
            var member = await e.Guild.GetMemberAsync(e.User.Id);

            await member.RevokeRoleAsync(e.Guild.GetRole(this.Bot.Guilds[e.Guild.Id].BumpReminder.RoleId));
        }
    }
}
