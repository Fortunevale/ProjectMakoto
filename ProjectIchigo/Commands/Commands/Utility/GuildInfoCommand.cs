﻿namespace ProjectIchigo.Commands;

internal class GuildInfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
			ulong? rawGuildId = (ulong?)arguments["guildId"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            ulong guildId = rawGuildId ?? ctx.Guild.Id;

			if (guildId == 0)
				guildId = ctx.Guild.Id;

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Fetching guild info..`").AsBotLoading(ctx));

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

                embed.AddField(new DiscordEmbedField(GetString(t.Commands.GuildInfo.MemberTitle), $"👥 `{guild.Members.Count}` **{GetString(t.Commands.GuildInfo.MemberTitle)}**\n" +
                                  $"🟢 `{guild.Members.Where(x => (x.Value?.Presence?.Status ?? UserStatus.Offline) != UserStatus.Offline).Count()}` **{GetString(t.Commands.GuildInfo.OnlineMembers)}**\n" +
                                  $"🛑 `{guild.MaxMembers}` **{GetString(t.Commands.GuildInfo.MaxMembers)}**\n"));
                
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.GuildInfo.GuildTitle), $"👤 **{GetString(t.Commands.GuildInfo.Owner)}**: {guild.Owner.Mention} (`{guild.Owner.UsernameWithDiscriminator}`)\n" +
                                  $"🕒 **{GetString(t.Commands.GuildInfo.Creation)}**: {guild.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({guild.CreationTimestamp.ToTimestamp()})\n" +
                                  $"🗺 **{GetString(t.Commands.GuildInfo.Locale)}**: `{guild.PreferredLocale}`\n" +
                                  $"🔮 `{guild.PremiumSubscriptionCount}` **{GetString(t.Commands.GuildInfo.Boosts)} (`{guild.PremiumTier}`)**\n\n" +
                                  $"😀 `{guild.Emojis.Count}` **{GetString(t.Commands.EmojiStealer.Emoji)}**\n" +
                                  $"🖼 `{guild.Stickers.Count}` **{GetString(t.Commands.EmojiStealer.Sticker)}**\n\n" +
                                  $"{(guild.WidgetEnabled ?? false).ToPillEmote(ctx.Bot)} **{GetString(t.Commands.GuildInfo.Widget)}**\n" +
                                  $"{(guild.IsCommunity).ToPillEmote(ctx.Bot)} **{GetString(t.Commands.GuildInfo.Community)}**", true));
                
                embed.AddField(new DiscordEmbedField("Security", $"{(guild.MfaLevel == MfaLevel.Enabled).ToPillEmote(ctx.Bot)} **2FA required for mods**\n" +
                                  $"{(guild.Features.Features.Any(x => x == GuildFeaturesEnum.HasMembershipScreeningEnabled)).ToPillEmote(ctx.Bot)} **Membership Screening**\n" +
                                  $"{(guild.Features.Features.Any(x => x == GuildFeaturesEnum.HasWelcomeScreenEnabled)).ToPillEmote(ctx.Bot)} **Welcome Screen**\n" +
                                  $"🚪 **Verification Level**: `{guild.VerificationLevel}`\n" +
                                  $"🔍 **Scan for explicit content of**: `{guild.ExplicitContentFilter switch { ExplicitContentFilter.Disabled => "No members", ExplicitContentFilter.MembersWithoutRoles => "Members without roles", ExplicitContentFilter.AllMembers => "All members", _ => "?", }}`\n" +
                                  $"⚠ **NSFW Level**: `{guild.NsfwLevel switch { NsfwLevel.Default => "No Rating", NsfwLevel.Explicit => "Explicit, Only suitable for mature audiences", NsfwLevel.Safe => "Safe, Suitable for all age groups", NsfwLevel.Age_Restricted => "Questionable, May only be suitable for mature audiences", _ => "?", }}`\n" +
                                  $"💬 **Default Notifications**: `{guild.DefaultMessageNotifications switch { DefaultMessageNotifications.AllMessages => "All messages", DefaultMessageNotifications.MentionsOnly => "Mentions only", _ => "?", }}`\n", true));

                embed.AddField(new DiscordEmbedField("Special Channels", $"📑 **Rules**: {guild.RulesChannel?.Mention ?? "`None`"}\n" +
                                  $"📰 **Community Updates**: {guild.PublicUpdatesChannel?.Mention ?? "`None`"}\n\n" +
                                  $"⌨ **Inactive Channel**: {guild.AfkChannel?.Mention ?? "`None`"}\n" +
                                  $"> **Inactive Timeout**: `{((long)guild.AfkTimeout).GetHumanReadable()}`\n\n" +
                                  $"🤖 **System Messages**: {guild.SystemChannel?.Mention ?? "`None`"}\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressJoinNotifications)).ToPillEmote(ctx.Bot)} **Welcome Messages**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressJoinNotificationReplies)).ToPillEmote(ctx.Bot)} **Welcome Sticker Replies**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressPremiumSubscriptions)).ToPillEmote(ctx.Bot)} **Boost Messages**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressRoleSubbscriptionPurchaseNotification)).ToPillEmote(ctx.Bot)} **Role Purchase Message**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressRoleSubbscriptionPurchaseNotificationReplies)).ToPillEmote(ctx.Bot)} **Role Purchase Message Replies**\n" +
                                  $"> {(!guild.SystemChannelFlags.HasSystemChannelFlag(SystemChannelFlags.SuppressGuildReminderNotifications)).ToPillEmote(ctx.Bot)} **Server Setup Tips**\n"));

                embed.AddField(new DiscordEmbedField("Guild Features", $"{string.Join(", ", guild.RawFeatures.Select(x => $"`{string.Join(" ", x.Replace("_", " ").ToLower().Split(" ").Select(x => x.FirstLetterToUpper()))}`"))}"));

                DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

                if (!guild.VanityUrlCode.IsNullOrWhiteSpace())
                    builder.AddComponents(new DiscordLinkButtonComponent($"https://discord.gg/{guild.VanityUrlCode}", "Join Server", false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                await RespondOrEdit(embed);
			}
			catch (DisCatSharp.Exceptions.UnauthorizedException)
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
                    }.AsBotInfo(ctx, "", $"Info fetched via Discord Guild Preview");

                    embed.AddField(new DiscordEmbedField("Members", $"👥 `{preview.ApproximateMemberCount}` **Members**\n" +
                                  $"🟢 `{preview.ApproximatePresenceCount}` **Online Members**\n"));

                    embed.AddField(new DiscordEmbedField("Guild Details", $"🕒 **Creation Date**: {preview.CreationTimestamp.ToTimestamp(TimestampFormat.LongDateTime)} ({preview.CreationTimestamp.ToTimestamp()})\n" +
                                  $"😀 `{preview.Emojis.Count}` **Emojis**\n" +
                                  $"🖼 `0` **Stickers**\n", true));

                    embed.AddField(new DiscordEmbedField("Guild Features", $"{string.Join(", ", preview.Features.Select(x => $"`{string.Join(" ", x.Replace("_", " ").ToLower().Split(" ").Select(x => x.FirstLetterToUpper()))}`"))}"));


                    DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

                    string invite = "";

					try { invite = JsonConvert.DeserializeObject<Entities.DiscordWidget>(await client.GetStringAsync($"https://discord.com/api/guilds/{guildId}/widget.json")).instant_invite; } catch { }

                    if (!invite.IsNullOrWhiteSpace())
                        builder.AddComponents(new DiscordLinkButtonComponent(invite, "Join Server", false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

                    await RespondOrEdit(builder);
                }
				catch (DisCatSharp.Exceptions.NotFoundException)
				{
					try
					{
						Entities.DiscordWidget widget = JsonConvert.DeserializeObject<Entities.DiscordWidget>(await client.GetStringAsync($"https://discord.com/api/guilds/{guildId}/widget.json"));

                        var embed = new DiscordEmbedBuilder
						{
                            Title = widget.name,
                        }.AsBotInfo(ctx, "", $"Info fetched via Discord Guild Widget");

                        embed.AddField(new DiscordEmbedField("Members", $"🟢 `{widget.presence_count}` **Online Members**\n"));

                        DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed);

						if (!widget.instant_invite.IsNullOrWhiteSpace())
							builder.AddComponents(new DiscordLinkButtonComponent(widget.instant_invite, "Join Server", false, DiscordEmoji.FromUnicode("🔗").ToComponent()));

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
                            }.AsBotInfo(ctx, "", $"Info fetched via Mee6 Leaderboard");

                            embed.AddField(new DiscordEmbedField("Members", $"👥 `{mee6.players.Length}` **Members**\n"));

                            await RespondOrEdit(embed);
                        }
						catch (Exception)
						{
                            var embed = new DiscordEmbedBuilder
                            {
                                Description = $"`Could not fetch any information about the server you specified.`",
                            }.AsBotError(ctx);

                            await RespondOrEdit(embed);
                        }
					}
				}
			}
        });
    }
}