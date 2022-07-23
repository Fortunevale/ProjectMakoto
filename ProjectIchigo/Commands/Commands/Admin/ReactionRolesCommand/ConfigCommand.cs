namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                Color = EmbedColors.Loading,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = "`Loading Reaction Roles..`"
            });

            await ReactionRolesCommandAbstractions.CheckForInvalid(ctx);

            var AddButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Add a new reaction role", (ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Count > 100), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var RemoveButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove a reaction role", (ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Count == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`{ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Count} reaction roles are set up.`"
            };

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                    AddButton, RemoveButton
            })
            .AddComponents(Resources.CancelButton));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == AddButton.CustomId)
            {
                var action_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.AwaitingInput,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = "`Please copy and send the message link of the message you want the reaction role to be added to.`",
                    ImageUrl = "https://cdn.discordapp.com/attachments/906976602557145110/967753175241203712/unknown.png"
                };

                if (ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Count > 100)
                {
                    action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                    action_embed.Color = EmbedColors.Error;
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));

                var link = await ctx.Client.GetInteractivity().WaitForMessageAsync(x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.User.Id, TimeSpan.FromMinutes(2));

                if (link.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                try
                { _ = link.Result.DeleteAsync(); }
                catch { }

                if (!Regex.IsMatch(link.Result.Content, Resources.Regex.DiscordChannelUrl))
                {
                    action_embed.Description = $"`This doesn't look correct. A message url should look something like these:`\n" +
                                               $"`http://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                               $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                               $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                               $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                    action_embed.Color = EmbedColors.Error;
                    action_embed.ImageUrl = "";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (!link.Result.Content.TryParseMessageLink(out ulong GuildId, out ulong ChannelId, out ulong MessageId))
                {
                    action_embed.Description = $"`This doesn't look correct. A message url should look something like these:`\n" +
                                               $"`http://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                               $"`https://discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                               $"`https://ptb.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`\n" +
                                               $"`https://canary.discord.com/channels/012345678901234567/012345678901234567/012345678912345678`";
                    action_embed.Color = EmbedColors.Error;
                    action_embed.ImageUrl = "";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (GuildId != ctx.Guild.Id)
                {
                    action_embed.Description = $"`The link you provided leads to another server.`";
                    action_embed.Color = EmbedColors.Error;
                    action_embed.ImageUrl = "";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (!ctx.Guild.Channels.ContainsKey(ChannelId))
                {
                    action_embed.Description = $"`The link you provided leads to a channel that doesn't exist.`";
                    action_embed.Color = EmbedColors.Error;
                    action_embed.ImageUrl = "";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                var channel = ctx.Guild.GetChannel(ChannelId);

                if (!channel.TryGetMessage(MessageId, out DiscordMessage reactionMessage))
                {
                    action_embed.Description = $"`The link you provided leads a message that doesn't exist or the bot has no access to.`";
                    action_embed.Color = EmbedColors.Error;
                    action_embed.ImageUrl = "";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                action_embed.Description = "`Please react with the emoji you want to use for the reaction role to the target message.`";
                action_embed.ImageUrl = "";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));

                var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == reactionMessage.Id, TimeSpan.FromMinutes(2));

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
                    action_embed.Color = EmbedColors.Error;
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                action_embed.Description = "`Please select the role you want to use.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));

                try
                {
                    var role = await PromptRoleSelection();

                    if (ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Count > 100)
                    {
                        action_embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                        action_embed.Color = EmbedColors.Error;
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }


                    if (ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Any(x => (x.Key == MessageId && x.Value.EmojiName == emoji.GetUniqueDiscordName())))
                    {
                        action_embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                        action_embed.Color = EmbedColors.Error;
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    if (ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Any(x => x.Value.RoleId == role.Id))
                    {
                        action_embed.Description = $"`The specified role is already being used in another reaction role.`";
                        action_embed.Color = EmbedColors.Error;
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                        await Task.Delay(5000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Add(new KeyValuePair<ulong, Entities.ReactionRoles>(reactionMessage.Id, new Entities.ReactionRoles
                    {
                        ChannelId = ChannelId,
                        RoleId = role.Id,
                        EmojiId = emoji.Id,
                        EmojiName = emoji.GetUniqueDiscordName()
                    }));

                    await reactionMessage.CreateReactionAsync(emoji);

                    action_embed.Color = EmbedColors.Info;
                    action_embed.Description = $"`Added role` {role.Mention} `to message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {emoji} `.`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(action_embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }
            }
            else if (e.Result.Interaction.Data.CustomId == RemoveButton.CustomId)
            {
                try
                {
                    var roleuuid = await PromptCustomSelection(ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles
                                                    .Select(x => new DiscordSelectComponentOption($"@{ctx.Guild.GetRole(x.Value.RoleId).Name}", x.Value.UUID, $"in Channel #{ctx.Guild.GetChannel(x.Value.ChannelId).Name}", emoji: new DiscordComponentEmoji(x.Value.GetEmoji(ctx.Client)))).ToList());

                    var obj = ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.First(x => x.Value.UUID == roleuuid);

                    var role = ctx.Guild.GetRole(obj.Value.RoleId);
                    var channel = ctx.Guild.GetChannel(obj.Value.ChannelId);
                    var reactionMessage = await channel.GetMessageAsync(obj.Key);
                    _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

                    ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(obj);

                    embed.Color = EmbedColors.Info;
                    embed.Description = $"`Removed role` {role.Mention} `from message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {obj.Value.GetEmoji(ctx.Client)} `.`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    return;
                }
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}