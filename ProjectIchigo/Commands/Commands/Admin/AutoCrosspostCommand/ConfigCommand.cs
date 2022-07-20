namespace ProjectIchigo.Commands.AutoCrosspostCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            foreach (var b in ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.ToList())
                if (!ctx.Guild.Channels.ContainsKey(b))
                    ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(b);

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Auto Crosspost Settings • {ctx.Guild.Name}" },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = AutoCrosspostCommandAbstractions.GetCurrentConfiguration(ctx)
            };

            var SetDelayButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set delay", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
            var ExcludeBots = new DiscordButtonComponent((ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.ExcludeBots ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Exclude Bots", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🤖")));
            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                    ExcludeBots,
                    SetDelayButton
            })
            .AddComponents(new List<DiscordComponent>
            {
                    AddButton,
                    RemoveButton
            }).AddComponents(Resources.CancelButton));

            var Button = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.Result.Interaction.Data.CustomId == ExcludeBots.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.ExcludeBots = !ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.ExcludeBots;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == SetDelayButton.CustomId)
            {
                var modal = new DiscordInteractionModalBuilder("Define a new delay", Guid.NewGuid().ToString())
                    .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minutes", "Minutes (5 max)", "", 1, 2, true, "1"))
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

                    if (seconds > 59 || seconds < 0 || minutes > 5 || minutes < 0)
                    {
                        throw new Exception();
                    }

                    length = length.Add(TimeSpan.FromSeconds(seconds));
                    length = length.Add(TimeSpan.FromMinutes(minutes));

                    if (length > TimeSpan.FromMinutes(5) || length < TimeSpan.FromSeconds(1))
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("❌ `The duration has to be between 1 second and 5 minutes.`").WithColor(EmbedColors.Error)));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting = Convert.ToInt32(length.TotalSeconds);

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
            else if (Button.Result.Interaction.Data.CustomId == AddButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count >= 5)
                {
                    embed.Description = $"`You cannot add more than 5 channels to crosspost. Need more? Ask for approval on our development server:` {ctx.Bot._status.DevelopmentServerInvite}";
                    embed.Color = EmbedColors.Error;
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                DiscordChannel channel;

                try
                {
                    channel = await PromptChannelSelection();
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }

                if (channel.Type != ChannelType.News)
                {
                    embed.Description = "`The channel you selected is not an announcement channel.`";
                    embed.Color = EmbedColors.Error;
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count >= 5)
                {
                    embed.Description = $"`You cannot add more than 5 channels to crosspost. Need more? Ask for approval on our development server:` {ctx.Bot._status.DevelopmentServerInvite}";
                    embed.Color = EmbedColors.Error;
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (!ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Contains(channel.Id))
                    ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Add(channel.Id);

                await ExecuteCommand(ctx, arguments);
                return;

            }
            else if (Button.Result.Interaction.Data.CustomId == RemoveButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count == 0)
                {
                    embed.Description = $"`No Crosspost Channels are set up.`";
                    embed.Color = EmbedColors.Error;
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                ulong ChannelToRemove;

                try
                {
                    var channel = await PromptCustomSelection(ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels
                        .Select(x => new DiscordSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList());

                    ChannelToRemove = Convert.ToUInt64(channel);
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }

                if (ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Contains(ChannelToRemove))
                    ctx.Bot._guilds.List[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(ChannelToRemove);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}