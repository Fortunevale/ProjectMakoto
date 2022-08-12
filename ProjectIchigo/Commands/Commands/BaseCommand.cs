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
            if (Context.CurrentMember.GetRoleHighestPosition() > role.Value.Position && Context.Member.GetRoleHighestPosition() > role.Value.Position && !role.Value.IsManaged && role.Value.Id != Context.Guild.EveryoneRole.Id)
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
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(Context.ResponseMessage.Embeds[0]).SetAwaitingInput(Context)).AddComponents(dropdown).WithContent(Context.ResponseMessage.Content);

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
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(Context.ResponseMessage.Embeds[0]).SetAwaitingInput(Context)).AddComponents(dropdown).WithContent(Context.ResponseMessage.Content);

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
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(Context.ResponseMessage.Embeds[0]).SetAwaitingInput(Context)).AddComponents(dropdown).WithContent(Context.ResponseMessage.Content);

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

    internal async Task<ComponentInteractionCreateEventArgs> PromptModalWithRetry(DiscordInteraction interaction, DiscordInteractionModalBuilder builder, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null) => await PromptModalWithRetry(interaction, builder, null, ResetToOriginalEmbed, timeOutOverride);

    internal async Task<ComponentInteractionCreateEventArgs> PromptModalWithRetry(DiscordInteraction interaction, DiscordInteractionModalBuilder builder, DiscordEmbedBuilder customEmbed = null, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null)
    {
        timeOutOverride ??= TimeSpan.FromMinutes(15);

        var oriEmbed = Context.ResponseMessage.Embeds[0];

        var ReOpen = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Re-Open Modal", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔄")));

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(customEmbed ?? new DiscordEmbedBuilder
        {
            Description = "`Waiting for a modal response..`"
        }.SetAwaitingInput(Context)).AddComponents(new List<DiscordComponent> { ReOpen, Resources.CancelButton }));

        ComponentInteractionCreateEventArgs FinishedInteraction = null;

        bool FinishedSelection = false;
        bool ExceptionOccured = false;
        bool Cancelled = false;
        Exception exception = null;

        await interaction.CreateInteractionModalResponseAsync(builder);

        Context.Client.ComponentInteractionCreated += RunInteraction;

        async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == Context.ResponseMessage.Id && e.User.Id == Context.User.Id)
                    {
                        if (e.Interaction.Data.CustomId == builder.CustomId)
                        {
                            Context.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            FinishedInteraction = e;
                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == ReOpen.CustomId)
                        {
                            await e.Interaction.CreateInteractionModalResponseAsync(builder);
                        }
                        else if (e.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            Cancelled = true;
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

        int TimeoutSeconds = (int)(timeOutOverride.Value.TotalSeconds * 2);

        while (!FinishedSelection && !ExceptionOccured && !Cancelled && TimeoutSeconds >= 0)
        {
            await Task.Delay(500);
            TimeoutSeconds--;
        }

        Context.Client.ComponentInteractionCreated -= RunInteraction;

        if (ResetToOriginalEmbed)
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(oriEmbed));

        if (ExceptionOccured)
            throw exception;

        if (TimeoutSeconds <= 0)
            throw new ArgumentException("Modal not submitted");
        
        if (Cancelled)
            throw new CancelCommandException("", null);

        return FinishedInteraction;
    }

    internal async Task<TimeSpan> PromptModalForTimeSpan(DiscordInteraction interaction, TimeSpan? MaxTime = null, TimeSpan ? MinTime = null, TimeSpan? DefaultTime = null, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null)
    {
        MinTime ??= TimeSpan.Zero;
        MaxTime ??= TimeSpan.FromDays(356);
        DefaultTime ??= TimeSpan.FromSeconds(30);

        var modal = new DiscordInteractionModalBuilder().WithTitle("Select a time span").WithCustomId(Guid.NewGuid().ToString());

        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "seconds", "Seconds (max. 59)", "0", 1, 2, true, $"{DefaultTime.Value.Seconds}"));

        if (MaxTime.Value.TotalMinutes >= 1)
            modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minutes", $"Minutes (max. {(MaxTime.Value.TotalMinutes >= 60 ? "59" : $"{((int)MaxTime.Value.TotalMinutes)}" )})", $"0", 1, 2, true, $"{DefaultTime.Value.Minutes}"));

        if (MaxTime.Value.TotalHours >= 1)
            modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "hours", $"Hours (max. {(MaxTime.Value.TotalHours >= 24 ? "23" : $"{((int)MaxTime.Value.TotalHours)}")})", "0", 1, 2, true, $"{DefaultTime.Value.Hours}"));
        
        if (MaxTime.Value.TotalDays >= 1)
            modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "days", $"Days (max. {((int)MaxTime.Value.TotalDays)})", "0", 1, 3, true, $"{DefaultTime.Value.Days}"));

        InteractionCreateEventArgs Response;

        try
        {
            Response = await PromptModalWithRetry(interaction, modal, false);
        }
        catch (Exception)
        {
            throw;
        }

        TimeSpan length = TimeSpan.FromSeconds(0);

        if ((Response.Interaction.Data.Components.Any(x => x.Components.First().CustomId == "seconds") && !Response.Interaction.Data.Components.First(x => x.Components.First().CustomId == "seconds").Components.First().Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.Components.First().CustomId == "minutes") && !Response.Interaction.Data.Components.First(x => x.Components.First().CustomId == "minutes").Components.First().Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.Components.First().CustomId == "hours") && !Response.Interaction.Data.Components.First(x => x.Components.First().CustomId == "hours").Components.First().Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.Components.First().CustomId == "days") && !Response.Interaction.Data.Components.First(x => x.Components.First().CustomId == "days").Components.First().Value.IsDigitsOnly()))
            throw new InvalidOperationException("Invalid TimeSpan");

        double seconds = Response.Interaction.Data.Components.Any(x => x.Components.First().CustomId == "seconds") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.Components.First().CustomId == "seconds").Components.First().Value)) : 0;
        double minutes = Response.Interaction.Data.Components.Any(x => x.Components.First().CustomId == "minutes") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.Components.First().CustomId == "minutes").Components.First().Value)) : 0;
        double hours = Response.Interaction.Data.Components.Any(x => x.Components.First().CustomId == "hours") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.Components.First().CustomId == "hours").Components.First().Value)) : 0;
        double days = Response.Interaction.Data.Components.Any(x => x.Components.First().CustomId == "days") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.Components.First().CustomId == "days").Components.First().Value)) : 0;

        length = length.Add(TimeSpan.FromSeconds(seconds));
        length = length.Add(TimeSpan.FromMinutes(minutes));
        length = length.Add(TimeSpan.FromHours(hours));
        length = length.Add(TimeSpan.FromDays(days));

        if (length > MaxTime || length < MinTime)
            throw new InvalidOperationException("Invalid TimeSpan");

        return length;
    }

    public void ModifyToTimedOut(bool Delete = false)
    {
        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(Context.ResponseMessage.Embeds[0]).WithFooter(Context.ResponseMessage.Embeds[0].Footer.Text + " • Interaction timed out").WithColor(DiscordColor.Gray)));

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
                _ = RespondOrEdit("✅ `Interaction ended.`");
                _ = Context.ResponseMessage.DeleteAsync();
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
            Description = "The user you tagged is required to be on this server for this command to run.",
        }.SetError(Context));
    }
    
    public void SendMaintenanceError()
    {
        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = $"You dont have permissions to use the command `{Context.Prefix}{Context.CommandName}`. You need to be <@411950662662881290> to use this command.",
        }.SetError(Context));
    }

    public void SendAdminError()
    {
        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = $"You dont have permissions to use the command `{Context.Prefix}{Context.CommandName}`. You need to be `Administrator` to use this command.",
        }.SetError(Context));
    }
    
    public void SendPermissionError(Permissions perms)
    {
        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = $"You dont have permissions to use the command `{Context.Prefix}{Context.CommandName}`. You need to be `{perms.ToPermissionString()}` to use this command.",
        }.SetError(Context));
    }
    
    public void SendOwnPermissionError(Permissions perms)
    {
        if (perms is Permissions.AccessChannels or Permissions.SendMessages or Permissions.EmbedLinks)
            return;

        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = $"The bot is missing permissions to run this command. Please assign the bot `{perms.ToPermissionString()}` to use this command."
        }.SetError(Context));
    }
    
    public void SendSourceError(Enums.CommandType commandType)
    {
        _ = commandType switch
        {
            Enums.CommandType.ApplicationCommand => RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = $"This command is exclusive to application commands.",
            }.SetError(Context)),
            Enums.CommandType.PrefixCommand => RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = $"This command is exclusive to prefixed commands."
            }.SetError(Context)),
            _ => throw new ArgumentException("Invalid Source defined."),
        };
    }

    public void SendSyntaxError()
    {
        if (Context.CommandType != Enums.CommandType.PrefixCommand)
            throw new ArgumentException("Syntax Error can only be generated for Prefix Commands.");

        var ctx = Context.OriginalCommandContext;

        var embed = new DiscordEmbedBuilder
        {
            Description = $"**`{ctx.Prefix}{ctx.Command.Name}{(ctx.RawArgumentString != "" ? $" {ctx.RawArgumentString.SanitizeForCodeBlock().Replace("\\", "")}" : "")}` is not a valid way of using this command.**\nUse it like this instead: `{ctx.Prefix}{ctx.Command.GenerateUsage()}`\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
        }.SetError(Context);

        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).WithContent(Context.User.Mention));
    }
    
    public void SendVoiceStateError()
    {
        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"`You aren't in a voice channel.`",
        }.SetError(Context)).WithContent(Context.User.Mention));
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
            SendAdminError();
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
