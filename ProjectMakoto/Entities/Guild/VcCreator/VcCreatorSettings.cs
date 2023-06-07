// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class VcCreatorSettings
{
    public VcCreatorSettings(Guild guild, Bot bot)
    {
        this.Parent = guild;

        this._bot = bot;
        this._CreatedChannels.ItemsChanged += CreatedChannelsUpdated;
    }

    private Guild Parent { get; set; }
    private DiscordGuild cachedGuild { get; set; }

    private Bot _bot { get; set; }

    private ulong _Channel { get; set; } = 0;
    public ulong Channel
    {
        get => this._Channel; set
        {
            this._Channel = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "vccreator_channelid", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    public ObservableDictionary<ulong, VcCreatorDetails> CreatedChannels { get => this._CreatedChannels; set { this._CreatedChannels = value; this._CreatedChannels.ItemsChanged += CreatedChannelsUpdated; } }

    private ObservableDictionary<ulong, VcCreatorDetails> _CreatedChannels { get; set; } = new();

    public Dictionary<ulong, DateTime> LastCreatedChannel = new();

    private async void CreatedChannelsUpdated(object? sender, ObservableListUpdate<KeyValuePair<ulong, VcCreatorDetails>> e)
    {
        while (!this._bot.status.DiscordGuildDownloadCompleted)
            await Task.Delay(1000);

        this.cachedGuild ??= await this._bot.discordClient.GetGuildAsync(this.Parent.ServerId);

        await Task.Delay(5000);

        for (int i = 0; i < this.CreatedChannels.Count; i++)
        {
            var b = this.CreatedChannels.ElementAt(i);

            if (!this.cachedGuild.Channels.ContainsKey(b.Key))
            {
                _logger.LogDebug("Channel '{Channel}' was deleted, deleting Vc Creator Entry.", b.Key);
                this.CreatedChannels.Remove(b.Key);
                i--;
            }
        }

        foreach (var b in this.CreatedChannels)
            if (!b.Value.EventsRegistered)
            {
                Task.Run(async () =>
                {
                    b.Value.EventsRegistered = true;
                    async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
                    {
                        Task.Run(async () =>
                        {
                            if (e.Before?.Channel?.Id == b.Key || e.After?.Channel?.Id == b.Key)
                            {
                                var channel = (e.After?.Channel?.Id != 0 ? e.After.Channel : null) ?? e.Before.Channel;
                                var users = channel.Users.Where(x => !x.IsBot).ToList();

                                if (users.Count <= 0)
                                {
                                    _logger.LogDebug("Channel '{Channel}' is now empty, deleting.", b.Key);

                                    await channel.DeleteAsync();
                                    this.CreatedChannels.Remove(b.Key);
                                    return;
                                }

                                if (e.User.Id == b.Value.OwnerId && e.After?.Channel?.Id != b.Key)
                                {
                                    _logger.LogDebug("The owner of channel '{Channel}' left, assigning new owner.", b.Key);
                                    var newOwner = users.SelectRandom();

                                    b.Value.OwnerId = newOwner.Id;

                                    await channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription($"`The channel is now owned by` {newOwner.Mention}.").WithColor(EmbedColors.Info));
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
                                        await channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription($"{e.User.Mention} `joined.`").WithColor(EmbedColors.Success).WithAuthor("User joined", "", AuditLogIcons.UserAdded));
                                    }
                                    else
                                    {
                                        await channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription($"{e.User.Mention} `left.`").WithColor(EmbedColors.Error).WithAuthor("User left", "", AuditLogIcons.UserLeft));
                                    }
                                }
                            }
                        }).Add(this._bot);
                    }

                    Task.Run(async () =>
                    {
                        await Task.Delay(5000);

                        var channel = await this._bot.discordClient.GetChannelAsync(b.Key);

                        if (channel.Users.Count <= 0)
                        {
                            _logger.LogDebug("No one joined channel '{Channel}', deleting.", b.Key);

                            await channel.DeleteAsync();
                            this.CreatedChannels.Remove(b.Key);
                            return;
                        }
                    }).Add(this._bot);

                    this._bot.discordClient.VoiceStateUpdated += VoiceStateUpdated;
                    _logger.LogDebug("Created VcCreator Event for '{Channel}'", b.Key);

                    while (this.CreatedChannels.ContainsKey(b.Key))
                        await Task.Delay(500);

                    this._bot.discordClient.VoiceStateUpdated -= VoiceStateUpdated;
                    _logger.LogDebug("Deleted VcCreator Event for '{Channel}'", b.Key);
                }).Add(this._bot);
            }

    }
}
