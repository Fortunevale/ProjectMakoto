// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class LevelRewardsCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.LevelRewards;

#pragma warning disable CS8321 // Local function is declared but never used
            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.LevelRewards;

                var str = "";
                if (ctx.DbGuild.LevelRewards.Count != 0)
                {
                    foreach (var b in ctx.DbGuild.LevelRewards.OrderBy(x => x.Level))
                    {
                        if (!ctx.Guild.Roles.ContainsKey(b.RoleId))
                        {
                            _ = ctx.DbGuild.LevelRewards.Remove(b);
                            continue;
                        }

                        str += $"**{ctx.BaseCommand.GetString(CommandKey.Level)}**: `{b.Level}`\n" +
                                $"**{ctx.BaseCommand.GetString(CommandKey.Role)}**: <@&{b.RoleId}> (`{b.RoleId}`)\n" +
                                $"**{ctx.BaseCommand.GetString(CommandKey.Message)}**: `{b.Message}`\n";

                        str += "\n\n";
                    }
                }
                else
                {
                    str = ctx.BaseCommand.GetString(CommandKey.NoRewardsSetup, true);
                }

                return str;
            }
#pragma warning restore CS8321 // Local function is declared but never used

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var CurrentPage = 0;

            var embed = new DiscordEmbedBuilder()
            {
                Description = this.GetString(CommandKey.Loading, true)
            }.AsLoading(ctx, this.GetString(CommandKey.Title));

            _ = await this.RespondOrEdit(embed);

            embed = embed.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var selected = "";

            async Task RefreshMessage()
            {
                List<DiscordStringSelectComponentOption> DefinedRewards = new();

                embed.Description = "";

                foreach (var reward in ctx.DbGuild.LevelRewards.ToList().OrderBy(x => x.Level))
                {
                    if (!ctx.Guild.Roles.ContainsKey(reward.RoleId))
                    {
                        _ = ctx.DbGuild.LevelRewards.Remove(reward);
                        continue;
                    }

                    var role = ctx.Guild.GetRole(reward.RoleId);

                    DefinedRewards.Add(new DiscordStringSelectComponentOption($"{this.GetString(CommandKey.Level)} {reward.Level}: @{role.Name}", role.Id.ToString(), $"{reward.Message.TruncateWithIndication(100)}", (selected == role.Id.ToString()), new DiscordComponentEmoji(role.Color.GetClosestColorEmoji(ctx.Client))));

                    if (selected == role.Id.ToString())
                    {
                        embed.Description = $"**{this.GetString(CommandKey.Level)}**: `{reward.Level}`\n" +
                                            $"**{this.GetString(CommandKey.Role)}**: <@&{reward.RoleId}> (`{reward.RoleId}`)\n" +
                                            $"**{this.GetString(CommandKey.Message)}**: `{reward.Message}`\n";
                    }
                }

                if (DefinedRewards.Count > 0)
                {
                    if (embed.Description == "")
                        embed.Description = this.GetString(CommandKey.SelectPrompt, true);
                }
                else
                {
                    embed.Description = this.GetString(CommandKey.NoRewardsSetup, true);
                }

                var PreviousPage = new DiscordButtonComponent(ButtonStyle.Primary, "PreviousPage", this.GetString(this.t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
                var NextPage = new DiscordButtonComponent(ButtonStyle.Primary, "NextPage", this.GetString(this.t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

                var Add = new DiscordButtonComponent(ButtonStyle.Success, "Add", this.GetString(CommandKey.AddNewButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var Modify = new DiscordButtonComponent(ButtonStyle.Primary, "Modify", this.GetString(CommandKey.ModifyButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔄")));
                var Delete = new DiscordButtonComponent(ButtonStyle.Danger, "Delete", this.GetString(CommandKey.RemoveButton), false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));

                var Dropdown = new DiscordStringSelectComponent(this.GetString(CommandKey.SelectDropdown), DefinedRewards.Skip(CurrentPage * 20).Take(20).ToList(), "RewardSelection");
                embed = embed.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));
                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                if (DefinedRewards.Count > 0)
                    _ = builder.AddComponents(Dropdown);

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
                    _ = builder.AddComponents(Row1);

                _ = builder.AddComponents(Row2);

                _ = builder.AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot));

                _ = await this.RespondOrEdit(builder);
            }

            CancellationTokenSource cancellationTokenSource = new();

            async Task SelectInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
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
                                this.ModifyToTimedOut(true);
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
                            var selectedLevel = -1;
                            var selectedCustomText = this.GetGuildString(CommandKey.DefaultCustomText);

                            while (true)
                            {
                                var SelectRole = new DiscordButtonComponent((selectedRole is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectRoleButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                                var SelectLevel = new DiscordButtonComponent((selectedLevel is -1 ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.SelectLevelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✨")));
                                var SelectCustomText = new DiscordButtonComponent((selectedCustomText.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeMessageButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗯")));
                                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Common.Submit), (selectedRole is null || selectedLevel is -1 || selectedCustomText.IsNullOrWhiteSpace()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.Role, CommandKey.Level, CommandKey.Message);

                                var action_embed = new DiscordEmbedBuilder
                                {
                                    Description = $"`{this.GetString(CommandKey.Role).PadRight(pad)}`: {(selectedRole is null ? this.GetString(this.t.Common.NotSelected, true) : selectedRole.Mention)}\n" +
                                                  $"`{this.GetString(CommandKey.Level).PadRight(pad)}`: {(selectedLevel is -1 ? this.GetString(this.t.Common.NotSelected, true) : selectedLevel.ToEmotes())}\n" +
                                                  $"`{this.GetString(CommandKey.Message).PadRight(pad)}`: `{selectedCustomText}`"
                                }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

                                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed)
                                    .AddComponents(new List<DiscordComponent> { SelectRole, SelectLevel, SelectCustomText, Finish })
                                    .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

                                var Menu = await ctx.WaitForButtonAsync();

                                if (Menu.TimedOut)
                                {
                                    this.ModifyToTimedOut();
                                    return;
                                }

                                if (Menu.GetCustomId() == SelectRole.CustomId)
                                {
                                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                    var RoleResult = await this.PromptRoleSelection();

                                    if (RoleResult.TimedOut)
                                    {
                                        this.ModifyToTimedOut();
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
                                            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoRoles, true)));
                                            await Task.Delay(3000);
                                            return;
                                        }

                                        throw RoleResult.Exception;
                                    }

                                    if (RoleResult.Result.Id == ctx.DbGuild.BumpReminder.RoleId)
                                    {
                                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.CantUseRole, true)));
                                        await Task.Delay(3000);
                                        continue;
                                    }

                                    selectedRole = RoleResult.Result;
                                    continue;
                                }
                                else if (Menu.GetCustomId() == SelectLevel.CustomId)
                                {
                                    var modal = new DiscordInteractionModalBuilder(this.GetString(CommandKey.Title), Guid.NewGuid().ToString())
                                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "level", this.GetString(CommandKey.Level), "2", 1, 3, true, (selectedLevel is -1 ? 2 : selectedLevel).ToString()));


                                    var ModalResult = await this.PromptModalWithRetry(Menu.Result.Interaction, modal, false);

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
                                    var modal = new DiscordInteractionModalBuilder(this.GetString(CommandKey.Title), Guid.NewGuid().ToString())
                                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "message", this.GetString(CommandKey.Message), this.GetGuildString(CommandKey.DefaultCustomText), 1, 256, true, selectedCustomText));


                                    var ModalResult = await this.PromptModalWithRetry(Menu.Result.Interaction, modal, false);

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

                                    InteractionCreateEventArgs Response = ModalResult.Result;

                                    var newMessage = Response.Interaction.GetModalValueByCustomId("message");

                                    if (newMessage.Length > 256)
                                    {
                                        action_embed.Description = this.GetString(CommandKey.MessageTooLong, true);
                                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsError(ctx, this.GetString(CommandKey.Title))));
                                        await Task.Delay(3000);
                                        continue;
                                    }

                                    selectedCustomText = newMessage;
                                    continue;
                                }
                                else if (Menu.GetCustomId() == Finish.CustomId)
                                {
                                    if (selectedRole.Id == ctx.DbGuild.BumpReminder.RoleId)
                                    {
                                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.CantUseRole, true)));
                                        await Task.Delay(3000);
                                        await this.ExecuteCommand(ctx, arguments);
                                        return;
                                    }

                                    ctx.DbGuild.LevelRewards.Add(new()
                                    {
                                        Level = selectedLevel,
                                        RoleId = selectedRole.Id,
                                        Message = selectedCustomText
                                    });

                                    action_embed.Description = this.GetString(CommandKey.AddedNewReward, true, new TVar("Role", $"<@&{selectedRole.Id}>"), new TVar("Level", selectedLevel));
                                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed.AsSuccess(ctx, this.GetString(CommandKey.Title))));

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
                                .WithTitle(this.GetString(CommandKey.Title))
                                .WithCustomId(Guid.NewGuid().ToString())
                                .AddTextComponents(new DiscordTextComponent(TextComponentStyle.Small, "new_text", this.GetString(CommandKey.Message), null, 0, 256, false, ctx.DbGuild.LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)).Message));
                            ;

                            var ModalResult = await this.PromptModalWithRetry(e.Interaction, modal, false);

                            if (ModalResult.TimedOut)
                            {
                                this.ModifyToTimedOut(true);
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
                                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription(this.GetString(CommandKey.MessageTooLong, true)).AsError(ctx, this.GetString(CommandKey.Title))));
                                await Task.Delay(5000);
                                await this.ExecuteCommand(ctx, arguments);
                                return;
                            }

                            ctx.DbGuild.LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)).Message = result;

                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == "Delete")
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            _ = ctx.DbGuild.LevelRewards.Remove(ctx.DbGuild.LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)));

                            if (ctx.DbGuild.LevelRewards.Count == 0)
                            {
                                await this.ExecuteCommand(ctx, arguments);
                                return;
                            }

                            embed.Description = this.GetString(CommandKey.SelectPrompt, true);
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

                            this.DeleteOrInvalidate();
                            return;
                        }
                    }
                }).Add(ctx.Bot, ctx);
            }

            await RefreshMessage();

            _ = Task.Delay(120000, cancellationTokenSource.Token).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                    this.ModifyToTimedOut(true);
                }
            });

            ctx.Client.ComponentInteractionCreated += SelectInteraction;
        });
    }
}