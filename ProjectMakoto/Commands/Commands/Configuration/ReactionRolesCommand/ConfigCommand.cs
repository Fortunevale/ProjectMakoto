// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Commands.ReactionRolesCommand;

internal sealed class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Config.ReactionRoles;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(CommandKey.LoadingReactionRoles, true)
            }.AsLoading(ctx, GetString(CommandKey.Title)));

            await ReactionRolesCommandAbstractions.CheckForInvalid(ctx);

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.AddNewReactionRole), (ctx.DbGuild.ReactionRoles.Count > 100), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(CommandKey.RemoveReactionRole), (ctx.DbGuild.ReactionRoles.Count == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            var embed = new DiscordEmbedBuilder
            {
                Description = GetString(CommandKey.ReactionRoleCount, true, new TVar("Count", ctx.DbGuild.ReactionRoles.Count))
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                AddButton, RemoveButton
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == AddButton.CustomId)
            {
                DiscordMessage selectedMessage = null;
                DiscordEmoji selectedEmoji = null;
                DiscordRole selectedRole = null;

                while (true)
                {
                    var SelectMessage = new DiscordButtonComponent((selectedMessage is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(CommandKey.SelectMessage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
                    var SelectEmoji = new DiscordButtonComponent((selectedEmoji is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(CommandKey.SelectEmoji), (selectedMessage is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("😀")));
                    var SelectRole = new DiscordButtonComponent((selectedRole is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(CommandKey.SelectRole), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Common.Submit), (selectedMessage is null || selectedRole is null || selectedEmoji is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.Message, CommandKey.Emoji, CommandKey.Role);

                    var action_embed = new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(CommandKey.Message).PadRight(pad)}`: {(selectedMessage is null ? GetString(t.Common.NotSelected, true) : $"[`{GetString(CommandKey.JumpToMessage)}`]({selectedMessage.JumpLink})")}\n" +
                                      $"`{GetString(CommandKey.Emoji).PadRight(pad)}`: {(selectedEmoji is null ? GetString(t.Common.NotSelected, true) : selectedEmoji.ToString())}\n" +
                                      $"`{GetString(CommandKey.Role).PadRight(pad)}`: {(selectedRole is null ? GetString(t.Common.NotSelected, true) : selectedRole.Mention)}"
                    }.AsAwaitingInput(ctx, GetString(CommandKey.Title));


                    if (ctx.DbGuild.ReactionRoles.Count > 100)
                    {
                        action_embed.Description = GetString(CommandKey.ReactionRoleLimitReached, true);
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                        .AddComponents(new List<DiscordComponent> { SelectMessage, SelectEmoji, SelectRole, Finish })
                        .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    if (Menu.GetCustomId() == SelectMessage.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder(GetString(CommandKey.Title), Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "url", GetString(CommandKey.MessageUrl), "https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678", null, null, true));

                        var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, new DiscordEmbedBuilder
                        {
                            Description = GetString(CommandKey.MessageUrlInstructions, true),
                            ImageUrl = "https://cdn.discordapp.com/attachments/906976602557145110/967753175241203712/unknown.png"
                        }.AsAwaitingInput(ctx, GetString(CommandKey.Title)), false);

                        if (ModalResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            continue;
                        }
                        else if (ModalResult.Errored)
                        {
                            throw ModalResult.Exception;
                        }

                        var url = ModalResult.Result.Interaction.GetModalValueByCustomId("url");

                        if (!RegexTemplates.DiscordChannelUrl.IsMatch(url) || !url.TryParseMessageLink(out ulong GuildId, out ulong ChannelId, out ulong MessageId))
                        {
                            action_embed.Description = $"{GetString(CommandKey.InvalidMessageUrl, true)}\n" +
                                                       $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                       $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                       $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                            action_embed.ImageUrl = "";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (GuildId != ctx.Guild.Id)
                        {
                            action_embed.Description = GetString(CommandKey.MessageUrlWrongGuild, true);
                            action_embed.ImageUrl = "";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (!ctx.Guild.Channels.ContainsKey(ChannelId))
                        {
                            action_embed.Description = GetString(CommandKey.MessageUrlNoChannel, true);
                            action_embed.ImageUrl = "";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        var channel = ctx.Guild.GetChannel(ChannelId);

                        if (!channel.TryGetMessage(MessageId, out DiscordMessage reactionMessage))
                        {
                            action_embed.Description = GetString(CommandKey.MessageUrlNoMessage, true);
                            action_embed.ImageUrl = "";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        selectedMessage = reactionMessage;
                        continue;
                    }
                    else if (Menu.GetCustomId() == SelectEmoji.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        action_embed.Description = GetString(CommandKey.ReactWithEmoji, true);
                        action_embed.ImageUrl = "";
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsAwaitingInput(ctx, GetString(CommandKey.Title))));

                        var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == selectedMessage.Id, TimeSpan.FromMinutes(2));

                        if (emoji_wait.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }

                        try
                        { _ = emoji_wait.Result.Message.DeleteReactionAsync(emoji_wait.Result.Emoji, ctx.User); }
                        catch { }

                        var emoji = emoji_wait.Result.Emoji;

                        if (emoji.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji.Id))
                        {
                            action_embed.Description = GetString(CommandKey.NoAccessToEmoji, true);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (ctx.DbGuild.ReactionRoles.Any(x => (x.Key == selectedMessage.Id && x.Value.EmojiName == emoji.GetUniqueDiscordName())))
                        {
                            action_embed.Description = GetString(CommandKey.EmojiAlreadyUsed, true);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        selectedEmoji = emoji;
                        continue;
                    }
                    else if (Menu.GetCustomId() == SelectRole.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await RespondOrEdit(action_embed.WithDescription(GetString(CommandKey.SelectRolePrompt, true)).AsAwaitingInput(ctx, GetString(CommandKey.Title)));

                        var RoleResult = await PromptRoleSelection();

                        if (RoleResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (RoleResult.Cancelled)
                        {
                            continue;
                        }
                        else if (RoleResult.Failed)
                        {
                            if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                            {
                                await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(CommandKey.NoRoles, true)));
                                await Task.Delay(3000);
                                continue;
                            }

                            throw RoleResult.Exception;
                        }

                        if (ctx.DbGuild.ReactionRoles.Any(x => x.Value.RoleId == RoleResult.Result.Id))
                        {
                            action_embed.Description = GetString(CommandKey.RoleAlreadyUsed, true);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        selectedRole = RoleResult.Result;
                        continue;
                    }
                    else if (Menu.GetCustomId() == Finish.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (ctx.DbGuild.ReactionRoles.Count > 100)
                        {
                            action_embed.Description = GetString(CommandKey.ReactionRoleLimitReached, true);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(5000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (ctx.DbGuild.ReactionRoles.Any(x => x.Value.RoleId == selectedRole.Id))
                        {
                            action_embed.Description = GetString(CommandKey.RoleAlreadyUsed, true);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(5000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (selectedEmoji.Id != 0 && !ctx.Guild.Emojis.ContainsKey(selectedEmoji.Id))
                        {
                            action_embed.Description = GetString(CommandKey.NoAccessToEmoji, true);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(5000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (ctx.DbGuild.ReactionRoles.Any(x => (x.Key == selectedMessage.Id && x.Value.EmojiName == selectedEmoji.GetUniqueDiscordName())))
                        {
                            action_embed.Description = GetString(CommandKey.EmojiAlreadyUsed, true);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
                            await Task.Delay(5000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }

                        ctx.DbGuild.ReactionRoles.Add(new KeyValuePair<ulong, ReactionRoleEntry>(selectedMessage.Id, new()
                        {
                            ChannelId = selectedMessage.Channel.Id,
                            RoleId = selectedRole.Id,
                            EmojiId = selectedEmoji.Id,
                            EmojiName = selectedEmoji.GetUniqueDiscordName()
                        }));

                        await selectedMessage.CreateReactionAsync(selectedEmoji);

                        embed.Description = GetString(CommandKey.AddedReactionRole, true,
                            new TVar("Role", selectedRole.Mention),
                            new TVar("User", selectedMessage.Author.Mention),
                            new TVar("Channel", selectedMessage.Channel.Mention),
                            new TVar("Emoji", selectedEmoji));
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsSuccess(ctx, GetString(CommandKey.Title))));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                    else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    return;
                }
            }
            else if (e.GetCustomId() == RemoveButton.CustomId)
            {
                var RoleResult = await PromptCustomSelection(ctx.DbGuild.ReactionRoles
                    .Select(x => new DiscordStringSelectComponentOption($"@{ctx.Guild.GetRole(x.Value.RoleId).Name}", x.Value.UUID, $"in Channel #{ctx.Guild.GetChannel(x.Value.ChannelId).Name}", emoji: new DiscordComponentEmoji(x.Value.GetEmoji(ctx.Client)))).ToList());

                if (RoleResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (RoleResult.Errored)
                {
                    throw RoleResult.Exception;
                }

                var obj = ctx.DbGuild.ReactionRoles.First(x => x.Value.UUID == RoleResult.Result);

                if (ctx.Guild.GetChannel(obj.Value.ChannelId).TryGetMessage(obj.Key, out var reactionMessage))
                    _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

                var role = ctx.Guild.GetRole(obj.Value.RoleId);

                ctx.DbGuild.ReactionRoles.Remove(obj);


                embed.Description = GetString(CommandKey.RemovedReactionRole, true,
                    new TVar("Role", role.Mention),
                    new TVar("User", reactionMessage?.Author.Mention ?? "`/`"),
                    new TVar("Channel", reactionMessage?.Channel.Mention ?? "`/`"),
                    new TVar("Emoji", obj.Value.GetEmoji(ctx.Client)));
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, GetString(CommandKey.Title))));
                await Task.Delay(5000);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}