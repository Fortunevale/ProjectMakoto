namespace ProjectIchigo.Commands;
internal abstract class BaseCommand
{
    internal SharedCommandContext Context { private get; set; }

    public abstract Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments);

    public async Task ExecuteCommand(CommandContext ctx, Bot _bot, Dictionary<string, object> arguments)
    {
        Context = new SharedCommandContext(this, ctx, _bot);
        await ExecuteCommand(Context, arguments);
    }
    
    public async Task ExecuteCommand(InteractionContext ctx, Bot _bot, Dictionary<string, object> arguments)
    {
        Context = new SharedCommandContext(this, ctx, _bot);
        await ExecuteCommand(Context, arguments);
    }

    internal async Task<DiscordMessage> RespondOrEdit(DiscordMessageBuilder discordMessageBuilder)
    {
        switch (Context.CommandType)
        {
            case Enums.CommandType.ApplicationCommand:
            {
                DiscordWebhookBuilder discordWebhookBuilder = new DiscordWebhookBuilder();

                discordWebhookBuilder.AddComponents(discordMessageBuilder.Components);
                discordWebhookBuilder.AddEmbeds(discordMessageBuilder.Embeds);
                discordWebhookBuilder.Content = discordMessageBuilder.Content;

                return await Context.OriginalInteractionContext.EditResponseAsync(discordWebhookBuilder);
            }

            case Enums.CommandType.PrefixCommand:
            {
                if (Context.ResponseMessage is not null)
                {
                    await Context.ResponseMessage.ModifyAsync(discordMessageBuilder);
                    Context.ResponseMessage = await Context.ResponseMessage.Refresh();

                    return Context.ResponseMessage;
                }

                var msg = await Context.Channel.SendMessageAsync(discordMessageBuilder);

                Context.ResponseMessage = msg;

                return msg;
            }
            
            case Enums.CommandType.Custom:
            {
                if (Context.ResponseMessage is not null)
                {
                    await Context.ResponseMessage.ModifyAsync(discordMessageBuilder);
                    Context.ResponseMessage = await Context.ResponseMessage.Refresh();

                    return Context.ResponseMessage;
                }

                var msg = await Context.Channel.SendMessageAsync(discordMessageBuilder);

                Context.ResponseMessage = msg;

                return msg;
            }
        }

        throw new NotImplementedException();
    }
}
