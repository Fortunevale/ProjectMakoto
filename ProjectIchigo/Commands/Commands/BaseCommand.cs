namespace ProjectIchigo.Commands;
public abstract class BaseCommand
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

        if (!(await CheckOwnPermissions(Permissions.SendMessages)))
            return;

        if (!(await CheckOwnPermissions(Permissions.EmbedLinks)))
            return;

        if (!(await CheckOwnPermissions(Permissions.AddReactions)))
            return;

        if (!(await CheckOwnPermissions(Permissions.AccessChannels)))
            return;

        if (!(await CheckOwnPermissions(Permissions.AttachFiles)))
            return;

        if (!(await CheckOwnPermissions(Permissions.ManageMessages)))
            return;

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

        if (!(await CheckOwnPermissions(Permissions.SendMessages)))
            return;
        
        if (!(await CheckOwnPermissions(Permissions.EmbedLinks)))
            return;
        
        if (!(await CheckOwnPermissions(Permissions.AddReactions)))
            return;
        
        if (!(await CheckOwnPermissions(Permissions.AccessChannels)))
            return;
        
        if (!(await CheckOwnPermissions(Permissions.AttachFiles)))
            return;
        
        if (!(await CheckOwnPermissions(Permissions.ManageMessages)))
            return;
        
        if (!(await BeforeExecution(Context)))
            return;

        await ExecuteCommand(Context, arguments);
    }
    
    public async Task ExecuteCommand(ContextMenuContext ctx, Bot _bot, Dictionary<string, object> arguments = null, bool Ephemeral = true, bool InitiateInteraction = true)
    {
        if (InitiateInteraction)
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = Ephemeral
            });

        Context = new SharedCommandContext(this, ctx, _bot);

        if (!(await CheckOwnPermissions(Permissions.SendMessages)))
            return;

        if (!(await CheckOwnPermissions(Permissions.EmbedLinks)))
            return;

        if (!(await CheckOwnPermissions(Permissions.AddReactions)))
            return;

        if (!(await CheckOwnPermissions(Permissions.AccessChannels)))
            return;

        if (!(await CheckOwnPermissions(Permissions.AttachFiles)))
            return;

        if (!(await CheckOwnPermissions(Permissions.ManageMessages)))
            return;

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

                var files = new Dictionary<string, Stream>();

                foreach (var b in discordMessageBuilder.Files)
                    files.Add(b.FileName, b.Stream);

                discordWebhookBuilder.AddComponents(discordMessageBuilder.Components);
                discordWebhookBuilder.AddEmbeds(discordMessageBuilder.Embeds);
                discordWebhookBuilder.AddFiles(files);
                discordWebhookBuilder.Content = discordMessageBuilder.Content;

                var msg = await Context.OriginalInteractionContext.EditResponseAsync(discordWebhookBuilder);
                Context.ResponseMessage = msg;
                return msg;
            }
            
            case Enums.CommandType.ContextMenu:
            {
                DiscordWebhookBuilder discordWebhookBuilder = new();

                var files = new Dictionary<string, Stream>();

                foreach (var b in discordMessageBuilder.Files)
                    files.Add(b.FileName, b.Stream);

                discordWebhookBuilder.AddComponents(discordMessageBuilder.Components);
                discordWebhookBuilder.AddEmbeds(discordMessageBuilder.Embeds);
                discordWebhookBuilder.AddFiles(files);
                discordWebhookBuilder.Content = discordMessageBuilder.Content;

                var msg = await Context.OriginalContextMenuContext.EditResponseAsync(discordWebhookBuilder);
                Context.ResponseMessage = msg;
                return msg;
            }

            case Enums.CommandType.PrefixCommand:
            {
                if (Context.ResponseMessage is not null)
                {
                    if (discordMessageBuilder.Files?.Any() ?? false)
                    {
                        await Context.ResponseMessage.DeleteAsync();
                        var msg1 = await Context.Channel.SendMessageAsync(discordMessageBuilder);
                        Context.ResponseMessage = msg1;
                        return Context.ResponseMessage;
                    }

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
                    if (discordMessageBuilder.Files?.Any() ?? false)
                    {
                        await Context.ResponseMessage.DeleteAsync();
                        var msg1 = await Context.Channel.SendMessageAsync(discordMessageBuilder);
                        Context.ResponseMessage = msg1;
                        return Context.ResponseMessage;
                    }

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

    internal async Task<DiscordRole> PromptRoleSelection(bool IncludeCreateForMe = false, string CreateForMeName = "Role", bool IncludeDisable = false, string DisableString = "Disable")
    {
        List<DiscordSelectComponentOption> roles = new();

        if (IncludeCreateForMe)
            roles.Add(new DiscordSelectComponentOption($"Create one for me..", "create_for_me", "", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕"))));

        if (IncludeDisable)
            roles.Add(new DiscordSelectComponentOption(DisableString, "disable", "", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌"))));

        foreach (var role in Context.Guild.Roles.OrderByDescending(x => x.Value.Position))
        {
            if (Context.CurrentMember.GetHighestPosition() > role.Value.Position && Context.Member.GetHighestPosition() > role.Value.Position && !role.Value.IsManaged && role.Value.Id != Context.Guild.EveryoneRole.Id)
                roles.Add(new DiscordSelectComponentOption($"@{role.Value.Name} ({role.Value.Id})", role.Value.Id.ToString(), "", false, new DiscordComponentEmoji(role.Value.Color.GetClosestColorEmoji(Context.Client))));
        }

        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();
        string NextPageId = Guid.NewGuid().ToString();
        string PrevPageId = Guid.NewGuid().ToString();

        DiscordRole Role = null;

        bool FinishedSelection = false;
        bool ExceptionOccured = false;
        Exception exception = null;

        async Task RefreshRoleList()
        {
            var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
            var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

            var dropdown = new DiscordSelectComponent("Select a role..", roles.Skip(currentPage * 25).Take(25), SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(Context.ResponseMessage.Embeds[0]).WithColor(EmbedColors.AwaitingInput)).AddComponents(dropdown).WithContent(Context.ResponseMessage.Content);

            if (roles.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            await RespondOrEdit(builder);
        }

        _ = RefreshRoleList();

        int TimeoutSeconds = 60;

        async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == Context.ResponseMessage.Id && e.User.Id == Context.User.Id)
                    {
                        TimeoutSeconds = 60;
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            Context.Client.ComponentInteractionCreated -= RunDropdownInteraction;

                            if (e.Values.First() is "create_for_me")
                                Role = await Context.Guild.CreateRoleAsync(CreateForMeName);
                            else if (e.Values.First() is "disable")
                                Role = null;
                            else
                                Role = Context.Guild.GetRole(Convert.ToUInt64(e.Values.First()));


                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == PrevPageId)
                        {
                            currentPage--;
                            await RefreshRoleList();
                        }
                        else if (e.Interaction.Data.CustomId == NextPageId)
                        {
                            currentPage++;
                            await RefreshRoleList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    ExceptionOccured = true;
                    FinishedSelection = true;
                    throw;
                }
            }).Add(Context.Bot._watcher, Context);
        }

        Context.Client.ComponentInteractionCreated += RunDropdownInteraction;

        while (!FinishedSelection && TimeoutSeconds >= 0)
        {
            await Task.Delay(1000);
            TimeoutSeconds--;
        }

        Context.Client.ComponentInteractionCreated -= RunDropdownInteraction;

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(Context.ResponseMessage.Embeds[0]).WithContent(Context.ResponseMessage.Content));

        if (ExceptionOccured)
            throw exception;

        if (TimeoutSeconds <= 0)
            throw new ArgumentException("No selection made");

        return Role;
    }

    internal async Task<DiscordChannel> PromptChannelSelection(bool IncludeCreateForMe = false, string CreateForMeName = "Channel", ChannelType CreateFormeChannelType = ChannelType.Text, bool IncludeDisable = false, string DisableString = "Disable")
    {
        List<DiscordSelectComponentOption> channels = new();

        if (IncludeCreateForMe)
            channels.Add(new DiscordSelectComponentOption($"Create one for me..", "create_for_me", "", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕"))));

        if (IncludeDisable)
            channels.Add(new DiscordSelectComponentOption(DisableString, "disable", "", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌"))));

        foreach (var category in await Context.Guild.GetOrderedChannelsAsync())
        {
            foreach (var b in category.Value)
                channels.Add(new DiscordSelectComponentOption(
                    $"#{b.Name} ({b.Id})",
                    b.Id.ToString(),
                    $"{(category.Key != 0 ? $"{b.Parent.Name} " : "")}"));
        }

        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();
        string NextPageId = Guid.NewGuid().ToString();
        string PrevPageId = Guid.NewGuid().ToString();

        DiscordChannel Channel = null;

        bool FinishedSelection = false;
        bool ExceptionOccured = false;
        Exception exception = null;

        async Task RefreshRoleList()
        {
            var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
            var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

            var dropdown = new DiscordSelectComponent("Select a channel..", channels.Skip(currentPage * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>, SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(Context.ResponseMessage.Embeds[0]).WithColor(EmbedColors.AwaitingInput)).AddComponents(dropdown).WithContent(Context.ResponseMessage.Content);

            if (channels.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            await RespondOrEdit(builder);
        }

        _ = RefreshRoleList();

        int TimeoutSeconds = 60;

        async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == Context.ResponseMessage.Id && e.User.Id == Context.User.Id)
                    {
                        TimeoutSeconds = 60;
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            Context.Client.ComponentInteractionCreated -= RunDropdownInteraction;

                            if (e.Values.First() is "create_for_me")
                                Channel = await Context.Guild.CreateChannelAsync(CreateForMeName, CreateFormeChannelType);
                            else if (e.Values.First() is "disable")
                                Channel = null;
                            else
                                Channel = Context.Guild.GetChannel(Convert.ToUInt64(e.Values.First()));

                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == PrevPageId)
                        {
                            currentPage--;
                            await RefreshRoleList();
                        }
                        else if (e.Interaction.Data.CustomId == NextPageId)
                        {
                            currentPage++;
                            await RefreshRoleList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    ExceptionOccured = true;
                    FinishedSelection = true;
                    throw;
                }
            }).Add(Context.Bot._watcher, Context);
        }

        Context.Client.ComponentInteractionCreated += RunDropdownInteraction;

        while (!FinishedSelection && TimeoutSeconds >= 0)
        {
            await Task.Delay(1000);
            TimeoutSeconds--;
        }

        Context.Client.ComponentInteractionCreated -= RunDropdownInteraction;

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(Context.ResponseMessage.Embeds[0]).WithContent(Context.ResponseMessage.Content));

        if (ExceptionOccured)
            throw exception;

        if (TimeoutSeconds <= 0)
            throw new ArgumentException("No selection made");

        return Channel;
    }

    internal async Task<string> PromptCustomSelection(List<DiscordSelectComponentOption> options, string CustomPlaceHolder = "Select an option..")
    {
        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();
        string NextPageId = Guid.NewGuid().ToString();
        string PrevPageId = Guid.NewGuid().ToString();

        string Selection = null;

        bool FinishedSelection = false;
        bool ExceptionOccured = false;
        Exception exception = null;

        async Task Refresh()
        {
            var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
            var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

            var dropdown = new DiscordSelectComponent(CustomPlaceHolder, options.Skip(currentPage * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>, SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(Context.ResponseMessage.Embeds[0]).WithColor(EmbedColors.AwaitingInput)).AddComponents(dropdown).WithContent(Context.ResponseMessage.Content);

            if (options.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            await RespondOrEdit(builder);
        }

        _ = Refresh();

        int TimeoutSeconds = 60;

        async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == Context.ResponseMessage.Id && e.User.Id == Context.User.Id)
                    {
                        TimeoutSeconds = 60;
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            Context.Client.ComponentInteractionCreated -= RunDropdownInteraction;

                            Selection = e.Values.First();

                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == PrevPageId)
                        {
                            currentPage--;
                            await Refresh();
                        }
                        else if (e.Interaction.Data.CustomId == NextPageId)
                        {
                            currentPage++;
                            await Refresh();
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    ExceptionOccured = true;
                    FinishedSelection = true;
                    throw;
                }
            }).Add(Context.Bot._watcher, Context);
        }

        Context.Client.ComponentInteractionCreated += RunDropdownInteraction;

        while (!FinishedSelection && TimeoutSeconds >= 0)
        {
            await Task.Delay(1000);
            TimeoutSeconds--;
        }

        Context.Client.ComponentInteractionCreated -= RunDropdownInteraction;

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(Context.ResponseMessage.Embeds[0]).WithContent(Context.ResponseMessage.Content));

        if (ExceptionOccured)
            throw exception;

        if (TimeoutSeconds <= 0)
            throw new ArgumentException("No selection made");

        return Selection;
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
    
    public void DeleteOrFinish()
    {
        switch (Context.CommandType)
        {
            case Enums.CommandType.ApplicationCommand:
                _ = RespondOrEdit("✅ `Finished command`");
                _ = Context.OriginalInteractionContext.DeleteResponseAsync();
                break;
            case Enums.CommandType.ContextMenu:
                _ = RespondOrEdit("✅ `Finished command`");
                _ = Context.OriginalInteractionContext.DeleteResponseAsync();
                break;
            default:
                _ = Context.ResponseMessage.DeleteAsync();
                break;
        }
    }

    public void SendNoMemberError()
    {
        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = Context.Guild.IconUrl,
                Name = Context.Guild.Name
            },
            Description = "The user you tagged is required to be on this server for this command to run.",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{Context.User.UsernameWithDiscriminator} attempted to use \"{Context.Prefix}{Context.CommandName}\"",
                IconUrl = Context.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = EmbedColors.Error
        });
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
    
    public void SendOwnPermissionError(Permissions perms)
    {
        if (perms is Permissions.AccessChannels or Permissions.SendMessages or Permissions.EmbedLinks)
            return;

        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = Context.Guild.IconUrl,
                Name = Context.Guild.Name
            },
            Description = $"The bot is missing permissions to run this command. Please assign the bot `{perms.ToPermissionString()}` to use this command.",
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
    
    public void SendVoiceStateError()
    {
        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"❌ `You aren't in a voice channel.`",
            Color = EmbedColors.Error,
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                Name = Context.Guild.Name,
                IconUrl = Context.Guild.IconUrl
            },
            Footer = Context.GenerateUsedByFooter(),
            Timestamp = DateTime.UtcNow
        }).WithContent(Context.User.Mention));
    }

    public async Task<bool> CheckVoiceState()
    {
        if (Context.Member.VoiceState is null)
        {
            SendVoiceStateError();
            return false;
        }

        return true;
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
    
    public async Task<bool> CheckPermissions(Permissions perms)
    {
        if (!Context.Member.Permissions.HasPermission(perms))
        {
            SendPermissionError(perms);
            return false;
        }

        return true;
    }
    
    public async Task<bool> CheckOwnPermissions(Permissions perms)
    {
        if (!Context.CurrentMember.Permissions.HasPermission(perms))
        {
            SendOwnPermissionError(perms);
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
