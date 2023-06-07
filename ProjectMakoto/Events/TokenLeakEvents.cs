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

internal sealed class TokenLeakEvents
{
    internal TokenLeakEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        CheckMessage(sender, e.Guild, e.Message).Add(this._bot);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        if (e.MessageBefore?.Content != e.Message?.Content)
            CheckMessage(sender, e.Guild, e.Message).Add(this._bot);
    }

    internal async Task CheckMessage(DiscordClient sender, DiscordGuild guild, DiscordMessage e)
    {
        string prefix = guild.GetGuildPrefix(_bot);

        if (e?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        if (e.WebhookMessage || guild is null)
            return;

        if (!this._bot.guilds[guild.Id].TokenLeakDetection.DetectTokens)
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
            DiscordUser botUser = null!;
            try
            {
                botUser = await GetBotInfo(sender, botId);
            }
            catch { }

            string owner = this._bot.status.LoadedConfig.Secrets.Github.TokenLeakRepoOwner;
            string repo = this._bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo;
            long seconds = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

            string fileName = $"token_leak_{e.Author.Id}_{guild.Id}_{e.Channel.Id}_{seconds}.md";
            string content = $"## Token of {botUser?.Id.ToString() ?? "unknown"} (Owner {e.Author.Id})\n\nBot {token}";

            await this._bot.githubClient.Repository.Content.CreateFile(owner, repo, $"automatic/{fileName}", new CreateFileRequest("Upload token to invalidate", content, "main"));
            InvalidateCount++;
        }

        string s = (InvalidateCount > 1 ? "s" : "");

        _ = e.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(
        new DiscordEmbedBuilder()
        .WithColor(EmbedColors.Error)
        .WithAuthor(sender.CurrentUser.GetUsername(), null, sender.CurrentUser.AvatarUrl)
        .WithDescription($"`Heads up!`\n\n" +
                         $"`I've detected {InvalidateCount} authentication token{s} within your last message. The token{s} will soon be invalidated and the owner{(s.IsNullOrWhiteSpace() ? "" : "(s)")} of the bot{s} will receive {(s.IsNullOrWhiteSpace() ? "an " : "")}official notification{s} from Discord.`\n\n" +
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
