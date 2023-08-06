// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Octokit;

namespace ProjectMakoto.Commands;

internal sealed class ReportTranslationCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Utility.ReportTranslation;

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var affectedType = (ReportTranslationType)arguments["affectedType"];
            var reasonType = (ReportTranslationReason)arguments["reasonType"];
            var component = (string)arguments["component"];
            var additionalInformation = (string?)arguments["additionalInformation"];

            var tos_version = 1;

            if (ctx.DbUser.TranslationReports.AcceptedTOS != tos_version)
            {
                var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", this.GetString(CommandKey.AcceptTos), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));

                var tos_embed = new DiscordEmbedBuilder
                {
                    Description = this.GetString(CommandKey.Tos,
                        new TVar("1", 1.ToEmotes()),
                        new TVar("2", 2.ToEmotes()),
                        new TVar("3", 3.ToEmotes()),
                        new TVar("4", 4.ToEmotes()))
                }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

                if (ctx.DbUser.TranslationReports.AcceptedTOS != 0 && ctx.DbUser.TranslationReports.AcceptedTOS < tos_version)
                {
                    tos_embed.Description = tos_embed.Description.Insert(0, $"**{this.GetString(CommandKey.TosChangedNotice)}**\n\n");
                }

                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed).AddComponents(button));

                var TosAccept = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

                if (TosAccept.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }

                await TosAccept.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.DbUser.TranslationReports.AcceptedTOS = tos_version;
            }

            if (ctx.Bot.status.LoadedConfig.Secrets.Github.TokenExperiation.GetTotalSecondsUntil() <= 0)
                throw new Exception("Required login data for report outdated.");

            if (ctx.DbUser.TranslationReports.FirstRequestTime.GetTimespanSince() > TimeSpan.FromHours(24))
            {
                ctx.DbUser.TranslationReports.RequestCount = 0;
                ctx.DbUser.TranslationReports.FirstRequestTime = DateTime.UtcNow;
            }

            if (ctx.DbUser.TranslationReports.RequestCount >= 3)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.RatelimitReached, true, new TVar("Timestamp", ctx.DbUser.TranslationReports.FirstRequestTime.AddHours(24).ToTimestamp())))
                    .AsError(ctx, this.GetString(CommandKey.Title)));
                return;
            }

            var YesButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Common.Yes), false, "✅".UnicodeToEmoji().ToComponent());
            var NoButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetString(this.t.Common.No), false, "❌".UnicodeToEmoji().ToComponent());

            _ = await this.RespondOrEdit(new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"{this.GetString(CommandKey.ConfirmationPrompt, true)}")
                    .AsAwaitingInput(ctx, this.GetString(CommandKey.Title)))
                .AddComponents(YesButton, NoButton));

            var result = await ctx.ResponseMessage.WaitForButtonAsync(ctx.User);

            if (result.TimedOut)
            {
                this.ModifyToTimedOut();
                return;
            }

            if (result.Result.GetCustomId() != YesButton.CustomId)
            {
                this.DeleteOrInvalidate();
                return;
            }

            string GetReason(ReportTranslationReason reason)
            {
                return reason switch
                {
                    ReportTranslationReason.MissingTranslation => "Missing Translation",
                    ReportTranslationReason.IncorrectTranslation => "Incorrect Translation",
                    ReportTranslationReason.ValuesNotFilledIntoString => "Values Missing in Strings",
                    ReportTranslationReason.Other => "Other",
                    _ => throw new NotImplementedException(),
                };
            }

            string GetType(ReportTranslationType type)
            {
                return Enum.GetName(typeof(ReportTranslationType), type);
            }

            var issue = await ctx.Bot.GithubClient.Issue.Create(ctx.Bot.status.LoadedConfig.Secrets.Github.Username,
                ctx.Bot.status.LoadedConfig.Secrets.Github.Repository,
                new NewIssue($"{GetReason(reasonType)}: {component.FullSanitize()}")
                {
                    Body = 
                        $"### Component Type: `{GetType(affectedType)}`\n" +
                        $"### Affected Component: `{component.SanitizeForCode().Replace("@", "")}`\n" +
                        $"```\n" +
                        $"{additionalInformation?.Replace("@", "") ?? "No additional information supplied."}\n" +
                        $"```\n" +
                        $"</br></br></br>\n" +
                        $"**Submission Details**\n" +
                        $"</br>\n" +
                        $"<img align=\"left\" style=\"align:center;\" width=\"32\" height=\"32\" src=\"{ctx.User.AvatarUrl}\"> [`{ctx.User.GetUsernameWithIdentifier().SanitizeForCode()}`]({ctx.User.AvatarUrl}) (`{ctx.User.Id}`)\n\n" +
                        $"<img align=\"left\" style=\"align:center;\" width=\"32\" height=\"32\" src=\"{ctx.Guild.IconUrl}\"> [`{ctx.Guild.Name.SanitizeForCode()}`]({ctx.Guild.IconUrl}) (`{ctx.Guild.Id}`)\n"
                });

            try
            {
                _ = await ctx.Bot.GithubClient.Issue.Labels.ReplaceAllForIssue(ctx.Bot.status.LoadedConfig.Secrets.Github.Username, ctx.Bot.status.LoadedConfig.Secrets.Github.Repository, issue.Number, new string[] { "Translations", "Low Priority" });
            }
            catch (Exception ex)
            {
                _logger.LogWarn("Failed to update labels on reported issue", ex);
            }

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                .WithDescription(this.GetString(CommandKey.ReportSubmitted, true))
                .AsSuccess(ctx, this.GetString(CommandKey.Title)));
        });
    }
}