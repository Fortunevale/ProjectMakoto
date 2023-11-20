// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Octokit;

namespace ProjectMakoto.Events;

internal sealed class TokenLeakEvents(Bot bot) : RequiresTranslation(bot)
{
    Translations.events.tokenDetection tKey
        => this.Bot.LoadedTranslations.Events.TokenDetection;

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        _ = this.CheckMessage(sender, e.Guild, e.Message).Add(this.Bot);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        if (e.MessageBefore?.Content != e.Message?.Content)
            _ = this.CheckMessage(sender, e.Guild, e.Message).Add(this.Bot);
    }

    internal async Task CheckMessage(DiscordClient sender, DiscordGuild guild, DiscordMessage e)
    {
        var prefix = guild.GetGuildPrefix(this.Bot);

        if (e?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        if (e.WebhookMessage || guild is null)
            return;

        if (!this.Bot.Guilds[guild.Id].TokenLeakDetection.DetectTokens)
            return;

        var matchCollection = RegexTemplates.Token.Matches(e.Content);

        if (!matchCollection.IsNotNullAndNotEmpty())
            return;

        var filtered_matches = matchCollection.GroupBy(x => x.Value).Select<IGrouping<string, Match>, Match>(x => x.First());

        _ = e.DeleteAsync();

        var InvalidateCount = 0;

        foreach (var token in filtered_matches)
        {
            var botId = token.Groups["botid"].Value!;
            DiscordUser? botUser = null;
            try { botUser = await this.GetBotInfo(sender, botId); } catch { }

            if (botUser is null)
            {
                _logger.LogDebug("Not uploading detected token, no bot user was fetched.");
                continue;
            }

            var owner = this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepoOwner;
            var repo = this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo;
            var seconds = (long)DateTime.UtcNow.Subtract(DateTime.MinValue).TotalSeconds;

            if (this.Bot.TokenInvalidator.SearchForString(token.Value).Item1)
            {
                _logger.LogDebug("Not uploading detected token, token already present in repository.");
                continue;
            }

            var fileName = $"token_leak_{e.Author.Id}_{guild.Id}_{e.Channel.Id}_{seconds}.md";
            var content = $"## Token of {botUser?.Id.ToString() ?? "unknown"} (Owner {e.Author.Id})\n\nBot {token}";

            _ = await this.Bot.GithubClient.Repository.Content.CreateFile(owner, repo, $"automatic/{fileName}", new CreateFileRequest("Upload token to invalidate", content, "main"));
            InvalidateCount++;
        }

        if (InvalidateCount > 0)
            _ = this.Bot.TokenInvalidator.Pull();

        var s = (InvalidateCount > 1 ? "s" : "");

        _ = e.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(
        new DiscordEmbedBuilder()
        .WithColor(EmbedColors.Error)
        .WithAuthor(sender.CurrentUser.GetUsername(), null, sender.CurrentUser.AvatarUrl)
        .WithDescription(this.tKey.TokenInvalidated.Get(this.Bot.Guilds[e.Guild.Id]).Build(true, false, new TVar("Count", filtered_matches.Count()))))
        .WithContent(e.Author.Mention));
    }

    private async Task<DiscordUser> GetBotInfo(DiscordClient client, string botId)
    {
        var ulongId = Convert.ToUInt64(Base64Decode(botId + "=="));
        var bot = await client.GetUserAsync(ulongId);
        return bot;
    }

    public static string Base64Decode(string base64)
    {
        var base64Bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(base64Bytes);
    }
}
