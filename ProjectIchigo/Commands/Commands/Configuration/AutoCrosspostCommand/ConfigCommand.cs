namespace ProjectIchigo.Commands.AutoCrosspostCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.ToList())
                if (!ctx.Guild.Channels.ContainsKey(b))
                    ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Remove(b);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = AutoCrosspostCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, "Auto Crosspost");

            var SetDelayButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set delay", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
            var ExcludeBots = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].Crosspost.ExcludeBots ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Exclude Bots", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🤖")));
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
            }).AddComponents(MessageComponents.CancelButton));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.GetCustomId() == ExcludeBots.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.guilds[ctx.Guild.Id].Crosspost.ExcludeBots = !ctx.Bot.guilds[ctx.Guild.Id].Crosspost.ExcludeBots;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == SetDelayButton.CustomId)
            {

                var ModalResult = await PromptModalForTimeSpan(Button.Result.Interaction, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(ctx.Bot.guilds[ctx.Guild.Id].Crosspost.DelayBeforePosting), false);

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
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`The duration has to be between 1 second and 5 minutes.`").AsError(ctx, "Auto Crosspost")));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                    else if (ModalResult.Exception.GetType() == typeof(ArgumentException))
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`Invalid Time Span`").AsError(ctx, "Auto Crosspost")));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ModalResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].Crosspost.DelayBeforePosting = Convert.ToInt32(ModalResult.Result.TotalSeconds);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == AddButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Count >= 50)
                {
                    embed.Description = $"`You cannot add more than 50 channels to crosspost. Need more? Ask for approval on our development server:` {ctx.Bot.status.DevelopmentServerInvite}";
                    embed = embed.AsError(ctx, "Auto Crosspost");
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                var ChannelResult = await PromptChannelSelection(ChannelType.News);

                if (ChannelResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (ChannelResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ChannelResult.Failed)
                {
                    if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any announcement channels in your server.`"));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                if (ChannelResult.Result.Type != ChannelType.News)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`The channel you selected is not an announcement channel.`").AsError(ctx, "Auto Crosspost")));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Count >= 50)
                {
                    await RespondOrEdit(embed.WithDescription($"`You cannot add more than 50 channels to crosspost. Need more? Ask for approval on our development server:` {ctx.Bot.status.DevelopmentServerInvite}").AsError(ctx, "Auto Crosspost"));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (!ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Contains(ChannelResult.Result.Id))
                    ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Add(ChannelResult.Result.Id);

                await ExecuteCommand(ctx, arguments);
                return;

            }
            else if (Button.GetCustomId() == RemoveButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Count == 0)
                {
                    await RespondOrEdit(embed.WithDescription($"`No Crosspost Channels are set up.`").AsError(ctx, "Auto Crosspost"));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                var ChannelResult = await PromptCustomSelection(ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels
                        .Select(x => new DiscordSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList());

                if (ChannelResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (ChannelResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ChannelResult.Errored)
                {
                    throw ChannelResult.Exception;
                }

                ulong ChannelToRemove = Convert.ToUInt64(ChannelResult.Result);

                if (ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Contains(ChannelToRemove))
                    ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Remove(ChannelToRemove);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}