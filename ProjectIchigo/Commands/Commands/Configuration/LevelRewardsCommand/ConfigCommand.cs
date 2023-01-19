namespace ProjectIchigo.Commands.LevelRewardsCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            int CurrentPage = 0;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"`Loading Level Rewards..`"
            }.AsLoading(ctx, "Level Rewards");

            await RespondOrEdit(embed);

            embed = embed.AsAwaitingInput(ctx, "Level Rewards");

            string selected = "";

            async Task RefreshMessage()
            {
                List<DiscordStringSelectComponentOption> DefinedRewards = new();

                embed.Description = "";

                foreach (var reward in ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.ToList().OrderBy(x => x.Level))
                {
                    if (!ctx.Guild.Roles.ContainsKey(reward.RoleId))
                    {
                        ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Remove(reward);
                        continue;
                    }

                    var role = ctx.Guild.GetRole(reward.RoleId);

                    DefinedRewards.Add(new DiscordStringSelectComponentOption($"Level {reward.Level}: @{role.Name}", role.Id.ToString(), $"{reward.Message.TruncateWithIndication(100)}", (selected == role.Id.ToString()), new DiscordComponentEmoji(role.Color.GetClosestColorEmoji(ctx.Client))));

                    if (selected == role.Id.ToString())
                    {
                        embed.Description = $"**Level**: `{reward.Level}`\n" +
                                            $"**Role**: <@&{reward.RoleId}> (`{reward.RoleId}`)\n" +
                                            $"**Message**: `{reward.Message}`\n";
                    }
                }

                if (DefinedRewards.Count > 0)
                {
                    if (embed.Description == "")
                        embed.Description = "`Please select a Level Reward to modify or delete.`";
                }
                else
                {
                    embed.Description = "`No Level Rewards are defined.`";
                }

                var PreviousPage = new DiscordButtonComponent(ButtonStyle.Primary, "PreviousPage", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
                var NextPage = new DiscordButtonComponent(ButtonStyle.Primary, "NextPage", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

                var Add = new DiscordButtonComponent(ButtonStyle.Success, "Add", "Add new Level Reward", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var Modify = new DiscordButtonComponent(ButtonStyle.Primary, "Modify", "Modify Message", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔄")));
                var Delete = new DiscordButtonComponent(ButtonStyle.Danger, "Delete", "Delete", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));

                var Dropdown = new DiscordStringSelectComponent("Select a Level Reward..", DefinedRewards.Skip(CurrentPage * 20).Take(20).ToList(), "RewardSelection");
                embed = embed.AsAwaitingInput(ctx, "Level Rewards");
                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                if (DefinedRewards.Count > 0)
                    builder.AddComponents(Dropdown);

                List<DiscordComponent> Row1 = new();
                List<DiscordComponent> Row2 = new();

                if (DefinedRewards.Skip(CurrentPage * 20).Count() > 20)
                    Row1.Add(NextPage);

                if (CurrentPage != 0)
                    Row1.Add(PreviousPage);

                Row2.Add(Add);

                if (selected != "")
                {
                    Row2.Add(Modify);
                    Row2.Add(Delete);
                }

                if (Row1.Count > 0)
                    builder.AddComponents(Row1);

                builder.AddComponents(Row2);

                builder.AddComponents(MessageComponents.GetCancelButton(ctx.DbUser));

                await RespondOrEdit(builder);
            }

            CancellationTokenSource cancellationTokenSource = new();

            async Task SelectInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        _ = Task.Delay(120000, cancellationTokenSource.Token).ContinueWith(x =>
                        {
                            if (x.IsCompletedSuccessfully)
                            {
                                ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                                ModifyToTimedOut(true);
                            }
                        });

                        if (e.GetCustomId() == "RewardSelection")
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            selected = e.Values.First();
                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == "Add")
                        {
                            ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            DiscordRole selectedRole = null;
                            int selectedLevel = -1;
                            string selectedCustomText = "You received ##Role##!";

                            while (true)
                            {
                                var SelectRole = new DiscordButtonComponent((selectedRole is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Role", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                                var SelectLevel = new DiscordButtonComponent((selectedLevel is -1 ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Level", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✨")));
                                var SelectCustomText = new DiscordButtonComponent((selectedCustomText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Change Message", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Submit", (selectedRole is null || selectedLevel is -1 || selectedCustomText.IsNullOrWhiteSpace()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                                var action_embed = new DiscordEmbedBuilder
                                {
                                    Description = $"`Role   `: {(selectedRole is null ? "`Not yet selected.`" : selectedRole.Mention)}\n" +
                                                  $"`Level  `: {(selectedLevel is -1 ? "`Not yet selected.`" : selectedLevel.ToEmotes())}\n" +
                                                  $"`Message`: `{selectedCustomText}`"
                                }.AsAwaitingInput(ctx, "Level Rewards");

                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                                    .AddComponents(new List<DiscordComponent> { SelectRole, SelectLevel, SelectCustomText, Finish })
                                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

                                var Menu = await ctx.WaitForButtonAsync();

                                if (Menu.TimedOut)
                                {
                                    ModifyToTimedOut();
                                    return;
                                }

                                if (Menu.GetCustomId() == SelectRole.CustomId)
                                {
                                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                    action_embed.Description = $"`Select a role to assign.`";
                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));

                                    var RoleResult = await PromptRoleSelection();

                                    if (RoleResult.TimedOut)
                                    {
                                        ModifyToTimedOut();
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
                                            return;
                                        }

                                        throw RoleResult.Exception;
                                    }

                                    if (RoleResult.Result.Id == ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId)
                                    {
                                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`You cannot set the bump reminder role to be automatically assigned as reward.`"));
                                        await Task.Delay(3000);
                                        continue;
                                    }

                                    selectedRole = RoleResult.Result;
                                    continue;
                                }
                                else if (Menu.GetCustomId() == SelectLevel.CustomId)
                                {
                                    var modal = new DiscordInteractionModalBuilder("Input Level", Guid.NewGuid().ToString())
                                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "level", "Level", "2", 1, 3, true, (selectedLevel is -1 ? 2 : selectedLevel).ToString()));


                                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, false);

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

                                    InteractionCreateEventArgs Response = ModalResult.Result;
                                    var rawInt = Response.Interaction.GetModalValueByCustomId("level");

                                    uint level;

                                    try
                                    {
                                        level = Convert.ToUInt32(rawInt);

                                        if (level < 2)
                                            throw new Exception("");
                                    }
                                    catch (Exception)
                                    {
                                        action_embed.Description = "`You must specify a valid level.`";
                                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Level Rewards")));
                                        await Task.Delay(3000);
                                        continue;
                                    }

                                    selectedLevel = (int)level;
                                    continue;
                                }
                                else if (Menu.GetCustomId() == SelectCustomText.CustomId)
                                {
                                    var modal = new DiscordInteractionModalBuilder("Define new custom message", Guid.NewGuid().ToString())
                                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "message", "Custom Message", "You received ##Role##!", 1, 256, true, selectedCustomText));


                                    var ModalResult = await PromptModalWithRetry(Menu.Result.Interaction, modal, false);

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

                                    InteractionCreateEventArgs Response = ModalResult.Result;

                                    var newMessage = Response.Interaction.GetModalValueByCustomId("message");

                                    if (newMessage.Length > 256)
                                    {
                                        action_embed.Description = "`Your custom message can't contain more than 256 characters.`";
                                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, "Level Rewards")));
                                        await Task.Delay(3000);
                                        continue;
                                    }

                                    selectedCustomText = newMessage;
                                    continue;
                                }
                                else if (Menu.GetCustomId() == Finish.CustomId)
                                {
                                    if (selectedRole.Id == ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId)
                                    {
                                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`You cannot set the bump reminder role to be automatically assigned as reward.`"));
                                        await Task.Delay(3000);
                                        await ExecuteCommand(ctx, arguments);
                                        return;
                                    }

                                    ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Add(new Entities.LevelRewardEntry
                                    {
                                        Level = selectedLevel,
                                        RoleId = selectedRole.Id,
                                        Message = selectedCustomText
                                    });

                                    action_embed.Description = $"`The role` <@&{selectedRole.Id}> `({selectedRole.Id}) will be assigned at Level {selectedLevel}.`";
                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsSuccess(ctx, "Level Rewards")));

                                    await Task.Delay(5000);
                                    await RefreshMessage();
                                    ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                    return;
                                }
                                else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                                {
                                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                    await RefreshMessage();
                                    ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                    return;
                                }

                                return;
                            }
                        }
                        else if (e.GetCustomId() == "Modify")
                        {
                            var modal = new DiscordInteractionModalBuilder()
                                .WithTitle("Define a new custom message")
                                .WithCustomId(Guid.NewGuid().ToString())
                                .AddTextComponents(new DiscordTextComponent(TextComponentStyle.Small, "new_text", "Custom Message (<256 characters)", null, 0, 256, false, ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)).Message));

                            var ModalResult = await PromptModalWithRetry(e.Interaction, modal, false);

                            if (ModalResult.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }
                            else if (ModalResult.Cancelled)
                            {
                                await RefreshMessage();
                                return;
                            }
                            else if (ModalResult.Errored)
                            {
                                throw ModalResult.Exception;
                            }

                            InteractionCreateEventArgs Response = ModalResult.Result;
                            var result = Response.Interaction.GetModalValueByCustomId("new_text");

                            if (result.Length > 256)
                            {
                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription("`Your custom message can't contain more than 256 characters.`").AsError(ctx, "Level Rewards")));
                                await Task.Delay(5000);
                                await ExecuteCommand(ctx, arguments);
                                return;
                            }

                            ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)).Message = result;

                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == "Delete")
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Remove(ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)));

                            if (ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Count == 0)
                            {
                                embed.Description = $"`There are no more Level Rewards to display.`";
                                embed = embed.AsSuccess(ctx, "Level Rewards");
                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                await ExecuteCommand(ctx, arguments);
                                return;
                            }

                            embed.Description = $"`Select a Level Reward to modify.`";
                            selected = "";

                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == "PreviousPage")
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            CurrentPage--;
                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == "NextPage")
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            CurrentPage++;
                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            DeleteOrInvalidate();
                            return;
                        }
                    }
                }).Add(ctx.Bot.watcher, ctx);
            }

            await RefreshMessage();

            _ = Task.Delay(120000, cancellationTokenSource.Token).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                    ModifyToTimedOut(true);
                }
            });

            ctx.Client.ComponentInteractionCreated += SelectInteraction;
        });
    }
}