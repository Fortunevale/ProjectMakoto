// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class PhishingCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Config.Phishing;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.DetectPhishingLinks, CommandKey.RedirectWarning, CommandKey.AbuseIpDbReports, CommandKey.PunishmentType,
                                                                       CommandKey.CustomPunishmentReason, CommandKey.CustomTimeoutLength);

                return $"💀 `{GetString(CommandKey.DetectPhishingLinks).PadRight(pad)}` : {ctx.DbGuild.PhishingDetection.DetectPhishing.ToEmote(ctx.Bot)}\n" +
                       $"⚠ `{GetString(CommandKey.RedirectWarning).PadRight(pad)}` : {ctx.DbGuild.PhishingDetection.WarnOnRedirect.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetAbuseIpDb(ctx.Bot)} `{GetString(CommandKey.AbuseIpDbReports).PadRight(pad)}` : {ctx.DbGuild.PhishingDetection.AbuseIpDbReports.ToEmote(ctx.Bot)}\n" +
                       $"🔨 `{GetString(CommandKey.PunishmentType).PadRight(pad)}` : `{GetTypeString(ctx.DbGuild.PhishingDetection.PunishmentType)}`\n" +
                       $"💬 `{GetString(CommandKey.CustomPunishmentReason).PadRight(pad)}` : `{ctx.DbGuild.PhishingDetection.CustomPunishmentReason}`\n" +
                       $"🕒 `{GetString(CommandKey.CustomTimeoutLength).PadRight(pad)}` : `{ctx.DbGuild.PhishingDetection.CustomPunishmentLength.GetHumanReadable()}`";
            }

            string GetTypeString(PhishingPunishmentType type)
            {
                return type switch
                {
                    PhishingPunishmentType.Delete => GetString(CommandKey.PunishmentTypeDelete),
                    PhishingPunishmentType.Timeout => GetString(CommandKey.PunishmentTypeTimeout),
                    PhishingPunishmentType.Kick => GetString(CommandKey.PunishmentTypeKick),
                    PhishingPunishmentType.Ban => GetString(CommandKey.PunishmentTypeBan),
                    PhishingPunishmentType.SoftBan => GetString(CommandKey.PunishmentTypeSoftban),
                        _ => throw new NotImplementedException(),
                };
            }
            
            string GetTypeDescriptionString(PhishingPunishmentType type)
            {
                return type switch
                {
                    PhishingPunishmentType.Delete => GetString(CommandKey.PunishmentTypeDeleteDescription),
                    PhishingPunishmentType.Timeout => GetString(CommandKey.PunishmentTypeTimeoutDescription),
                    PhishingPunishmentType.Kick => GetString(CommandKey.PunishmentTypeKickDescription),
                    PhishingPunishmentType.Ban => GetString(CommandKey.PunishmentTypeBanDescription),
                    PhishingPunishmentType.SoftBan => GetString(CommandKey.PunishmentTypeSoftbanDescription),
                        _ => throw new NotImplementedException(),
                };
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            var ToggleDetectionButton = new DiscordButtonComponent((ctx.DbGuild.PhishingDetection.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleDetection), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💀")));
            var ToggleWarningButton = new DiscordButtonComponent((ctx.DbGuild.PhishingDetection.WarnOnRedirect ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleWarning), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠")));
            var ToggleAbuseIpDbButton = new DiscordButtonComponent((ctx.DbGuild.PhishingDetection.AbuseIpDbReports ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.AbuseIpDbReports), false, new DiscordComponentEmoji(EmojiTemplates.GetAbuseIpDb(ctx.Bot)));
            var ChangePunishmentButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.ChangePunishmentType), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔨")));
            var ChangeReasonButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), GetString(CommandKey.ChangePunishmentReason), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeTimeoutLengthButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), GetString(CommandKey.ChangeTimeoutLength), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                { ToggleDetectionButton },
                { ToggleWarningButton },
                { ToggleAbuseIpDbButton },
            })
            .AddComponents(new List<DiscordComponent>
            {
                { ChangePunishmentButton },
                { ChangeReasonButton },
                { ChangeTimeoutLengthButton }
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.GetCustomId() == ToggleDetectionButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.DbGuild.PhishingDetection.DetectPhishing = !ctx.DbGuild.PhishingDetection.DetectPhishing;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ToggleWarningButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.DbGuild.PhishingDetection.WarnOnRedirect = !ctx.DbGuild.PhishingDetection.WarnOnRedirect;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ToggleAbuseIpDbButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.DbGuild.PhishingDetection.AbuseIpDbReports = !ctx.DbGuild.PhishingDetection.AbuseIpDbReports;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangePunishmentButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var e = await PromptCustomSelection(Enum.GetNames(typeof(PhishingPunishmentType)).Select(x =>
                {
                    var type = Enum.Parse<PhishingPunishmentType>(x);
                    return new DiscordStringSelectComponentOption(GetTypeString(type), x, GetTypeDescriptionString(type));
                }));

                if (e.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                switch (e.Result)
                {
                    case "Ban":
                        ctx.DbGuild.PhishingDetection.PunishmentType = PhishingPunishmentType.Ban;
                        break;
                    case "SoftBan":
                        ctx.DbGuild.PhishingDetection.PunishmentType = PhishingPunishmentType.SoftBan;
                        break;
                    case "Kick":
                        ctx.DbGuild.PhishingDetection.PunishmentType = PhishingPunishmentType.Kick;
                        break;
                    case "Timeout":
                        ctx.DbGuild.PhishingDetection.PunishmentType = PhishingPunishmentType.Timeout;
                        break;
                    case "Delete":
                        ctx.DbGuild.PhishingDetection.PunishmentType = PhishingPunishmentType.Delete;
                        break;
                }

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeReasonButton.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder(GetString(CommandKey.Title), Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "new_reason", GetString(CommandKey.DefineNewReason), "", null, null, true, ctx.DbGuild.PhishingDetection.CustomPunishmentReason));

                var ModalResult = await PromptModalWithRetry(Button.Result.Interaction, modal, false);

                if (ModalResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (ModalResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ModalResult.Errored)
                {
                    throw ModalResult.Exception;
                }

                ctx.DbGuild.PhishingDetection.CustomPunishmentReason = ModalResult.Result.Interaction.GetModalValueByCustomId("new_reason");

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeTimeoutLengthButton.CustomId)
            {
                if (ctx.DbGuild.PhishingDetection.PunishmentType != PhishingPunishmentType.Timeout)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription(GetString(CommandKey.NotUsingType, true, new TVar("Type", GetString(CommandKey.PunishmentTypeTimeout))))));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }


                var ModalResult = await PromptModalForTimeSpan(Button.Result.Interaction, TimeSpan.FromDays(28), TimeSpan.FromSeconds(10), ctx.DbGuild.PhishingDetection.CustomPunishmentLength, false);

                if (ModalResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (ModalResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ModalResult.Errored)
                {
                    if (ModalResult.Exception.GetType() == typeof(InvalidOperationException))
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription(GetString(CommandKey.InvalidDuration, true)).AsError(ctx, GetString(CommandKey.Title))));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ModalResult.Exception;
                }

                ctx.DbGuild.PhishingDetection.CustomPunishmentLength = ModalResult.Result;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}