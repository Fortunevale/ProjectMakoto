// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Octokit;

namespace ProjectMakoto.Commands.DevTools;

internal sealed class CreateIssueCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckMaintenance() && await this.CheckSource(Enums.CommandType.ApplicationCommand));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var UseOldTagsSelector = (bool)arguments["UseOldTagsSelector"];

            if (ctx.Bot.status.LoadedConfig.Secrets.Github.TokenExperiation.GetTotalSecondsUntil() <= 0)
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithContent($"❌ `The GitHub Token expired, please update.`"));
                return;
            }

            var labels = await ctx.Bot.GithubClient.Issue.Labels.GetAllForRepository(ctx.Bot.status.LoadedConfig.Secrets.Github.Username, ctx.Bot.status.LoadedConfig.Secrets.Github.Repository);

            var modal = new DiscordInteractionModalBuilder().WithCustomId(Guid.NewGuid().ToString()).WithTitle("Create new Issue on Github")
                .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "New issue", 4, 250, true))
                .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "description", "Description", required: false));

            if (!UseOldTagsSelector)
                _ = modal.AddModalComponents(new DiscordStringSelectComponent("Select tags", labels.Select(x => new DiscordStringSelectComponentOption(x.Name, x.Name.ToLower().MakeValidFileName(), "", false, new DiscordComponentEmoji(new DiscordColor(x.Color).GetClosestColorEmoji(ctx.Client)))), "labels", 1, labels.Count));
            else
                _ = modal.AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "labels", "Labels", "", null, null, false, $"Put a # in front of every label you want to add.\n\n{string.Join("\n", labels.Select(x => x.Name))}"));

            await ctx.OriginalInteractionContext.CreateModalResponseAsync(modal);

            CancellationTokenSource cancellationTokenSource = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    if (e.GetCustomId() == modal.CustomId)
                    {
                        cancellationTokenSource.Cancel();
                        ctx.Client.ComponentInteractionCreated -= RunInteraction;

                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        var followup = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent(":arrows_counterclockwise: `Submitting your issue..`"));

                        var labelComp = e.Interaction.Data.Components.Where(x => x.CustomId == "labels").First();

                        var title = e.Interaction.Data.Components.Where(x => x.CustomId == "title").First().Value;
                        var description = e.Interaction.Data.Components.Where(x => x.CustomId == "description").First().Value;
                        var labels = labelComp.Type == ComponentType.StringSelect
                            ? labelComp.Values.ToList()
                            : labelComp.Value.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Where(x => x.StartsWith('#')).Select(x => x.Replace("#", "")).ToList();

                        if (ctx.Bot.status.LoadedConfig.Secrets.Github.TokenExperiation.GetTotalSecondsUntil() <= 0)
                        {
                            _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($"❌ `The GitHub Token expired, please update.`"));
                            return;
                        }

                        var issue = await ctx.Bot.GithubClient.Issue.Create(ctx.Bot.status.LoadedConfig.Secrets.Github.Username, ctx.Bot.status.LoadedConfig.Secrets.Github.Repository, new NewIssue(title) { Body = $"{(description.IsNullOrWhiteSpace() ? "_No description provided_" : description)}\n\n<b/>\n\n##### <img align=\"left\" style=\"align:center;\" width=\"32\" height=\"32\" src=\"{ctx.User.AvatarUrl}\">_Submitted by [`{ctx.User.GetUsernameWithIdentifier()}`]({ctx.User.ProfileUrl}) (`{ctx.User.Id}`) via Discord._" });

                        if (labels.Count > 0)
                            _ = await ctx.Bot.GithubClient.Issue.Labels.ReplaceAllForIssue(ctx.Bot.status.LoadedConfig.Secrets.Github.Username, ctx.Bot.status.LoadedConfig.Secrets.Github.Repository, issue.Number, labels.ToArray());

                        _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($"✅ `Issue submitted:` {issue.HtmlUrl}"));
                    }
                }).Add(ctx.Bot, ctx);
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
