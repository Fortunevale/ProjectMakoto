// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class VcCreatorEvents(Bot bot) : RequiresTranslation(bot)
{
    Translations.events.vcCreator tKey
        => this.Bot.LoadedTranslations.Events.VcCreator;

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (e.After?.Channel?.Id == this.Bot.Guilds[e.Guild.Id].VcCreator.Channel)
        {
            try
            {
                var member = await e.User.ConvertToMember(e.Guild);

                _ = this.Bot.Guilds[e.Guild.Id].VcCreator.LastCreatedChannel.TryAdd(e.User.Id, DateTime.MinValue);
                if (e.After.Channel.Parent is null || e.After.Channel.Parent.Children.Count >= 50 || e.Guild.Channels.Count >= 500 || this.Bot.Guilds[e.Guild.Id].VcCreator.LastCreatedChannel[e.User.Id].GetTimespanSince() < TimeSpan.FromSeconds(30))
                {
                    await member.DisconnectFromVoiceAsync();
                    return;
                }

                this.Bot.Guilds[e.Guild.Id].VcCreator.LastCreatedChannel[e.User.Id] = DateTime.UtcNow;

                var name = this.tKey.DefaultChannelName.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("User", member.DisplayName.SanitizeForCode()));

                foreach (var b in this.Bot.ProfanityList)
                    name = name.Replace(b, new String('*', b.Length));

                var newChannel = await e.Guild.CreateChannelAsync(name, ChannelType.Voice, e.After.Channel.Parent, default, null, 8);
                this.Bot.Guilds[e.Guild.Id].VcCreator.CreatedChannels = this.Bot.Guilds[e.Guild.Id].VcCreator.CreatedChannels.Add(new()
                {
                    ChannelId = newChannel.Id,
                    OwnerId = member.Id
                });

                await member.ModifyAsync(x => x.VoiceChannel = newChannel);

                await Task.Delay(1000);

                _ = await newChannel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithContent(e.User.Mention)
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithAuthor(e.Guild.Name, "", e.Guild.IconUrl)
                        .WithColor(EmbedColors.Info)
                        .WithTimestamp(DateTime.UtcNow)
                        .WithDescription(this.tKey.DefaultChannelName.Get(this.Bot.Guilds[e.Guild.Id]).Build(true, new TVar("Command", "'/vcc'")))));
            }
            catch (Exception)
            {
                try
                {
                    await (await e.User.ConvertToMember(e.Guild)).DisconnectFromVoiceAsync();
                }
                catch { }
                throw;
            }
        }
    }
}
