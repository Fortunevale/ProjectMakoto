// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class AutoCrosspostCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.AutoCrosspost;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            foreach (var b in ctx.DbGuild.Crosspost.CrosspostChannels.ToList())
                if (!ctx.Guild.Channels.ContainsKey(b))
                    ctx.DbGuild.Crosspost.CrosspostChannels.Remove(b);

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.AutoCrosspost;

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.ExcludeBots, CommandKey.DelayBeforePosting);

                return $"🤖 `{CommandKey.ExcludeBots.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Crosspost.ExcludeBots.ToEmote(ctx.Bot)}\n" +
                       $"🕒 `{CommandKey.DelayBeforePosting.Get(ctx.DbUser).PadRight(pad)}`: `{TimeSpan.FromSeconds(ctx.DbGuild.Crosspost.DelayBeforePosting).GetHumanReadable()}`\n\n" +
                       $"{(ctx.DbGuild.Crosspost.CrosspostChannels.Count != 0 ? string.Join("\n\n", ctx.DbGuild.Crosspost.CrosspostChannels.Select(x => $"<#{x}> `[#{ctx.Guild.GetChannel(x).Name}]`")) : CommandKey.NoCrosspostChannels.Get(ctx.DbUser).Build(true))}";
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            var SetDelayButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.SetDelayButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
            var ExcludeBots = new DiscordButtonComponent((ctx.DbGuild.Crosspost.ExcludeBots ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleExcludeBotsButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🤖")));
            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.AddChannelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(CommandKey.RemoveChannelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

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
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            if (Button.GetCustomId() == ExcludeBots.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.DbGuild.Crosspost.ExcludeBots = !ctx.DbGuild.Crosspost.ExcludeBots;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == SetDelayButton.CustomId)
            {

                var ModalResult = await PromptModalForTimeSpan(Button.Result.Interaction, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(ctx.DbGuild.Crosspost.DelayBeforePosting), false);

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
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription(GetString(CommandKey.DurationLimit, true)).AsError(ctx, GetString(CommandKey.Title))));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                    else if (ModalResult.Exception.GetType() == typeof(ArgumentException))
                    {
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ModalResult.Exception;
                }

                ctx.DbGuild.Crosspost.DelayBeforePosting = Convert.ToInt32(ModalResult.Result.TotalSeconds);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == AddButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (ctx.DbGuild.Crosspost.CrosspostChannels.Count >= 20)
                {
                    embed.Description = GetString(CommandKey.ChannelLimit, true, new TVar("Invite", ctx.Bot.status.DevelopmentServerInvite));
                    embed = embed.AsError(ctx, GetString(CommandKey.Title));
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
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(this.t.Commands.Common.Errors.NoChannels, true)));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                if (ChannelResult.Result.Type != ChannelType.News)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription(GetString(this.t.Commands.Common.Errors.NoChannels, true)).AsError(ctx, GetString(CommandKey.Title))));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (ctx.DbGuild.Crosspost.CrosspostChannels.Count >= 50)
                {
                    await RespondOrEdit(embed.WithDescription(GetString(CommandKey.ChannelLimit, true, new TVar("Invite", ctx.Bot.status.DevelopmentServerInvite))).AsError(ctx, GetString(CommandKey.Title)));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (!ctx.DbGuild.Crosspost.CrosspostChannels.Contains(ChannelResult.Result.Id))
                    ctx.DbGuild.Crosspost.CrosspostChannels.Add(ChannelResult.Result.Id);

                await ExecuteCommand(ctx, arguments);
                return;

            }
            else if (Button.GetCustomId() == RemoveButton.CustomId)
            {
                _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (ctx.DbGuild.Crosspost.CrosspostChannels.Count == 0)
                {
                    await RespondOrEdit(embed.WithDescription(GetString(CommandKey.NoCrosspostChannels, true)).AsError(ctx, GetString(CommandKey.Title)));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                var ChannelResult = await PromptCustomSelection(ctx.DbGuild.Crosspost.CrosspostChannels
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

                if (ctx.DbGuild.Crosspost.CrosspostChannels.Contains(ChannelToRemove))
                    ctx.DbGuild.Crosspost.CrosspostChannels.Remove(ChannelToRemove);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}