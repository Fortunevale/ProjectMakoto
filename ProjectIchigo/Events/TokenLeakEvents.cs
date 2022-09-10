namespace ProjectIchigo.Events;

internal class TokenLeakEvents
{
    internal TokenLeakEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        CheckMessage(sender, e.Guild, e.Message).Add(_bot.watcher);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        if (e.MessageBefore?.Content != e.Message?.Content)
            CheckMessage(sender, e.Guild, e.Message).Add(_bot.watcher);
    }

    internal async Task CheckMessage(DiscordClient sender, DiscordGuild guild, DiscordMessage e)
    {
        if (e.Content.StartsWith($";;"))
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Content.StartsWith($";;{command.Key}"))
                    return;

        if (e.WebhookMessage || guild is null)
            return;

        if (!_bot.guilds[guild.Id].TokenLeakDetectionSettings.DetectTokens)
            return;

        var matchCollection = RegexTemplates.Token.Matches(e.Content);

        if (!matchCollection.IsNotNullAndNotEmpty())
            return;

        var filtered_matches = matchCollection.GroupBy(x => x.Value).Select<IGrouping<string, Match>, Match>(x => x.First());

        _ = e.DeleteAsync();

        var client = new GitHubClient(new ProductHeaderValue("Project-Ichigo"));

        var tokenAuth = new Credentials(_bot.status.LoadedConfig.Secrets.Github.Token);
        client.Credentials = tokenAuth;

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

            string owner = _bot.status.LoadedConfig.Secrets.Github.TokenLeakRepoOwner;
            string repo = _bot.status.LoadedConfig.Secrets.Github.TokenLeakRepo;
            long seconds = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;

            string fileName = $"token_leak_{e.Author.Id}_{guild.Id}_{e.Channel.Id}_{seconds}.md";
            string content = $"## Token of {botUser?.Id.ToString() ?? "unknown"} (Owner {e.Author.Id})\n\nBot {token}";

            await client.Repository.Content.CreateFile(owner, repo, $"automatic/{fileName}", new CreateFileRequest("Upload token to invalidate", content, "main"));
            InvalidateCount++;
        }

        string s = (InvalidateCount > 1 ? "s" : "");

        _ = e.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(
        new DiscordEmbedBuilder()
        .SetBotError(new SharedCommandContext(e, _bot), "Token Leak Protection")
        .WithDescription($"`Heads up!`\n\n" +
                         $"`I've detected {InvalidateCount} authentication token{s} within your last message. The token{s} will soon be invalidated and the owner{(s.IsNullOrWhiteSpace() ? "" : "(s)")} of the bot{s} will receive {(s.IsNullOrWhiteSpace() ? "an " : "")}official notification{s} from Discord.`\n\n" +
                         $"`You can disable the token check on this server via '/tokendetectionsettings config'.`"))
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
