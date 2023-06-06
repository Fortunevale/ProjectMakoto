// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class VcCreatorEvents
{
    internal VcCreatorEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (e.After?.Channel?.Id == this._bot.guilds[e.Guild.Id].VcCreator.Channel)
        {
            try
            {
                DiscordMember member = await e.User.ConvertToMember(e.Guild);

                if (!this._bot.guilds[e.Guild.Id].VcCreator.LastCreatedChannel.ContainsKey(e.User.Id))
                    this._bot.guilds[e.Guild.Id].VcCreator.LastCreatedChannel.Add(e.User.Id, DateTime.MinValue);

                if (e.After.Channel.Parent is null || e.After.Channel.Parent.Children.Count >= 50 || this._bot.guilds[e.Guild.Id].VcCreator.LastCreatedChannel[e.User.Id].GetTimespanSince() < TimeSpan.FromSeconds(30))
                {
                    await member.DisconnectFromVoiceAsync();
                    return;
                }

                this._bot.guilds[e.Guild.Id].VcCreator.LastCreatedChannel[e.User.Id] = DateTime.UtcNow;

                var name = $"{member.DisplayName.SanitizeForCode()}'s Channel";

                foreach (var b in this._bot.profanityList)
                    name = name.Replace(b, new String('*', b.Length));

                var newChannel = await e.Guild.CreateChannelAsync(name, ChannelType.Voice, e.After.Channel.Parent, default, null, 8);
                this._bot.guilds[e.Guild.Id].VcCreator.CreatedChannels.Add(newChannel.Id, new VcCreatorDetails
                {
                    OwnerId = member.Id
                });

                await member.ModifyAsync(x => x.VoiceChannel = newChannel);

                await Task.Delay(1000);

                await newChannel.SendMessageAsync(new DiscordMessageBuilder().WithContent(e.User.Mention).WithEmbed(new DiscordEmbedBuilder().WithAuthor(e.Guild.Name, "", e.Guild.IconUrl).WithColor(EmbedColors.Info).WithTimestamp(DateTime.UtcNow)
                    .WithDescription($"This is your temporary personal channel.\n\nIf this channel becomes empty, it'll be deleted. Use the `/vcc` commands to manage this channel.")));
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
