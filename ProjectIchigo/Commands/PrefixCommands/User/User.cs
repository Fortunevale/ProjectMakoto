namespace ProjectIchigo.PrefixCommands;

internal class User : BaseCommandModule
{
    public Bot _bot { private get; set; }



    [Command("help"),
    CommandModule("user"),
    Description("Shows all available commands, their usage and their description")]
    public async Task Help(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                return;

            List<KeyValuePair<string, string>> Commands = new();


            foreach (var command in ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()))
            {
                if (command.Value.CustomAttributes.OfType<CommandModuleAttribute>() is null)
                    continue;

                string module = command.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString;

                switch (module)
                {
                    case "admin":
                        if (!ctx.Member.IsAdmin(_bot._status))
                            continue;
                        break;
                    case "maintainence":
                        if (!ctx.Member.IsMaintenance(_bot._status))
                            continue;
                        break;
                    case "hidden":
                        continue;
                    default:
                        break;
                }

                Commands.Add(new KeyValuePair<string, string>($"{module.FirstLetterToUpper()} Commands", $"`{ctx.Prefix}{command.Value.GenerateUsage()}` - _{command.Value.Description}{command.Value.Aliases.GenerateAliases()}_"));
            }

            var Fields = Commands.PrepareEmbedFields();
            var Embeds = Fields.Select(x => new KeyValuePair<string, string>(x.Key, x.Value
                .Replace("##Prefix##", ctx.Prefix)
                .Replace("##n##", "\n")))
                .ToList().PrepareEmbeds("", "All available commands will be listed below.\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n" +
                                            "**Do not include the brackets when using commands, they're merely an indicator for requirement.**");

            try
            {
                foreach (var b in Embeds)
                    await ctx.Member.SendMessageAsync(embed: b.WithAuthor(ctx.Guild.Name, "", ctx.Guild.IconUrl).WithFooter(ctx.GenerateUsedByFooter().Text, ctx.GenerateUsedByFooter().IconUrl).WithTimestamp(DateTime.UtcNow).WithColor(EmbedColors.Info).Build());

                var successembed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Description = ":mailbox_with_mail: `You got mail! Please check your dm's.`",
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Color = EmbedColors.Success
                };

                await ctx.Channel.SendMessageAsync(embed: successembed);
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Description = "❌ `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Color = EmbedColors.Error,
                    ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                };

                if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                    errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                await ctx.Channel.SendMessageAsync(embed: errorembed);
            }
            catch (Exception)
            {
                throw;
            }
        }).Add(_bot._watcher, ctx);
    }



    [Command("user-info"), Aliases("userinfo"),
    CommandModule("user"),
    Description("Shows information about you or the mentioned user")]
    public async Task UserInfo(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new UserInfoCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("avatar"), Aliases("pfp"),
    CommandModule("user"),
    Description("Sends your or the mentioned user's avatar as an embedded image")]
    public async Task Avatar(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new AvatarCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("banner"),
    CommandModule("user"),
    Description("Sends your or the mentioned user's banner as an embedded image")]
    public async Task Banner(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new BannerCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("rank"), Aliases("level", "lvl"),
    CommandModule("user"),
    Description("Shows your or the mentioned user's rank and rank progress")]
    public async Task Rank(CommandContext ctx, DiscordUser victim = null)
    {
        Task.Run(async () =>
        {
            await new RankCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "victim", victim }
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("leaderboard"),
    CommandModule("user"),
    Description("Shows the current experience leaderboard")]
    public async Task Leaderboard(CommandContext ctx, [Description("3-50")]int ShowAmount = 10)
    {
        Task.Run(async () =>
        {
            await new LeaderboardCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
            {
                { "ShowAmount", ShowAmount }
            });
        }).Add(_bot._watcher, ctx);
    }



    [Command("submit-url"),
    CommandModule("user"),
    Description("Allows submission of new malicous urls to our database")]
    public async Task UrlSubmit(CommandContext ctx, [Description("URL")]string url)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                return;

            if (!_bot._users.List.ContainsKey(ctx.User.Id))
                _bot._users.List.Add(ctx.User.Id, new Users.Info(_bot));

            if (!_bot._users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS)
            {
                var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", "I accept these conditions", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));

                var tos_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Link Submission • {ctx.Guild.Name}" },
                    Color = EmbedColors.Important,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"{1.DigitsToEmotes()}. You may not submit URLs that are non-malicous.\n" +
                                  $"{2.DigitsToEmotes()}. You may not spam submissions.\n" +
                                  $"{3.DigitsToEmotes()}. You may not submit unregistered domains.\n" +
                                  $"{4.DigitsToEmotes()}. You may not submit shortened URLs.\n\n" +
                                  $"We reserve the right to ban you for any reason that may not be listed.\n" +
                                  $"**Failing to follow these conditions may get you or your guild blacklisted from using this bot.**\n" +
                                  $"**This includes, but is not limited to, pre-existing guilds with your ownership and future guilds.**\n\n" +
                                  $"To accept these conditions, please click the button below. If you do not see a button to interact with, update your discord client."
                };

                var tos_accept = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(tos_embed).AddComponents(button));

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Message.Id == tos_accept.Id && e.User.Id == ctx.User.Id)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                            _bot._users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS = true;

                            var accepted_button = new DiscordButtonComponent(ButtonStyle.Success, "no_id", "Conditions accepted", true, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));
                            await tos_accept.ModifyAsync(new DiscordMessageBuilder().WithEmbed(tos_embed.WithColor(EmbedColors.Success)).AddComponents(accepted_button));

                            _ = ctx.Command.ExecuteAsync(ctx);

                            _ = Task.Delay(10000).ContinueWith(x =>
                            {
                                _ = tos_accept.DeleteAsync();
                            });
                        }
                    }).Add(_bot._watcher, ctx);
                };

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                try
                {
                    await Task.Delay(60000);
                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                    tos_embed.Footer.Text += " • Interaction timed out";
                    await tos_accept.ModifyAsync(new DiscordMessageBuilder().WithEmbed(tos_embed));
                    await Task.Delay(5000);
                    _ = tos_accept.DeleteAsync();
                }
                catch { }
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Phishing Link Submission • {ctx.Guild.Name}" },
                Color = EmbedColors.Processing,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`Processing your request..`"
            };

            var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));

            if (_bot._users.List[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(_bot._status))
            {
                embed.Description = $"`You cannot submit a domain for the next {_bot._users.List[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45).GetTimespanUntil().GetHumanReadable()}.`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = msg.ModifyAsync(embed.Build());
                return;
            }

            if (_bot._submittedUrls.List.Any(x => x.Value.Submitter == ctx.User.Id) && !ctx.User.IsMaintenance(_bot._status))
            {
                if (_bot._submittedUrls.List.Where(x => x.Value.Submitter == ctx.User.Id).Count() >= 10)
                {
                    embed.Description = $"`You have 10 open url submissions. Please wait before trying to submit another url.`";
                    embed.Color = EmbedColors.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = msg.ModifyAsync(embed.Build());
                    return; 
                }
            }

            if (_bot._submissionBans.Users.ContainsKey(ctx.User.Id))
            {
                embed.Description = $"`You are banned from submitting URLs.`\n" +
                                    $"`Reason: {_bot._submissionBans.Users[ctx.User.Id].Reason}`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = msg.ModifyAsync(embed.Build());
                return;
            }

            if (_bot._submissionBans.Guilds.ContainsKey(ctx.Guild.Id))
            {
                embed.Description = $"`This guild is banned from submitting URLs.`\n" +
                                    $"`Reason: {_bot._submissionBans.Guilds[ctx.Guild.Id].Reason}`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = msg.ModifyAsync(embed.Build());
                return;
            }

            string domain = url.ToLower();

            if (domain.StartsWith("https://") || domain.StartsWith("http://"))
                domain = domain.Replace("https://", "").Replace("http://", "");

            if (domain.Contains('/'))
                domain = domain.Remove(domain.IndexOf("/"), domain.Length - domain.IndexOf("/"));

            if (!domain.Contains('.') || domain.Contains(' '))
            {
                embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') you're trying to submit is invalid.`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = msg.ModifyAsync(embed.Build());
                return;
            }

            embed.Description = $"`You are about to submit the domain '{domain.SanitizeForCodeBlock()}'. When submitting, your public user account and guild will be tracked and visible to verifiers. Do you want to proceed?`";
            embed.Color = EmbedColors.Success;
            embed.Author.IconUrl = Resources.LogIcons.Info;

            var continue_button = new DiscordButtonComponent(ButtonStyle.Success, "continue", "Submit domain", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
            var cancel_button = new DiscordButtonComponent(ButtonStyle.Danger, "cancel", "Cancel", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));


            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
                {
                    { continue_button },
                    { cancel_button }
                }));

            bool InteractionExecuted = false;

            async Task RunSubmissionInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    Task.Run(async () =>
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Message?.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            InteractionExecuted = true;
                            ctx.Client.ComponentInteractionCreated -= RunSubmissionInteraction;

                            if (e.Interaction.Data.CustomId == "continue")
                            {
                                embed.Description = $"`Submitting your domain..`";
                                embed.Color = EmbedColors.Loading;
                                embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                embed.Description = $"`Checking if your domain is already in the database..`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                foreach (var b in _bot._phishingUrls.List)
                                {
                                    if (domain.Contains(b.Key))
                                    {
                                        embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') is already present in the database. Thanks for trying to contribute regardless.`";
                                        embed.Color = EmbedColors.Error;
                                        embed.Author.IconUrl = Resources.LogIcons.Error;
                                        _ = msg.ModifyAsync(embed.Build());
                                        return;
                                    }
                                }

                                embed.Description = $"`Checking if your domain has already been submitted before..`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                foreach (var b in _bot._submittedUrls.List)
                                {
                                    if (b.Value.Url == domain)
                                    {
                                        embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') has already been submitted. Thanks for trying to contribute regardless.`";
                                        embed.Color = EmbedColors.Error;
                                        embed.Author.IconUrl = Resources.LogIcons.Error;
                                        _ = msg.ModifyAsync(embed.Build());
                                        return;
                                    }
                                }

                                embed.Description = $"`Creating submission..`";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                var channel = await ctx.Client.GetChannelAsync(_bot._status.LoadedConfig.UrlSubmissionsChannelId);

                                var continue_button = new DiscordButtonComponent(ButtonStyle.Success, "accept_submission", "Accept submission", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
                                var cancel_button = new DiscordButtonComponent(ButtonStyle.Danger, "deny_submission", "Deny submission", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));
                                var ban_user_button = new DiscordButtonComponent(ButtonStyle.Danger, "ban_user", "Deny submission & ban submitter", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));
                                var ban_guild_button = new DiscordButtonComponent(ButtonStyle.Danger, "ban_guild", "Deny submission & ban guild", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));

                                var subbmited_msg = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                                {
                                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Info, Name = $"Phishing Link Submission" },
                                    Color = EmbedColors.Success,
                                    Timestamp = DateTime.UtcNow,
                                    Description = $"`Submitted Url`: `{domain.SanitizeForCodeBlock()}`\n" +
                                                    $"`Submission by`: `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`\n" +
                                                    $"`Submitted on `: `{ctx.Guild.Name} ({ctx.Guild.Id})`"
                                }).AddComponents(new List<DiscordComponent>
                                    {
                                        { continue_button },
                                        { cancel_button },
                                        { ban_user_button },
                                        { ban_guild_button },
                                    }));

                                _bot._submittedUrls.List.Add(subbmited_msg.Id, new SubmittedUrls.UrlInfo
                                {
                                    Url = domain,
                                    Submitter = ctx.User.Id,
                                    GuildOrigin = ctx.Guild.Id
                                });

                                _bot._users.List[ctx.User.Id].UrlSubmissions.LastTime = DateTime.UtcNow;

                                embed.Description = $"`Submission created. Thanks for your contribution.`";
                                embed.Color = EmbedColors.Success;
                                embed.Author.IconUrl = Resources.LogIcons.Info;
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                            }
                            else if (e.Interaction.Data.CustomId == "cancel")
                            {
                                _ = msg.DeleteAsync();
                            }
                        }
                    }).Add(_bot._watcher, ctx);
                });
            };

            ctx.Client.ComponentInteractionCreated += RunSubmissionInteraction;

            try
            {
                await Task.Delay(60000);

                if (InteractionExecuted)
                    return;

                ctx.Client.ComponentInteractionCreated -= RunSubmissionInteraction;

                embed.Footer.Text += " • Interaction timed out";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();
            }
            catch { }
        }).Add(_bot._watcher, ctx);
    }



    [Command("emoji"), Aliases("emojis", "emote", "steal", "grab", "sticker", "stickers"),
    CommandModule("user"),
    Description("Steals emojis of the message that this command was replied to")]
    public async Task EmojiStealer(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, new SharedCommandContext(ctx.Message, _bot)))
                return;

            ulong messageid;

            if (ctx.Message.ReferencedMessage is not null)
            {
                messageid = ctx.Message.ReferencedMessage.Id;
            }
            else
            {
                _ = ctx.SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Downloading emojis of this message..`",
                Color = EmbedColors.Processing,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            var msg = await ctx.Message.ReferencedMessage.RespondAsync(embed: embed);

            HttpClient client = new();

            var bMessage = await ctx.Channel.GetMessageAsync(Convert.ToUInt64(messageid));

            List<string> EmoteList = new();
            Dictionary<ulong, EmojiStealer> SanitizedEmoteList = new();

            List<string> Emotes = bMessage.Content.GetEmotes();
            List<string> AnimatedEmotes = bMessage.Content.GetAnimatedEmotes();

            if (Emotes is null && AnimatedEmotes is null && (bMessage.Stickers is null || bMessage.Stickers.Count == 0))
            {
                embed.Description = $"`This message doesn't contain any emojis or stickers.`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                await msg.ModifyAsync(embed: embed.Build());

                return;
            }

            if (Emotes is not null)
                foreach (var b in Emotes)
                    if (!SanitizedEmoteList.ContainsKey(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value)))
                        SanitizedEmoteList.Add(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value), new EmojiStealer { Animated = false, Name = Regex.Match(b, @"((?<=:)[^ ]*(?=:))").Value, Type = EmojiType.EMOJI });

            if (AnimatedEmotes is not null)
                foreach (var b in AnimatedEmotes)
                    if (!SanitizedEmoteList.ContainsKey(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value)))
                        SanitizedEmoteList.Add(Convert.ToUInt64(Regex.Match(b, @"(\d+(?=>))").Value), new EmojiStealer { Animated = true, Name = Regex.Match(b, @"((?<=:)[^ ]*(?=:))").Value, Type = EmojiType.EMOJI });

            bool ContainsStickers = bMessage.Stickers.Count > 0;

            string guid = Guid.NewGuid().ToString().MakeValidFileName();

            if (Directory.Exists($"emotes-{guid}"))
                Directory.Delete($"emotes-{guid}", true);

            Directory.CreateDirectory($"emotes-{guid}");

            if (SanitizedEmoteList.Count > 0)
            {
                embed.Description = $"`Downloading {SanitizedEmoteList.Count} emojis of this message..`";
                await msg.ModifyAsync(embed: embed.Build());

                foreach (var b in SanitizedEmoteList.ToList())
                {
                    try
                    {
                        var EmoteStream = await client.GetStreamAsync($"https://cdn.discordapp.com/emojis/{b.Key}.{(b.Value.Animated ? "gif" : "png")}");

                        string FileExists = "";
                        int FileExistsInt = 1;

                        string fileName = $"{b.Value.Name}{FileExists}.{(b.Value.Animated ? "gif" : "png")}".MakeValidFileName('_');

                        while (File.Exists($"emotes-{guid}/{fileName}"))
                        {
                            FileExistsInt++;
                            FileExists = $" ({FileExistsInt})";

                            fileName = $"{b.Value.Name}{FileExists}.{(b.Value.Animated ? "gif" : "png")}".MakeValidFileName('_');
                        }

                        using (var fileStream = File.Create($"emotes-{guid}/{fileName}"))
                        {
                            EmoteStream.CopyTo(fileStream);
                            await fileStream.FlushAsync();
                        }

                        SanitizedEmoteList[b.Key].Path = $"emotes-{guid}/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to download an emote", ex);
                        SanitizedEmoteList.Remove(b.Key);
                    }
                } 
            }

            if (ContainsStickers)
            {
                embed.Description = $"`Downloading {bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()).Count()} stickers of this message..`";
                await msg.ModifyAsync(embed: embed.Build());

                foreach (var b in bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()))
                {
                    var StickerStream = await client.GetStreamAsync(b.Url);

                    string FileExists = "";
                    int FileExistsInt = 1;

                    string fileName = $"{b.Name}{FileExists}.png".MakeValidFileName('_');

                    while (File.Exists($"emotes-{guid}/{fileName}"))
                    {
                        FileExistsInt++;
                        FileExists = $" ({FileExistsInt})";

                        fileName = $"{b.Name}{FileExists}.png".MakeValidFileName('_');
                    }

                    using (var fileStream = File.Create($"emotes-{guid}/{fileName}"))
                    {
                        StickerStream.CopyTo(fileStream);
                        await fileStream.FlushAsync();
                    }

                    SanitizedEmoteList.Add(b.Id, new EmojiStealer { Animated = false, Name = b.Name, Path = $"emotes-{guid}/{fileName}", Type = EmojiType.STICKER });
                }
            }

            if (SanitizedEmoteList.Count == 0)
            {
                embed.Description = $"`Couldn't download any emojis or stickers from this message.`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                await msg.ModifyAsync(embed: embed.Build());

                return;
            }

            string emojiText = "";

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                emojiText += "emojis";

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                emojiText += $"{(emojiText.Length > 0 ? " and stickers" : "stickers")}";

            embed.Author.IconUrl = ctx.Guild.IconUrl;
            embed.Description = $"`Select how you want to receive the downloaded {emojiText}.`";

            bool IncludeStickers = false;

            if (!SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                IncludeStickers = true;

            var IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", "Include Stickers", !SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI), new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));
            
            var AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", "Add Emoji(s) to Server", (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || IncludeStickers), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var ZipPrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "ZipPrivateMessage", "Direct Message as Zip File", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖥")));
            var SinglePrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "SinglePrivateMessage", "Direct Message as Single Files", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📱")));

            var SendHereButton = new DiscordButtonComponent(ButtonStyle.Secondary, "SendHere", "In this chat as Zip File", !(ctx.Member.Permissions.HasPermission(Permissions.AttachFiles)), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                builder.AddComponents(IncludeStickersButton);

            builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

            await msg.ModifyAsync(builder);

            CancellationTokenSource cancellationTokenSource = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message?.Id == msg.Id && e.User.Id == ctx.User.Id)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        cancellationTokenSource.Cancel();
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == AddToServerButton.CustomId)
                        {
                            if (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers))
                            {
                                _ = ctx.SendPermissionError(Permissions.ManageEmojisAndStickers);
                                return;
                            }
                            
                            if (IncludeStickers)
                            {
                                embed.Description = $"`You cannot add any emoji(s) to the server while including stickers.`";
                                embed.Color = EmbedColors.Error;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                await msg.ModifyAsync(embed: embed.Build());

                                return;
                            }

                            bool Pinned = false;
                            bool DiscordWarning = false;

                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Added 0/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                            for (int i = 0; i < SanitizedEmoteList.Count; i++)
                            {
                                try
                                {
                                    Task task;

                                    if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                        continue;

                                    using (var fileStream = File.OpenRead(SanitizedEmoteList.ElementAt(i).Value.Path))
                                    {
                                        task = ctx.Guild.CreateEmojiAsync(SanitizedEmoteList.ElementAt(i).Value.Name, fileStream);
                                    }

                                    int WaitSeconds = 0;

                                    while (task.Status == TaskStatus.WaitingForActivation)
                                    {
                                        WaitSeconds++;

                                        if (WaitSeconds > 10 && !DiscordWarning)
                                        {
                                            embed.Description = $"`Added {i}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`\n\n" +
                                                                                $"_The bot most likely hit a rate limit. This can take up to an hour to continue._\n" +
                                                                                $"_If you want this to change, go scream at Discord. There's nothing i can do._";
                                            await msg.ModifyAsync(embed: embed.Build());

                                            DiscordWarning = true;

                                            try
                                            {
                                                await msg.PinAsync();
                                                Pinned = true;
                                            }
                                            catch { }
                                        }
                                        await Task.Delay(1000);
                                    }


                                    if (Pinned)
                                        try
                                        {
                                            await msg.UnpinAsync();
                                            Pinned = false;
                                        }
                                        catch { }

                                    embed.Description = $"`Added {i}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to this server..`";
                                    await msg.ModifyAsync(embed: embed.Build());
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to add an emote to guild", ex);
                                }
                            }

                            embed.Thumbnail = null;
                            embed.Color = EmbedColors.Success;
                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.Description = $"✅ `Downloaded and added {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} emojis to the server.`";
                            await msg.ModifyAsync(embed: embed.Build());
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == SinglePrivateMessageButton.CustomId)
                        {
                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Sending the {emojiText} in your DMs..`";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                            try
                            {
                                for (int i = 0; i < SanitizedEmoteList.Count; i++)
                                {
                                    if (!IncludeStickers)
                                        if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                            continue;

                                    using (var fileStream = File.OpenRead(SanitizedEmoteList.ElementAt(i).Value.Path))
                                    {
                                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent($"`{i + 1}/{(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())}` `{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}`").WithFile($"{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}", fileStream));
                                    }

                                    await Task.Delay(1000);
                                }

                                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent($"Heyho! Here's the {emojiText} you requested."));
                            }
                            catch (DisCatSharp.Exceptions.UnauthorizedException)
                            {
                                var errorembed = new DiscordEmbedBuilder
                                {
                                    Author = new DiscordEmbedBuilder.EmbedAuthor
                                    {
                                        Name = ctx.Guild.Name,
                                        IconUrl = ctx.Guild.IconUrl
                                    },
                                    Description = "❌ `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                                    Footer = ctx.GenerateUsedByFooter(),
                                    Timestamp = DateTime.UtcNow,
                                    Color = EmbedColors.Error,
                                    ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                                };

                                if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                                    errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                                await msg.ModifyAsync(embed: errorembed.Build());
                                _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }

                            embed.Thumbnail = null;
                            embed.Color = EmbedColors.Success;
                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.Description = $"✅ `Downloaded and sent {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText} to your DMs.`";
                            await msg.ModifyAsync(embed: embed.Build());
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == ZipPrivateMessageButton.CustomId || e.Interaction.Data.CustomId == SendHereButton.CustomId)
                        {
                            embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                            embed.Description = $"`Preparing your Zip File..`";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                            if (Directory.Exists($"zipfile-{guid}"))
                                Directory.Delete($"zipfile-{guid}", true);

                            Directory.CreateDirectory($"zipfile-{guid}");

                            for (int i = 0; i < SanitizedEmoteList.Count; i++)
                            {
                                if (!IncludeStickers)
                                    if (SanitizedEmoteList.ElementAt(i).Value.Type != EmojiType.EMOJI)
                                        continue;

                                string NewFilename = $"{SanitizedEmoteList.ElementAt(i).Value.Name}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}";

                                int FileExistsInt = 1;

                                while (File.Exists($"zipfile-{guid}/{NewFilename}"))
                                {
                                    FileExistsInt++;
                                    NewFilename = $"{SanitizedEmoteList.ElementAt(i).Value.Name}_{FileExistsInt}.{(SanitizedEmoteList.ElementAt(i).Value.Animated == true ? "gif" : "png")}";
                                }

                                File.Copy(SanitizedEmoteList.ElementAt(i).Value.Path, $"zipfile-{guid}/{NewFilename}");
                            }

                            ZipFile.CreateFromDirectory($"zipfile-{guid}", $"Emotes-{guid}.zip");

                            if (e.Interaction.Data.CustomId == ZipPrivateMessageButton.CustomId)
                            {
                                embed.Description = $"`Sending your Zip File in DMs..`";
                                await msg.ModifyAsync(embed: embed.Build());

                                try
                                {
                                    using (var fileStream = File.OpenRead($"Emotes-{guid}.zip"))
                                    {
                                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emojis.zip", fileStream).WithContent("Heyho! Here's the emojis you requested."));
                                    }
                                }
                                catch (DisCatSharp.Exceptions.UnauthorizedException)
                                {
                                    var errorembed = new DiscordEmbedBuilder
                                    {
                                        Author = new DiscordEmbedBuilder.EmbedAuthor
                                        {
                                            Name = ctx.Guild.Name,
                                            IconUrl = ctx.Guild.IconUrl
                                        },
                                        Description = "❌ `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                                        Footer = ctx.GenerateUsedByFooter(),
                                        Timestamp = DateTime.UtcNow,
                                        Color = EmbedColors.Error,
                                        ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                                    };

                                    if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                                        errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                                    await msg.ModifyAsync(embed: errorembed.Build());
                                    _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                                    return;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }

                                embed.Thumbnail = null;
                                embed.Color = EmbedColors.Success;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                embed.Description = $"✅ `Downloaded and sent {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText} to your DMs.`";
                                await msg.ModifyAsync(embed: embed.Build());
                            }
                            else if (e.Interaction.Data.CustomId == SendHereButton.CustomId)
                            {
                                if (!ctx.Member.Permissions.HasPermission(Permissions.AttachFiles))
                                {
                                    _ = ctx.SendPermissionError(Permissions.AttachFiles);
                                    return;
                                }

                                embed.Description = $"`Sending your Zip File..`";
                                await msg.ModifyAsync(embed: embed.Build());

                                embed.Thumbnail = null;
                                embed.Color = EmbedColors.Success;
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                embed.Description = $"✅ `Downloaded {(IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.Type == EmojiType.EMOJI).Count())} {emojiText}. Attached is a Zip File containing them.`";

                                using (var fileStream = File.OpenRead($"Emotes-{guid}.zip"))
                                {
                                    await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emotes.zip", fileStream).WithEmbed(embed.Build()));
                                }

                                await msg.DeleteAsync();
                            }
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            return;
                        }
                        else if (e.Interaction.Data.CustomId == IncludeStickersButton.CustomId)
                        {
                            IncludeStickers = !IncludeStickers;

                            if (!IncludeStickers)
                            {
                                if (!SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.EMOJI))
                                    IncludeStickers = true;
                            }

                            IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", "Include Stickers", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));
                            AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", "Add Emoji(s) to Server", (!ctx.Member.Permissions.HasPermission(Permissions.ManageEmojisAndStickers) || IncludeStickers), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));

                            var builder = new DiscordMessageBuilder().WithEmbed(embed);

                            if (SanitizedEmoteList.Any(x => x.Value.Type == EmojiType.STICKER))
                                builder.AddComponents(IncludeStickersButton);

                            builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

                            await msg.ModifyAsync(builder);
                        }

                        cancellationTokenSource = new();
                        ctx.Client.ComponentInteractionCreated += RunInteraction;

                        try
                        {
                            await Task.Delay(60000, cancellationTokenSource.Token);
                            _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                            embed.Footer.Text += " • Interaction timed out";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                            await Task.Delay(5000);
                            _ = msg.DeleteAsync();

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        }
                        catch { }
                    }

                }).Add(_bot._watcher, ctx);
            }

            try
            {
                await Task.Delay(60000, cancellationTokenSource.Token);
                _ = CleanupFilesAndDirectories(new List<string> { $"emotes-{guid}", $"zipfile-{guid}" }, new List<string> { $"Emotes-{guid}.zip" });
                embed.Footer.Text += " • Interaction timed out";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();

                ctx.Client.ComponentInteractionCreated -= RunInteraction;
            }
            catch { }
        }).Add(_bot._watcher, ctx);
    }
}
