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
        Task.Run(async () =>
        {
            if (e.Guild is null)
                return;

            var Delete = new DiscordButtonComponent(ButtonStyle.Danger, "DeleteEmbedMessage", "Delete", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));

            do
            {
                if (RegexTemplates.DiscordChannelUrl.IsMatch(e.Message.Content))
                {
                    if (!_bot.guilds[e.Guild.Id].EmbedMessage.UseEmbedding)
                        break;

                    if (await _bot.users[e.Message.Author.Id].Cooldown.WaitForModerate(sender, new SharedCommandContext(e.Message, _bot), true))
                        break;

                    var matches = RegexTemplates.DiscordChannelUrl.Matches(e.Message.Content);

                    foreach (Match b in matches.GroupBy(x => x.Value).Select(y => y.FirstOrDefault()).Take(2))
                    {
                        if (!b.Value.TryParseMessageLink(out ulong GuildId, out ulong ChannelId, out ulong MessageId))
                            continue;

                        if (GuildId != e.Guild.Id)
                            continue;

                        if (!e.Guild.Channels.ContainsKey(ChannelId))
                            continue;

                        var channel = e.Guild.GetChannel(ChannelId);
                        var perms = channel.PermissionsFor(await e.Author.ConvertToMember(e.Guild));

                        if (!perms.HasPermission(Permissions.AccessChannels) || !perms.HasPermission(Permissions.ReadMessageHistory))
                            continue;

                        if (!channel.TryGetMessage(MessageId, out var message))
                            continue;

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
            } while (false);

            if (RegexTemplates.GitHubUrl.IsMatch(e.Message.Content))
            {
                if (!_bot.guilds[e.Guild.Id].EmbedMessage.UseGithubEmbedding)
                    return;

                if (await _bot.users[e.Message.Author.Id].Cooldown.WaitForModerate(sender, new SharedCommandContext(e.Message, _bot), true))
                    return;

                var matches = RegexTemplates.GitHubUrl.Matches(e.Message.Content);

                foreach (Match b in matches.GroupBy(x => x.Value).Select(y => y.FirstOrDefault()).Take(2))
                {
                    string fileUrl = b.Value;
                    fileUrl = fileUrl.Replace("github.com", "raw.githubusercontent.com");
                    fileUrl = fileUrl.Replace("/blob", "");
                    fileUrl = fileUrl[..fileUrl.LastIndexOf("#")];

                    string repoOwner = b.Groups[1].Value;
                    string repoName = b.Groups[2].Value;

                    string relativeFilePath = b.Groups[5].Value;

                    string fileEnding = "";

                    try
                    {
                        fileEnding = relativeFilePath.Remove(0, relativeFilePath.LastIndexOf(".") + 1);
                    }
                    catch { }

                    uint StartLine = Convert.ToUInt32(b.Groups[6].Value.Replace("L", ""));
                    uint EndLine = Convert.ToUInt32(b.Groups[8].Value.IsNullOrWhiteSpace() ? $"{StartLine}" : b.Groups[8].Value.Replace("L", ""));

                    if (EndLine < StartLine)
                        return;

                    var rawFile = await new HttpClient().GetStringAsync(fileUrl);
                    rawFile = rawFile.ReplaceLineEndings("\n");

                    var lines = rawFile.Split("\n").Skip((int)(StartLine - 1)).Take((int)(EndLine - (StartLine - 1))).Select(x => x.Replace("\t", "    ")).ToList();

                    if (!lines.IsNotNullAndNotEmpty())
                        return;

                    int shortestIndent = -1;

                    foreach (var c in lines)
                    {
                        int currentIndent = 0;

                        foreach (var d in c)
                        {
                            if (d is ' ' or '\t')
                                currentIndent++;
                            else
                                break;
                        }

                        if (currentIndent < shortestIndent || shortestIndent == -1)
                            shortestIndent = currentIndent;
                    }

                    lines = lines.Select(x => x.Remove(0, shortestIndent)).ToList();

                    string content = $"`{relativeFilePath}` {(StartLine != EndLine ? $"lines {StartLine} to {EndLine}" : $"line {StartLine}")}\n\n" +
                                      $"```{fileEnding}\n" +
                                      $"{string.Join("\n", lines)}\n" +
                                      $"```";

                    content = content.TruncateWithIndication(1997);

                    if (!content.EndsWith("```"))
                        content += "```";

                    var msg = await e.Message.RespondAsync(new DiscordMessageBuilder().WithContent(content).AddComponents(Delete));
                    _ = e.Message.ModifySuppressionAsync(true);
                }
            }
        }).Add(_bot.watcher);
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            if (e.GetCustomId() == "DeleteEmbedMessage")
            {
                var fullMsg = await e.Message.Refetch();

                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if ((fullMsg.Reference is not null && fullMsg.ReferencedMessage is null) || 
                (fullMsg.ReferencedMessage is not null && fullMsg.ReferencedMessage.Author.Id == e.Interaction.User.Id) ||
                (await e.User.ConvertToMember(e.Interaction.Guild)).Roles.Any(x => (x.CheckPermission(Permissions.ManageMessages) == PermissionLevel.Allowed) || (x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed)))
                {
                    _ = fullMsg.DeleteAsync().ContinueWith(x =>
                    {
                        if (!x.IsCompletedSuccessfully)
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
