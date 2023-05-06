// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class ReportHostCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string url = (string)arguments["url"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            int tos_version = 3;

            if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS != tos_version)
            {
                var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", GetString(t.Commands.Utility.ReportHost.AcceptTos), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));

                var tos_embed = new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Utility.ReportHost.Tos,
                        new TVar("1", 1.ToEmotes()),
                        new TVar("2", 2.ToEmotes()),
                        new TVar("3", 3.ToEmotes()),
                        new TVar("4", 4.ToEmotes()))
                }.AsAwaitingInput(ctx, GetString(t.Commands.Utility.ReportHost.Title));

                if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS != 0 && ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS < tos_version)
                {
                    tos_embed.Description = tos_embed.Description.Insert(0, $"**{GetString(t.Commands.Utility.ReportHost.TosChangedNotice)}**\n\n");
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed).AddComponents(button));

                var TosAccept = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

                if (TosAccept.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                await TosAccept.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS = tos_version;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Utility.ReportHost.Processing, true)
            }.AsLoading(ctx, GetString(t.Commands.Utility.ReportHost.Title));

            await RespondOrEdit(embed);

            if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                embed.Description = GetString(t.Commands.Utility.ReportHost.CooldownError, true,
                    new TVar("Timestamp", ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45).ToTimestamp()));
                _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.Utility.ReportHost.Title)));
                return;
            }

            if (ctx.Bot.submittedUrls.Any(x => x.Value.Submitter == ctx.User.Id) && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                if (ctx.Bot.submittedUrls.Where(x => x.Value.Submitter == ctx.User.Id).Count() >= 5)
                {
                    embed.Description = GetString(t.Commands.Utility.ReportHost.LimitError, true);
                    _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.Utility.ReportHost.Title)));
                    return;
                }
            }

            if (ctx.Bot.phishingUrlSubmissionUserBans.TryGetValue(ctx.User.Id, out PhishingSubmissionBanDetails userBan))
            {
                embed.Description = $"`{GetString(t.Commands.Utility.ReportHost.UserBan)}`\n" +
                                    $"`{GetString(t.Common.Reason)}: {userBan.Reason}`";
                _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.Utility.ReportHost.Title)));
                return;
            }

            if (ctx.Bot.phishingUrlSubmissionGuildBans.TryGetValue(ctx.Guild.Id, out PhishingSubmissionBanDetails guildBan))
            {
                embed.Description = $"`{GetString(t.Commands.Utility.ReportHost.GuildBan)}`\n" +
                                    $"`{GetString(t.Common.Reason)}: {guildBan.Reason}`";
                _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.Utility.ReportHost.Title)));
                return;
            }

            string host;

            try
            {
                host = new UriBuilder(url).Host;
            }
            catch (Exception)
            {
                embed.Description = GetString(t.Commands.Utility.ReportHost.InvalidHost, true, 
                    new TVar("Host", url, true));
                _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.Utility.ReportHost.Title)));
                return;
            }

            embed.Description = GetString(t.Commands.Utility.ReportHost.ConfirmHost, true,
                new TVar("Host", host, true));
            embed.AsAwaitingInput(ctx, GetString(t.Commands.Utility.ReportHost.Title));

            var ContinueButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Common.Confirm), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
            {
                { ContinueButton },
                { MessageComponents.GetCancelButton(ctx.DbUser) }
            }));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            await e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ContinueButton.CustomId)
            {
                embed.AsLoading(ctx, GetString(t.Commands.Utility.ReportHost.Title));

                embed.Description = GetString(t.Commands.Utility.ReportHost.DatabaseCheck, true);
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot.phishingUrls)
                {
                    if (host.Contains(b.Key))
                    {
                        embed.Description = GetString(t.Commands.Utility.ReportHost.DatabaseError, true, new TVar("Host", host, true));
                        embed.AsError(ctx, GetString(t.Commands.Utility.ReportHost.Title));
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = GetString(t.Commands.Utility.ReportHost.SubmissionCheck, true);
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot.submittedUrls)
                {
                    if (b.Value.Url == host)
                    {
                        embed.Description = GetString(t.Commands.Utility.ReportHost.SubmissionError, true, new TVar("Host", host, true));
                        embed.AsError(ctx, GetString(t.Commands.Utility.ReportHost.Title));
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = GetString(t.Commands.Utility.ReportHost.CreatingSubmission, true);
                await RespondOrEdit(embed);

                var channel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.UrlSubmissions);

                var AcceptSubmission = new DiscordButtonComponent(ButtonStyle.Success, "accept_submission", "Accept submission", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
                var DenySubmission = new DiscordButtonComponent(ButtonStyle.Danger, "deny_submission", "Deny submission", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanUserButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_user", "Deny submission & ban submitter", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanGuildButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_guild", "Deny submission & ban guild", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));

                var subbmited_msg = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = StatusIndicatorIcons.Success, Name = GetString(t.Commands.Utility.ReportHost.Title) },
                    Color = EmbedColors.Success,
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Submitted host`: `{host.SanitizeForCode()}`\n" +
                                  $"`Submission by `: `{ctx.User.GetUsername()} ({ctx.User.Id})`\n" +
                                  $"`Submitted on  `: `{ctx.Guild.Name} ({ctx.Guild.Id})`"
                })
                .AddComponents(new List<DiscordComponent>
                {
                    { AcceptSubmission },
                    { DenySubmission },
                    { BanUserButton },
                    { BanGuildButton },
                }));

                ctx.Bot.submittedUrls.Add(subbmited_msg.Id, new SubmittedUrlEntry
                {
                    Url = host,
                    Submitter = ctx.User.Id,
                    GuildOrigin = ctx.Guild.Id
                });

                ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime = DateTime.UtcNow;

                embed.Description = GetString(t.Commands.Utility.ReportHost.SubmissionCreated, true);
                embed.AsSuccess(ctx, GetString(t.Commands.Utility.ReportHost.Title));
                await RespondOrEdit(embed);
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}