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

            var ToggleDetectionButton = new DiscordButtonComponent((ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Detection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💀")));
            var ToggleWarningButton = new DiscordButtonComponent((ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Redirect Warning", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠")));
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

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == ToggleDetectionButton.CustomId)
            {
                ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing = !ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.DetectPhishing;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ToggleWarningButton.CustomId)
            {
                ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect = !ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.WarnOnRedirect;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ChangePunishmentButton.CustomId)
            {
                var dropdown = new DiscordSelectComponent("Select an action..", new List<DiscordSelectComponentOption>
                    {
                        { new DiscordSelectComponentOption("Ban", "Ban", "Bans the user if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Kick", "Kick", "Kicks the user if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Timeout", "Timeout", "Times the user out if a scam link has been detected") },
                        { new DiscordSelectComponentOption("Delete", "Delete", "Only deletes the message containing the detected scam link") },
                    }, "selection");

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(dropdown));

                async Task ChangePunishmentInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        ctx.Client.ComponentInteractionCreated -= ChangePunishmentInteraction;

                        switch (e.Values.First())
                        {
                            case "Ban":
                                ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.BAN;
                                break;
                            case "Kick":
                                ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.KICK;
                                break;
                            case "Timeout":
                                ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.TIMEOUT;
                                break;
                            case "Delete":
                                ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType = PhishingPunishmentType.DELETE;
                                break;
                        }

                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                };

                _ = Task.Delay(60000).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                    {
                        ctx.Client.ComponentInteractionCreated -= ChangePunishmentInteraction;
                        ModifyToTimedOut(true);
                    }
                });

                ctx.Client.ComponentInteractionCreated += ChangePunishmentInteraction;
            }
            else if (e.Result.Interaction.Data.CustomId == ChangeReasonButton.CustomId)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("Please specify a new Ban/Kick Reason.\n" +
                                                                                                    "_Type `cancel` or `.` to cancel._\n\n" +
                                                                                                    "**Placeholders**\n" +
                                                                                                    "`%R` - _A placeholder for the reason_")));

                var reason = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(60));

                if (reason.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                _ = reason.Result.DeleteAsync();

                if (reason.Result.Content.ToLower() is not "cancel" and not ".")
                    ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentReason = reason.Result.Content;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ChangeTimeoutLengthButton.CustomId)
            {
                if (ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.PunishmentType != PhishingPunishmentType.TIMEOUT)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`You aren't using 'Timeout' as your Punishment`")));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("Please specify how long the timeout should last with one of the following suffixes:\n" +
                                                                                                    "`d` - _Days (default)_\n" +
                                                                                                    "`h` - _Hours_\n" +
                                                                                                    "`m` - _Minutes_\n" +
                                                                                                    "`s` - _Seconds_\n\n" +
                                                                                                    "e.g.: `31h = 31 Hours`")));

                var reason = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(60));

                if (reason.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                _ = reason.Result.DeleteAsync();

                if (reason.Result.Content.ToLower() is "cancel" or ".")
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                try
                {
                    if (!TimeSpan.TryParse(reason.Result.Content, out TimeSpan length))
                    {
                        switch (reason.Result.Content[^1..])
                        {
                            case "d":
                                length = TimeSpan.FromDays(Convert.ToInt32(reason.Result.Content.Replace("d", "")));
                                break;
                            case "h":
                                length = TimeSpan.FromHours(Convert.ToInt32(reason.Result.Content.Replace("h", "")));
                                break;
                            case "m":
                                length = TimeSpan.FromMinutes(Convert.ToInt32(reason.Result.Content.Replace("m", "")));
                                break;
                            case "s":
                                length = TimeSpan.FromSeconds(Convert.ToInt32(reason.Result.Content.Replace("s", "")));
                                break;
                            default:
                                length = TimeSpan.FromDays(Convert.ToInt32(reason.Result.Content));
                                return;
                        }
                    }

                    if (length > TimeSpan.FromDays(28) || length < TimeSpan.FromSeconds(1))
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("The duration has to be between 1 second and 28 days.")));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    ctx.Bot._guilds.List[ctx.Guild.Id].PhishingDetectionSettings.CustomPunishmentLength = length;

                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (Exception)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("Invalid duration")));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}