namespace ProjectIchigo.Commands;

public abstract class BaseCommand
{
    internal SharedCommandContext ctx { private get; set; }

    #region Execution
    public virtual async Task<bool> BeforeExecution(SharedCommandContext ctx)
    {
        return true;
    }

    public abstract Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments = null);

    public async Task ExecuteCommand(CommandContext ctx, Bot _bot, Dictionary<string, object> arguments = null)
    {
        this.ctx = new SharedCommandContext(this, ctx, _bot);

        if (await BasePreExecutionCheck())
            await ExecuteCommand(this.ctx, arguments);
    }

    public async Task ExecuteCommand(InteractionContext ctx, Bot _bot, Dictionary<string, object> arguments = null, bool Ephemeral = true, bool InitiateInteraction = true)
    {
        if (InitiateInteraction)
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = Ephemeral
            });

        this.ctx = new SharedCommandContext(this, ctx, _bot);

        this.ctx.RespondedToInitial = InitiateInteraction;

        if (await BasePreExecutionCheck())
            await ExecuteCommand(this.ctx, arguments);
    }

    public async Task ExecuteCommand(ContextMenuContext ctx, Bot _bot, Dictionary<string, object> arguments = null, bool Ephemeral = true, bool InitiateInteraction = true)
    {
        if (InitiateInteraction)
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = Ephemeral
            });

        this.ctx = new SharedCommandContext(this, ctx, _bot);

        this.ctx.RespondedToInitial = InitiateInteraction;

        if (await BasePreExecutionCheck())
            await ExecuteCommand(this.ctx, arguments);
    }

    internal async Task<bool> BasePreExecutionCheck()
    {
        if (!(await CheckOwnPermissions(Permissions.SendMessages)))
            return false;

        if (!(await CheckOwnPermissions(Permissions.EmbedLinks)))
            return false;

        if (!(await CheckOwnPermissions(Permissions.AddReactions)))
            return false;

        if (!(await CheckOwnPermissions(Permissions.AccessChannels)))
            return false;

        if (!(await CheckOwnPermissions(Permissions.AttachFiles)))
            return false;

        if (!(await CheckOwnPermissions(Permissions.ManageMessages)))
            return false;

        if (!(await BeforeExecution(this.ctx)))
            return false;

        if (this.ctx.Bot.objectedUsers.Contains(ctx.User.Id) && this.ctx.CommandName != "data object" && this.ctx.CommandName != "object")
        {
            SendDataError();
            return false;
        }

        if (this.ctx.Bot.bannedUsers.ContainsKey(ctx.User.Id))
        {
            SendUserBanError(this.ctx.Bot.bannedUsers[ctx.User.Id]);
            return false;
        }

        if (this.ctx.Bot.bannedGuilds.ContainsKey(ctx.Guild?.Id ?? 0))
        {
            SendGuildBanError(this.ctx.Bot.bannedGuilds[ctx.Guild?.Id ?? 0]);
            return false;
        }


        if (this.ctx.User.IsBot)
            return false;

        return true;
    }
    #endregion

    #region RespondOrEdit
    internal async Task<DiscordMessage> RespondOrEdit(DiscordEmbed embed)
        => await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

    internal async Task<DiscordMessage> RespondOrEdit(DiscordEmbedBuilder embed)
        => await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.Build()));

    internal async Task<DiscordMessage> RespondOrEdit(string content)
        => await RespondOrEdit(new DiscordMessageBuilder().WithContent(content));

    internal async Task<DiscordMessage> RespondOrEdit(DiscordMessageBuilder discordMessageBuilder)
    {
        switch (ctx.CommandType)
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

                var msg = await ctx.OriginalInteractionContext.EditResponseAsync(discordWebhookBuilder);
                ctx.ResponseMessage = msg;
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

                var msg = await ctx.OriginalContextMenuContext.EditResponseAsync(discordWebhookBuilder);
                ctx.ResponseMessage = msg;
                return msg;
            }

            case Enums.CommandType.PrefixCommand:
            {
                if (ctx.ResponseMessage is not null)
                {
                    if (discordMessageBuilder.Files?.Any() ?? false)
                    {
                        await ctx.ResponseMessage.DeleteAsync();
                        var msg1 = await ctx.Channel.SendMessageAsync(discordMessageBuilder);
                        ctx.ResponseMessage = msg1;
                        return ctx.ResponseMessage;
                    }

                    await ctx.ResponseMessage.ModifyAsync(discordMessageBuilder);
                    ctx.ResponseMessage = await ctx.ResponseMessage.Refetch();

                    return ctx.ResponseMessage;
                }

                var msg = await ctx.Channel.SendMessageAsync(discordMessageBuilder);

                ctx.ResponseMessage = msg;

                return msg;
            }

            case Enums.CommandType.Custom:
            {
                if (ctx.ResponseMessage is not null)
                {
                    if (discordMessageBuilder.Files?.Any() ?? false)
                    {
                        await ctx.ResponseMessage.DeleteAsync();
                        var msg1 = await ctx.Channel.SendMessageAsync(discordMessageBuilder);
                        ctx.ResponseMessage = msg1;
                        return ctx.ResponseMessage;
                    }

                    await ctx.ResponseMessage.ModifyAsync(discordMessageBuilder);
                    ctx.ResponseMessage = await ctx.ResponseMessage.Refetch();

                    return ctx.ResponseMessage;
                }

                var msg = await ctx.Channel.SendMessageAsync(discordMessageBuilder);

                ctx.ResponseMessage = msg;

                return msg;
            }
        }

        throw new NotImplementedException();
    }
    #endregion


    #region Selections
    internal async Task<DiscordRole> PromptRoleSelection(RolePromptConfiguration configuration = null)
    {
        configuration ??= new();

        List<DiscordSelectComponentOption> roles = new();

        var RefreshListButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Refresh List", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔁")));
        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Confirm Selection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

        var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
        var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();

        DiscordRole Role = null;

        string Selected = "";

        bool FinishedSelection = false;
        bool Exceptionoccurred = false;
        Exception exception = null;

        async Task RefreshList()
        {
            roles.Clear();

            if (!configuration.CreateRoleOption.IsNullOrWhiteSpace())
                roles.Add(new DiscordSelectComponentOption($"Create one for me..", "create_for_me", "", ("create_for_me" == Selected), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕"))));

            if (!configuration.DisableOption.IsNullOrWhiteSpace())
                roles.Add(new DiscordSelectComponentOption(configuration.DisableOption, "disable", "", ("disable" == Selected), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌"))));

            foreach (var b in ctx.Guild.Roles.OrderByDescending(x => x.Value.Position))
            {
                if (ctx.CurrentMember.GetRoleHighestPosition() > b.Value.Position && ctx.Member.GetRoleHighestPosition() > b.Value.Position && !b.Value.IsManaged && b.Value.Id != ctx.Guild.EveryoneRole.Id)
                    roles.Add(new DiscordSelectComponentOption($"@{b.Value.Name} ({b.Value.Id})", b.Value.Id.ToString(), "", (b.Value.Id.ToString() == Selected), new DiscordComponentEmoji(b.Value.Color.GetClosestColorEmoji(ctx.Client))));
            }

            if (!roles.Any(x => x.Default))
                Selected = "";

            if (!roles.IsNotNullAndNotEmpty())
                throw new NullReferenceException("No valid role found.");
        }

        await RefreshList();

        async Task RefreshMessage()
        {
            var dropdown = new DiscordSelectComponent("Select a role..", roles.Skip(currentPage * 25).Take(25), SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]).SetAwaitingInput(ctx)).AddComponents(dropdown).WithContent(ctx.ResponseMessage.Content);

            if (!roles.Skip(currentPage * 25).Any())
                currentPage--;

            if (currentPage < 0)
                currentPage = 0;

            if (roles.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            if (Selected.IsNullOrWhiteSpace())
                ConfirmSelectionButton.Disable();
            else
                ConfirmSelectionButton.Enable();

            builder.AddComponents(new List<DiscordComponent> { RefreshListButton, ConfirmSelectionButton });
            builder.AddComponents(MessageComponents.CancelButton);

            await RespondOrEdit(builder);
        }

        _ = RefreshMessage();

        Stopwatch sw = new();
        sw.Start();

        async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        sw.Restart();
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            Selected = e.Values.First();
                            roles = roles.Select(x => new DiscordSelectComponentOption(x.Label, x.Value, x.Description, (x.Value == Selected), x.Emoji)).ToList();

                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == RefreshListButton.CustomId)
                        {
                            await RefreshList();
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == ConfirmSelectionButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            if (Selected is "create_for_me")
                                Role = await ctx.Guild.CreateRoleAsync(configuration.CreateRoleOption);
                            else if (Selected is "disable")
                                Role = null;
                            else
                                Role = ctx.Guild.GetRole(Convert.ToUInt64(Selected));

                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == previousPageButton.CustomId)
                        {
                            currentPage--;
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == nextPageButton.CustomId)
                        {
                            currentPage++;
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
                            throw new CancelCommandException("Cancelled", null);
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    Exceptionoccurred = true;
                    FinishedSelection = true;
                }
            });
        }

        ctx.Client.ComponentInteractionCreated += RunInteraction;

        while (!FinishedSelection && sw.Elapsed <= TimeSpan.FromSeconds(60))
        {
            await Task.Delay(100);
        }

        ctx.Client.ComponentInteractionCreated -= RunInteraction;

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(ctx.ResponseMessage.Embeds[0]).WithContent(ctx.ResponseMessage.Content));

        if (Exceptionoccurred)
            throw exception;

        if (sw.Elapsed >= TimeSpan.FromSeconds(60))
            throw new ArgumentException("No selection made");

        return Role;
    }

    internal async Task<DiscordChannel> PromptChannelSelection(ChannelType? channelType = null, ChannelPromptConfiguration configuration = null)
        => await PromptChannelSelection(((channelType is null || !channelType.HasValue) ? null : new ChannelType[] { channelType.Value }), configuration);

    internal async Task<DiscordChannel> PromptChannelSelection(ChannelType[]? channelTypes = null, ChannelPromptConfiguration configuration = null)
    {
        configuration ??= new();

        List<DiscordSelectComponentOption> channels = new();

        var RefreshListButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Refresh List", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔁")));
        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Confirm Selection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

        var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
        var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();

        DiscordChannel Channel = null;

        string Selected = "";

        bool FinishedSelection = false;
        bool ExceptionOccurred = false;
        Exception exception = null;

        async Task RefreshList()
        {
            channels.Clear();

            if (configuration.CreateChannelOption is not null)
                channels.Add(new DiscordSelectComponentOption($"Create one for me..", "create_for_me", "", ("create_for_me" == Selected), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕"))));

            if (!configuration.DisableOption.IsNullOrWhiteSpace())
                channels.Add(new DiscordSelectComponentOption(configuration.DisableOption, "disable", "", ("disable" == Selected), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌"))));

            foreach (var category in ctx.Guild.GetOrderedChannels())
            {
                foreach (var b in category.Value)
                    if (channelTypes is null || channelTypes.Contains(b.Type))
                        channels.Add(new DiscordSelectComponentOption(
                        $"{b.GetIcon()}{b.Name} ({b.Id})",
                        b.Id.ToString(),
                        $"{(category.Key != 0 ? $"{b.Parent.Name} " : "")}", (b.Id.ToString() == Selected)));
            }

            if (!channels.Any(x => x.Default))
                Selected = "";

            if (!channels.IsNotNullAndNotEmpty())
                throw new NullReferenceException("No valid channel found.");
        }

        await RefreshList();

        async Task RefreshMessage()
        {
            var dropdown = new DiscordSelectComponent("Select a channel..", channels.Skip(currentPage * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>, SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]).SetAwaitingInput(ctx)).AddComponents(dropdown).WithContent(ctx.ResponseMessage.Content);

            if (!channels.Skip(currentPage * 25).Any())
                currentPage--;

            if (currentPage < 0)
                currentPage = 0;

            if (channels.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            if (Selected.IsNullOrWhiteSpace())
                ConfirmSelectionButton.Disable();
            else
                ConfirmSelectionButton.Enable();

            builder.AddComponents(new List<DiscordComponent> { RefreshListButton, ConfirmSelectionButton });
            builder.AddComponents(MessageComponents.CancelButton);

            await RespondOrEdit(builder);
        }

        _ = RefreshMessage();

        Stopwatch sw = new();
        sw.Start();

        async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        sw.Restart();
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            Selected = e.Values.First();
                            channels = channels.Select(x => new DiscordSelectComponentOption(x.Label, x.Value, x.Description, (x.Value == Selected), x.Emoji)).ToList();

                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == ConfirmSelectionButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            if (Selected is "create_for_me")
                                Channel = await ctx.Guild.CreateChannelAsync(configuration.CreateChannelOption.Name, configuration.CreateChannelOption.ChannelType);
                            else if (Selected is "disable")
                                Channel = null;
                            else
                                Channel = ctx.Guild.GetChannel(Convert.ToUInt64(Selected));

                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == RefreshListButton.CustomId)
                        {
                            await RefreshList();
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == previousPageButton.CustomId)
                        {
                            currentPage--;
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == nextPageButton.CustomId)
                        {
                            currentPage++;
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
                            throw new CancelCommandException("Cancelled", null);
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    ExceptionOccurred = true;
                    FinishedSelection = true;
                }
            });
        }

        ctx.Client.ComponentInteractionCreated += RunInteraction;

        while (!FinishedSelection && sw.Elapsed <= TimeSpan.FromSeconds(60))
        {
            await Task.Delay(100);
        }

        ctx.Client.ComponentInteractionCreated -= RunInteraction;
        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(ctx.ResponseMessage.Embeds[0]).WithContent(ctx.ResponseMessage.Content));

        if (ExceptionOccurred)
            throw exception;

        if (sw.Elapsed >= TimeSpan.FromSeconds(60))
            throw new ArgumentException("No selection made");

        return Channel;
    }

    internal async Task<string> PromptCustomSelection(List<DiscordSelectComponentOption> options, string CustomPlaceHolder = "Select an option..")
    {
        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Confirm Selection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

        var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
        var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();
        

        string Selection = null;

        string Selected = "";

        bool FinishedSelection = false;
        bool Exceptionoccurred = false;
        Exception exception = null;

        async Task RefreshMessage()
        {
            var dropdown = new DiscordSelectComponent(CustomPlaceHolder, options.Skip(currentPage * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>, SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]).SetAwaitingInput(ctx)).AddComponents(dropdown).WithContent(ctx.ResponseMessage.Content);

            if (options.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            builder.AddComponents(ConfirmSelectionButton);
            builder.AddComponents(MessageComponents.CancelButton);

            await RespondOrEdit(builder);
        }

        _ = RefreshMessage();

        Stopwatch sw = new();
        sw.Start();

        async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        sw.Restart();
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            Selected = e.Values.First();
                            options = options.Select(x => new DiscordSelectComponentOption(x.Label, x.Value, x.Description, (x.Value == Selected), x.Emoji)).ToList();

                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == ConfirmSelectionButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;

                            Selection = Selected;

                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == previousPageButton.CustomId)
                        {
                            currentPage--;
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == nextPageButton.CustomId)
                        {
                            currentPage++;
                            await RefreshMessage();
                        }
                        else if (e.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
                            throw new CancelCommandException("Cancelled", null);
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    Exceptionoccurred = true;
                    FinishedSelection = true;
                }
            });
        }

        ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

        while (!FinishedSelection && sw.Elapsed <= TimeSpan.FromSeconds(60))
        {
            await Task.Delay(100);
        }

        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(ctx.ResponseMessage.Embeds[0]).WithContent(ctx.ResponseMessage.Content));

        if (Exceptionoccurred)
            throw exception;

        if (sw.Elapsed >= TimeSpan.FromSeconds(60))
            throw new ArgumentException("No selection made");

        return Selection;
    }
    #endregion

    #region Modals
    internal async Task<ComponentInteractionCreateEventArgs> PromptModalWithRetry(DiscordInteraction interaction, DiscordInteractionModalBuilder builder, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null) => await PromptModalWithRetry(interaction, builder, null, ResetToOriginalEmbed, timeOutOverride);

    internal async Task<ComponentInteractionCreateEventArgs> PromptModalWithRetry(DiscordInteraction interaction, DiscordInteractionModalBuilder builder, DiscordEmbedBuilder customEmbed = null, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null, bool open = true)
    {
        timeOutOverride ??= TimeSpan.FromMinutes(15);

        var oriEmbed = ctx.ResponseMessage.Embeds[0];

        var ReOpen = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Re-Open Modal", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔄")));

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(customEmbed ?? new DiscordEmbedBuilder
        {
            Description = "`Waiting for a modal response..`"
        }.SetAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { ReOpen, MessageComponents.CancelButton }));

        ComponentInteractionCreateEventArgs FinishedInteraction = null;

        bool FinishedSelection = false;
        bool Exceptionoccurred = false;
        bool Cancelled = false;
        Exception exception = null;

        if (open)
            await interaction.CreateInteractionModalResponseAsync(builder);

        ctx.Client.ComponentInteractionCreated += RunInteraction;

        async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        if (e.Interaction.Data.CustomId == builder.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            FinishedInteraction = e;
                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == ReOpen.CustomId)
                        {
                            await e.Interaction.CreateInteractionModalResponseAsync(builder);
                        }
                        else if (e.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            Cancelled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    Exceptionoccurred = true;
                    FinishedSelection = true;
                    throw;
                }
            }).Add(ctx.Bot.watcher, ctx);
        }

        int TimeoutSeconds = (int)(timeOutOverride.Value.TotalSeconds * 2);

        while (!FinishedSelection && !Exceptionoccurred && !Cancelled && TimeoutSeconds >= 0)
        {
            await Task.Delay(500);
            TimeoutSeconds--;
        }

        ctx.Client.ComponentInteractionCreated -= RunInteraction;

        if (ResetToOriginalEmbed)
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(oriEmbed));

        if (Exceptionoccurred)
            throw exception;

        if (TimeoutSeconds <= 0)
            throw new ArgumentException("Modal not submitted");

        if (Cancelled)
            throw new CancelCommandException("", null);

        return FinishedInteraction;
    }


    internal async Task<TimeSpan> PromptModalForTimeSpan(DiscordInteraction interaction, TimeSpan? MaxTime = null, TimeSpan? MinTime = null, TimeSpan? DefaultTime = null, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null)
    {
        MinTime ??= TimeSpan.Zero;
        MaxTime ??= TimeSpan.FromDays(356);
        DefaultTime ??= TimeSpan.FromSeconds(30);

        var modal = new DiscordInteractionModalBuilder().WithTitle("Select a time span").WithCustomId(Guid.NewGuid().ToString());

        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "seconds", "Seconds (max. 59)", "0", 1, 2, true, $"{DefaultTime.Value.Seconds}"));

        if (MaxTime.Value.TotalMinutes >= 1)
            modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minutes", $"Minutes (max. {(MaxTime.Value.TotalMinutes >= 60 ? "59" : $"{((int)MaxTime.Value.TotalMinutes)}")})", $"0", 1, 2, true, $"{DefaultTime.Value.Minutes}"));

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

        if ((Response.Interaction.Data.Components.Any(x => x.CustomId == "seconds") && !Response.Interaction.Data.Components.First(x => x.CustomId == "seconds").Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.CustomId == "minutes") && !Response.Interaction.Data.Components.First(x => x.CustomId == "minutes").Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.CustomId == "hours") && !Response.Interaction.Data.Components.First(x => x.CustomId == "hours").Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.CustomId == "days") && !Response.Interaction.Data.Components.First(x => x.CustomId == "days").Value.IsDigitsOnly()))
            throw new InvalidOperationException("Invalid TimeSpan");

        double seconds = Response.Interaction.Data.Components.Any(x => x.CustomId == "seconds") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.CustomId == "seconds").Value)) : 0;
        double minutes = Response.Interaction.Data.Components.Any(x => x.CustomId == "minutes") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.CustomId == "minutes").Value)) : 0;
        double hours = Response.Interaction.Data.Components.Any(x => x.CustomId == "hours") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.CustomId == "hours").Value)) : 0;
        double days = Response.Interaction.Data.Components.Any(x => x.CustomId == "days") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.CustomId == "days").Value)) : 0;

        length = length.Add(TimeSpan.FromSeconds(seconds));
        length = length.Add(TimeSpan.FromMinutes(minutes));
        length = length.Add(TimeSpan.FromHours(hours));
        length = length.Add(TimeSpan.FromDays(days));

        if (length > MaxTime || length < MinTime)
            throw new InvalidOperationException("Invalid TimeSpan");

        return length;
    }

    internal async Task<DateTime> PromptModalForDateTime(DiscordInteraction interaction, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null)
    {
        var modal = new DiscordInteractionModalBuilder().WithTitle("Select a time span").WithCustomId(Guid.NewGuid().ToString());

        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "hour", "Hour", $"{DateTime.UtcNow.Hour}", 1, 2, true, $"{DateTime.UtcNow.Hour}"));
        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minute", "Minute", $"{DateTime.UtcNow.Minute}", 1, 2, true, $"{DateTime.UtcNow.Minute}"));
        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "day", "Day", $"{DateTime.UtcNow.Day}", 1, 2, true, $"{DateTime.UtcNow.Day}"));
        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "month", "Month", $"{DateTime.UtcNow.Month}", 1, 2, true, $"{DateTime.UtcNow.Month}"));
        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "year", "Year", $"{DateTime.UtcNow.Year}", 1, 2, true, $"{DateTime.UtcNow.Year}"));

        InteractionCreateEventArgs Response;

        try
        {
            Response = await PromptModalWithRetry(interaction, modal, false);
        }
        catch (Exception)
        {
            throw;
        }

        if ((Response.Interaction.Data.Components.Any(x => x.CustomId == "hour") && !Response.Interaction.Data.Components.First(x => x.CustomId == "hour").Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.CustomId == "minute") && !Response.Interaction.Data.Components.First(x => x.CustomId == "minute").Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.CustomId == "day") && !Response.Interaction.Data.Components.First(x => x.CustomId == "day").Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.CustomId == "month") && !Response.Interaction.Data.Components.First(x => x.CustomId == "month").Value.IsDigitsOnly()) ||
            (Response.Interaction.Data.Components.Any(x => x.CustomId == "year") && !Response.Interaction.Data.Components.First(x => x.CustomId == "year").Value.IsDigitsOnly()))
            throw new InvalidOperationException("Invalid");

        int hour = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("hour"));
        int minute = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("minute"));
        int day = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("day"));
        int month = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("month"));
        int year = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("year"));

        return new DateTime(year, month, day, hour, minute, 0);
    } 
    #endregion

    internal async Task<(Stream stream, int fileSize)> PromptForFileUpload(TimeSpan? timeOutOverride = null)
    {
        timeOutOverride ??= TimeSpan.FromMinutes(15);

        if (ctx.Bot.uploadInteractions.ContainsKey(ctx.User.Id))
        {
            if (ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.GetTotalSecondsUntil() > 0 && !ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled)
                throw new AlreadyAppliedException("");

            ctx.Bot.uploadInteractions.Remove(ctx.User.Id);
        }

        ctx.Bot.uploadInteractions.Add(ctx.User.Id, new UserUpload
        {
            TimeOut = DateTime.UtcNow.Add(timeOutOverride.Value)
        });

        while (ctx.Bot.uploadInteractions.ContainsKey(ctx.User.Id) && !ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled && ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.GetTotalSecondsUntil() > 0)
        {
            await Task.Delay(500);
        }

        if (!ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled)
            throw new ArgumentException("");

        int size = ctx.Bot.uploadInteractions[ctx.User.Id].FileSize;
        Stream stream = ctx.Bot.uploadInteractions[ctx.User.Id].UploadedData;

        ctx.Bot.uploadInteractions.Remove(ctx.User.Id);
        return (stream, size);
    }

    #region FinishInteraction
    public void ModifyToTimedOut(bool Delete = false)
    {
        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]).WithFooter(ctx.ResponseMessage.Embeds[0].Footer.Text + " • Interaction timed out").WithColor(DiscordColor.Gray)));

        if (Delete)
            Task.Delay(5000).ContinueWith(_ =>
            {
                if (!ctx.ResponseMessage?.Flags.Value.HasMessageFlag(MessageFlags.Ephemeral) ?? false)
                    ctx.ResponseMessage.DeleteAsync();
            });
    }

    public void DeleteOrInvalidate()
    {
        switch (ctx.CommandType)
        {
            case Enums.CommandType.ContextMenu:
            case Enums.CommandType.ApplicationCommand:
                _ = RespondOrEdit("✅ _`Interaction finished.`_");
                break;
            default:
                _ = ctx.ResponseMessage.DeleteAsync();
                break;
        }
    }
    #endregion

    #region Checks
    public async Task<bool> CheckVoiceState()
    {
        if (ctx.Member.VoiceState is null)
        {
            SendVoiceStateError();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckMaintenance()
    {
        if (!ctx.User.IsMaintenance(ctx.Bot.status))
        {
            SendMaintenanceError();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckAdmin()
    {
        if (!ctx.Member.IsAdmin(ctx.Bot.status))
        {
            SendAdminError();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckPermissions(Permissions perms)
    {
        if (!ctx.Member.Permissions.HasPermission(perms))
        {
            SendPermissionError(perms);
            return false;
        }

        return true;
    }

    public async Task<bool> CheckOwnPermissions(Permissions perms)
    {
        if (!ctx.CurrentMember.Permissions.HasPermission(perms))
        {
            SendOwnPermissionError(perms);
            return false;
        }

        return true;
    }

    public async Task<bool> CheckSource(Enums.CommandType commandType)
    {
        if (ctx.CommandType != commandType)
        {
            SendSourceError(commandType);
            return false;
        }

        return true;
    }
    #endregion

    #region ErrorTemplates
    public void SendNoMemberError()
    => _ = RespondOrEdit(new DiscordEmbedBuilder()
    {
        Description = "The user you tagged is required to be on this server for this command to run.",
    }.SetError(ctx));

    public void SendMaintenanceError()
        => _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = $"You dont have permissions to use the command `{ctx.Prefix}{ctx.CommandName}`. You need to be <@411950662662881290> to use this command.",
        }.SetError(ctx));

    public void SendAdminError()
        => _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = $"You dont have permissions to use the command `{ctx.Prefix}{ctx.CommandName}`. You need to be `Administrator` to use this command.",
        }.SetError(ctx));

    public void SendPermissionError(Permissions perms)
        => _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = $"You dont have permissions to use the command `{ctx.Prefix}{ctx.CommandName}`. You need to be `{perms.ToPermissionString()}` to use this command.",
        }.SetError(ctx));

    public void SendVoiceStateError()
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"`You aren't in a voice channel.`",
        }.SetError(ctx)).WithContent(ctx.User.Mention));

    public void SendUserBanError(BlacklistEntry entry)
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"`You are currently banned from using this bot: {entry.Reason.SanitizeForCode()}`",
        }.SetError(ctx)).WithContent(ctx.User.Mention));

    public void SendGuildBanError(BlacklistEntry entry)
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"`This guild is currently banned from using this bot: {entry.Reason.SanitizeForCode()}`",
        }.SetError(ctx)).WithContent(ctx.User.Mention));

    public void SendSourceError(Enums.CommandType commandType)
        => _ = commandType switch
        {
            Enums.CommandType.ApplicationCommand => RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = $"This command is exclusive to application commands.",
            }.SetError(ctx)),
            Enums.CommandType.PrefixCommand => RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = $"This command is exclusive to prefixed commands."
            }.SetError(ctx)),
            _ => throw new ArgumentException("Invalid Source defined."),
        };

    public void SendDataError()
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"`You objected to having your data being processed. To run commands, please run '{ctx.Prefix}data object' again to re-allow data processing.`",
        }.SetError(ctx)).WithContent(ctx.User.Mention));

    public void SendOwnPermissionError(Permissions perms)
    {
        if (perms is Permissions.AccessChannels or Permissions.SendMessages or Permissions.EmbedLinks)
            return;

        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = $"The bot is missing permissions to run this command. Please assign the bot `{perms.ToPermissionString()}` to use this command."
        }.SetError(ctx));
    }

    public void SendSyntaxError()
    {
        if (this.ctx.CommandType != Enums.CommandType.PrefixCommand)
            throw new ArgumentException("Syntax Error can only be generated for Prefix Commands.");

        var ctx = this.ctx.OriginalCommandContext;

        var embed = new DiscordEmbedBuilder
        {
            Description = $"**`{ctx.Prefix}{ctx.Command.Name}{(ctx.RawArgumentString != "" ? $" {ctx.RawArgumentString.SanitizeForCode().Replace("\\", "")}" : "")}` is not a valid way of using this command.**\nUse it like this instead: `{ctx.Prefix}{ctx.Command.GenerateUsage()}`\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
        }.SetError(this.ctx);

        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).WithContent(this.ctx.User.Mention));
    }
    #endregion
}
