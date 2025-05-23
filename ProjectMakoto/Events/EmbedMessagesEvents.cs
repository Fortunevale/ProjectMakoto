// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class EmbedMessagesEvents(Bot bot) : RequiresTranslation(bot)
{
    Translations.events.embedMessages tKey => this.t.Events.EmbedMessages;

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Guild is null)
            return;

        var Delete = new DiscordButtonComponent(ButtonStyle.Danger, "DeleteEmbedMessage", this.tKey.Delete.Get(this.Bot.Guilds[e.Guild.Id]), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));


        do
        {
            if (RegexTemplates.DiscordChannelUrl.IsMatch(e.Message.Content))
            {
                if (!this.Bot.Guilds[e.Guild.Id].EmbedMessage.UseEmbedding)
                    break;

                if (await this.Bot.Users[e.Message.Author.Id].Cooldown.WaitForModerate(new SharedCommandContext(e.Message, this.Bot, "message_embed"), true))
                    break;

                var matches = RegexTemplates.DiscordChannelUrl.Matches(e.Message.Content);

                foreach (var b in matches.GroupBy(x => x.Value).Select(y => y.FirstOrDefault()).Take(2))
                {
                    if (!b.Value.TryParseMessageLink(out var GuildId, out var ChannelId, out var MessageId))
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

                    var JumpToMessage = new DiscordLinkButtonComponent(message.JumpLink.ToString(), this.t.Common.JumpToMessage.Get(this.Bot.Guilds[e.Guild.Id]), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
                    
                    var msg = await e.Message.RespondAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = message.Author.AvatarUrl, Name = $"{message.Author.GetUsernameWithIdentifier()}" },
                        Color = message.Author.BannerColor ?? EmbedColors.Info,
                        Description = $"{message.ConvertToText()}".TruncateWithIndication(2000),
                        ImageUrl = (message.Attachments?.Count > 0 && (message.Attachments[0].Filename.EndsWith(".png")
                                                                    || message.Attachments[0].Filename.EndsWith(".jpeg")
                                                                    || message.Attachments[0].Filename.EndsWith(".jpg")
                                                                    || message.Attachments[0].Filename.EndsWith(".gif")) ? message.Attachments[0].Url : ""),
                        Timestamp = message.Timestamp,
                    }).AddComponents(JumpToMessage, Delete));
                }
            }
        } while (false);

        if (RegexTemplates.GitHubUrl.IsMatch(e.Message.Content))
        {
            if (!this.Bot.Guilds[e.Guild.Id].EmbedMessage.UseGithubEmbedding)
                return;

            SharedCommandContext ctx = new(e.Message, this.Bot, "github_embed");
            if (await this.Bot.Users[e.Message.Author.Id].Cooldown.WaitForModerate(ctx, true))
                return;

            ctx.BaseCommand.DeleteOrInvalidate();

            var matches = RegexTemplates.GitHubUrl.Matches(e.Message.Content);

            foreach (var b in matches.GroupBy(x => x.Value).Select(y => y.FirstOrDefault()).Take(2))
            {
                var fileUrl = b.Value;
                fileUrl = fileUrl.Replace("github.com", "raw.githubusercontent.com");
                fileUrl = fileUrl.Replace("/blob", "");
                fileUrl = fileUrl[..fileUrl.LastIndexOf('#')];

                var repoOwner = b.Groups[1].Value;
                var repoName = b.Groups[2].Value;

                var relativeFilePath = b.Groups[5].Value;

                var fileEnding = "";

                try
                {
                    fileEnding = relativeFilePath.Remove(0, relativeFilePath.LastIndexOf('.') + 1);
                }
                catch { }

                var StartLine = Convert.ToUInt32(b.Groups[6].Value.Replace("L", ""));
                var EndLine = Convert.ToUInt32(b.Groups[8].Value.IsNullOrWhiteSpace() ? $"{StartLine}" : b.Groups[8].Value.Replace("L", ""));

                if (EndLine < StartLine)
                    return;

                var rawFile = await new HttpClient().GetStringAsync(fileUrl);
                rawFile = rawFile.ReplaceLineEndings("\n");

                var lines = rawFile.Split("\n").Skip((int)(StartLine - 1)).Take((int)(EndLine - (StartLine - 1))).Select(x => x.Replace("\t", "    ")).ToList();

                if (!lines.IsNotNullAndNotEmpty())
                    return;

                var shortestIndent = -1;

                foreach (var c in lines)
                {
                    var currentIndent = 0;

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

                var content = $"`{relativeFilePath}` {(StartLine != EndLine ? this.tKey.Lines.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Start", StartLine), new TVar("End", EndLine)) : this.tKey.Line.Get(this.Bot.Guilds[e.Guild.Id]).Build(new TVar("Start", StartLine)))}\n\n" +
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
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (e.GetCustomId() == "DeleteEmbedMessage")
        {
            var fullMsg = await e.Message.Refetch();

            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            _ = (fullMsg.Reference is not null && fullMsg.ReferencedMessage is null) ||
            (fullMsg.ReferencedMessage is not null && fullMsg.ReferencedMessage.Author.Id == e.Interaction.User.Id) ||
            (await e.User.ConvertToMember(e.Interaction.Guild)).Roles.Any(x => (x.CheckPermission(Permissions.ManageMessages) == PermissionLevel.Allowed) || (x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed))
                ? fullMsg.DeleteAsync().ContinueWith(x =>
                {
                    if (!x.IsCompletedSuccessfully)
                        _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ `{this.tKey.FailedToDelete.Get(this.Bot.Guilds[e.Guild.Id])}`").AsEphemeral());
                })
                : e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ `{this.tKey.NotAuthor.Get(this.Bot.Guilds[e.Guild.Id])}`").AsEphemeral());
        }
    }
}
