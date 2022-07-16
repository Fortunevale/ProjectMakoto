namespace ProjectIchigo.Commands;
internal abstract class BaseCommand
{
    internal SharedCommandContext Context { private get; set; }

    public virtual async Task<bool> BeforeExecution(SharedCommandContext ctx)
    {
        return true;
    }

    public abstract Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments = null);

    public async Task ExecuteCommand(CommandContext ctx, Bot _bot, Dictionary<string, object> arguments = null)
    {
        Context = new SharedCommandContext(this, ctx, _bot);

        if (!(await BeforeExecution(Context)))
            return;

        await ExecuteCommand(Context, arguments);
    }
    
    public async Task ExecuteCommand(InteractionContext ctx, Bot _bot, Dictionary<string, object> arguments = null)
    {
        Context = new SharedCommandContext(this, ctx, _bot);

        if (!(await BeforeExecution(Context)))
            return;

        await ExecuteCommand(Context, arguments);
    }

    internal async Task<DiscordMessage> RespondOrEdit(DiscordEmbed embed) => await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

    internal async Task<DiscordMessage> RespondOrEdit(DiscordEmbedBuilder embed) => await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.Build()));

    internal async Task<DiscordMessage> RespondOrEdit(string content) => await RespondOrEdit(new DiscordMessageBuilder().WithContent(content));

    internal async Task<DiscordMessage> RespondOrEdit(DiscordMessageBuilder discordMessageBuilder)
    {
        switch (Context.CommandType)
        {
            case Enums.CommandType.ApplicationCommand:
            {
                DiscordWebhookBuilder discordWebhookBuilder = new();

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

    public void SendMaintenanceError()
    {
        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = Context.Guild.IconUrl,
                Name = Context.Guild.Name
            },
            Description = "You dont have permissions to use this command. You need to be <@411950662662881290> to use this command.",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{Context.User.UsernameWithDiscriminator} attempted to use \"{Context.Prefix}{Context.CommandName}\"",
                IconUrl = Context.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = EmbedColors.Error
        });
    }

    public void SendAdminError()
    {
        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = Context.Guild.IconUrl,
                Name = Context.Guild.Name
            },
            Description = $"You dont have permissions to use this command. You need to be `Admin` to use this command.",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{Context.User.UsernameWithDiscriminator} attempted to use \"{Context.Prefix}{Context.CommandName}\"",
                IconUrl = Context.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = EmbedColors.Error
        });
    }
    
    public void SendModError()
    {
        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = Context.Guild.IconUrl,
                Name = Context.Guild.Name
            },
            Description = $"You dont have permissions to use this command. You need to be `Mod` to use this command.",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{Context.User.UsernameWithDiscriminator} attempted to use \"{Context.Prefix}{Context.CommandName}\"",
                IconUrl = Context.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = EmbedColors.Error
        });
    }
    
    public void SendPermissionError(Permissions perms)
    {
        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = Context.Guild.IconUrl,
                Name = Context.Guild.Name
            },
            Description = $"You dont have permissions to use this command. You need to be `{perms.ToPermissionString()}` to use this command.",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{Context.User.UsernameWithDiscriminator} attempted to use \"{Context.Prefix}{Context.CommandName}\"",
                IconUrl = Context.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = EmbedColors.Error
        });
    }
}
