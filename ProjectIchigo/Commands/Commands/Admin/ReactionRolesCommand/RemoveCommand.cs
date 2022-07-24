namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class RemoveCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordMessage message;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                Color = EmbedColors.AwaitingInput,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = "`Removing reaction role..`"
            };

            await RespondOrEdit(embed);

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
                        embed.Description = $"`Please react with the emoji you want to remove from the target message.`";
                        embed.Color = EmbedColors.AwaitingInput;
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                        var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == message.Id, TimeSpan.FromMinutes(2));

                        if (emoji_wait.TimedOut)
                        {
                            ModifyToTimedOut();
                            return;
                        }

                        emoji_parameter = emoji_wait.Result.Emoji;
                        break;
                    }
                    default:
                        throw new ArgumentException("Interaction expected");
                }
            }

            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Any(x => x.Key == message.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName()))
            {
                embed.Description = $"`The specified message doesn't contain specified reaction.`";
                embed.Color = EmbedColors.Error;
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                return;
            }

            var obj = ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.First(x => x.Key == message.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName());

            var role = ctx.Guild.GetRole(obj.Value.RoleId);
            var channel = ctx.Guild.GetChannel(obj.Value.ChannelId);
            var reactionMessage = await channel.GetMessageAsync(obj.Key);
            _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

            ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(obj);

            embed.Color = EmbedColors.Info;
            embed.Description = $"`Removed role` {role.Mention} `from message sent by` {reactionMessage.Author.Mention} `in` {reactionMessage.Channel.Mention} `with emoji` {obj.Value.GetEmoji(ctx.Client)} `.`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        });
    }
}