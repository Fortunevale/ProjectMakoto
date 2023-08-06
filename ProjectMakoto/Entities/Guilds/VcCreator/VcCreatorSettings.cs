// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class VcCreatorSettings : RequiresParent<Guild>
{
    public VcCreatorSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    Translations.events.vcCreator tKey
        => this.Bot.LoadedTranslations.Events.VcCreator;

    private DiscordGuild cachedGuild { get; set; }

    private ulong _Channel { get; set; } = 0;
    public ulong Channel
    {
        get => this._Channel; set
        {
            this._Channel = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "vccreator_channelid", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    public ObservableDictionary<ulong, VcCreatorDetails> CreatedChannels { get => this._CreatedChannels; set { this._CreatedChannels = value; this._CreatedChannels.ItemsChanged += this.CreatedChannelsUpdated; } }

    private ObservableDictionary<ulong, VcCreatorDetails> _CreatedChannels { get; set; } = new();

    public Dictionary<ulong, DateTime> LastCreatedChannel = new();

    private void CreatedChannelsUpdated(object? sender, ObservableListUpdate<KeyValuePair<ulong, VcCreatorDetails>> e)
    {
        _ = Task.Run(async () =>
        {
            while (!this.Bot.status.DiscordGuildDownloadCompleted)
                await Task.Delay(1000);

            this.cachedGuild ??= await this.Bot.DiscordClient.GetGuildAsync(this.Parent.Id);

            await Task.Delay(5000);

            for (var i = 0; i < this.CreatedChannels.Count; i++)
            {
                var b = this.CreatedChannels.ElementAt(i);

                if (!this.cachedGuild.Channels.ContainsKey(b.Key))
                {
                    _logger.LogDebug("Channel '{Channel}' was deleted, deleting Vc Creator Entry.", b.Key);
                    _ = this.CreatedChannels.Remove(b.Key);
                    i--;
                }
            }

            foreach (var b in this.CreatedChannels)
                if (!b.Value.EventsRegistered)
                {
                    _ = Task.Run(async () =>
                    {
                        b.Value.EventsRegistered = true;
                        async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
                        {
                            _ = Task.Run(async () =>
                            {
                                if (e.Before?.Channel?.Id == b.Key || e.After?.Channel?.Id == b.Key)
                                {
                                    var channel = (e.After?.Channel?.Id != 0 ? e.After.Channel : null) ?? e.Before.Channel;
                                    var users = channel.Users.Where(x => !x.IsBot).ToList();

                                    if (users.Count <= 0)
                                    {
                                        _logger.LogDebug("Channel '{Channel}' is now empty, deleting.", b.Key);

                                        await channel.DeleteAsync();
                                        _ = this.CreatedChannels.Remove(b.Key);
                                        return;
                                    }

                                    if (e.User.Id == b.Value.OwnerId && e.After?.Channel?.Id != b.Key)
                                    {
                                        _logger.LogDebug("The owner of channel '{Channel}' left, assigning new owner.", b.Key);
                                        var newOwner = users.SelectRandom();

                                        b.Value.OwnerId = newOwner.Id;

                                        _ = await channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription(this.tKey.NewOwner.Get(this.Parent).Build(true, new TVar("User", newOwner.Mention))).WithColor(EmbedColors.Info));
                                        return;
                                    }

                                    if (b.Value.BannedUsers.Contains(e.After?.User?.Id ?? 0))
                                    {
                                        var u = (await e.User.ConvertToMember(this.cachedGuild));

                                        _logger.LogDebug("Banned user in channel '{Channel}' joined, disconnecting.", b.Key);
                                        if (u.Permissions.HasPermission(Permissions.Administrator) || u.Permissions.HasPermission(Permissions.ManageChannels) || u.Permissions.HasPermission(Permissions.ModerateMembers) || u.Permissions.HasPermission(Permissions.KickMembers) || u.Permissions.HasPermission(Permissions.BanMembers) || u.Permissions.HasPermission(Permissions.MuteMembers) || u.Permissions.HasPermission(Permissions.DeafenMembers))
                                            return;

                                        await u.DisconnectFromVoiceAsync();
                                        return;
                                    }

                                    if (e.Before?.Channel?.Id != e.After?.Channel?.Id)
                                    {
                                        if (e.After?.Channel?.Id == b.Key)
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

                            var channel = await this.Bot.DiscordClient.GetChannelAsync(b.Key);

                            if (channel.Users.Count <= 0)
                            {
                                _logger.LogDebug("No one joined channel '{Channel}', deleting.", b.Key);

                                await channel.DeleteAsync();
                                _ = this.CreatedChannels.Remove(b.Key);
                                return;
                            }
                        }).Add(this.Bot);

                        this.Bot.DiscordClient.VoiceStateUpdated += VoiceStateUpdated;
                        _logger.LogDebug("Created VcCreator Event for '{Channel}'", b.Key);

                        while (this.CreatedChannels.ContainsKey(b.Key))
                            await Task.Delay(500);

                        this.Bot.DiscordClient.VoiceStateUpdated -= VoiceStateUpdated;
                        _logger.LogDebug("Deleted VcCreator Event for '{Channel}'", b.Key);
                    }).Add(this.Bot);
                }
        });
    }
}
