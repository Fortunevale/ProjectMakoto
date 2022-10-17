namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = "`Loading Reaction Roles..`"
            }.AsLoading(ctx, "Reaction Roles"));

            await ReactionRolesCommandAbstractions.CheckForInvalid(ctx);

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add a new reaction role", (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Count > 100), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove a reaction role", (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Count == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`{ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Count} reaction roles are set up.`"
            }.AsAwaitingInput(ctx, "Reaction Roles");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                AddButton, RemoveButton
            })
            .AddComponents(MessageComponents.CancelButton));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == AddButton.CustomId)
            {
                DiscordMessage selectedMessage = null;
                DiscordEmoji selectedEmoji = null;
                DiscordRole selectedRole = null;

                while (true)
                {
                    var SelectMessage = new DiscordButtonComponent((selectedMessage is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Message", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
                    var SelectEmoji = new DiscordButtonComponent((selectedEmoji is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Emoji", (selectedMessage is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("😀")));
                    var SelectRole = new DiscordButtonComponent((selectedRole is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Role", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                    var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Submit", (selectedMessage is null || selectedRole is null || selectedEmoji is null), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    var action_embed = new DiscordEmbedBuilder
                    {
                        Description = $"`Message`: {(selectedMessage is null ? "`Not yet selected.`" : $"[`Jump to message`]({selectedMessage.JumpLink})")}\n" +
                                      $"`Emoji  `: {(selectedEmoji is null ? "`Not yet selected.`" : selectedEmoji.ToString())}\n" +
                                      $"`Role   `: {(selectedRole is null ? "`Not yet selected.`" : selectedRole.Mention)}"
                    }.AsAwaitingInput(ctx, "Reaction Roles");


                    if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Count > 100)
                    {
                        action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                        .AddComponents(new List<DiscordComponent> { SelectMessage, SelectEmoji, SelectRole, Finish })
                        .AddComponents(MessageComponents.CancelButton));

                    var Menu = await ctx.WaitForButtonAsync();

                    if (Menu.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    if (Menu.Result.Interaction.Data.CustomId == SelectMessage.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder("Input Message Url", Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "url", "Message Url", "https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678", null, null, true));

                        var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, new DiscordEmbedBuilder
                        {
                            Description = "`Please copy and paste the message link of the message you want the reaction role to be added to.`",
                            ImageUrl = "https://cdn.discordapp.com/attachments/906976602557145110/967753175241203712/unknown.png"
                        }.AsAwaitingInput(ctx, "Reaction Roles"), false);

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
                            action_embed.Description = $"`This doesn't look correct. A message url should look something like these:`\n" +
                                                       $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                       $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                                       $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                            action_embed.ImageUrl = "";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (GuildId != ctx.Guild.Id)
                        {
                            action_embed.Description = $"`The link you provided leads to another server.`";
                            action_embed.ImageUrl = "";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (!ctx.Guild.Channels.ContainsKey(ChannelId))
                        {
                            action_embed.Description = $"`The link you provided leads to a channel that doesn't exist.`";
                            action_embed.ImageUrl = "";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(3000);
                            continue;
                        }

                        var channel = ctx.Guild.GetChannel(ChannelId);

                        if (!channel.TryGetMessage(MessageId, out DiscordMessage reactionMessage))
                        {
                            action_embed.Description = $"`The link you provided leads a message that doesn't exist or the bot has no access to.`";
                            action_embed.ImageUrl = "";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(3000);
                            continue;
                        }

                        selectedMessage = reactionMessage;
                        continue;
                    }
                    else if (Menu.Result.Interaction.Data.CustomId == SelectEmoji.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        action_embed.Description = "`Please react with the emoji you want to use for the reaction role to the target message.`";
                        action_embed.ImageUrl = "";
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsAwaitingInput(ctx, "Reaction Roles")));

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
                            action_embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(3000);
                            continue;
                        }

                        if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Any(x => (x.Key == selectedMessage.Id && x.Value.EmojiName == emoji.GetUniqueDiscordName())))
                        {
                            action_embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(3000);
                            continue;
                        }

                        selectedEmoji = emoji;
                        continue;
                    }
                    else if (Menu.Result.Interaction.Data.CustomId == SelectRole.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await RespondOrEdit(action_embed.WithDescription("`Please select the role you want to use.`").AsAwaitingInput(ctx, "Reaction Roles"));

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
                                await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any roles in your server.`"));
                                await Task.Delay(3000);
                                continue;
                            }

                            throw RoleResult.Exception;
                        }

                        if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Any(x => x.Value.RoleId == RoleResult.Result.Id))
                        {
                            action_embed.Description = $"`The specified role is already being used in another reaction role.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(3000);
                            continue;
                        }

                        selectedRole = RoleResult.Result;
                        continue;
                    }
                    else if (Menu.Result.Interaction.Data.CustomId == Finish.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Count > 100)
                        {
                            action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(5000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Any(x => x.Value.RoleId == selectedRole.Id))
                        {
                            action_embed.Description = $"`The specified role is already being used in another reaction role.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(5000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (selectedEmoji.Id != 0 && !ctx.Guild.Emojis.ContainsKey(selectedEmoji.Id))
                        {
                            action_embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(5000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }

                        if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Any(x => (x.Key == selectedMessage.Id && x.Value.EmojiName == selectedEmoji.GetUniqueDiscordName())))
                        {
                            action_embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Reaction Roles")));
                            await Task.Delay(5000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }

                        ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Add(new KeyValuePair<ulong, Entities.ReactionRoleEntry>(selectedMessage.Id, new ReactionRoleEntry
                        {
                            ChannelId = selectedMessage.Channel.Id,
                            RoleId = selectedRole.Id,
                            EmojiId = selectedEmoji.Id,
                            EmojiName = selectedEmoji.GetUniqueDiscordName()
                        }));

                        await selectedMessage.CreateReactionAsync(selectedEmoji);

                        action_embed.Description = $"`Added role` {selectedRole.Mention} `to message sent by` {selectedMessage.Author?.Mention ?? "-"} `in` {selectedMessage.Channel.Mention} `with emoji` {selectedEmoji} `.`";
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsSuccess(ctx, "Reaction Roles")));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }
                    else if (Menu.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
                    {
                        _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    return;
                }
            }
            else if (e.Result.Interaction.Data.CustomId == RemoveButton.CustomId)
            {
                var RoleResult = await PromptCustomSelection(ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles
                    .Select(x => new DiscordSelectComponentOption($"@{ctx.Guild.GetRole(x.Value.RoleId).Name}", x.Value.UUID, $"in Channel #{ctx.Guild.GetChannel(x.Value.ChannelId).Name}", emoji: new DiscordComponentEmoji(x.Value.GetEmoji(ctx.Client)))).ToList());

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

                var obj = ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.First(x => x.Value.UUID == RoleResult.Result);

                if (ctx.Guild.GetChannel(obj.Value.ChannelId).TryGetMessage(obj.Key, out var reactionMessage))
                    _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

                var role = ctx.Guild.GetRole(obj.Value.RoleId);                

                ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(obj);

                embed.Description = $"`Removed role` {role.Mention} `from message sent by` {reactionMessage?.Author.Mention ?? "`/`"} `in` {reactionMessage?.Channel.Mention ?? "`/`"} `with emoji` {obj.Value.GetEmoji(ctx.Client)} `.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, "Reaction Roles")));
                await Task.Delay(5000);
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