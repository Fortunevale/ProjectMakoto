namespace ProjectIchigo.Events;

internal class EmbedMessagesEvents
{
    internal EmbedMessagesEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].EmbedMessageSettings.UseEmbedding)
                return;

            if (RegexTemplates.DiscordChannelUrl.IsMatch(e.Message.Content))
            {
                if (await _bot.users[e.Message.Author.Id].Cooldown.WaitForModerate(sender, new SharedCommandContext(e.Message, _bot)))
                    return;

                var matches = RegexTemplates.DiscordChannelUrl.Matches(e.Message.Content);

                foreach (Match b in matches.GroupBy(x => x.Value).Select(y => y.FirstOrDefault()).Take(2))
                {
                    if (!b.Value.TryParseMessageLink(out ulong GuildId, out ulong ChannelId, out ulong MessageId))
                        continue;

                    if (GuildId != e.Guild.Id)
                        return;

                    if (!e.Guild.Channels.ContainsKey(ChannelId))
                        return;

                    var channel = e.Guild.GetChannel(ChannelId);
                    var perms = channel.PermissionsFor(await e.Author.ConvertToMember(e.Guild));

                    if (!perms.HasPermission(Permissions.AccessChannels) || !perms.HasPermission(Permissions.ReadMessageHistory))
                        return;

                    if (!channel.TryGetMessage(MessageId, out var message))
                        return;

                    var Delete = new DiscordButtonComponent(ButtonStyle.Danger, "DeleteEmbedMessage", "Delete", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));

                    var msg = await e.Message.RespondAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = message.Author.AvatarUrl, Name = $"{message.Author.UsernameWithDiscriminator} ({message.Author.Id})" },
                        Color = message.Author.BannerColor ?? EmbedColors.Info,
                        Description = $"[`Jump to message`]({message.JumpLink})\n\n{message.Content}".TruncateWithIndication(2000),
                        ImageUrl = (message.Attachments?.Count > 0 && (message.Attachments[0].FileName.EndsWith(".png")
                                                                    || message.Attachments[0].FileName.EndsWith(".jpeg")
                                                                    || message.Attachments[0].FileName.EndsWith(".jpg")
                                                                    || message.Attachments[0].FileName.EndsWith(".gif")) ? message.Attachments[0].Url : ""),
                        Timestamp = message.Timestamp,
                    }).AddComponents(Delete));
                }
            }
        });
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            if (e.Interaction.Data.CustomId == "DeleteEmbedMessage")
            {
                var fullMsg = await e.Message.Refresh();

                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if ((fullMsg.Reference is not null && fullMsg.ReferencedMessage is null) || 
                (fullMsg.ReferencedMessage is not null && fullMsg.ReferencedMessage.Author.Id == e.Interaction.User.Id) ||
                (await e.User.ConvertToMember(e.Interaction.Guild)).Roles.Any(x => (x.CheckPermission(Permissions.ManageMessages) == PermissionLevel.Allowed) || (x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed)))
                {
                    _ = fullMsg.DeleteAsync().ContinueWith(x =>
                    {
                        if (x.IsCompletedSuccessfully)
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("✅ `The message was deleted.`").AsEphemeral());
                        else
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("❌ `Failed to delete the message.`").AsEphemeral());
                    });
                }
                else
                {
                    _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("❌ `You are not the message author of the referenced message.`").AsEphemeral());
                }
            }
        });
    }
}
