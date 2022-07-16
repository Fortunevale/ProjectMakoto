namespace ProjectIchigo.ApplicationCommands;

internal class Maintainers : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("dev_tools", "Developer Tools used to develop/manage Project Ichigo")]
    public class DevTools : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("info", "Shows information about the current guild and bot")]
        public async Task InfoCommand(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

                await new InfoCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("botnick", "Changes the bot's nickname on the current server.")]
        public async Task BotNick(InteractionContext ctx, [Option("nickname", "The new nickname")] string newNickname = "")
        {
            Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

                await new BotnickCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "newNickname", newNickname }
                });
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("globalban", "Bans a user from all servers opted into globalbans")]
        public async Task GlobalBanCommand(InteractionContext ctx, [Option("user", "The user to ban")]DiscordUser victim, [Option("reason", "The reason")]string reason = "-")
        {
            Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

                await new GlobalBanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                    { "reason", reason },
                });
            }).Add(_bot._watcher, ctx);
        }
        
        [SlashCommand("globalunban", "Removes a user from global bans (doesn't unban user from all servers)")]
        public async Task GlobalUnnanCommand(InteractionContext ctx, [Option("user", "The user to unban")]DiscordUser victim)
        {
            Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

                await new GlobalUnbanCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "victim", victim },
                });
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("log", "Change the bot's log level")]
        public async Task Log(InteractionContext ctx, [Option("loglevel", "The new loglevel")] LogLevel Level)
        {
            Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

                await new Commands.LogCommand().ExecuteCommand(ctx, _bot, new Dictionary<string, object>
                {
                    { "Level", Level },
                });
            }).Add(_bot._watcher, ctx);
        }
        
        [SlashCommand("stop", "Shuts down the bot")]
        public async Task Stop(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

                await new StopCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("save", "Save all data to Database")]
        public async Task Save(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

                await new SaveCommand().ExecuteCommand(ctx, _bot);
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("create-issue", "Create a new issue on Project-Ichigo's Github Repository")]
        public async Task CreateIssue(InteractionContext ctx, [Option("use_old_tag_selector", "Allows the use of the legacy tag selector.")] bool UseOldTagsSelector = false)
        {
            Task.Run(async () =>
            {
                if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
                {
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent($"❌ `The GitHub Token expired, please update.`"));
                    return;
                }

                var client = new GitHubClient(new ProductHeaderValue("Project-Ichigo"));

                var tokenAuth = new Credentials(Secrets.Secrets.GithubToken);
                client.Credentials = tokenAuth;

                var labels = await client.Issue.Labels.GetAllForRepository(Secrets.Secrets.GithubUsername, Secrets.Secrets.GithubRepository);

                var modal = new DiscordInteractionModalBuilder().WithCustomId(Guid.NewGuid().ToString()).WithTitle("Create new Issue on Github")
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "New issue", 4, 250, true))
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "description", "Description", required: false));

                if (!UseOldTagsSelector)
                    modal.AddModalComponents(new DiscordSelectComponent("Select tags", labels.Select(x => new DiscordSelectComponentOption(x.Name, x.Name.ToLower().MakeValidFileName(), "", false, new DiscordComponentEmoji(new DiscordColor(x.Color).GetClosestColorEmoji(ctx.Client)))), "labels", 1, labels.Count));
                else
                    modal.AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "labels", "Labels", "", null, null, false, $"Put a # in front of every label you want to add.\n\n{string.Join("\n", labels.Select(x => x.Name))}"));

                await ctx.CreateModalResponseAsync(modal);

                CancellationTokenSource cancellationTokenSource = new();

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Interaction.Data.CustomId == modal.CustomId)
                        {
                            cancellationTokenSource.Cancel();
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            var followup = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent(":arrows_counterclockwise: `Submitting your issue..`"));

                            var labelComp = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "labels").First().Components.First();

                            string title = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "title").First().Components.First().Value;
                            string description = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "description").First().Components.First().Value;
                            List<string> labels;

                            if (labelComp.Type == ComponentType.Select)
                            {
                                labels = labelComp.Values.ToList();
                            }
                            else
                            {
                                labels = labelComp.Value.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Where(x => x.StartsWith("#")).Select(x => x.Replace("#", "")).ToList();
                            }

                            if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
                            {
                                _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($"❌ `The GitHub Token expired, please update.`"));
                                return;
                            }

                            var issue = await client.Issue.Create(Secrets.Secrets.GithubUsername, Secrets.Secrets.GithubRepository, new NewIssue(title) { Body = $"{(description.IsNullOrWhiteSpace() ? "_No description provided_" : description)}\n\n<b/>\n\n##### <img align=\"left\" style=\"align:center;\" width=\"32\" height=\"32\" src=\"{ctx.User.AvatarUrl}\">_Submitted by [`{ctx.User.UsernameWithDiscriminator}`]({ctx.User.ProfileUrl}) (`{ctx.User.Id}`) via Discord._" });

                            if (labels.Count > 0)
                                await client.Issue.Labels.ReplaceAllForIssue(Secrets.Secrets.GithubUsername, Secrets.Secrets.GithubRepository, issue.Number, labels.ToArray());

                            _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($"✅ `Issue submitted:` {issue.HtmlUrl}"));
                        }
                    }).Add(_bot._watcher, ctx);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(15), cancellationTokenSource.Token);

                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                }
                catch { }
            }).Add(_bot._watcher, ctx);
        }
    }

#if DEBUG
    [SlashCommandGroup("debug", "Debug commands, only registered in this server.")]
    public class Debug : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("throw", "Throw.")]
        public async Task Throw(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                if (!ctx.User.IsMaintenance(_bot._status))
                {
                    _ = ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"❌ `This command is restricted to Staff Members of Project Ichigo.`"));
                    return;
                }

                throw new InvalidCastException();
            }).Add(_bot._watcher, ctx);
        }

        [SlashCommand("test-component-modify", "Debug")]
        public async Task TestComponentModify(InteractionContext ctx, [Option("Refetch", "Whether to refetch the message")] bool Refetch)
        {
            try
            {
                _ = ctx.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var msg = await ctx.Channel.SendMessageAsync("Test Message: This could be showing the user that something is loading");

                await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("Loading could be done").AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "a", "button")));

                // Refetch the message to hopefully update it's components object
                // This doesn't make a difference right now

                if (Refetch)
                    msg = await msg.Channel.GetMessageAsync(msg.Id);

                var x = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(1)); // This will throw because there's no components in the message object

                if (x.TimedOut)
                    return;

                await msg.ModifyAsync(new DiscordMessageBuilder().WithContent("button worked 😎"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        
        [SlashCommand("rawuserinfo", "Debug")]
        public async Task RawUserInfo(InteractionContext ctx, [Option("user", "The user")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"```json\n{JsonConvert.SerializeObject(await user.GetFromApiAsync(), Formatting.Indented)}\n```"));
        }
    }
#endif
}
