namespace ProjectIchigo.Commands;

internal class CreateIssueCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckMaintenance() && await CheckSource(Enums.CommandType.ApplicationCommand));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            bool UseOldTagsSelector = (bool)arguments["UseOldTagsSelector"];

            if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithContent($"❌ `The GitHub Token expired, please update.`"));
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

            await ctx.OriginalInteractionContext.CreateModalResponseAsync(modal);

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

                        var labelComp = e.Interaction.Data.Components.Where(x => x.CustomId == "labels").First();

                        string title = e.Interaction.Data.Components.Where(x => x.CustomId == "title").First().Value;
                        string description = e.Interaction.Data.Components.Where(x => x.CustomId == "description").First().Value;
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
                }).Add(ctx.Bot.watcher, ctx);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(15), cancellationTokenSource.Token);

                ctx.Client.ComponentInteractionCreated -= RunInteraction;
            }
            catch { }
        });
    }
}
