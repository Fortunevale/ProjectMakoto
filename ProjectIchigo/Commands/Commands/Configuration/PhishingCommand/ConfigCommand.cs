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
            }.SetAwaitingInput(ctx, "Phishing Protection");

            var ToggleDetectionButton = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Detection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💀")));
            var ToggleWarningButton = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Redirect Warning", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠")));
            var ChangePunishmentButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Punishment", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔨")));
            var ChangeReasonButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Change Reason", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeTimeoutLengthButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Change Timeout Length", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                { ToggleDetectionButton },
                { ToggleWarningButton },
            })
            .AddComponents(new List<DiscordComponent>
            {
                { ChangePunishmentButton },
                { ChangeReasonButton },
                { ChangeTimeoutLengthButton }
            }).AddComponents(Resources.CancelButton));

            var Button = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.Result.Interaction.Data.CustomId == ToggleDetectionButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing = !ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == ToggleWarningButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect = !ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == ChangePunishmentButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var dropdown = new DiscordSelectComponent("Select an action..", new List<DiscordSelectComponentOption>
                    {
                        { new DiscordSelectComponentOption("Ban", "Ban", "Bans the user if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Kick", "Kick", "Kicks the user if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Timeout", "Timeout", "Times the user out if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Delete", "Delete", "Only deletes the message containing the detected scam link") },
                    }, "selection");

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                var e = await ctx.Client.GetInteractivity().WaitForSelectAsync(ctx.ResponseMessage, x => x.User.Id == ctx.User.Id, TimeSpan.FromMinutes(2));

                if (e.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                switch (e.Result.Values.First())
                {
                    case "Ban":
                        ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.BAN;
                        break;
                    case "Kick":
                        ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.KICK;
                        break;
                    case "Timeout":
                        ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.TIMEOUT;
                        break;
                    case "Delete":
                        ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.DELETE;
                        break;
                }

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == ChangeReasonButton.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder("Define a new reason", Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "new_reason", "New reason | Use %R to insert default reason", "", null, null, true, ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason));

                InteractionCreateEventArgs Response = null;

                try
                {
                    Response = await PromptModalWithRetry(Button.Result.Interaction, modal, false);
                }
                catch (CancelCommandException)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason = Response.Interaction.GetModalValueByCustomId("new_reason");

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == ChangeTimeoutLengthButton.CustomId)
            {
                if (ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType != PhishingPunishmentType.TIMEOUT)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`You aren't using 'Timeout' as your Punishment`")));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                TimeSpan Response;

                try
                {
                    Response = await PromptModalForTimeSpan(Button.Result.Interaction, TimeSpan.FromDays(28), TimeSpan.FromSeconds(10), ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength, false);
                }
                catch (CancelCommandException)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (InvalidOperationException)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`The duration has to be between 10 seconds and 28 days.`").SetError(ctx, "Phishing Protection")));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }

                ctx.Bot.guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength = Response;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}