// Project Makoto
// Copyright (C) 2024  Fortunevale
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
            var CommandKey = this.t.Commands.Utility.GuildInfo;

            var rawGuildId = (string?)arguments["guild"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            var guildId = rawGuildId?.ToUInt64() ?? ctx.Guild.Id;

            if (guildId == 0)
                guildId = ctx.Guild.Id;

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.Fetching, true)).AsLoading(ctx));

            _ = Directory.CreateDirectory("cache");

            try
            {
                var guild = await ctx.Client.GetGuildAsync(guildId);

                //var imageHash = guild.DiscoverySplashHash ?? guild.SplashHash ?? "";
                //var imageUrl = guild.DiscoverySplashUrl ?? guild.SplashUrl ?? "";
                //if (!File.Exists($"cache/{imageHash}") && !imageHash.IsNullOrWhiteSpace())
                //{
                //    var fileExtension = imageUrl[..(imageUrl.LastIndexOf('?'))];
                //    fileExtension = fileExtension[(fileExtension.LastIndexOf(".") + 1)..];

                //    using (var outputStream = new MemoryStream())
                //    {
                //        var arguments = FFMpegArguments
                //            .FromPipeInput(new StreamPipeSource(await new HttpClient().GetStreamAsync(imageUrl)))
                //            .OutputToPipe(new StreamPipeSink(outputStream), x => x
                //                .ForceFormat("image2")
                //                .WithVideoCodec(fileExtension)
                //                .WithArgument(new CustomArgument("-vf scale=2048:256:force_original_aspect_ratio=decrease,pad=2048:256:-1:-1")));

                //        _ = await arguments.ProcessAsynchronously();

                //        using (var file = new FileStream($"cache/{imageHash}", FileMode.Create, FileAccess.Write))
                //        {
                //            outputStream.Position = 0;
                //            await outputStream.CopyToAsync(file);
                //        }
                //    }
                //}

                var embed = new DiscordEmbedBuilder
                {
                    Title = guild.Name,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = guild.IconUrl ?? AuditLogIcons.QuestionMark,
                    },
                    //ImageUrl = $"attachment://banner.png",
                    Description = $"{(guild.Description.IsNullOrWhiteSpace() ? "" : $"{guild.Description}\n\n")}",
                }.AsInfo(ctx);

                _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.MemberTitle), $"👥 `{guild.Members.Count}` **{this.GetString(CommandKey.MemberTitle)}**\n" +
                                  $"🟢 `{guild.Members.Where(x => (x.Value?.Presence?.Status ?? UserStatus.Offline) != UserStatus.Offline).Count()}` **{this.GetString(CommandKey.OnlineMembers)}**\n" +
                                  $"🛑 `{guild.MaxMembers}` **{this.GetString(CommandKey.MaxMembers)}**\n"));

                _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.GuildTitle), $"👤 **{this.GetString(CommandKey.Owner)}**: {guild.Owner.Mention} (`{guild.Owner.GetUsernameWithIdentifier()}`)\n" +
                                  $"🕒 **{this.GetString(CommandKey.Creation)}**: {guild.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({guild.CreationTimestamp.ToTimestamp()})\n" +
                                  $"🗺 **{this.GetString(CommandKey.Locale)}**: `{guild.PreferredLocale}`\n" +
                                  $"🔮 `{guild.PremiumSubscriptionCount}` **{this.GetString(CommandKey.Boosts)} (`{guild.PremiumTier switch { PremiumTier.None => this.GetString(CommandKey.BoostsNone), PremiumTier.TierOne => this.GetString(CommandKey.BoostsTierOne), PremiumTier.TierTwo => this.GetString(CommandKey.BoostsTierTwo), PremiumTier.TierThree => this.GetString(CommandKey.BoostsTierThree), PremiumTier.Unknown => "?", _ => "?", }}`)**\n\n" +
                                  $"😀 `{guild.Emojis.Count}` **{this.GetString(this.t.Commands.Utility.EmojiStealer.Emoji)}**\n" +
                                  $"🖼 `{guild.Stickers.Count}` **{this.GetString(this.t.Commands.Utility.EmojiStealer.Sticker)}**\n\n" +
                                  $"{(guild.WidgetEnabled ?? false).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.Widget)}**\n" +
                                  $"{(guild.IsCommunity).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.Community)}**", true));

                _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.Security), $"{(guild.MfaLevel == MfaLevel.Enabled).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.MultiFactor)}**\n" +
                                  $"{(guild.Features.Features.Any(x => x == GuildFeaturesEnum.HasMembershipScreeningEnabled)).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.Screening)}**\n" +
                                  $"{(guild.Features.Features.Any(x => x == GuildFeaturesEnum.HasWelcomeScreenEnabled)).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.WelcomeScreen)}**\n" +
                                  $"🚪 **{this.GetString(CommandKey.Verification)}**: `{guild.VerificationLevel switch { VerificationLevel.None => this.GetString(CommandKey.VerificationNone), VerificationLevel.Low => this.GetString(CommandKey.VerificationLow), VerificationLevel.Medium => this.GetString(CommandKey.VerificationMedium), VerificationLevel.High => this.GetString(CommandKey.VerificationHigh), VerificationLevel.Highest => this.GetString(CommandKey.VerificationHighest), _ => "?", }}`\n" +
                                  $"🔍 **{this.GetString(CommandKey.ExplicitContent)}**: `{guild.ExplicitContentFilter switch { ExplicitContentFilter.Disabled => this.GetString(CommandKey.ExplicitContentNone), ExplicitContentFilter.MembersWithoutRoles => this.GetString(CommandKey.ExplicitContentNoRoles), ExplicitContentFilter.AllMembers => this.GetString(CommandKey.ExplicitContentEveryone), _ => "?", }}`\n" +
                                  $"⚠ **{this.GetString(CommandKey.Nsfw)}**: `{guild.NsfwLevel switch { NsfwLevel.Default => this.GetString(CommandKey.NsfwNoRating), NsfwLevel.Explicit => this.GetString(CommandKey.NsfwExplicit), NsfwLevel.Safe => this.GetString(CommandKey.NsfwSafe), NsfwLevel.Age_Restricted => this.GetString(CommandKey.NsfwQuestionable), _ => "?", }}`\n" +
                                  $"💬 **{this.GetString(CommandKey.DefaultNotifications)}**: `{guild.DefaultMessageNotifications switch { DefaultMessageNotifications.AllMessages => this.GetString(CommandKey.DefaultNotificationsAll), DefaultMessageNotifications.MentionsOnly => this.GetString(CommandKey.DefaultNotificationsMentions), _ => "?", }}`\n", true));

                _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.SpecialChannels), $"📑 **{this.GetString(CommandKey.Rules)}**: {guild.RulesChannel?.Mention ?? this.GetString(this.t.Common.Off, true)}\n" +
                                  $"📰 **{this.GetString(CommandKey.CommunityUpdates)}**: {guild.PublicUpdatesChannel?.Mention ?? this.GetString(this.t.Common.Off, true)}\n\n" +
                                  $"⌨ **{this.GetString(CommandKey.InactiveChannel)}**: {guild.AfkChannel?.Mention ?? this.GetString(this.t.Common.Off, true)}\n" +
                                  $"> **{this.GetString(CommandKey.InactiveTimeout)}**: `{((long)guild.AfkTimeout).GetHumanReadable()}`\n\n" +
                                  $"🤖 **{this.GetString(CommandKey.SystemMessages)}**: {guild.SystemChannel?.Mention ?? this.GetString(this.t.Common.Off, true)}\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressJoinNotifications)).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.SystemMessagesWelcome)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressJoinNotificationReplies)).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.SystemMessagesWelcomeStickers)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressPremiumSubscriptions)).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.SystemMessagesBoost)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressRoleSubbscriptionPurchaseNotification)).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.SystemMessagesRole)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressRoleSubbscriptionPurchaseNotificationReplies)).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.SystemMessagesRoleSticker)}**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressGuildReminderNotifications)).ToPillEmote(ctx.Bot)} **{this.GetString(CommandKey.SystemMessagesSetupTips)}**\n"));

                if (guild.RawFeatures.Count > 0)
                    _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.GuildFeatures), $"{string.Join(", ", guild.RawFeatures.Select(x => $"`{string.Join(" ", x.Replace("_", " ").ToLower().Split(" ").Select(x => x.FirstLetterToUpper()))}`"))}"));

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                if (!guild.VanityUrlCode.IsNullOrWhiteSpace())
                    _ = builder.AddComponents(new DiscordLinkButtonComponent($"https://discord.gg/{guild.VanityUrlCode}", this.GetString(CommandKey.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                _ = await this.RespondOrEdit(new DiscordMessageBuilder()
                    .WithEmbed(embed)
                    .AddComponents(new DiscordLinkButtonComponent(guild.BannerUrl ?? "https://discord.gg", this.GetString(CommandKey.Banner), guild.BannerUrl is null),
                    new DiscordLinkButtonComponent(guild.SplashUrl ?? "https://discord.gg", this.GetString(CommandKey.Splash), guild.BannerUrl is null),
                    new DiscordLinkButtonComponent(guild.DiscoverySplashUrl ?? "https://discord.gg", this.GetString(CommandKey.DiscoverySplash), guild.BannerUrl is null),
                    new DiscordLinkButtonComponent(guild.HomeHeaderUrl ?? "https://discord.gg", this.GetString(CommandKey.HomeHeader), guild.HomeHeaderUrl is null)));

                //if (imageHash.IsNullOrWhiteSpace())
                //    _ = await this.RespondOrEdit(embed);
                //else
                //{
                //    using (var file = new FileStream($"cache/{imageHash}", FileMode.Open, FileAccess.Read))
                //    {
                //        _ = await this.RespondOrEdit(new DiscordMessageBuilder()
                //            .WithEmbed(embed)
                //            .WithFile("banner.png", file));
                //    }
                //}
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
                        //ImageUrl = preview.SplashUrl ?? preview.DiscoverySplashUrl ?? "",
                        Description = preview.Description ?? "",
                    }.AsInfo(ctx, "", this.GetString(CommandKey.GuildPreviewNotice));

                    _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.MemberTitle), $"👥 `{preview.ApproximateMemberCount}` **{this.GetString(CommandKey.MemberTitle)}**\n" +
                                  $"🟢 `{preview.ApproximatePresenceCount}` **{this.GetString(CommandKey.OnlineMembers)}**\n"));

                    _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.GuildTitle), $"🕒 **{this.GetString(CommandKey.Creation)}**: {preview.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({preview.CreationTimestamp.ToTimestamp()})\n" +
                                  $"😀 `{preview.Emojis.Count}` **{this.GetString(this.t.Commands.Utility.EmojiStealer.Emoji)}**\n" +
                                  $"🖼 `{preview.Stickers.Count}` **{this.GetString(this.t.Commands.Utility.EmojiStealer.Sticker)}**\n", true));

                    _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.GuildFeatures), $"{string.Join(", ", preview.Features.Select(x => $"`{string.Join(" ", x.Replace("_", " ").ToLower().Split(" ").Select(x => x.FirstLetterToUpper()))}`"))}"));


                    var builder = new DiscordMessageBuilder().WithEmbed(embed);

                    var invite = "";

                    try { invite = (await ctx.Client.GetGuildWidgetAsync(guildId)).InstantInviteUrl; } catch { }

                    if (!invite.IsNullOrWhiteSpace())
                        _ = builder.AddComponents(new DiscordLinkButtonComponent(invite, this.GetString(CommandKey.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

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
                        }.AsInfo(ctx, "", this.GetString(CommandKey.GuildWidgetNotice));

                        _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.MemberTitle), $"🟢 `{widget.PresenceCount}` **{this.GetString(CommandKey.OnlineMembers)}**\n"));

                        var builder = new DiscordMessageBuilder().WithEmbed(embed);

                        if (!widget.InstantInviteUrl.IsNullOrWhiteSpace())
                            _ = builder.AddComponents(new DiscordLinkButtonComponent(widget.InstantInviteUrl, this.GetString(CommandKey.JoinServer), false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

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
                                //ImageUrl = mee6.banner_url ?? "",
                            }.AsInfo(ctx, "", this.GetString(CommandKey.Mee6Notice));

                            _ = embed.AddField(new DiscordEmbedField(this.GetString(CommandKey.MemberTitle), $"👥 `{mee6.players.Length}` **{this.GetString(CommandKey.MemberTitle)}**\n"));

                            _ = await this.RespondOrEdit(embed);
                        }
                        catch (Exception)
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Description = this.GetString(CommandKey.NoGuildFound, true),
                            }.AsError(ctx);

                            _ = await this.RespondOrEdit(embed);
                        }
                    }
                }
            }
        });
    }
}