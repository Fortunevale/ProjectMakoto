namespace ProjectIchigo.Commands.AutoUnarchiveCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = $"{AutoUnarchiveCommandAbstractions.GetCurrentConfiguration(ctx)}\n\nThis module allows you to automatically unarchive threads of certain channels. **You will need to lock threads to actually archive them.**"
            }.SetAwaitingInput(ctx, "Auto Thread Unarchiver");

            var Add = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Add new channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var Remove = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove a channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Add,
                Remove
            })
            .AddComponents(MessageComponents.CancelButton));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == Add.CustomId)
            {
                DiscordChannel channel;

                try
                {
                    channel = await PromptChannelSelection(new ChannelType[] { ChannelType.Text, ChannelType.Forum });
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                catch (CancelCommandException)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (NullReferenceException)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any text or forum channels in your server.`"));
                    await Task.Delay(3000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (!ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Contains(channel.Id))
                    ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Add(channel.Id);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == Remove.CustomId)
            {
                ulong ChannelToRemove;

                try
                {
                    var channel = await PromptCustomSelection(ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads
                        .Select(x => new DiscordSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList());

                    ChannelToRemove = Convert.ToUInt64(channel);
                }
                catch (CancelCommandException)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                if (ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Contains(ChannelToRemove))
                    ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Remove(ChannelToRemove);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}