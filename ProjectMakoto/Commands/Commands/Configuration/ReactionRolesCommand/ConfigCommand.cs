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
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.ReactionRoles;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = this.GetString(CommandKey.LoadingReactionRoles, true)
            }.AsLoading(ctx, this.GetString(CommandKey.Title)));

            _ = await ReactionRolesCommandAbstractions.CheckForInvalid(ctx);

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.AddNewReactionRole), (ctx.DbGuild.ReactionRoles.Count > 100), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetString(CommandKey.RemoveReactionRole), (ctx.DbGuild.ReactionRoles.Count == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            var embed = new DiscordEmbedBuilder
            {
                Description = this.GetString(CommandKey.ReactionRoleCount, true, new TVar("Count", ctx.DbGuild.ReactionRoles.Count))
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                AddButton, RemoveButton
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
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
                    var SelectMessage = new DiscordButtonComponent((selectedMessage is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectMessage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
                    var SelectEmoji = new DiscordButtonComponent((selectedEmoji is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectEmoji), (selectedMessage is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("😀")));
                    var SelectRole = new DiscordButtonComponent((selectedRole is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectRole), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Common.Submit), (selectedMessage is null || selectedRole is null || selectedEmoji is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.Message, CommandKey.Emoji, CommandKey.Role);

                    var action_embed = new DiscordEmbedBuilder
                    {
                        Description = $"`{this.GetString(CommandKey.Message).PadRight(pad)}`: {(selectedMessage is null ? this.GetString(this.t.Common.NotSelected, true) : $"[`{this.GetString(this.t.Common.JumpToMessage)}`]({selectedMessage.JumpLink})")}\n" +
                                      $"`{this.GetString(CommandKey.Emoji).PadRight(pad)}`: {(selectedEmoji is null ? this.GetString(this.t.Common.NotSelected, true) : selectedEmoji.ToString())}\n" +
                                      $"`{this.GetString(CommandKey.Role).PadRight(pad)}`: {(selectedRole is null ? this.GetString(this.t.Common.NotSelected, true) : selectedRole.Mention)}"
                    }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));


                    if (ctx.DbGuild.ReactionRoles.Count > 100)
                    {
                        action_embed.Description = this.GetString(CommandKey.ReactionRoleLimitReached, true);
                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                        await Task.Delay(5000);
                        await this.ExecuteCommand(ctx, arguments);
                        return;
                    }

                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                        .AddComponents(new List<DiscordComponent> { SelectMessage, SelectEmoji, SelectRole, Finish })
                        .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        this.ModifyToTimedOut();
                        return;
                    }

                    if (Menu.GetCustomId() == SelectMessage.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder(this.GetString(CommandKey.Title), Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "url", this.GetString(CommandKey.MessageUrl), "https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678", null, null, true));

                        var ModalResult = await this.PromptModalWithRetry(Menu.Result.Interaction, modal, new DiscordEmbedBuilder
                        {
                            Description = this.GetString(CommandKey.MessageUrlInstructions, true),
                            ImageUrl = "https://cdn.discordapp.com/attachments/906976602557145110/967753175241203712/unknown.png"
                        }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title)), false);

                        if (ModalResult.TimedOut)
                        {
                            this.ModifyToTimedOut(true);
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

                        if (!RegexTemplates.DiscordChannelUrl.IsMatch(url) || !url.TryParseMessageLink(out var GuildId, out var ChannelId, out var MessageId))
                        {
                            action_embed.Description = $"{this.GetString(CommandKey.InvalidMessageUrl, true)}\n" +
                                                       $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                       $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                       $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                            action_embed.ImageUrl = "";
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (GuildId != ctx.Guild.Id)
                        {
                            action_embed.Description = this.GetString(CommandKey.MessageUrlWrongGuild, true);
                            action_embed.ImageUrl = "";
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (!ctx.Guild.Channels.ContainsKey(ChannelId))
                        {
                            action_embed.Description = this.GetString(CommandKey.MessageUrlNoChannel, true);
                            action_embed.ImageUrl = "";
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        var channel = ctx.Guild.GetChannel(ChannelId);

                        if (!channel.TryGetMessage(MessageId, out var reactionMessage))
                        {
                            action_embed.Description = this.GetString(CommandKey.MessageUrlNoMessage, true);
                            action_embed.ImageUrl = "";
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        selectedMessage = reactionMessage;
                        continue;
                    }
                    else if (Menu.GetCustomId() == SelectEmoji.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        action_embed.Description = this.GetString(CommandKey.ReactWithEmoji, true);
                        action_embed.ImageUrl = "";
                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsAwaitingInput(ctx, this.GetString(CommandKey.Title))));

                        var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == selectedMessage.Id, TimeSpan.FromMinutes(2));

                        if (emoji_wait.TimedOut)
                        {
                            this.ModifyToTimedOut(true);
                            return;
                        }

                        try
                        { _ = emoji_wait.Result.Message.DeleteReactionAsync(emoji_wait.Result.Emoji, ctx.User); }
                        catch { }

                        var emoji = emoji_wait.Result.Emoji;

                        if (emoji.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji.Id))
                        {
                            action_embed.Description = this.GetString(CommandKey.NoAccessToEmoji, true);
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (ctx.DbGuild.ReactionRoles.Any(x => (x.Key == selectedMessage.Id && x.Value.EmojiName == emoji.GetUniqueDiscordName())))
                        {
                            action_embed.Description = this.GetString(CommandKey.EmojiAlreadyUsed, true);
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(3000);
                            continue;
                        }

                        selectedEmoji = emoji;
                        continue;
                    }
                    else if (Menu.GetCustomId() == SelectRole.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        _ = await this.RespondOrEdit(action_embed.WithDescription(this.GetString(CommandKey.SelectRolePrompt, true)).AsAwaitingInput(ctx, this.GetString(CommandKey.Title)));

                        var RoleResult = await this.PromptRoleSelection();

                        if (RoleResult.TimedOut)
                        {
                            this.ModifyToTimedOut(true);
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
                                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.NoRoles, true)));
                                await Task.Delay(3000);
                                continue;
                            }

                            throw RoleResult.Exception;
                        }

                        if (ctx.DbGuild.ReactionRoles.Any(x => x.Value.RoleId == RoleResult.Result.Id))
                        {
                            action_embed.Description = this.GetString(CommandKey.RoleAlreadyUsed, true);
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
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
                            action_embed.Description = this.GetString(CommandKey.ReactionRoleLimitReached, true);
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(5000);
                            await this.ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (ctx.DbGuild.ReactionRoles.Any(x => x.Value.RoleId == selectedRole.Id))
                        {
                            action_embed.Description = this.GetString(CommandKey.RoleAlreadyUsed, true);
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(5000);
                            await this.ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (selectedEmoji.Id != 0 && !ctx.Guild.Emojis.ContainsKey(selectedEmoji.Id))
                        {
                            action_embed.Description = this.GetString(CommandKey.NoAccessToEmoji, true);
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(5000);
                            await this.ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (ctx.DbGuild.ReactionRoles.Any(x => (x.Key == selectedMessage.Id && x.Value.EmojiName == selectedEmoji.GetUniqueDiscordName())))
                        {
                            action_embed.Description = this.GetString(CommandKey.EmojiAlreadyUsed, true);
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                            await Task.Delay(5000);
                            await this.ExecuteCommand(ctx, arguments);
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

                        embed.Description = this.GetString(CommandKey.AddedReactionRole, true,
                            new TVar("Role", selectedRole.Mention),
                            new TVar("User", selectedMessage.Author.Mention),
                            new TVar("Channel", selectedMessage.Channel.Mention),
                            new TVar("Emoji", selectedEmoji));
                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsSuccess(ctx, this.GetString(CommandKey.Title))));
                        await Task.Delay(5000);
                        await this.ExecuteCommand(ctx, arguments);
                        return;
                    }
                    else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await this.ExecuteCommand(ctx, arguments);
                        return;
                    }

                    return;
                }
            }
            else if (e.GetCustomId() == RemoveButton.CustomId)
            {
                var RoleResult = await this.PromptCustomSelection(ctx.DbGuild.ReactionRoles
                    .Select(x => new DiscordStringSelectComponentOption($"@{ctx.Guild.GetRole(x.Value.RoleId).Name}", x.Value.UUID, $"in Channel #{ctx.Guild.GetChannel(x.Value.ChannelId).Name}", emoji: new DiscordComponentEmoji(x.Value.GetEmoji(ctx.Client)))).ToList());

                if (RoleResult.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
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

                _ = ctx.DbGuild.ReactionRoles.Remove(obj);


                embed.Description = this.GetString(CommandKey.RemovedReactionRole, true,
                    new TVar("Role", role.Mention),
                    new TVar("User", reactionMessage?.Author.Mention ?? "`/`"),
                    new TVar("Channel", reactionMessage?.Channel.Mention ?? "`/`"),
                    new TVar("Emoji", obj.Value.GetEmoji(ctx.Client)));
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, this.GetString(CommandKey.Title))));
                await Task.Delay(5000);
                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}