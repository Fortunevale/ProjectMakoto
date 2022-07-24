namespace ProjectIchigo.Commands.LevelRewardsCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            int CurrentPage = 0;

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Level Rewards • {ctx.Guild.Name}" },
                Color = EmbedColors.Loading,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`Loading Level Rewards..`"
            };

            await RespondOrEdit(embed);
            embed.Author.IconUrl = ctx.Guild.IconUrl;
            embed.Color = EmbedColors.AwaitingInput;

            string selected = "";

            async Task RefreshMessage()
            {
                List<DiscordSelectComponentOption> DefinedRewards = new();

                embed.Description = "";

                foreach (var reward in ctx.Bot._guilds[ctx.Guild.Id].LevelRewards.ToList().OrderBy(x => x.Level))
                {
                    if (!ctx.Guild.Roles.ContainsKey(reward.RoleId))
                    {
                        ctx.Bot._guilds[ctx.Guild.Id].LevelRewards.Remove(reward);
                        continue;
                    }

                    var role = ctx.Guild.GetRole(reward.RoleId);

                    DefinedRewards.Add(new DiscordSelectComponentOption($"Level {reward.Level}: @{role.Name}", role.Id.ToString(), $"{reward.Message}", (selected == role.Id.ToString()), new DiscordComponentEmoji(role.Color.GetClosestColorEmoji(ctx.Client))));

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
                var Delete = new DiscordButtonComponent(ButtonStyle.Danger, "Delete", "Delete", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));

                var Dropdown = new DiscordSelectComponent("Select a Level Reward..", DefinedRewards.Skip(CurrentPage * 20).Take(20).ToList(), "RewardSelection");
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

                builder.AddComponents(Resources.CancelButton);

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

                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == "RewardSelection")
                        {
                            selected = e.Values.First();
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == "Add")
                        {
                            ctx.Client.ComponentInteractionCreated -= SelectInteraction;
                            embed.Description = $"`Select a role to assign.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                            DiscordRole role;

                            try
                            {
                                role = await PromptRoleSelection();
                            }
                            catch (ArgumentException)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }

                            if (ctx.Bot._guilds[ctx.Guild.Id].LevelRewards.Any(x => x.RoleId == role.Id))
                            {
                                embed.Description = "`The role you're trying to add has already been assigned to a level.`";
                                embed.Color = EmbedColors.Error;
                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                await RefreshMessage();
                                ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                return;
                            }

                            embed.Description = $"`Selected` <@&{role.Id}> `({role.Id}). At what Level should this role be assigned?`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                            var LevelResult = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id);

                            if (LevelResult.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }

                            int level;

                            try
                            {
                                level = Convert.ToInt32(LevelResult.Result.Content);
                            }
                            catch (Exception)
                            {
                                embed.Description = "`You must specify a valid level.`";
                                embed.Color = EmbedColors.Error;
                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                await RefreshMessage();
                                ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                return;
                            }

                            _ = LevelResult.Result.DeleteAsync();

                            string Message = "";

                            embed.Description = $"`Selected` <@&{role.Id}> `({role.Id}). It will be assigned at Level {level}. Please type out a custom message or send 'cancel', 'continue' or '.' to use the default message. (<256 characters)`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                            var CustomMessageResult = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id, TimeSpan.FromMinutes(5));

                            if (CustomMessageResult.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }

                            _ = CustomMessageResult.Result.DeleteAsync();

                            if (CustomMessageResult.Result.Content.Length > 256)
                            {
                                embed.Description = "`Your custom message can't contain more than 256 characters.`";
                                embed.Color = EmbedColors.Error;
                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                await RefreshMessage();
                                ctx.Client.ComponentInteractionCreated += SelectInteraction;
                                return;
                            }

                            if (CustomMessageResult.Result.Content is not "cancel" and not "continue" and not ".")
                                Message = CustomMessageResult.Result.Content;

                            ctx.Bot._guilds[ctx.Guild.Id].LevelRewards.Add(new Entities.LevelReward
                            {
                                Level = level,
                                RoleId = role.Id,
                                Message = (string.IsNullOrEmpty(Message) ? "You received ##Role##!" : Message)
                            });

                            embed.Description = $"`The role` <@&{role.Id}> `({role.Id}) will be assigned at Level {level}.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                            await Task.Delay(5000);
                            await RefreshMessage();
                            ctx.Client.ComponentInteractionCreated += SelectInteraction;
                        }
                        else if (e.Interaction.Data.CustomId == "Modify")
                        {
                            embed.Description = $"{embed.Description}\n\n`Please type out your new custom message (<256 characters). Type 'cancel' to cancel.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                            var result = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id, TimeSpan.FromMinutes(5));

                            if (result.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }

                            _ = result.Result.DeleteAsync();

                            if (result.Result.Content.Length > 256)
                            {
                                embed.Description = "`Your custom message can't contain more than 256 characters.`";
                                embed.Color = EmbedColors.Error;
                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                await ExecuteCommand(ctx, arguments);
                                return;
                            }

                            if (result.Result.Content.ToLower() != "cancel")
                            {
                                ctx.Bot._guilds[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)).Message = result.Result.Content;
                            }

                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == "Delete")
                        {
                            ctx.Bot._guilds[ctx.Guild.Id].LevelRewards.Remove(ctx.Bot._guilds[ctx.Guild.Id].LevelRewards.First(x => x.RoleId == Convert.ToUInt64(selected)));

                            if (ctx.Bot._guilds[ctx.Guild.Id].LevelRewards.Count == 0)
                            {
                                embed.Description = $"`There are no more Level Rewards to display.`";
                                embed.Color = EmbedColors.Success;
                                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                await ExecuteCommand(ctx, arguments);
                                return;
                            }

                            embed.Description = $"`Select a Level Reward to modify.`";
                            selected = "";

                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == "PreviousPage")
                        {
                            CurrentPage--;
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == "NextPage")
                        {
                            CurrentPage++;
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                        {
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }
                    }
                }).Add(ctx.Bot._watcher, ctx);
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