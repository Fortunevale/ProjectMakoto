namespace ProjectIchigo.Entities;

public class VcCreatorSettings
{
    public VcCreatorSettings(Guild guild, Bot bot)
    {
        Parent = guild;

        this._bot = bot;
    }

    private Guild Parent { get; set; }
    private DiscordGuild cachedGuild { get; set; }

    private Bot _bot { get; set; }

    private ulong _Channel { get; set; } = 0;
    public ulong Channel
    {
        get => _Channel; set
        {
            _Channel = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "vccreator_channelid", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    public ObservableDictionary<ulong, VcCreatorDetails> CreatedChannels { get => _CreatedChannels; set { _CreatedChannels = value; _CreatedChannels.ItemsChanged += CreatedChannelsUpdated; } }

    private ObservableDictionary<ulong, VcCreatorDetails> _CreatedChannels { get; set; } = new();

    private async void CreatedChannelsUpdated(object? sender, ObservableListUpdate<KeyValuePair<ulong, VcCreatorDetails>> e)
    {
        cachedGuild ??= await _bot.discordClient.GetGuildAsync(Parent.ServerId);

        foreach (var b in CreatedChannels.ToList())
        {
            if (!cachedGuild.Channels.ContainsKey(b.Key))
            {
                _logger.LogDebug($"Channel '{b.Key}' was deleted, deleting Vc Creator Entry.");
                CreatedChannels.Remove(b.Key);
            }
        }

        foreach (var b in CreatedChannels)
            if (!b.Value.EventsRegistered)
            {
                Task.Run(async () =>
                {
                    async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
                    {
                        Task.Run(async () =>
                        {
                            if (e.Channel.Id == b.Key)
                            {
                                if (e.Channel.Users.Count <= 0)
                                {
                                    _logger.LogDebug($"Channel '{b.Key}' is now empty, deleting.");

                                    await e.Channel.DeleteAsync();
                                    CreatedChannels.Remove(b.Key);
                                    return;
                                }

                                if (e.User.Id == b.Value.OwnerId && e.After.Channel.Id != b.Key)
                                {
                                    _logger.LogDebug($"The owner of channel '{b.Key}' left, assigning new owner.");
                                    var newOwner = e.Channel.Users.SelectRandom();

                                    b.Value.OwnerId = newOwner.Id;

                                    await e.Channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription($"`The channel is now owned by `{newOwner.Mention}.").WithColor(EmbedColors.Info));
                                    return;
                                }

                                if (b.Value.BannedUsers.Contains(e.After?.User?.Id ?? 0))
                                {
                                    _logger.LogDebug($"Banned user in channel '{b.Key}' joined, disconnecting.");
                                    await (await e.User.ConvertToMember(cachedGuild)).DisconnectFromVoiceAsync();
                                    return;
                                }
                            }
                        }).Add(_bot.watcher);
                    }

                    _bot.discordClient.VoiceStateUpdated += VoiceStateUpdated;
                    _logger.LogDebug($"Created VcCreator Event for '{b.Key}'");

                    while (CreatedChannels.ContainsKey(b.Key))
                        await Task.Delay(500);

                    _bot.discordClient.VoiceStateUpdated -= VoiceStateUpdated;
                    _logger.LogDebug($"Deleted VcCreator Event for '{b.Key}'");
                }).Add(_bot.watcher);
            }

    }
}
