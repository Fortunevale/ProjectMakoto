// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class GlobalUnbanCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victims = await DiscordExtensions.ParseStringAsUserArray((string)arguments["victims"], ctx.Client);
            var UnbanFromGuilds = (bool)arguments["UnbanFromGuilds"];

            if (victims?.Length <= 0)
            {
                _ = this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Please provide user(s).`").AsError(ctx, "Global Ban"));
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
                                    CurrentStatus.Removed => DiscordEmoji.FromUnicode("✅"),
                                    CurrentStatus.InQueue => DiscordEmoji.FromUnicode("🕒"),
                                    CurrentStatus.InProgress => EmojiTemplates.GetLoading(ctx.Bot),
                                    _ => throw new NotImplementedException(),
                                };

                                var text = x.Value switch
                                {
                                    CurrentStatus.Invalid => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `This user is not on the global ban list.`",
                                    CurrentStatus.Removed => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `User was removed from the global ban list.`",
                                    CurrentStatus.InQueue => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `In queue..`",
                                    CurrentStatus.InProgress => $"**`{x.Key.GetUsernameWithIdentifier()} ({x.Key.Id})`**\n{EmojiTemplates.GetInVisible(ctx.Bot)} `Processing..`",
                                    _ => throw new NotImplementedException(),
                                };

                                return $"{emoji} {text}";
                            }))}";
                    }

                    var embed = new DiscordEmbedBuilder();

                    var done = true;

                    if (currentStatus.All(x => x.Value is CurrentStatus.Removed))
                        _ = embed.AsSuccess(ctx, "Global Ban").WithDescription(desc.TruncateWithIndication(2000));
                    else if (currentStatus.All(x => x.Value is CurrentStatus.Removed or CurrentStatus.Invalid))
                        _ = embed.AsWarning(ctx, "Global Ban").WithDescription(desc.TruncateWithIndication(2000));
                    else
                    {
                        _ = embed.AsLoading(ctx, "Global Ban").WithDescription($"`Removing Global ban for {currentStatus.Count} users..`\n\n{desc}".TruncateWithIndication(2000));
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

                if (!ctx.Bot.globalBans.ContainsKey(victim.Key.Id))
                {
                    currentStatus[victim.Key] = CurrentStatus.Invalid;
                    continue;
                }

                _ = ctx.Bot.globalBans.Remove(victim.Key.Id);
                currentStatus[victim.Key] = CurrentStatus.Removed;

                var Success = 0;
                var Failed = 0;

                if (UnbanFromGuilds)
                    foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
                    {
                        if (!ctx.Bot.Guilds.ContainsKey(b.Key))
                            ctx.Bot.Guilds.Add(b.Key, new Guild(ctx.Bot, b.Key));

                        if (ctx.Bot.Guilds[b.Key].Join.AutoBanGlobalBans)
                        {
                            try
                            {
                                var Ban = await b.Value.GetBanAsync(victim.Key);

                                if (Ban.Reason.StartsWith("Globalban: "))
                                    await b.Value.UnbanMemberAsync(victim.Key, $"Globalban removed.");

                                Success++;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Exception occurred while trying to unban user from {guild}", b.Key);
                                Failed++;
                            }
                        }
                    }

                var announceChannel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
                _ = await announceChannel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.CurrentUser.GetUsername(),
                        IconUrl = AuditLogIcons.UserBanRemoved
                    },
                    Description = $"{victim.Key.Mention} `{victim.Key.GetUsernameWithIdentifier()}` (`{victim.Key.Id}`) was removed from the global ban list.\n\n" +
                                  $"Moderator: {ctx.User.Mention} `{ctx.User.GetUsernameWithIdentifier()}` (`{ctx.User.Id}`)",
                    Color = EmbedColors.Success,
                    Timestamp = DateTime.UtcNow
                });
            }
        });
    }

    private enum CurrentStatus
    {
        InQueue,
        InProgress,
        Removed,
        Invalid
    }
}
