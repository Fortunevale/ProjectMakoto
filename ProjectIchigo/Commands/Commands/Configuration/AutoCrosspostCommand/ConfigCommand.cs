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

            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.ToList())
                if (!ctx.Guild.Channels.ContainsKey(b))
                    ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(b);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = AutoCrosspostCommandAbstractions.GetCurrentConfiguration(ctx)
            }.SetAwaitingInput(ctx, "Auto Crosspost");

            var SetDelayButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set delay", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
            var ExcludeBots = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.ExcludeBots ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Exclude Bots", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🤖")));
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

                ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.ExcludeBots = !ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.ExcludeBots;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == SetDelayButton.CustomId)
            {
                TimeSpan Response;

                try
                {
                    Response = await PromptModalForTimeSpan(Button.Result.Interaction, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting), false);
                }
                catch (CancelCommandException)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (InvalidOperationException)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`The duration has to be between 1 second and 5 minutes.`").SetError(ctx, "Auto Crosspost")));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }

                ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.DelayBeforePosting = Convert.ToInt32(Response.TotalSeconds);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == AddButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count >= 5)
                {
                    embed.Description = $"`You cannot add more than 5 channels to crosspost. Need more? Ask for approval on our development server:` {ctx.Bot.status.DevelopmentServerInvite}";
                    embed = embed.SetError(ctx, "Auto Crosspost");
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
                    embed = embed.SetError(ctx, "Auto Crosspost");
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count >= 5)
                {
                    embed.Description = $"`You cannot add more than 5 channels to crosspost. Need more? Ask for approval on our development server:` {ctx.Bot.status.DevelopmentServerInvite}";
                    embed = embed.SetError(ctx, "Auto Crosspost");
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (!ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Contains(channel.Id))
                    ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Add(channel.Id);

                await ExecuteCommand(ctx, arguments);
                return;

            }
            else if (Button.Result.Interaction.Data.CustomId == RemoveButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Count == 0)
                {
                    embed.Description = $"`No Crosspost Channels are set up.`";
                    embed = embed.SetError(ctx, "Auto Crosspost");
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                ulong ChannelToRemove;

                try
                {
                    var channel = await PromptCustomSelection(ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels
                        .Select(x => new DiscordSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList());

                    ChannelToRemove = Convert.ToUInt64(channel);
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }

                if (ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Contains(ChannelToRemove))
                    ctx.Bot.guilds[ctx.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(ChannelToRemove);

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