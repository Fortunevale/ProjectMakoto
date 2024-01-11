// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Entities.Guilds;

public sealed class VcCreatorSettings : RequiresParent<Guild>
{
    public VcCreatorSettings(Bot bot, Guild parent) : base(bot, parent)
    {
        this.CreatedChannelsUpdated();
    }

    Translations.events.vcCreator tKey
        => this.Bot.LoadedTranslations.Events.VcCreator;

    private DiscordGuild cachedGuild { get; set; }

    [ColumnName("vccreator_channelid"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong Channel
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "vccreator_channelid", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "vccreator_channelid", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("vccreator_channellist"), ColumnType(ColumnTypes.LongText), WithCollation, Default("[]")]
    public VcCreatorDetails[] CreatedChannels
    {
        get => JsonConvert.DeserializeObject<VcCreatorDetails[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "vccreator_channellist", this.Bot.DatabaseClient.mainDatabaseConnection)).Select(x =>
        {
            x.Bot = this.Bot;
            x.Parent = this;

            return x;
        }).ToArray();
        set
        {
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "vccreator_channellist", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
            this.CreatedChannelsUpdated();
        }
    }

    [JsonIgnore]
    public Dictionary<ulong, DateTime> LastCreatedChannel = new();

    private void CreatedChannelsUpdated()
    {
        _ = Task.Run(async () =>
        {
            while (!this.Bot.status.DiscordGuildDownloadCompleted)
                await Task.Delay(1000);

            this.cachedGuild ??= await this.Bot.DiscordClient.GetShard(this.Parent.Id).GetGuildAsync(this.Parent.Id);

            await Task.Delay(5000);

            for (var i = 0; i < this.CreatedChannels.Length; i++)
            {
                var b = this.CreatedChannels.ElementAt(i);

                if (!this.cachedGuild.Channels.ContainsKey(b.OwnerId))
                {
                    _logger.LogDebug("Channel '{Channel}' was deleted, deleting Vc Creator Entry.", b.OwnerId);
                    this.CreatedChannels = this.CreatedChannels.Remove(x => x.ChannelId.ToString(), b);
                    i--;
                }
            }

            foreach (var b in this.CreatedChannels)
                if (!b.EventsRegistered)
                {
                    _ = Task.Run(async () =>
                    {
                        b.EventsRegistered = true;
                        async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
                        {
                            _ = Task.Run(async () =>
                            {
                                if (e.Before?.Channel?.Id == b.ChannelId || e.After?.Channel?.Id == b.ChannelId)
                                {
                                    var channel = (e.After?.Channel?.Id != 0 ? e.After.Channel : null) ?? e.Before.Channel;
                                    var users = channel.Users.Where(x => !x.IsBot).ToList();

                                    if (users.Count <= 0)
                                    {
                                        _logger.LogDebug("Channel '{Channel}' is now empty, deleting.", b.ChannelId);

                                        await channel.DeleteAsync();
                                        this.CreatedChannels = this.CreatedChannels.Remove(x => x.ChannelId.ToString(), b);
                                        return;
                                    }

                                    if (e.User.Id == b.OwnerId && e.After?.Channel?.Id != b.ChannelId)
                                    {
                                        _logger.LogDebug("The owner of channel '{Channel}' left, assigning new owner.", b.ChannelId);
                                        var newOwner = users.SelectRandom();

                                        b.OwnerId = newOwner.Id;

                                        _ = await channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription(this.tKey.NewOwner.Get(this.Parent).Build(true, new TVar("User", newOwner.Mention))).WithColor(EmbedColors.Info));
                                        return;
                                    }

                                    if (b.BannedUsers.Contains(e.After?.User?.Id ?? 0))
                                    {
                                        var u = await e.User.ConvertToMember(this.cachedGuild);

                                        _logger.LogDebug("Banned user in channel '{Channel}' joined, disconnecting.", b.ChannelId);
                                        if (u.Permissions.HasPermission(Permissions.Administrator) || u.Permissions.HasPermission(Permissions.ManageChannels) || u.Permissions.HasPermission(Permissions.ModerateMembers) || u.Permissions.HasPermission(Permissions.KickMembers) || u.Permissions.HasPermission(Permissions.BanMembers) || u.Permissions.HasPermission(Permissions.MuteMembers) || u.Permissions.HasPermission(Permissions.DeafenMembers))
                                            return;

                                        await u.DisconnectFromVoiceAsync();
                                        return;
                                    }

                                    if (e.Before?.Channel?.Id != e.After?.Channel?.Id)
                                    {
                                        if (e.After?.Channel?.Id == b.ChannelId)
                                        {
                                            _ = await channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription(this.tKey.UserJoined.Get(this.Parent).Build(true, new TVar("User", e.User.Mention))).WithColor(EmbedColors.Success).WithAuthor(this.Bot.LoadedTranslations.Events.Actionlog.UserJoined.Get(this.Parent), "", AuditLogIcons.UserAdded));
                                        }
                                        else
                                        {
                                            _ = await channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription(this.tKey.UserLeft.Get(this.Parent).Build(true, new TVar("User", e.User.Mention))).WithColor(EmbedColors.Error).WithAuthor(this.Bot.LoadedTranslations.Events.Actionlog.UserLeft.Get(this.Parent), "", AuditLogIcons.UserLeft));
                                        }
                                    }
                                }
                            }).Add(this.Bot);
                        }

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(5000);

                            var channel = await this.Bot.DiscordClient.GetShard(this.Parent.Id).GetChannelAsync(b.ChannelId);

                            if (channel.Users.Count <= 0)
                            {
                                _logger.LogDebug("No one joined channel '{Channel}', deleting.", b.ChannelId);

                                await channel.DeleteAsync();
                                this.CreatedChannels = this.CreatedChannels.Remove(x => x.ChannelId.ToString(), b);
                                return;
                            }
                        }).Add(this.Bot);

                        this.Bot.DiscordClient.VoiceStateUpdated += VoiceStateUpdated;
                        _logger.LogDebug("Created VcCreator Event for '{Channel}'", b.ChannelId);

                        while (this.CreatedChannels.Any(x => x.ChannelId == b.ChannelId))
                            await Task.Delay(500);

                        this.Bot.DiscordClient.VoiceStateUpdated -= VoiceStateUpdated;
                        _logger.LogDebug("Deleted VcCreator Event for '{Channel}'", b.ChannelId);
                    }).Add(this.Bot);
                }
        });
    }
}
