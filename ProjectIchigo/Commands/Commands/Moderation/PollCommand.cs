namespace ProjectIchigo.Commands;

internal class PollCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            DiscordRole SelectedRole = null;
            DiscordChannel SelectedChannel = null;

            string SelectedPrompt = null;
            List<DiscordSelectComponentOption> SelectedOptions = new();

            while (true)
            {
                var SelectRoleButton = new DiscordButtonComponent((SelectedRole is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Role", false, DiscordEmoji.FromUnicode("👥").ToComponent());
                var SelectChannelButton = new DiscordButtonComponent((SelectedChannel is null ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Select Channel", false, DiscordEmoji.FromUnicode("📢").ToComponent());

                var SelectPromptButton = new DiscordButtonComponent((SelectedPrompt.IsNullOrWhiteSpace() ? ButtonStyle.Primary : ButtonStyle.Secondary), Guid.NewGuid().ToString(), "Set Poll", false, DiscordEmoji.FromUnicode("❓").ToComponent());
            
                var AddOptionButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add new option", (SelectedOptions.Count >= 20), DiscordEmoji.FromUnicode("➕").ToComponent());
                var RemoveOptionButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Remove option", (SelectedOptions.Count <= 0), DiscordEmoji.FromUnicode("➖").ToComponent());
                var Finish = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Submit", (SelectedChannel is null || SelectedPrompt.IsNullOrWhiteSpace() || !SelectedOptions.IsNotNullAndNotEmpty()), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
            
                var embed = new DiscordEmbedBuilder().WithDescription($"`Poll Content     `: {(SelectedPrompt.IsNullOrWhiteSpace() ? "`Not yet selected.`" : $"{SelectedPrompt.Sanitize()}")}\n" +
                                                                      $"`Available Options`: {(!SelectedOptions.IsNotNullAndNotEmpty() ? "`No options set.`" : $"{string.Join(", ", SelectedOptions.Select(x => $"`{x.Label.TruncateWithIndication(10)}`"))}")}\n\n" +
                                                                      $"`Role to mention  `: {(SelectedRole is null ? "`No Role selected`" : SelectedRole.Mention)}\n" +
                                                                      $"`Selected Channel `: {(SelectedChannel is null ? "`Not yet selected.`" : SelectedChannel.Mention)}")
                                                     .SetAwaitingInput(ctx);

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
                    .AddComponents(SelectPromptButton, AddOptionButton, RemoveOptionButton)
                    .AddComponents(SelectRoleButton, SelectChannelButton, Finish)
                    .AddComponents(MessageComponents.CancelButton));

                var Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                if (Menu.Result.GetCustomId() == SelectRoleButton.CustomId)
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
                            await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any roles in your server.`"));
                            await Task.Delay(3000);
                            continue;
                        }

                        throw RoleResult.Exception;
                    }

                    SelectedRole = RoleResult.Result;
                    continue;
                }
                else if (Menu.Result.GetCustomId() == SelectChannelButton.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    var ChannelResult = await PromptChannelSelection(ChannelType.Text);

                    if (ChannelResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (ChannelResult.Cancelled)
                    {
                        continue;
                    }
                    else if (ChannelResult.Failed)
                    {
                        if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any text channels in your server.`"));
                            await Task.Delay(3000);
                            continue;
                        }

                        throw ChannelResult.Exception;
                    }

                    SelectedChannel = ChannelResult.Result;
                    continue;
                }
                else if (Menu.Result.GetCustomId() == SelectPromptButton.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder("New Poll", Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "prompt", "Poll Content", "", 1, 256, true));

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

                    SelectedPrompt = ModalResult.Result.Interaction.GetModalValueByCustomId("prompt").Truncate(256);
                    continue;
                }
                else if (Menu.Result.GetCustomId() == AddOptionButton.CustomId)
                {
                    var modal = new DiscordInteractionModalBuilder("New Poll", Guid.NewGuid().ToString())
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "", 1, 20, true))
                        .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "description", "Description", "", 1, 256, true));

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

                    var title = ModalResult.Result.Interaction.GetModalValueByCustomId("title").Truncate(20);
                    var desc = ModalResult.Result.Interaction.GetModalValueByCustomId("description").Truncate(256);

                    var hash = (title + desc).GetSHA256();

                    if (SelectedOptions.Any(x => x.Value == hash || x.Label.ToLower() == title.ToLower()))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`This option has already been added.`"));
                        await Task.Delay(3000);
                        continue;
                    }

                    SelectedOptions.Add(new DiscordSelectComponentOption(title, hash, desc));
                    continue;
                }
                else if (Menu.Result.GetCustomId() == RemoveOptionButton.CustomId)
                {
                    _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    var SelectionResult = await PromptCustomSelection(SelectedOptions);

                    if (SelectionResult.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }
                    else if (SelectionResult.Cancelled)
                    {
                        continue;
                    }
                    else if (SelectionResult.Errored)
                    {
                        throw SelectionResult.Exception;
                    }

                    SelectedOptions = SelectedOptions.Where(x => x.Value != SelectionResult.Result).ToList();
                    continue;
                }
            }
        });
    }
}