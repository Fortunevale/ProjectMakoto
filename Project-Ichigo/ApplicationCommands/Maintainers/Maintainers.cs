using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Entities;

namespace Project_Ichigo.ApplicationCommands.Maintainers;

internal class Maintainers : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("github", "Interact with Project-Ichigo's Github Repository", (long)Permissions.UseApplicationCommands, true)]
    public class Github : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("create-issue", "Create a new issue on Project-Ichigo's Github Repository", (long)Permissions.UseApplicationCommands, true)]
        public async Task CreateIssue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                if (!ctx.User.IsMaintenance(_bot._status))
                {
                    _ = ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($":x: `This command is restricted to Staff Members of Project Ichigo.`"));
                    return;
                }

                if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
                {
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent($":x: `The GitHub Token expired, please update.`"));
                    return;
                }

                var client = new GitHubClient(new ProductHeaderValue("Project-Ichigo"));

                var tokenAuth = new Credentials(Secrets.Secrets.GithubToken);
                client.Credentials = tokenAuth;

                var labels = await client.Issue.Labels.GetAllForRepository("TheXorog", "Project-Ichigo");
                
                var modal = new DiscordInteractionModalBuilder().WithCustomId(Guid.NewGuid().ToString()).WithTitle("Create new Issue on Github")
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "New issue", 4, 250, true))
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "description", "Description", required: false))
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "labels", "Labels", "", null, null, false, $"Put a # in front of every label you want to add.\n\n{string.Join("\n", labels.Select(x => x.Name))}"));

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

                            string title = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "title").First().Components.First().Value;
                            string description = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "description").First().Components.First().Value;
                            string labelsraw = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "labels").First().Components.First().Value;

                            if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
                            {
                                _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($":x: `The GitHub Token expired, please update.`"));
                                return;
                            }

                            List<string> labels = labelsraw.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Where(x => x.StartsWith("#")).Select(x => x.Replace("#", "")).ToList();

                            var issue = await client.Issue.Create("TheXorog", "Project-Ichigo", new NewIssue(title) { Body = description });

                            if (labels.Count > 0)
                                await client.Issue.Labels.ReplaceAllForIssue("TheXorog", "Project-Ichigo", issue.Number, labels.ToArray());

                            await client.Issue.Assignee.AddAssignees("TheXorog", "Project-Ichigo", issue.Number, new AssigneesUpdate(new List<string> { "TheXorog" }));

                            _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($":white_check_mark: `Issue submitted:` {issue.HtmlUrl}"));
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
}
