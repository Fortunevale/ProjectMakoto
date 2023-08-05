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

internal sealed class TokenLeakEvents : RequiresTranslation
{
    public TokenLeakEvents(Bot bot) : base(bot)
    {
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        CheckMessage(sender, e.Guild, e.Message).Add(this.Bot);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        if (e.MessageBefore?.Content != e.Message?.Content)
            CheckMessage(sender, e.Guild, e.Message).Add(this.Bot);
    }

    internal async Task CheckMessage(DiscordClient sender, DiscordGuild guild, DiscordMessage e)
    {
        string prefix = guild.GetGuildPrefix(this.Bot);

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

        int InvalidateCount = 0;

        foreach (var token in filtered_matches)
        {
            var botId = token.Groups["botid"].Value!;
            DiscordUser? botUser = null;
            try { botUser = await GetBotInfo(sender, botId); } catch { }

            if (botUser is null)
            {
                _logger.LogDebug("Not uploading detected token, no bot user was fetched.");
                continue;
            }

            string owner = this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepoOwner;
            string repo = this.Bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo;
            long seconds = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

            if (this.Bot.TokenInvalidator.SearchForString(token.Value).Item1)
            {
                _logger.LogDebug("Not uploading detected token, token already present in repository.");
                continue;
            }

            string fileName = $"token_leak_{e.Author.Id}_{guild.Id}_{e.Channel.Id}_{seconds}.md";
            string content = $"## Token of {botUser?.Id.ToString() ?? "unknown"} (Owner {e.Author.Id})\n\nBot {token}";

            await this.Bot.GithubClient.Repository.Content.CreateFile(owner, repo, $"automatic/{fileName}", new CreateFileRequest("Upload token to invalidate", content, "main"));
            InvalidateCount++;
        }

        if (InvalidateCount > 0)
            _ = this.Bot.TokenInvalidator.Pull();

        string s = (InvalidateCount > 1 ? "s" : "");

        _ = e.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(
        new DiscordEmbedBuilder()
        .WithColor(EmbedColors.Error)
        .WithAuthor(sender.CurrentUser.GetUsername(), null, sender.CurrentUser.AvatarUrl)
        .WithDescription($"`Heads up!`\n\n" +
                         $"`I've detected {filtered_matches.Count()} authentication token{s} within your last message. The token{s} will soon be invalidated and the owner{(s.IsNullOrWhiteSpace() ? "" : "(s)")} of the bot{s} will receive {(s.IsNullOrWhiteSpace() ? "an " : "")}official notification{s} from Discord.`\n\n" +
                         $"`You can disable the token check on this server via '/tokendetection config'.`"))
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
