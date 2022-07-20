﻿namespace ProjectIchigo.Commands.AutoUnarchiveCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Auto Thread Unarchiver • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"{AutoUnarchiveCommandAbstractions.GetCurrentConfiguration(ctx)}\n\nThis module allows you to automatically unarchive threads of certain channels. **You will need to lock threads to actually archive them.**"
            };

            var Add = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Add new channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var Remove = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove a channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Add,
                Remove
            })
            .AddComponents(Resources.CancelButton));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

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
                    channel = await PromptChannelSelection();
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                if (!ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Contains(channel.Id))
                    ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Add(channel.Id);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == Remove.CustomId)
            {
                ulong ChannelToRemove;

                try
                {
                    var channel = await PromptCustomSelection(ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads
                        .Select(x => new DiscordSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList());

                    ChannelToRemove = Convert.ToUInt64(channel);
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                if (ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Contains(ChannelToRemove))
                    ctx.Bot._guilds.List[ctx.Guild.Id].AutoUnarchiveThreads.Remove(ChannelToRemove);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}