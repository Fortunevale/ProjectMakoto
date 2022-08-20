namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class RemoveAllCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
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
                Description = "`Removing all reaction roles..`"
            }.SetLoading(ctx, "Reaction Roles");

            await RespondOrEdit(embed);
            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Any(x => x.Key == message.Id))
            {
                embed.Description = $"`The specified message doesn't contain any reaction roles.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Reaction Roles")));
                return;
            }

            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Where(x => x.Key == message.Id).ToList())
                ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(b);

            _ = message.DeleteAllReactionsAsync();

            embed.Description = $"`Removed all reaction roles from message sent by` {message.Author.Mention} `in` {message.Channel.Mention} `.`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetSuccess(ctx, "Reaction Roles")));
        });
    }
}