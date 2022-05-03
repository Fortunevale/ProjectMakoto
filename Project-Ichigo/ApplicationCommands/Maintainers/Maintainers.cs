using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Entities;

namespace Project_Ichigo.ApplicationCommands.Maintainers;

internal class Maintainers : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("github", "Interact with Project-Ichigo's Github Repository", (long)Permissions.None, false)]
    public class Github : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            if (ctx.User.IsMaintenance(_bot._status))
                return true;

            _ = ctx.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent($"{false.BoolToEmote()} `This command is restricted to Staff Members of Project Ichigo.`"));
            return false;
        }

        [SlashCommand("create-issue", "Create a new issue on Project-Ichigo's Github Repository", (long)Permissions.None, false)]
        public async Task CreateIssue(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
                {
                    var followup = await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent($"{false.BoolToEmote()} `The GitHub Token expired, please update.`"));
                    return;
                }

                var modal = new DiscordInteractionModalBuilder().WithCustomId(Guid.NewGuid().ToString()).WithTitle("Create new Issue on Github")
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "New issue", 4, 250, true))
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "description", "Description"));

                await ctx.CreateModalResponseAsync(modal);

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Interaction.Data.CustomId == modal.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            var followup = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent("<a:L3:940100205720784936> `Submitting your issue..`"));

                            string title = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "title").First().Components.First().Value;
                            string description = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "description").First().Components.First().Value;

                            var client = new GitHubClient(new ProductHeaderValue("Project-Ichigo"));

                            if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
                            {
                                _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($"{false.BoolToEmote()} `The GitHub Token expired, please update.`"));
                                return;
                            }

                            var tokenAuth = new Credentials(Secrets.Secrets.GithubToken);
                            client.Credentials = tokenAuth;

                            var issue = await client.Issue.Create("TheXorog", "Project-Ichigo", new NewIssue(title) { Body = description });

                            _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($"{true.BoolToEmote()} `Issue submitted:` {issue.HtmlUrl}"));
                        }
                    }).Add(_bot._watcher);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(15));

                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                }
                catch { }
            }).Add(_bot._watcher);
        }
    }
}
