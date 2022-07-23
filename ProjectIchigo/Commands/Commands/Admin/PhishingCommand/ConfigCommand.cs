namespace ProjectIchigo.Commands.PhishingCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Protection Settings • {ctx.Guild.Name}" },
                Color = EmbedColors.Loading,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = PhishingCommandAbstractions.GetCurrentConfiguration(ctx)
            };

            var ToggleDetectionButton = new DiscordButtonComponent((ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Detection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💀")));
            var ToggleWarningButton = new DiscordButtonComponent((ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Redirect Warning", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠")));
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

                ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing = !ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == ToggleWarningButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect = !ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect;
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
                        ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.BAN;
                        break;
                    case "Kick":
                        ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.KICK;
                        break;
                    case "Timeout":
                        ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.TIMEOUT;
                        break;
                    case "Delete":
                        ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.DELETE;
                        break;
                }

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == ChangeReasonButton.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder("Define a new reason", Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "new_reason", "New reason | Use %R to insert default reason", "", null, null, true, ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason));

                await Button.Result.Interaction.CreateInteractionModalResponseAsync(modal);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`Waiting for modal..`")));

                var e = await ctx.Client.GetInteractivity().WaitForModalAsync(modal.CustomId, TimeSpan.FromMinutes(10));

                if (e.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason = e.Result.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "new_reason").First().Components.First().Value;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == ChangeTimeoutLengthButton.CustomId)
            {
                if (ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType != PhishingPunishmentType.TIMEOUT)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`You aren't using 'Timeout' as your Punishment`")));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                var modal = new DiscordInteractionModalBuilder("Define a new timeout length", Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "days", "Days (28 max)", "", 1, 2, true, "14"))
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "hours", "Hours (23 max)", "", 1, 2, true, "0"))
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minutes", "Minutes (59 max)", "", 1, 2, true, "0"))
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "seconds", "Seconds (59 max)", "", 1, 2, true, "0"));

                await Button.Result.Interaction.CreateInteractionModalResponseAsync(modal);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`Waiting for modal..`")));

                var e = await ctx.Client.GetInteractivity().WaitForModalAsync(modal.CustomId, TimeSpan.FromMinutes(10));

                _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                try
                {
                    TimeSpan length = TimeSpan.FromSeconds(0);

                    double seconds = Convert.ToDouble(Convert.ToUInt32(e.Result.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "seconds").First().Components.First().Value));
                    double minutes = Convert.ToDouble(Convert.ToUInt32(e.Result.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "minutes").First().Components.First().Value));
                    double hours = Convert.ToDouble(Convert.ToUInt32(e.Result.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "hours").First().Components.First().Value));
                    double days = Convert.ToDouble(Convert.ToUInt32(e.Result.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "days").First().Components.First().Value));

                    if (seconds > 59 || seconds < 0 || minutes > 59 || minutes < 0 || hours > 23 || hours < 0 || days > 28 || days < 0 )
                    {
                        throw new Exception();
                    }

                    length = length.Add(TimeSpan.FromSeconds(seconds));
                    length = length.Add(TimeSpan.FromMinutes(minutes));
                    length = length.Add(TimeSpan.FromHours(hours));
                    length = length.Add(TimeSpan.FromDays(days));

                    if (length > TimeSpan.FromDays(28) || length < TimeSpan.FromSeconds(10))
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("❌ `The duration has to be between 10 seconds and 28 days.`").WithColor(EmbedColors.Error)));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    ctx.Bot._guilds[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength = length;

                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (Exception)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("❌ `Invalid duration`").WithColor(EmbedColors.Error)));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
            }
            else if (Button.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}