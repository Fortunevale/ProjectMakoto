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
            var rawGuildId = (ulong?)arguments["guildId"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            var guildId = rawGuildId ?? ctx.Guild.Id;

            if (guildId == 0)
                guildId = ctx.Guild.Id;

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.GuildInfo.Fetching, true)).AsBotLoading(ctx));

            try
            {
                var guild = await ctx.Client.GetGuildAsync(guildId);

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

                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.MemberTitle), $"👥 `{guild.Members.Count}` **{this.GetString(this.t.Commands.Utility.GuildInfo.MemberTitle)}**\n" +
                                  $"🟢 `{guild.Members.Where(x => (x.Value?.Presence?.Status ?? UserStatus.Offline) != UserStatus.Offline).Count()}` **{this.GetString(this.t.Commands.Utility.GuildInfo.OnlineMembers)}**\n" +
                                  $"🛑 `{guild.MaxMembers}` **{this.GetString(this.t.Commands.Utility.GuildInfo.MaxMembers)}**\n"));

                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.GuildTitle), $"👤 **{this.GetString(this.t.Commands.Utility.GuildInfo.Owner)}**: {guild.Owner.Mention} (`{guild.Owner.GetUsernameWithIdentifier()}`)\n" +
                                  $"🕒 **{this.GetString(this.t.Commands.Utility.GuildInfo.Creation)}**: {guild.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({guild.CreationTimestamp.ToTimestamp()})\n" +
                                  $"🗺 **{this.GetString(this.t.Commands.Utility.GuildInfo.Locale)}**: `{guild.PreferredLocale}`\n" +
                                  $"🔮 `{guild.PremiumSubscriptionCount}` **{this.GetString(this.t.Commands.Utility.GuildInfo.Boosts)} (`{guild.PremiumTier switch { PremiumTier.None => this.GetString(this.t.Commands.Utility.GuildInfo.BoostsNone), PremiumTier.TierOne => this.GetString(this.t.Commands.Utility.GuildInfo.BoostsTierOne), PremiumTier.TierTwo => this.GetString(this.t.Commands.Utility.GuildInfo.BoostsTierTwo), PremiumTier.TierThree => this.GetString(this.t.Commands.Utility.GuildInfo.BoostsTierThree), PremiumTier.Unknown => "?", _ => "?", }}`)**\n\n" +
                                  $"😀 `{guild.Emojis.Count}` **{this.GetString(this.t.Commands.Utility.EmojiStealer.Emoji)}**\n" +
                                  $"🖼 `{guild.Stickers.Count}` **{this.GetString(this.t.Commands.Utility.EmojiStealer.Sticker)}**\n\n" +
                                  $"{(guild.WidgetEnabled ?? false).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.Widget)}**\n" +
                                  $"{(guild.IsCommunity).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.Community)}**", true));

                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.Security), $"{(guild.MfaLevel == MfaLevel.Enabled).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.MultiFactor)}**\n" +
                                  $"{(guild.Features.Features.Any(x => x == GuildFeaturesEnum.HasMembershipScreeningEnabled)).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.Screening)}**\n" +
                                  $"{(guild.Features.Features.Any(x => x == GuildFeaturesEnum.HasWelcomeScreenEnabled)).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.WelcomeScreen)}**\n" +
                                  $"🚪 **{this.GetString(this.t.Commands.Utility.GuildInfo.Verification)}**: `{guild.VerificationLevel switch { VerificationLevel.None => this.GetString(this.t.Commands.Utility.GuildInfo.VerificationNone), VerificationLevel.Low => this.GetString(this.t.Commands.Utility.GuildInfo.VerificationLow), VerificationLevel.Medium => this.GetString(this.t.Commands.Utility.GuildInfo.VerificationMedium), VerificationLevel.High => this.GetString(this.t.Commands.Utility.GuildInfo.VerificationHigh), VerificationLevel.Highest => this.GetString(this.t.Commands.Utility.GuildInfo.VerificationHighest), _ => "?", }}`\n" +
                                  $"🔍 **{this.GetString(this.t.Commands.Utility.GuildInfo.ExplicitContent)}**: `{guild.ExplicitContentFilter switch { ExplicitContentFilter.Disabled => this.GetString(this.t.Commands.Utility.GuildInfo.ExplicitContentNone), ExplicitContentFilter.MembersWithoutRoles => this.GetString(this.t.Commands.Utility.GuildInfo.ExplicitContentNoRoles), ExplicitContentFilter.AllMembers => this.GetString(this.t.Commands.Utility.GuildInfo.ExplicitContentEveryone), _ => "?", }}`\n" +
                                  $"⚠ **{this.GetString(this.t.Commands.Utility.GuildInfo.Nsfw)}**: `{guild.NsfwLevel switch { NsfwLevel.Default => this.GetString(this.t.Commands.Utility.GuildInfo.NsfwNoRating), NsfwLevel.Explicit => this.GetString(this.t.Commands.Utility.GuildInfo.NsfwExplicit), NsfwLevel.Safe => this.GetString(this.t.Commands.Utility.GuildInfo.NsfwSafe), NsfwLevel.Age_Restricted => this.GetString(this.t.Commands.Utility.GuildInfo.NsfwQuestionable), _ => "?", }}`\n" +
                                  $"💬 **{this.GetString(this.t.Commands.Utility.GuildInfo.DefaultNotifications)}**: `{guild.DefaultMessageNotifications switch { DefaultMessageNotifications.AllMessages => this.GetString(this.t.Commands.Utility.GuildInfo.DefaultNotificationsAll), DefaultMessageNotifications.MentionsOnly => this.GetString(this.t.Commands.Utility.GuildInfo.DefaultNotificationsMentions), _ => "?", }}`\n", true));

                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.SpecialChannels), $"📑 **{this.GetString(this.t.Commands.Utility.GuildInfo.Rules)}**: {guild.RulesChannel?.Mention ?? this.GetString(this.t.Common.Off, true)}\n" +
                                  $"📰 **{this.GetString(this.t.Commands.Utility.GuildInfo.CommunityUpdates)}**: {guild.PublicUpdatesChannel?.Mention ?? this.GetString(this.t.Common.Off, true)}\n\n" +
                                  $"⌨ **{this.GetString(this.t.Commands.Utility.GuildInfo.InactiveChannel)}**: {guild.AfkChannel?.Mention ?? this.GetString(this.t.Common.Off, true)}\n" +
                                  $"> **{this.GetString(this.t.Commands.Utility.GuildInfo.InactiveTimeout)}**: `{((long)guild.AfkTimeout).GetHumanReadable()}`\n\n" +
                                  $"🤖 **{this.GetString(this.t.Commands.Utility.GuildInfo.SystemMessages)}**: {guild.SystemChannel?.Mention ?? this.GetString(this.t.Common.Off, true)}\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressJoinNotifications)).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesWelcome)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressJoinNotificationReplies)).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesWelcomeStickers)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressPremiumSubscriptions)).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesBoost)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressRoleSubbscriptionPurchaseNotification)).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesRole)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressRoleSubbscriptionPurchaseNotificationReplies)).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesRoleSticker)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressGuildReminderNotifications)).ToPillEmote(ctx.Bot)} **{this.GetString(this.t.Commands.Utility.GuildInfo.SystemMessagesSetupTips)}**\n"));

                if (guild.RawFeatures.Count > 0)
                    _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.GuildFeatures), $"{string.Join(", ", guild.RawFeatures.Select(x => $"`{string.Join(" ", x.Replace("_", " ").ToLower().Split(" ").Select(x => x.FirstLetterToUpper()))}`"))}"));

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                if (!guild.VanityUrlCode.IsNullOrWhiteSpace())
                    _ = builder.AddComponents(new DiscordLinkButtonComponent($"https://discord.gg/{guild.VanityUrlCode}", this.GetString(this.t.Commands.Utility.GuildInfo.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                _ = await this.RespondOrEdit(embed);
            }
            catch (Exception ex1) when (ex1 is DisCatSharp.Exceptions.UnauthorizedException or
                                       DisCatSharp.Exceptions.NotFoundException)
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
                    }.AsBotInfo(ctx, "", this.GetString(this.t.Commands.Utility.GuildInfo.GuildPreviewNotice));

                    _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.MemberTitle), $"👥 `{preview.ApproximateMemberCount}` **{this.GetString(this.t.Commands.Utility.GuildInfo.MemberTitle)}**\n" +
                                  $"🟢 `{preview.ApproximatePresenceCount}` **{this.GetString(this.t.Commands.Utility.GuildInfo.OnlineMembers)}**\n"));

                    _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.GuildTitle), $"🕒 **{this.GetString(this.t.Commands.Utility.GuildInfo.Creation)}**: {preview.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({preview.CreationTimestamp.ToTimestamp()})\n" +
                                  $"😀 `{preview.Emojis.Count}` **{this.GetString(this.t.Commands.Utility.EmojiStealer.Emoji)}**\n" +
                                  $"🖼 `{preview.Stickers.Count}` **{this.GetString(this.t.Commands.Utility.EmojiStealer.Sticker)}**\n", true));

                    _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.GuildFeatures), $"{string.Join(", ", preview.Features.Select(x => $"`{string.Join(" ", x.Replace("_", " ").ToLower().Split(" ").Select(x => x.FirstLetterToUpper()))}`"))}"));


                    var builder = new DiscordMessageBuilder().WithEmbed(embed);

                    var invite = "";

                    try { invite = (await ctx.Client.GetGuildWidgetAsync(guildId)).InstantInviteUrl; } catch { }

                    if (!invite.IsNullOrWhiteSpace())
                        _ = builder.AddComponents(new DiscordLinkButtonComponent(invite, this.GetString(this.t.Commands.Utility.GuildInfo.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                    _ = await this.RespondOrEdit(builder);
                }
                catch (Exception ex2) when (ex2 is DisCatSharp.Exceptions.UnauthorizedException or
                                            DisCatSharp.Exceptions.NotFoundException)
                {
                    try
                    {
                        var widget = await ctx.Client.GetGuildWidgetAsync(guildId);

                        var embed = new DiscordEmbedBuilder
                        {
                            Title = widget.Name,
                        }.AsBotInfo(ctx, "", this.GetString(this.t.Commands.Utility.GuildInfo.GuildWidgetNotice));

                        _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.MemberTitle), $"🟢 `{widget.PresenceCount}` **{this.GetString(this.t.Commands.Utility.GuildInfo.OnlineMembers)}**\n"));

                        var builder = new DiscordMessageBuilder().WithEmbed(embed);

                        if (!widget.InstantInviteUrl.IsNullOrWhiteSpace())
                            _ = builder.AddComponents(new DiscordLinkButtonComponent(widget.InstantInviteUrl, this.GetString(this.t.Commands.Utility.GuildInfo.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                        _ = await this.RespondOrEdit(builder);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var mee6 = JsonConvert.DeserializeObject<Mee6Leaderboard>(await client.GetStringAsync($"https://mee6.xyz/api/plugins/levels/leaderboard/{guildId}"));

                            var embed = new DiscordEmbedBuilder
                            {
                                Title = mee6.guild.name,
                                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                                {
                                    Url = $"https://cdn.discordapp.com/icons/{guildId}/{mee6.guild.icon}.webp?size=96",
                                },
                                ImageUrl = mee6.banner_url ?? "",
                            }.AsBotInfo(ctx, "", this.GetString(this.t.Commands.Utility.GuildInfo.Mee6Notice));

                            _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.GuildInfo.MemberTitle), $"👥 `{mee6.players.Length}` **{this.GetString(this.t.Commands.Utility.GuildInfo.MemberTitle)}**\n"));

                            _ = await this.RespondOrEdit(embed);
                        }
                        catch (Exception)
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Description = this.GetString(this.t.Commands.Utility.GuildInfo.NoGuildFound, true),
                            }.AsBotError(ctx);

                            _ = await this.RespondOrEdit(embed);
                        }
                    }
                }
            }
        });
    }
}