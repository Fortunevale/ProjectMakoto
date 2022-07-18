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
    
    public async Task ExecuteCommand(InteractionContext ctx, Bot _bot, Dictionary<string, object> arguments = null, bool Ephemeral = true, bool InitiateInteraction = true)
    {
        if (InitiateInteraction)
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = Ephemeral
            });

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

                var msg = await Context.OriginalInteractionContext.EditResponseAsync(discordWebhookBuilder);
                Context.ResponseMessage = msg;
                return msg;
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

    public void ModifyToTimedOut(bool Delete = false)
    {
        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(Context.ResponseMessage.Embeds[0]).WithFooter(Context.ResponseMessage.Embeds[0].Footer.Text + " • Interaction timed out")));

        if (Delete)
            Task.Delay(5000).ContinueWith(_ =>
            {
                Context.ResponseMessage.DeleteAsync();
            });
    }

    public void DeleteOrInvalidate()
    {
        switch (Context.CommandType)
        {
            case Enums.CommandType.ApplicationCommand:
                _ = RespondOrEdit("❌ `Cancelled`");
                _ = Context.ResponseMessage.DeleteAsync();
                break;
            default:
                _ = Context.ResponseMessage.DeleteAsync();
                break;
        }
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
    
    public void SendSourceError(Enums.CommandType commandType)
    {
        _ = commandType switch
        {
            Enums.CommandType.ApplicationCommand => RespondOrEdit(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = Context.Guild.Name
                },
                Description = $"This command is exclusive to application commands.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{Context.User.UsernameWithDiscriminator} attempted to use \"{Context.Prefix}{Context.CommandName}\"",
                    IconUrl = Context.User.AvatarUrl
                },
                Timestamp = DateTime.UtcNow,
                Color = EmbedColors.Error
            }),
            Enums.CommandType.PrefixCommand => RespondOrEdit(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = Context.Guild.Name
                },
                Description = $"This command is exclusive to prefixed commands.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{Context.User.UsernameWithDiscriminator} attempted to use \"{Context.Prefix}{Context.CommandName}\"",
                    IconUrl = Context.User.AvatarUrl
                },
                Timestamp = DateTime.UtcNow,
                Color = EmbedColors.Error
            }),
            _ => throw new ArgumentException("Invalid Source defined."),
        };
    }

    public void SendSyntaxError()
    {
        if (Context.CommandType != Enums.CommandType.PrefixCommand)
            throw new ArgumentException("Syntax Error cannot be generated for Application Commands.");

        var ctx = Context.OriginalCommandContext;

        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = Context.Guild.IconUrl,
                Name = Context.Guild.Name
            },
            Description = $"**`{ctx.Prefix}{ctx.Command.Name}{(ctx.RawArgumentString != "" ? $" {ctx.RawArgumentString.SanitizeForCodeBlock().Replace("\\", "")}" : "")}` is not a valid way of using this command.**\nUse it like this instead: `{ctx.Prefix}{ctx.Command.GenerateUsage()}`\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
            Footer = ctx.GenerateUsedByFooter(),
            Timestamp = DateTime.UtcNow,
            Color = EmbedColors.Error
        };

        if (ctx.Client.GetCommandsNext()
            .RegisteredCommands[ctx.Command.Name].Overloads[0].Arguments[0].Type.Name is "DiscordUser" or "DiscordMember")
            embed.Description += "\n\n_Tip: Make sure you copied the user id and not a server, channel or message id._";

        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).WithContent(Context.User.Mention));
    }

    public async Task<bool> CheckMaintenance()
    {
        if (!Context.User.IsMaintenance(Context.Bot._status))
        {
            SendMaintenanceError();
            return false;
        }

        return true;
    }
    
    public async Task<bool> CheckAdmin()
    {
        if (!Context.Member.IsAdmin(Context.Bot._status))
        {
            SendMaintenanceError();
            return false;
        }

        return true;
    }
    
    public async Task<bool> CheckPerm(Permissions perms)
    {
        if (!Context.Member.Permissions.HasPermission(Permissions.ManageMessages))
        {
            SendPermissionError(perms);
            return false;
        }

        return true;
    }
    
    public async Task<bool> CheckSource(Enums.CommandType commandType)
    {
        if (Context.CommandType != commandType)
        {
            SendSourceError(commandType);
            return false;
        }

        return true;
    }
}
