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

            if (Regex.IsMatch(e.Message.Content, Resources.Regex.DiscordChannelUrl))
            {
                if (await _bot.users[e.Message.Author.Id].Cooldown.WaitForModerate(sender, new SharedCommandContext(e.Message, _bot)))
                    return;

                var matches = Regex.Matches(e.Message.Content, Resources.Regex.DiscordChannelUrl);

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

                    var Delete = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Delete", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));

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
                        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Deleting via the button is restricted to the original message author."}
                    }).AddComponents(Delete));

                    var interaction = await sender.GetInteractivity().WaitForButtonAsync(msg, e.Author, TimeSpan.FromMinutes(30));

                    if (interaction.TimedOut)
                    {
                        msg.ModifyToTimedOut(true);
                        return;
                    }

                    _ = interaction.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    if (interaction.Result.Interaction.Data.CustomId == Delete.CustomId)
                    {
                        _ = msg.DeleteAsync().ContinueWith(x =>
                        {
                            if (x.IsCompletedSuccessfully)
                                _ = interaction.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("✅ `The message was deleted.`").AsEphemeral());
                            else
                                _ = interaction.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("❌ `Failed to delete the message.`").AsEphemeral());
                        });
                    }
                }
            }
        });
    }
}
