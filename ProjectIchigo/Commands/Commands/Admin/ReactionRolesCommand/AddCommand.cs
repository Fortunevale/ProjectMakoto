namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class AddCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordMessage message;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                Color = EmbedColors.AwaitingInput,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = "`Adding reaction role..`"
            };

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
                            embed.Color = EmbedColors.AwaitingInput;
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                            role_parameter = await PromptRoleSelection();
                        }
                        catch (ArgumentException)
                        {
                            ModifyToTimedOut();
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
                        embed.Color = EmbedColors.AwaitingInput;
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

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
                            embed.Color = EmbedColors.Error;
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                            return;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Interaction expected");
                }
            }

            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (ctx.Bot._guilds.List[ctx.Guild.Id].ReactionRoles.Count > 100)
            {
                embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                embed.Color = EmbedColors.Error;
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                return;
            }

            if (emoji_parameter.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji_parameter.Id))
            {
                embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                embed.Color = EmbedColors.Error;
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                return;
            }

            if (ctx.Bot._guilds.List[ctx.Guild.Id].ReactionRoles.Any(x => (x.Key == message.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName())))
            {
                embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                embed.Color = EmbedColors.Error;
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                return;
            }

            if (ctx.Bot._guilds.List[ctx.Guild.Id].ReactionRoles.Any(x => x.Value.RoleId == role_parameter.Id))
            {
                embed.Description = $"`The specified role is already being used in another reaction role.`";
                embed.Color = EmbedColors.Error;
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                return;
            }

            await message.CreateReactionAsync(emoji_parameter);

            ctx.Bot._guilds.List[ctx.Guild.Id].ReactionRoles.Add(new KeyValuePair<ulong, Entities.ReactionRoles>(message.Id, new Entities.ReactionRoles
            {
                ChannelId = message.Channel.Id,
                RoleId = role_parameter.Id,
                EmojiId = emoji_parameter.Id,
                EmojiName = emoji_parameter.GetUniqueDiscordName()
            }));

            embed.Color = EmbedColors.Info;
            embed.Description = $"`Added role` {role_parameter.Mention} `to message sent by` {message.Author.Mention} `in` {message.Channel.Mention} `with emoji` {emoji_parameter} `.`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        });
    }
}