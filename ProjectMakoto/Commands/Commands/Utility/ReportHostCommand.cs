// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class ReportHostCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var url = (string)arguments["url"];

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var tos_version = 3;

            if (ctx.DbUser.UrlSubmissions.AcceptedTOS != tos_version)
            {
                var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", this.GetString(this.t.Commands.Utility.ReportHost.AcceptTos), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));

                var tos_embed = new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Utility.ReportHost.Tos,
                        new TVar("1", 1.ToEmotes()),
                        new TVar("2", 2.ToEmotes()),
                        new TVar("3", 3.ToEmotes()),
                        new TVar("4", 4.ToEmotes()))
                }.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title));

                if (ctx.DbUser.UrlSubmissions.AcceptedTOS != 0 && ctx.DbUser.UrlSubmissions.AcceptedTOS < tos_version)
                {
                    tos_embed.Description = tos_embed.Description.Insert(0, $"**{this.GetString(this.t.Commands.Utility.ReportHost.TosChangedNotice)}**\n\n");
                }

                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed).AddComponents(button));

                var TosAccept = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

                if (TosAccept.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }

                await TosAccept.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.DbUser.UrlSubmissions.AcceptedTOS = tos_version;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Utility.ReportHost.Processing, true)
            }.AsLoading(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title));

            _ = await this.RespondOrEdit(embed);

            if (ctx.DbUser.UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.CooldownError, true,
                    new TVar("Timestamp", ctx.DbUser.UrlSubmissions.LastTime.AddMinutes(45).ToTimestamp()));
                _ = this.RespondOrEdit(embed.AsError(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title)));
                return;
            }

            if (ctx.Bot.SubmittedHosts.Fetch().Any(x => x.Value.Submitter == ctx.User.Id) && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                if (ctx.Bot.SubmittedHosts.Fetch().Where(x => x.Value.Submitter == ctx.User.Id).Count() >= 5)
                {
                    embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.LimitError, true);
                    _ = this.RespondOrEdit(embed.AsError(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title)));
                    return;
                }
            }

            string host;

            try
            {
                host = new UriBuilder(url).Host;
            }
            catch (Exception)
            {
                embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.InvalidHost, true,
                    new TVar("Host", url, true));
                _ = this.RespondOrEdit(embed.AsError(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title)));
                return;
            }

            embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.ConfirmHost, true,
                new TVar("Host", host, true));
            _ = embed.AsAwaitingInput(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title));

            var ContinueButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Common.Confirm), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
            {
                { ContinueButton },
                { MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot) }
            }));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            await e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ContinueButton.CustomId)
            {
                _ = embed.AsLoading(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title));

                embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.DatabaseCheck, true);
                _ = await this.RespondOrEdit(embed);

                foreach (var b in ctx.Bot.PhishingHosts)
                {
                    if (host.Contains(b.Key))
                    {
                        embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.DatabaseError, true, new TVar("Host", host, true));
                        _ = embed.AsError(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title));
                        _ = this.RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.SubmissionCheck, true);
                _ = await this.RespondOrEdit(embed);

                foreach (var b in ctx.Bot.SubmittedHosts)
                {
                    if (b.Value.Url == host)
                    {
                        embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.SubmissionError, true, new TVar("Host", host, true));
                        _ = embed.AsError(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title));
                        _ = this.RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.CreatingSubmission, true);
                _ = await this.RespondOrEdit(embed);

                var channel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.UrlSubmissions);

                var AcceptSubmission = new DiscordButtonComponent(ButtonStyle.Success, "accept_submission", "Accept submission", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
                var DenySubmission = new DiscordButtonComponent(ButtonStyle.Danger, "deny_submission", "Deny submission", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanUserButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_user", "Deny submission & ban submitter", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanGuildButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_guild", "Deny submission & ban guild", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));

                var submittedMsg = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = StatusIndicatorIcons.Success, Name = this.GetString(this.t.Commands.Utility.ReportHost.Title) },
                    Color = EmbedColors.Success,
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Submitted host`: `{host.SanitizeForCode()}`\n" +
                                  $"`Submission by `: `{ctx.User.GetUsernameWithIdentifier()} ({ctx.User.Id})`\n" +
                                  $"`Submitted on  `: `{ctx.Guild.Name} ({ctx.Guild.Id})`"
                })
                .AddComponents(new List<DiscordComponent>
                {
                    { AcceptSubmission },
                    { DenySubmission },
                    { BanUserButton },
                    { BanGuildButton },
                }));

                ctx.Bot.SubmittedHosts.Add(submittedMsg.Id, new SubmittedUrlEntry(ctx.Bot, submittedMsg.Id)
                {
                    Url = host,
                    Submitter = ctx.User.Id,
                    GuildOrigin = ctx.Guild.Id
                });

                ctx.DbUser.UrlSubmissions.LastTime = DateTime.UtcNow;

                embed.Description = this.GetString(this.t.Commands.Utility.ReportHost.SubmissionCreated, true);
                _ = embed.AsSuccess(ctx, this.GetString(this.t.Commands.Utility.ReportHost.Title));
                _ = await this.RespondOrEdit(embed);
            }
            else if (e.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
            }
        });
    }
}