namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class AddCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordMessage message;

            var embed = new DiscordEmbedBuilder
            {
                Description = "`Adding reaction role..`"
            }.SetLoading(ctx, "Reaction Roles");

            await RespondOrEdit(embed);

            DiscordRole role_parameter;
            DiscordEmoji emoji_parameter;

            if (arguments?.ContainsKey("message") ?? false)
            {
                message = (DiscordMessage)arguments["message"];
            }
            else
            {
                switch (ctx.CommandType)
                {
                    case Enums.CommandType.PrefixCommand:
                    {
                        if (ctx.OriginalCommandContext.Message.ReferencedMessage is not null)
                        {
                            message = ctx.OriginalCommandContext.Message.ReferencedMessage;
                        }
                        else
                        {
                            SendSyntaxError();
                            return;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Message expected");
                }
            }

            if (message is null)
            {
                SendSyntaxError();
                return;
            }

            if (arguments?.ContainsKey("role_parameter") ?? false)
            {
                role_parameter = (DiscordRole)arguments["role_parameter"];
            }
            else
            {
                switch (ctx.CommandType)
                {
                    case Enums.CommandType.ContextMenu:
                    {
                        try
                        {
                            embed.Description = $"`Please select the role you want this reaction role to assign below.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetAwaitingInput(ctx, "Reaction Roles")));
                            role_parameter = await PromptRoleSelection();
                        }
                        catch (ArgumentException)
                        {
                            ModifyToTimedOut();
                            return;
                        }
                        catch (NullReferenceException)
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any roles in your server.`"));
                            await Task.Delay(3000);
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }
                        catch (CancelException)
                        {
                            DeleteOrInvalidate();
                            return;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Interaction expected");
                }
            }
            
            if (arguments?.ContainsKey("emoji_parameter") ?? false)
            {
                emoji_parameter = (DiscordEmoji)arguments["emoji_parameter"];
            }
            else
            {
                switch (ctx.CommandType)
                {
                    case Enums.CommandType.ContextMenu:
                    {
                        embed.Description = $"`Please react with the emoji you want to use to the target message.`";
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetAwaitingInput(ctx, "Reaction Roles")));

                        var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == message.Id, TimeSpan.FromMinutes(2));

                        if (emoji_wait.TimedOut)
                        {
                            ModifyToTimedOut();
                            return;
                        }

                        try
                        { _ = emoji_wait.Result.Message.DeleteReactionAsync(emoji_wait.Result.Emoji, ctx.User); }
                        catch { }

                        emoji_parameter = emoji_wait.Result.Emoji;

                        if (emoji_parameter.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji_parameter.Id))
                        {
                            embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Reaction Roles")));
                            return;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Interaction expected");
                }
            }

            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Count > 100)
            {
                embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Reaction Roles")));
                return;
            }

            if (emoji_parameter.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji_parameter.Id))
            {
                embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Reaction Roles")));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Any(x => (x.Key == message.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName())))
            {
                embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Reaction Roles")));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Any(x => x.Value.RoleId == role_parameter.Id))
            {
                embed.Description = $"`The specified role is already being used in another reaction role.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Reaction Roles")));
                return;
            }

            await message.CreateReactionAsync(emoji_parameter);

            ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Add(new KeyValuePair<ulong, Entities.ReactionRoleEntry>(message.Id, new Entities.ReactionRoleEntry
            {
                ChannelId = message.Channel.Id,
                RoleId = role_parameter.Id,
                EmojiId = emoji_parameter.Id,
                EmojiName = emoji_parameter.GetUniqueDiscordName()
            }));

            embed.Description = $"`Added role` {role_parameter.Mention} `to message sent by` {message.Author.Mention} `in` {message.Channel.Mention} `with emoji` {emoji_parameter} `.`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetSuccess(ctx, "Reaction Roles")));
        });
    }
}