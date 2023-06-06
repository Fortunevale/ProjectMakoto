// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.LevelRewardsCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Config.LevelRewards;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            int CurrentPage = 0;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = GetString(CommandKey.Loading, true)
            }.AsLoading(ctx, GetString(CommandKey.Title));

            await RespondOrEdit(embed);

            embed = embed.AsAwaitingInput(ctx, GetString(CommandKey.Title));

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

                    DefinedRewards.Add(new DiscordStringSelectComponentOption($"{GetString(CommandKey.Level)} {reward.Level}: @{role.Name}", role.Id.ToString(), $"{reward.Message.TruncateWithIndication(100)}", (selected == role.Id.ToString()), new DiscordComponentEmoji(role.Color.GetClosestColorEmoji(ctx.Client))));

                    if (selected == role.Id.ToString())
                    {
                        embed.Description = $"**{GetString(CommandKey.Level)}**: `{reward.Level}`\n" +
                                            $"**{GetString(CommandKey.Role)}**: <@&{reward.RoleId}> (`{reward.RoleId}`)\n" +
                                            $"**{GetString(CommandKey.Message)}**: `{reward.Message}`\n";
                    }
                }

                if (DefinedRewards.Count > 0)
                {
                    if (embed.Description == "")
                        embed.Description = GetString(CommandKey.SelectPrompt, true);
                }
                else
                {
                    embed.Description = GetString(CommandKey.NoRewardsSetup, true);
                }

                var PreviousPage = new DiscordButtonComponent(ButtonStyle.Primary, "PreviousPage", GetString(t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
                var NextPage = new DiscordButtonComponent(ButtonStyle.Primary, "NextPage", GetString(t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

                var Add = new DiscordButtonComponent(ButtonStyle.Success, "Add", GetString(CommandKey.AddNewButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var Modify = new DiscordButtonComponent(ButtonStyle.Primary, "Modify", GetString(CommandKey.ModifyButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔄")));
                var Delete = new DiscordButtonComponent(ButtonStyle.Danger, "Delete", GetString(CommandKey.RemoveButton), false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));

                var Dropdown = new DiscordStringSelectComponent(GetString(CommandKey.SelectDropdown), DefinedRewards.Skip(CurrentPage * 20).Take(20).ToList(), "RewardSelection");
                embed = embed.AsAwaitingInput(ctx, GetString(CommandKey.Title));
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

                builder.AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot));

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
                            string selectedCustomText = GetGuildString(CommandKey.DefaultCustomText);

                            while (true)
                            {
                                var SelectRole = new DiscordButtonComponent((selectedRole is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(CommandKey.SelectRoleButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                                var SelectLevel = new DiscordButtonComponent((selectedLevel is -1 ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(CommandKey.SelectLevelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✨")));
                                var SelectCustomText = new DiscordButtonComponent((selectedCustomText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), GetString(CommandKey.ChangeMessageButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Common.Submit), (selectedRole is null || selectedLevel is -1 || selectedCustomText.IsNullOrWhiteSpace()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.Role, CommandKey.Level, CommandKey.Message);

                                var action_embed = new DiscordEmbedBuilder
                                {
                                    Description = $"`{GetString(CommandKey.Role).PadRight(pad)}`: {(selectedRole is null ? GetString(t.Common.NotSelected, true) : selectedRole.Mention)}\n" +
                                                  $"`{GetString(CommandKey.Level).PadRight(pad)}`: {(selectedLevel is -1 ? GetString(t.Common.NotSelected, true) : selectedLevel.ToEmotes())}\n" +
                                                  $"`{GetString(CommandKey.Message).PadRight(pad)}`: `{selectedCustomText}`"
                                }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                                    .AddComponents(new List<DiscordComponent> { SelectRole, SelectLevel, SelectCustomText, Finish })
                                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                                var Menu = await ctx.WaitForButtonAsync();

                                if (Menu.TimedOut)
                                {
                                    ModifyToTimedOut();
                                    return;
                                }

                                if (Menu.GetCustomId() == SelectRole.CustomId)
                                {
                                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

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
                                            await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(t.Commands.Common.Errors.NoRoles, true)));
                                            await Task.Delay(3000);
                                            return;
                                        }

                                        throw RoleResult.Exception;
                                    }

                                    if (RoleResult.Result.Id == ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId)
                                    {
                                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(CommandKey.CantUseRole, true)));
                                        await Task.Delay(3000);
                                        continue;
                                    }

                                    selectedRole = RoleResult.Result;
                                    continue;
                                }
                                else if (Menu.GetCustomId() == SelectLevel.CustomId)
                                {
                                    var modal = new DiscordInteractionModalBuilder(GetString(CommandKey.Title), Guid.NewGuid().ToString())
                                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "level", GetString(CommandKey.Level), "2", 1, 3, true, (selectedLevel is -1 ? 2 : selectedLevel).ToString()));


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
                                        continue;
                                    }

                                    selectedLevel = (int)level;
                                    continue;
                                }
                                else if (Menu.GetCustomId() == SelectCustomText.CustomId)
                                {
                                    var modal = new DiscordInteractionModalBuilder(GetString(CommandKey.Title), Guid.NewGuid().ToString())
                                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "message", GetString(CommandKey.Message), GetGuildString(CommandKey.DefaultCustomText), 1, 256, true, selectedCustomText));


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
                                        action_embed.Description = GetString(CommandKey.MessageTooLong, true);
                                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, GetString(CommandKey.Title))));
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
                                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(CommandKey.CantUseRole, true)));
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

                                    action_embed.Description = GetString(CommandKey.AddedNewReward, true, new TVar("Role", $"<@&{selectedRole.Id}>"), new TVar("Level", selectedLevel));
                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsSuccess(ctx, GetString(CommandKey.Title))));

                                    await Task.Delay(5000);
                                    await RefreshMessage();
                                    ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                    return;
                                }
                                else if (Menu.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
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
                                .WithTitle(GetString(CommandKey.Title))
                                .WithCustomId(Guid.NewGuid().ToString())
                                .AddTextComponents(new DiscordTextComponent(TextComponentStyle.Small, "new_text", GetString(CommandKey.Message), null, 0, 256, false, ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)).Message)); ;

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
                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription(GetString(CommandKey.MessageTooLong, true)).AsError(ctx, GetString(CommandKey.Title))));
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
                                await ExecuteCommand(ctx, arguments);
                                return;
                            }

                            embed.Description = GetString(CommandKey.SelectPrompt, true);
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
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
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