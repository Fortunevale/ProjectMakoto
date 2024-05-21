// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;
internal sealed class GlobalBanCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victims = await DiscordExtensions.ParseStringAsUserArray((string)arguments["victims"], ctx.Client);
            var reason = (string)arguments["reason"];

            if (victims?.Length <= 0)
            {
                _ = this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Please provide user(s).`").AsError(ctx, "Global Ban"));
                return;
            }

            if (reason.IsNullOrWhiteSpace())
            {
                _ = this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Please provide a reason for the global ban.`").AsError(ctx, "Global Ban"));
                return;
            }

            var currentStatus = new Dictionary<DiscordUser, CurrentStatus>([
                    ..victims.Select(x => new KeyValuePair<DiscordUser, CurrentStatus>(x, CurrentStatus.InQueue)).ToList()
                ]);

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var desc = string.Empty;

                    lock (currentStatus)
                    {
                        desc = $"{string.Join("\n\n", currentStatus
                            .Select(x =>
                            {
                                var emoji = x.Value switch
                                {
                                    CurrentStatus.Invalid => DiscordEmoji.FromUnicode("❌"),
                                    CurrentStatus.Added => DiscordEmoji.FromUnicode("✅"),
                                    CurrentStatus.Changed => DiscordEmoji.FromUnicode("🔄"),
                                    CurrentStatus.InQueue => DiscordEmoji.FromUnicode("🕒"),
                                    CurrentStatus.InProgress => EmojiTemplates.GetLoading(ctx.Bot),
                                    _ => throw new NotImplementedException(),
                                };
                                
                                var text = x.Value switch
                                {
                                    CurrentStatus.Invalid => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `This user cannot be global banned.`",
                                    CurrentStatus.Added => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `User was added to global ban list.`",
                                    CurrentStatus.Changed => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `User was already global banned, updated entry.`",
                                    CurrentStatus.InQueue => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `In queue..`",
                                    CurrentStatus.InProgress => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `Processing..`",
                                    _ => throw new NotImplementedException(),
                                };

                                return $"{emoji} {text}";
                            }))}";
                    }

                    var embed = new DiscordEmbedBuilder();

                    var done = true;

                    if (currentStatus.All(x => x.Value is CurrentStatus.Changed or CurrentStatus.Added))
                        _ = embed.AsSuccess(ctx, "Global Ban").WithDescription(desc.TruncateWithIndication(2000));
                    else if (currentStatus.All(x => x.Value is CurrentStatus.Changed or CurrentStatus.Added or CurrentStatus.Invalid))
                        _ = embed.AsWarning(ctx, "Global Ban").WithDescription(desc.TruncateWithIndication(2000));
                    else
                    {
                        _ = embed.AsLoading(ctx, "Global Ban").WithDescription($"`Global banning {currentStatus.Count} users..`\n\n{desc}".TruncateWithIndication(2000));
                        done = false;
                    }

                    _ = await this.RespondOrEdit(embed);

                    if (done)
                        return;

                    await Task.Delay(1000);
                }
            }).Add(ctx.Bot, ctx);

            foreach (var victim in currentStatus)
            {
                currentStatus[victim.Key] = CurrentStatus.InProgress;
                await Task.Delay(2000);

                if (ctx.Bot.globalBans.ContainsKey(victim.Key.Id))
                {
                    ctx.Bot.globalBans[victim.Key.Id].Reason = reason;
                    ctx.Bot.globalBans[victim.Key.Id].Moderator = ctx.User.Id;
                    currentStatus[victim.Key] = CurrentStatus.Changed;

                    var announceChannel1 = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
                    _ = await announceChannel1.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.CurrentUser.GetUsername(),
                            IconUrl = AuditLogIcons.UserUpdated
                        },
                        Description = $"The global ban entry of {victim.Key.Mention} `{victim.Key.GetUsernameWithIdentifier()}` (`{victim.Key.Id}`) was updated.\n\n" +
                                      $"Reason: `{reason.SanitizeForCode()}`\n" +
                                      $"Moderator: {ctx.User.Mention} `{ctx.User.GetUsernameWithIdentifier()}` (`{ctx.User.Id}`)",
                        Color = EmbedColors.Warning,
                        Timestamp = DateTime.UtcNow
                    });
                    continue;
                }

                if (ctx.Bot.status.TeamMembers.Contains(victim.Key.Id))
                {
                    currentStatus[victim.Key] = CurrentStatus.Invalid;
                    continue;
                }

                ctx.Bot.globalBans.Add(victim.Key.Id, new(ctx.Bot, "globalbans", victim.Key.Id) { Reason = reason, Moderator = ctx.User.Id });

                var Success = 0;
                var Failed = 0;

                foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
                {
                    if (!ctx.Bot.Guilds.ContainsKey(b.Key))
                        ctx.Bot.Guilds.Add(b.Key, new Guild(ctx.Bot, b.Key));

                    if (ctx.Bot.Guilds[b.Key].Join.AutoBanGlobalBans)
                    {
                        try
                        {
                            await b.Value.BanMemberAsync(victim.Key.Id, 7, $"Globalban: {reason}");
                            Success++;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Exception occurred while trying to ban user from {guild}", b.Key);
                            Failed++;
                        }
                    }
                }

                currentStatus[victim.Key] = CurrentStatus.Added;

                var announceChannel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
                _ = await announceChannel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.CurrentUser.GetUsername(),
                        IconUrl = AuditLogIcons.UserBanned
                    },
                    Description = $"{victim.Key.Mention} `{victim.Key.GetUsernameWithIdentifier()}` (`{victim.Key.Id}`) was added to the global ban list.\n\n" +
                                  $"Reason: `{reason.SanitizeForCode()}`\n" +
                                  $"Moderator: {ctx.User.Mention} `{ctx.User.GetUsernameWithIdentifier()}` (`{ctx.User.Id}`)",
                    Color = EmbedColors.Error,
                    Timestamp = DateTime.UtcNow
                });
            }
        });
    }

    private enum CurrentStatus
    {
        InQueue,
        InProgress,
        Changed,
        Added,
        Invalid
    }
}
