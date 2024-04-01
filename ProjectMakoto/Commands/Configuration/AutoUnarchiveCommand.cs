// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Configuration;

internal sealed class AutoUnarchiveCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.AutoUnarchive;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                foreach (var b in ctx.DbGuild.AutoUnarchiveThreads.ToList())
                {
                    if (!ctx.Guild.Channels.ContainsKey(b))
                        ctx.DbGuild.AutoUnarchiveThreads = ctx.DbGuild.AutoUnarchiveThreads.Remove(x => x.ToString(), b);
                }

                return $"{(ctx.DbGuild.AutoUnarchiveThreads.Length != 0 ? string.Join("\n", ctx.DbGuild.AutoUnarchiveThreads.Select(x => $"{ctx.Guild.GetChannel(x).Mention} [`#{ctx.Guild.GetChannel(x).Name}`] (`{x}`)")) : ctx.Bot.LoadedTranslations.Commands.Config.AutoUnarchive.NoChannels.Get(ctx.DbUser).Build(true))}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = $"{GetCurrentConfiguration(ctx)}\n\n{this.GetString(CommandKey.Explanation)}"
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var Add = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(CommandKey.AddChannelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var Remove = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetString(CommandKey.RemoveChannelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Add,
                Remove
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Add.CustomId)
            {
                var ChannelResult = await this.PromptChannelSelection(new ChannelType[] { ChannelType.Text, ChannelType.Forum });

                if (ChannelResult.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }
                else if (ChannelResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ChannelResult.Failed)
                {
                    if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoChannels)));
                        await Task.Delay(3000);
                        await this.ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                if (!ctx.DbGuild.AutoUnarchiveThreads.Contains(ChannelResult.Result.Id))
                    ctx.DbGuild.AutoUnarchiveThreads = ctx.DbGuild.AutoUnarchiveThreads.Add(ChannelResult.Result.Id);

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == Remove.CustomId)
            {
                var ChannelResult = await this.PromptCustomSelection(ctx.DbGuild.AutoUnarchiveThreads
                        .Select(x => new DiscordStringSelectComponentOption($"#{ctx.Guild.GetChannel(x).Name} ({x})", x.ToString(), $"{(ctx.Guild.GetChannel(x).Parent is not null ? $"{ctx.Guild.GetChannel(x).Parent.Name}" : "")}")).ToList());

                if (ChannelResult.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }
                else if (ChannelResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ChannelResult.Errored)
                {
                    throw ChannelResult.Exception;
                }

                var ChannelToRemove = Convert.ToUInt64(ChannelResult.Result);

                if (ctx.DbGuild.AutoUnarchiveThreads.Contains(ChannelToRemove))
                    ctx.DbGuild.AutoUnarchiveThreads = ctx.DbGuild.AutoUnarchiveThreads.Remove(x => x.ToString(), ChannelToRemove);

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}