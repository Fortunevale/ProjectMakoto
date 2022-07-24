namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class RemoveAllCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordMessage message;

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

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                Color = EmbedColors.AwaitingInput,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = "`Removing all reaction roles..`"
            };

            await RespondOrEdit(embed);
            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Any(x => x.Key == message.Id))
            {
                embed.Description = $"`The specified message doesn't contain any reaction roles.`";
                embed.Color = EmbedColors.Error;
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                return;
            }

            foreach (var b in ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Where(x => x.Key == message.Id).ToList())
                ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(b);

            _ = message.DeleteAllReactionsAsync();

            embed.Color = EmbedColors.Info;
            embed.Description = $"`Removed all reaction roles from message sent by` {message.Author.Mention} `in` {message.Channel.Mention} `.`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        });
    }
}