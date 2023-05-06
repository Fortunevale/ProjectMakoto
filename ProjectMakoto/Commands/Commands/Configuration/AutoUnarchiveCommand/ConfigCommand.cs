// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.AutoUnarchiveCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = $"{AutoUnarchiveCommandAbstractions.GetCurrentConfiguration(ctx)}\n\nThis module allows you to automatically unarchive threads of certain channels. **You will need to lock threads to actually archive them.**"
            }.AsAwaitingInput(ctx, "Auto Thread Unarchiver");

            var Add = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Add new channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var Remove = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove a channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Add,
                Remove
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Add.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(new ChannelType[] { ChannelType.Text, ChannelType.Forum });

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
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any text or forum channels in your server.`"));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                if (!ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Contains(ChannelResult.Result.Id))
                    ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Add(ChannelResult.Result.Id);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == Remove.CustomId)
            {
                var ChannelResult = await PromptCustomSelection(ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads
                        .Select(x => new DiscordStringSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList());

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

                if (ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Contains(ChannelToRemove))
                    ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Remove(ChannelToRemove);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}