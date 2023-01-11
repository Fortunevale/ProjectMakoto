namespace ProjectIchigo.Commands.PhishingCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = PhishingCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, "Phishing Protection");

            var ToggleDetectionButton = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Detection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💀")));
            var ToggleWarningButton = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.WarnOnRedirect ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Redirect Warning", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠")));
            var ToggleAbuseIpDbButton = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.AbuseIpDbReports ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle AbuseIPDB Reports", false, new DiscordComponentEmoji(EmojiTemplates.GetAbuseIpDb(ctx.Bot)));
            var ChangePunishmentButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Punishment", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔨")));
            var ChangeReasonButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Change Reason", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeTimeoutLengthButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Change Timeout Length", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));

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
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.GetCustomId() == ToggleDetectionButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.DetectPhishing = !ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.DetectPhishing;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ToggleWarningButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.WarnOnRedirect = !ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.WarnOnRedirect;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ToggleAbuseIpDbButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.AbuseIpDbReports = !ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.AbuseIpDbReports;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangePunishmentButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var dropdown = new DiscordStringSelectComponent("Select an action..", new List<DiscordStringSelectComponentOption>
                    {
                        { new DiscordStringSelectComponentOption("Ban", "Ban", "Bans the user if a scam link has been detected") },
                        { new DiscordStringSelectComponentOption("Kick", "Kick", "Kicks the user if a scam link has been detected") },
                        { new DiscordStringSelectComponentOption("Timeout", "Timeout", "Times the user out if a scam link has been detected") },
                        { new DiscordStringSelectComponentOption("Delete", "Delete", "Only deletes the message containing the detected scam link") },
                    }, "selection");

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                var e = await ctx.Client.GetInteractivity().WaitForSelectAsync(ctx.ResponseMessage, x => x.User.Id == ctx.User.Id, ComponentType.StringSelect, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                switch (e.Result.Values.First())
                {
                    case "Ban":
                        ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.PunishmentType = PhishingPunishmentType.BAN;
                        break;
                    case "Kick":
                        ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.PunishmentType = PhishingPunishmentType.KICK;
                        break;
                    case "Timeout":
                        ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.PunishmentType = PhishingPunishmentType.TIMEOUT;
                        break;
                    case "Delete":
                        ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.PunishmentType = PhishingPunishmentType.DELETE;
                        break;
                }

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeReasonButton.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder("Define a new reason", Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "new_reason", "New reason | Use %R to insert default reason", "", null, null, true, ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.CustomPunishmentReason));

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

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.CustomPunishmentReason = ModalResult.Result.Interaction.GetModalValueByCustomId("new_reason");

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeTimeoutLengthButton.CustomId)
            {
                if (ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.PunishmentType != PhishingPunishmentType.TIMEOUT)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`You aren't using 'Timeout' as your Punishment`")));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }


                var ModalResult = await PromptModalForTimeSpan(Button.Result.Interaction, TimeSpan.FromDays(28), TimeSpan.FromSeconds(10), ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.CustomPunishmentLength, false);

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
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`The duration has to be between 10 seconds and 28 days.`").AsError(ctx, "Phishing Protection")));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ModalResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetection.CustomPunishmentLength = ModalResult.Result;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}