// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class GuildInfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            ulong? rawGuildId = (ulong?)arguments["guildId"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            ulong guildId = rawGuildId ?? ctx.Guild.Id;

            if (guildId == 0)
                guildId = ctx.Guild.Id;

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(this.t.Commands.Utility.GuildInfo.Fetching, true)).AsBotLoading(ctx));

            try
            {
                DiscordGuild guild = await ctx.Client.GetGuildAsync(guildId);

                var embed = new DiscordEmbedBuilder
                {
                    Title = guild.Name,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = guild.IconUrl ?? AuditLogIcons.QuestionMark,
                    },
                    ImageUrl = guild.DiscoverySplashUrl ?? guild.SplashUrl ?? "",
                    Description = $"{(guild.Description.IsNullOrWhiteSpace() ? "" : $"{guild.Description}\n\n")}",
                }.AsBotInfo(ctx);

                embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.MemberTitle), $"👥 `{guild.Members.Count}` **{GetString(this.t.Commands.Utility.GuildInfo.MemberTitle)}**\n" +
                                  $"🟢 `{guild.Members.Where(x => (x.Value?.Presence?.Status ?? UserStatus.Offline) != UserStatus.Offline).Count()}` **{GetString(this.t.Commands.Utility.GuildInfo.OnlineMembers)}**\n" +
                                  $"🛑 `{guild.MaxMembers}` **{GetString(this.t.Commands.Utility.GuildInfo.MaxMembers)}**\n"));

                embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.GuildTitle), $"👤 **{GetString(this.t.Commands.Utility.GuildInfo.Owner)}**: {guild.Owner.Mention} (`{guild.Owner.GetUsernameWithIdentifier()}`)\n" +
                                  $"🕒 **{GetString(this.t.Commands.Utility.GuildInfo.Creation)}**: {guild.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({guild.CreationTimestamp.ToTimestamp()})\n" +
                                  $"🗺 **{GetString(this.t.Commands.Utility.GuildInfo.Locale)}**: `{guild.PreferredLocale}`\n" +
                                  $"🔮 `{guild.PremiumSubscriptionCount}` **{GetString(this.t.Commands.Utility.GuildInfo.Boosts)} (`{guild.PremiumTier switch { PremiumTier.None => GetString(this.t.Commands.Utility.GuildInfo.BoostsNone), PremiumTier.TierOne => GetString(this.t.Commands.Utility.GuildInfo.BoostsTierOne), PremiumTier.TierTwo => GetString(this.t.Commands.Utility.GuildInfo.BoostsTierTwo), PremiumTier.TierThree => GetString(this.t.Commands.Utility.GuildInfo.BoostsTierThree), PremiumTier.Unknown => "?", _ => "?", }}`)**\n\n" +
                                  $"😀 `{guild.Emojis.Count}` **{GetString(this.t.Commands.Utility.EmojiStealer.Emoji)}**\n" +
                                  $"🖼 `{guild.Stickers.Count}` **{GetString(this.t.Commands.Utility.EmojiStealer.Sticker)}**\n\n" +
                                  $"{(guild.WidgetEnabled ?? false).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.Widget)}**\n" +
                                  $"{(guild.IsCommunity).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.Community)}**", true));

                embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.Security), $"{(guild.MfaLevel == MfaLevel.Enabled).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.MultiFactor)}**\n" +
                                  $"{(guild.Features.Features.Any(x => x == GuildFeaturesEnum.HasMembershipScreeningEnabled)).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.Screening)}**\n" +
                                  $"{(guild.Features.Features.Any(x => x == GuildFeaturesEnum.HasWelcomeScreenEnabled)).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.WelcomeScreen)}**\n" +
                                  $"🚪 **{GetString(this.t.Commands.Utility.GuildInfo.Verification)}**: `{guild.VerificationLevel switch { VerificationLevel.None => GetString(this.t.Commands.Utility.GuildInfo.VerificationNone), VerificationLevel.Low => GetString(this.t.Commands.Utility.GuildInfo.VerificationLow), VerificationLevel.Medium => GetString(this.t.Commands.Utility.GuildInfo.VerificationMedium), VerificationLevel.High => GetString(this.t.Commands.Utility.GuildInfo.VerificationHigh), VerificationLevel.Highest => GetString(this.t.Commands.Utility.GuildInfo.VerificationHighest), _ => "?", }}`\n" +
                                  $"🔍 **{GetString(this.t.Commands.Utility.GuildInfo.ExplicitContent)}**: `{guild.ExplicitContentFilter switch { ExplicitContentFilter.Disabled => GetString(this.t.Commands.Utility.GuildInfo.ExplicitContentNone), ExplicitContentFilter.MembersWithoutRoles => GetString(this.t.Commands.Utility.GuildInfo.ExplicitContentNoRoles), ExplicitContentFilter.AllMembers => GetString(this.t.Commands.Utility.GuildInfo.ExplicitContentEveryone), _ => "?", }}`\n" +
                                  $"⚠ **{GetString(this.t.Commands.Utility.GuildInfo.Nsfw)}**: `{guild.NsfwLevel switch { NsfwLevel.Default => GetString(this.t.Commands.Utility.GuildInfo.NsfwNoRating), NsfwLevel.Explicit => GetString(this.t.Commands.Utility.GuildInfo.NsfwExplicit), NsfwLevel.Safe => GetString(this.t.Commands.Utility.GuildInfo.NsfwSafe), NsfwLevel.Age_Restricted => GetString(this.t.Commands.Utility.GuildInfo.NsfwQuestionable), _ => "?", }}`\n" +
                                  $"💬 **{GetString(this.t.Commands.Utility.GuildInfo.DefaultNotifications)}**: `{guild.DefaultMessageNotifications switch { DefaultMessageNotifications.AllMessages => GetString(this.t.Commands.Utility.GuildInfo.DefaultNotificationsAll), DefaultMessageNotifications.MentionsOnly => GetString(this.t.Commands.Utility.GuildInfo.DefaultNotificationsMentions), _ => "?", }}`\n", true));

                embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.SpecialChannels), $"📑 **{GetString(this.t.Commands.Utility.GuildInfo.Rules)}**: {guild.RulesChannel?.Mention ?? GetString(this.t.Common.Off, true)}\n" +
                                  $"📰 **{GetString(this.t.Commands.Utility.GuildInfo.CommunityUpdates)}**: {guild.PublicUpdatesChannel?.Mention ?? GetString(this.t.Common.Off, true)}\n\n" +
                                  $"⌨ **{GetString(this.t.Commands.Utility.GuildInfo.InactiveChannel)}**: {guild.AfkChannel?.Mention ?? GetString(this.t.Common.Off, true)}\n" +
                                  $"> **{GetString(this.t.Commands.Utility.GuildInfo.InactiveTimeout)}**: `{((long)guild.AfkTimeout).GetHumanReadable()}`\n\n" +
                                  $"🤖 **{GetString(this.t.Commands.Utility.GuildInfo.SystemMessages)}**: {guild.SystemChannel?.Mention ?? GetString(this.t.Common.Off, true)}\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressJoinNotifications)).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesWelcome)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressJoinNotificationReplies)).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesWelcomeStickers)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressPremiumSubscriptions)).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesBoost)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressRoleSubbscriptionPurchaseNotification)).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesRole)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressRoleSubbscriptionPurchaseNotificationReplies)).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesRoleSticker)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressGuildReminderNotifications)).ToPillEmote(ctx.Bot)} **{GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesSetupTips)}**\n"));

                embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.GuildFeatures), $"{string.Join(", ", guild.RawFeatures.Select(x => $"`{string.Join(" ", x.Replace("_", " ").ToLower().Split(" ").Select(x => x.FirstLetterToUpper()))}`"))}"));

                DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

                if (!guild.VanityUrlCode.IsNullOrWhiteSpace())
                    builder.AddComponents(new DiscordLinkButtonComponent($"https://discord.gg/{guild.VanityUrlCode}", GetString(this.t.Commands.Utility.GuildInfo.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                await RespondOrEdit(embed);
            }
            catch (Exception ex1) when (ex1 is DisCatSharp.Exceptions.UnauthorizedException ||
                                       ex1 is DisCatSharp.Exceptions.NotFoundException)
            {
                HttpClient client = new();

                try
                {
                    var preview = await ctx.Client.GetGuildPreviewAsync(guildId);

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = preview.Name,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Url = preview.IconUrl ?? AuditLogIcons.QuestionMark,
                        },
                        ImageUrl = preview.SplashUrl ?? preview.DiscoverySplashUrl ?? "",
                        Description = preview.Description ?? "",
                    }.AsBotInfo(ctx, "", GetString(this.t.Commands.Utility.GuildInfo.GuildPreviewNotice));

                    embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.MemberTitle), $"👥 `{preview.ApproximateMemberCount}` **{GetString(this.t.Commands.Utility.GuildInfo.MemberTitle)}**\n" +
                                  $"🟢 `{preview.ApproximatePresenceCount}` **{GetString(this.t.Commands.Utility.GuildInfo.OnlineMembers)}**\n"));

                    embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.GuildTitle), $"🕒 **{GetString(this.t.Commands.Utility.GuildInfo.Creation)}**: {preview.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({preview.CreationTimestamp.ToTimestamp()})\n" +
                                  $"😀 `{preview.Emojis.Count}` **{GetString(this.t.Commands.Utility.EmojiStealer.Emoji)}**\n" +
                                  $"🖼 `{preview.Stickers.Count}` **{GetString(this.t.Commands.Utility.EmojiStealer.Sticker)}**\n", true));

                    embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.GuildFeatures), $"{string.Join(", ", preview.Features.Select(x => $"`{string.Join(" ", x.Replace("_", " ").ToLower().Split(" ").Select(x => x.FirstLetterToUpper()))}`"))}"));


                    DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

                    string invite = "";

                    try { invite = (await ctx.Client.GetGuildWidgetAsync(guildId)).InstantInviteUrl; } catch { }

                    if (!invite.IsNullOrWhiteSpace())
                        builder.AddComponents(new DiscordLinkButtonComponent(invite, GetString(this.t.Commands.Utility.GuildInfo.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                    await RespondOrEdit(builder);
                }
                catch (Exception ex2) when (ex2 is DisCatSharp.Exceptions.UnauthorizedException ||
                                            ex2 is DisCatSharp.Exceptions.NotFoundException)
                {
                    try
                    {
                        var widget = await ctx.Client.GetGuildWidgetAsync(guildId);

                        var embed = new DiscordEmbedBuilder
                        {
                            Title = widget.Name,
                        }.AsBotInfo(ctx, "", GetString(this.t.Commands.Utility.GuildInfo.GuildWidgetNotice));

                        embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.MemberTitle), $"🟢 `{widget.PresenceCount}` **{GetString(this.t.Commands.Utility.GuildInfo.OnlineMembers)}**\n"));

                        DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

                        if (!widget.InstantInviteUrl.IsNullOrWhiteSpace())
                            builder.AddComponents(new DiscordLinkButtonComponent(widget.InstantInviteUrl, GetString(this.t.Commands.Utility.GuildInfo.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                        await RespondOrEdit(builder);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Mee6Leaderboard mee6 = JsonConvert.DeserializeObject<Mee6Leaderboard>(await client.GetStringAsync($"https://mee6.xyz/api/plugins/levels/leaderboard/{guildId}"));

                            var embed = new DiscordEmbedBuilder
                            {
                                Title = mee6.guild.name,
                                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                                {
                                    Url = $"https://cdn.discordapp.com/icons/{guildId}/{mee6.guild.icon}.webp?size=96",
                                },
                                ImageUrl = mee6.banner_url ?? "",
                            }.AsBotInfo(ctx, "", GetString(this.t.Commands.Utility.GuildInfo.Mee6Notice));

                            embed.AddField(new DiscordEmbedField(GetString(this.t.Commands.Utility.GuildInfo.MemberTitle), $"👥 `{mee6.players.Length}` **{GetString(this.t.Commands.Utility.GuildInfo.MemberTitle)}**\n"));

                            await RespondOrEdit(embed);
                        }
                        catch (Exception)
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Description = GetString(this.t.Commands.Utility.GuildInfo.NoGuildFound, true),
                            }.AsBotError(ctx);

                            await RespondOrEdit(embed);
                        }
                    }
                }
            }
        });
    }
}