// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

public abstract class BaseCommand
{
    public SharedCommandContext ctx { private get; set; }
    public Translations t { get; set; }

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
        t = Bot.loadedTranslations;
        if (ctx.Bot.users.ContainsKey(ctx.User.Id) && !ctx.User.Locale.IsNullOrWhiteSpace() && ctx.Bot.users[ctx.User.Id].CurrentLocale != ctx.User.Locale)
        {
            ctx.Bot.users[ctx.User.Id].CurrentLocale = ctx.User.Locale;
            _logger.LogDebug("Updated language for User '{User}' to '{Locale}'", ctx.User.Id, ctx.User.Locale);
        }

        if (ctx.Bot.guilds.ContainsKey(ctx.Guild.Id) && !ctx.Guild.PreferredLocale.IsNullOrWhiteSpace() && ctx.Bot.guilds[ctx.Guild.Id].CurrentLocale != ctx.Guild.PreferredLocale)
        {
            ctx.Bot.guilds[ctx.Guild.Id].CurrentLocale = ctx.Guild.PreferredLocale;
            _logger.LogDebug("Updated language for Guild '{Guild}' to '{Locale}'", ctx.Guild.Id, ctx.Guild.PreferredLocale);
        }

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

        if ((this.ctx.Bot.objectedUsers.Contains(ctx.User.Id) || ctx.DbUser.Data.DeletionRequested) && this.ctx.CommandName != "data" && this.ctx.CommandName != "delete")
        {
            SendDataError();
            return false;
        }

        if (this.ctx.Bot.bannedUsers.TryGetValue(ctx.User.Id, out BlacklistEntry blacklistedUserDetails))
        {
            SendUserBanError(blacklistedUserDetails);
            return false;
        }

        if (this.ctx.Bot.bannedGuilds.TryGetValue(ctx.Guild?.Id ?? 0, out BlacklistEntry blacklistedGuildDetails))
        {
            SendGuildBanError(blacklistedGuildDetails);
            return false;
        }


        if (this.ctx.User.IsBot)
            return false;

        return true;
    }
    #endregion

    #region RespondOrEdit
    public async Task<DiscordMessage> RespondOrEdit(DiscordEmbed embed)
        => await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

    public async Task<DiscordMessage> RespondOrEdit(DiscordEmbedBuilder embed)
        => await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.Build()));

    public async Task<DiscordMessage> RespondOrEdit(string content)
        => await RespondOrEdit(new DiscordMessageBuilder().WithContent(content));

    public async Task<DiscordMessage> RespondOrEdit(DiscordMessageBuilder discordMessageBuilder)
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

    #region GetString
    TVar[] GetDefaultVars() 
        => new TVar[]
        {
            new TVar("CurrentCommand", ctx.Prefix + ctx.CommandName, false),
            new TVar("Bot", ctx.CurrentUser.Mention, false),
            new TVar("BotName", ctx.Client.CurrentApplication.Name, false),
            new TVar("FullBot", ctx.CurrentUser.GetUsernameWithIdentifier(), false),
            new TVar("BotDisplayName", ctx.CurrentUser.GetUsernameWithIdentifier(), false),
            new TVar("User", ctx.User.Mention, false),
            new TVar("UserName", ctx.User.GetUsername(), false),
            new TVar("FullUser", ctx.User.GetUsernameWithIdentifier(), false),
            new TVar("UserDisplayName", ctx.Member?.DisplayName ?? ctx.User.GetUsername(), false),
        };

    public string GetString(SingleTranslationKey key)
        => GetString(key, false, Array.Empty<TVar>());

    public string GetString(SingleTranslationKey key, params TVar[] vars)
        => GetString(key, false, vars);

    public string GetString(SingleTranslationKey key, bool Code = false, params TVar[] vars)
        => key.Get(ctx.Bot.users[ctx.User.Id]).Build(Code, vars.Concat(GetDefaultVars()).ToArray());



    public string GetString(MultiTranslationKey key)
        => GetString(key, false, false, Array.Empty<TVar>());

    public string GetString(MultiTranslationKey key, params TVar[] vars)
        => GetString(key, false, false, vars);

    public string GetString(MultiTranslationKey key, bool Code = false, params TVar[] vars)
        => GetString(key, true, false, vars);

    public string GetString(MultiTranslationKey key, bool Code = false, bool UseBoldMarker = false, params TVar[] vars)
        => key.Get(ctx.Bot.users[ctx.User.Id]).Build(Code, UseBoldMarker, vars.Concat(GetDefaultVars()).ToArray());



    public string GetGuildString(SingleTranslationKey key)
        => GetGuildString(key, false, Array.Empty<TVar>());

    public string GetGuildString(SingleTranslationKey key, params TVar[] vars)
        => GetGuildString(key, false, vars);

    public string GetGuildString(SingleTranslationKey key, bool Code = false, params TVar[] vars)
        => key.Get(ctx.DbGuild).Build(Code, vars.Concat(GetDefaultVars()).ToArray());



    public string GetGuildString(MultiTranslationKey key)
        => GetGuildString(key, false, false, Array.Empty<TVar>());

    public string GetGuildString(MultiTranslationKey key, params TVar[] vars)
        => GetGuildString(key, false, false, vars);

    public string GetGuildString(MultiTranslationKey key, bool Code = false, params TVar[] vars)
        => GetGuildString(key, Code, false, vars);

    public string GetGuildString(MultiTranslationKey key, bool Code = false, bool UseBoldMarker = false, params TVar[] vars)
        => key.Get(ctx.DbGuild).Build(Code, UseBoldMarker, vars.Concat(GetDefaultVars()).ToArray()); 
    #endregion

    #region Selections
    public async Task<InteractionResult<DiscordRole>> PromptRoleSelection(RolePromptConfiguration configuration = null, TimeSpan? timeOutOverride = null)
    {
        configuration ??= new();
        timeOutOverride ??= TimeSpan.FromSeconds(120);

        var CreateNewButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), GetString(t.Commands.Common.Prompts.CreateRoleForMe), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
        var DisableButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), configuration.DisableOption ?? GetString(t.Commands.Common.Prompts.Disable), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));
        var EveryoneButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), GetString(t.Commands.Common.Prompts.SelectEveryone), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👥")));
        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Common.Prompts.ConfirmSelection), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));


        string SelectionInteractionId = Guid.NewGuid().ToString();

        DiscordRole FinalSelection = null;

        string Selected = "";

        bool FinishedSelection = false;
        bool ExceptionOccurred = false;
        Exception ThrownException = null;

        async Task RefreshMessage()
        {
            var dropdown = new DiscordRoleSelectComponent(GetString(t.Commands.Common.Prompts.SelectARole), SelectionInteractionId, 1, 1, false);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]).AsAwaitingInput(ctx)).AddComponents(dropdown).WithContent(ctx.ResponseMessage.Content);

            if (Selected.IsNullOrWhiteSpace())
                ConfirmSelectionButton.Disable();
            else
                ConfirmSelectionButton.Enable();

            List<DiscordComponent> components = new();

            if (!configuration.CreateRoleOption.IsNullOrWhiteSpace())
                components.Add(CreateNewButton);
            
            if (!configuration.DisableOption.IsNullOrWhiteSpace())
                components.Add(DisableButton);
            
            if (configuration.IncludeEveryone)
                components.Add(EveryoneButton);

            if (components.Any())
                builder.AddComponents(components);

            builder.AddComponents(MessageComponents.GetCancelButton(ctx.DbUser), ConfirmSelectionButton);

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
                    if (e.Message?.Id == this.ctx.ResponseMessage.Id && e.User.Id == this.ctx.User.Id)
                    {
                        sw.Restart();
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.GetCustomId() == SelectionInteractionId)
                        {
                            Selected = e.Values[0];

                            try
                            {
                                DiscordRole role = ctx.Guild.GetRole(Convert.ToUInt64(Selected));

                                if (role.IsManaged || ctx.Member.GetRoleHighestPosition() <= role.Position)
                                {
                                    Selected = "";
                                    _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AsEphemeral().WithContent($"❌ {GetString(t.Commands.Common.Prompts.SelectedRoleUnavailable, true)}"));
                                }
                            }
                            catch { }

                            await RefreshMessage();
                        }
                        if (e.GetCustomId() == DisableButton.CustomId)
                        {
                            FinalSelection = null;
                            FinishedSelection = true;
                        }
                        if (e.GetCustomId() == CreateNewButton.CustomId)
                        {
                            FinalSelection = await this.ctx.Guild.CreateRoleAsync(configuration.CreateRoleOption);
                            FinishedSelection = true;
                        }
                        if (e.GetCustomId() == EveryoneButton.CustomId)
                        {
                            FinalSelection = ctx.Guild.EveryoneRole;
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == ConfirmSelectionButton.CustomId)
                        {
                            this.ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            FinalSelection = this.ctx.Guild.GetRole(Convert.ToUInt64(Selected));
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                            throw new CancelException();
                    }
                }
                catch (Exception ex)
                {
                    ThrownException = ex;
                    ExceptionOccurred = true;
                    FinishedSelection = true;
                }
            });
        }

        ctx.Client.ComponentInteractionCreated += RunInteraction;

        while (!FinishedSelection && sw.Elapsed <= timeOutOverride)
        {
            await Task.Delay(100);
        }

        ctx.Client.ComponentInteractionCreated -= RunInteraction;

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(ctx.ResponseMessage.Embeds[0]).WithContent(ctx.ResponseMessage.Content));

        if (ExceptionOccurred)
            return new InteractionResult<DiscordRole>(ThrownException);

        if (sw.Elapsed >= timeOutOverride)
            return new InteractionResult<DiscordRole>(new TimedOutException());

        return new InteractionResult<DiscordRole>(FinalSelection);
    }

    public async Task<InteractionResult<DiscordChannel>> PromptChannelSelection(ChannelType? channelType = null, ChannelPromptConfiguration configuration = null, TimeSpan? timeOutOverride = null)
        => await PromptChannelSelection(((channelType is null || !channelType.HasValue) ? null : new ChannelType[] { channelType.Value }), configuration, timeOutOverride);

    public async Task<InteractionResult<DiscordChannel>> PromptChannelSelection(ChannelType[]? channelTypes = null, ChannelPromptConfiguration configuration = null, TimeSpan? timeOutOverride = null)
    {
        configuration ??= new();
        timeOutOverride ??= TimeSpan.FromSeconds(120);

        List<DiscordStringSelectComponentOption> FetchedChannels = new();

        var CreateNewButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), GetString(t.Commands.Common.Prompts.CreateChannelForMe), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
        var DisableButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), configuration.DisableOption ?? GetString(t.Commands.Common.Prompts.Disable), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));
        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Common.Prompts.ConfirmSelection), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

        string SelectionInteractionId = Guid.NewGuid().ToString();

        DiscordChannel FinalSelection = null;

        string Selected = "";   

        bool FinishedSelection = false;
        bool ExceptionOccurred = false;
        Exception ThrownException = null;

        async Task RefreshMessage()
        {
            var dropdown = new DiscordChannelSelectComponent(GetString(t.Commands.Common.Prompts.SelectAChannel), channelTypes, SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]).AsAwaitingInput(ctx)).AddComponents(dropdown).WithContent(ctx.ResponseMessage.Content);

            if (Selected.IsNullOrWhiteSpace())
                ConfirmSelectionButton.Disable();
            else
                ConfirmSelectionButton.Enable();

            List<DiscordComponent> components = new();

            if (configuration.CreateChannelOption is not null)
                components.Add(CreateNewButton);

            if (!configuration.DisableOption.IsNullOrWhiteSpace())
                components.Add(DisableButton);

            if (components.Any())
                builder.AddComponents(components);

            builder.AddComponents(MessageComponents.GetCancelButton(ctx.DbUser), ConfirmSelectionButton);

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

                        if (e.GetCustomId() == SelectionInteractionId)
                        {
                            Selected = e.Values.First();
                            FetchedChannels = FetchedChannels.Select(x => new DiscordStringSelectComponentOption(x.Label, x.Value, x.Description, (x.Value == Selected), x.Emoji)).ToList();

                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == CreateNewButton.CustomId)
                        {
                            FinalSelection = await ctx.Guild.CreateChannelAsync(configuration.CreateChannelOption.Name, configuration.CreateChannelOption.ChannelType);
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == DisableButton.CustomId)
                        {
                            FinalSelection = null;
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == ConfirmSelectionButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            FinalSelection = ctx.Guild.GetChannel(Convert.ToUInt64(Selected));
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                            throw new CancelException();
                    }
                }
                catch (Exception ex)
                {
                    ThrownException = ex;
                    ExceptionOccurred = true;
                    FinishedSelection = true;
                }
            });
        }

        ctx.Client.ComponentInteractionCreated += RunInteraction;

        while (!FinishedSelection && sw.Elapsed <= timeOutOverride)
        {
            await Task.Delay(100);
        }

        ctx.Client.ComponentInteractionCreated -= RunInteraction;
        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(ctx.ResponseMessage.Embeds[0]).WithContent(ctx.ResponseMessage.Content));

        if (ExceptionOccurred)
            return new InteractionResult<DiscordChannel>(ThrownException);

        if (sw.Elapsed >= timeOutOverride)
            return new InteractionResult<DiscordChannel>(new TimedOutException());

        return new InteractionResult<DiscordChannel>(FinalSelection);
    }

    public async Task<InteractionResult<string>> PromptCustomSelection(List<DiscordStringSelectComponentOption> options, string? CustomPlaceHolder = null, TimeSpan? timeOutOverride = null)
    {
        timeOutOverride ??= TimeSpan.FromSeconds(120);
        CustomPlaceHolder ??= GetString(t.Commands.Common.Prompts.SelectAnOption);

        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Common.Prompts.ConfirmSelection), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

        var PrevPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
        var NextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

        int CurrentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();
        

        string FinalSelection = null;

        string Selected = options.FirstOrDefault(x => x.Default, null)?.Value ?? "";

        bool FinishedSelection = false;
        bool ExceptionOccurred = false;
        Exception ThrownException = null;

        async Task RefreshMessage()
        {
            var dropdown = new DiscordStringSelectComponent(CustomPlaceHolder, options.Skip(CurrentPage * 25).Take(25).Select(x => new DiscordStringSelectComponentOption(x.Label, x.Value, x.Description, (x.Value == Selected), x.Emoji)), SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]).AsAwaitingInput(ctx)).AddComponents(dropdown).WithContent(ctx.ResponseMessage.Content);

            if (options.Skip(CurrentPage * 25).Count() > 25)
                builder.AddComponents(NextPageButton);

            if (CurrentPage != 0)
                builder.AddComponents(PrevPageButton);

            if (Selected.IsNullOrWhiteSpace())
                ConfirmSelectionButton.Disable();
            else
                ConfirmSelectionButton.Enable();

            builder.AddComponents(MessageComponents.GetCancelButton(ctx.DbUser), ConfirmSelectionButton);

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

                        if (e.GetCustomId() == SelectionInteractionId)
                        {
                            Selected = e.Values.First();
                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == ConfirmSelectionButton.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            FinalSelection = Selected;

                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == PrevPageButton.CustomId)
                        {
                            CurrentPage--;
                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == NextPageButton.CustomId)
                        {
                            CurrentPage++;
                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                            throw new CancelException();
                    }
                }
                catch (Exception ex)
                {
                    ThrownException = ex;
                    ExceptionOccurred = true;
                    FinishedSelection = true;
                }
            });
        }

        ctx.Client.ComponentInteractionCreated += RunInteraction;

        while (!FinishedSelection && sw.Elapsed <= timeOutOverride)
        {
            await Task.Delay(100);
        }

        ctx.Client.ComponentInteractionCreated -= RunInteraction;

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(ctx.ResponseMessage.Embeds[0]).WithContent(ctx.ResponseMessage.Content));

        if (ExceptionOccurred)
            return new InteractionResult<string>(ThrownException);

        if (sw.Elapsed >= timeOutOverride)
            return new InteractionResult<string>(new TimedOutException());

        return new InteractionResult<string>(FinalSelection);
    }
    #endregion

    #region Modals
    public async Task<InteractionResult<ComponentInteractionCreateEventArgs>> PromptModalWithRetry(DiscordInteraction interaction, DiscordInteractionModalBuilder builder, bool ResetToOriginalEmbed = false, TimeSpan? timeOutOverride = null) 
        => await PromptModalWithRetry(interaction, builder, null, ResetToOriginalEmbed, timeOutOverride);

    public async Task<InteractionResult<ComponentInteractionCreateEventArgs>> PromptModalWithRetry(DiscordInteraction interaction, DiscordInteractionModalBuilder builder, DiscordEmbedBuilder customEmbed = null, bool ResetToOriginalEmbed = false, TimeSpan? timeOutOverride = null, bool open = true)
    {
        timeOutOverride ??= TimeSpan.FromMinutes(15);

        var oriEmbed = ctx.ResponseMessage.Embeds[0];

        var ReOpen = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.Commands.Common.Prompts.ReOpenModal), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔄")));

        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(customEmbed ?? new DiscordEmbedBuilder
        {
            Description = GetString(t.Commands.Common.Prompts.WaitingForModalResponse, true)
        }.AsAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { ReOpen, MessageComponents.GetCancelButton(ctx.DbUser) }));

        ComponentInteractionCreateEventArgs FinishedInteraction = null;

        bool FinishedSelection = false;
        bool ExceptionOccurred = false;
        bool Cancelled = false;
        Exception ThrownException = null;

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
                        if (e.GetCustomId() == builder.CustomId)
                        {
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            FinishedInteraction = e;
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == ReOpen.CustomId)
                        {
                            await e.Interaction.CreateInteractionModalResponseAsync(builder);
                        }
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            throw new CancelException();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ThrownException = ex;
                    ExceptionOccurred = true;
                    FinishedSelection = true;
                }
            }).Add(ctx.Bot.watcher, ctx);
        }

        int TimeoutSeconds = (int)(timeOutOverride.Value.TotalSeconds * 2);

        while (!FinishedSelection && !ExceptionOccurred && !Cancelled && TimeoutSeconds >= 0)
        {
            await Task.Delay(500);
            TimeoutSeconds--;
        }

        ctx.Client.ComponentInteractionCreated -= RunInteraction;

        if (ResetToOriginalEmbed)
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(oriEmbed));

        if (ExceptionOccurred)
            return new InteractionResult<ComponentInteractionCreateEventArgs>(ThrownException);

        if (TimeoutSeconds <= 0)
            return new InteractionResult<ComponentInteractionCreateEventArgs>(new TimeoutException());

        return new InteractionResult<ComponentInteractionCreateEventArgs>(FinishedInteraction);
    }


    public async Task<InteractionResult<TimeSpan>> PromptModalForTimeSpan(DiscordInteraction interaction, TimeSpan? MaxTime = null, TimeSpan? MinTime = null, TimeSpan? DefaultTime = null, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null)
    {
        MinTime ??= TimeSpan.Zero;
        MaxTime ??= TimeSpan.FromDays(356);
        DefaultTime ??= TimeSpan.FromSeconds(30);

        var modal = new DiscordInteractionModalBuilder().WithTitle(GetString(t.Commands.Common.Prompts.SelectATimeSpan)).WithCustomId(Guid.NewGuid().ToString());

        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "seconds", GetString(t.Commands.Common.Prompts.TimespanSeconds).Build(new TVar("Max", 59)), "0", 1, 2, true, $"{DefaultTime.Value.Seconds}"));

        if (MaxTime.Value.TotalMinutes >= 1)
            modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minutes", GetString(t.Commands.Common.Prompts.TimespanMinutes).Build(new TVar("Max", (MaxTime.Value.TotalMinutes >= 60 ? "59" : $"{((int)MaxTime.Value.TotalMinutes)}"))), $"0", 1, 2, true, $"{DefaultTime.Value.Minutes}"));

        if (MaxTime.Value.TotalHours >= 1)
            modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "hours", GetString(t.Commands.Common.Prompts.TimespanHours).Build(new TVar("Max", (MaxTime.Value.TotalHours >= 24 ? "23" : $"{((int)MaxTime.Value.TotalHours)}"))), "0", 1, 2, true, $"{DefaultTime.Value.Hours}"));

        if (MaxTime.Value.TotalDays >= 1)
            modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "days", GetString(t.Commands.Common.Prompts.TimespanDays).Build(new TVar("Max", ((int)MaxTime.Value.TotalDays))), "0", 1, 3, true, $"{DefaultTime.Value.Days}"));

        var ModalResult = await PromptModalWithRetry(interaction, modal, false);

        if (ModalResult.TimedOut)
        {
            return new InteractionResult<TimeSpan>(new TimedOutException());
        }
        else if (ModalResult.Cancelled)
        {
            return new InteractionResult<TimeSpan>(new CancelException());
        }
        else if (ModalResult.Errored)
        {
            return new InteractionResult<TimeSpan>(ModalResult.Exception);
        }

        InteractionCreateEventArgs Response = ModalResult.Result;

        TimeSpan length = TimeSpan.FromSeconds(0);

        try
        {
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
        }
        catch (Exception ex)
        {
            return new InteractionResult<TimeSpan>(ex);
        }

        if (length > MaxTime || length < MinTime)
            return new InteractionResult<TimeSpan>(new InvalidOperationException("Invalid TimeSpan"));

        return new InteractionResult<TimeSpan>(length);
    }

    public async Task<InteractionResult<DateTime>> PromptModalForDateTime(DiscordInteraction interaction, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null)
    {
        var modal = new DiscordInteractionModalBuilder().WithTitle(GetString(t.Commands.Common.Prompts.SelectADateTime)).WithCustomId(Guid.NewGuid().ToString());

        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minute", GetString(t.Commands.Common.Prompts.DateTimeMinute), GetString(t.Commands.Common.Prompts.DateTimeMinute), 1, 2, true, $"{DateTime.UtcNow.Minute}"));
        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "hour", GetString(t.Commands.Common.Prompts.DateTimeHour), GetString(t.Commands.Common.Prompts.DateTimeHour), 1, 2, true, $"{DateTime.UtcNow.Hour}"));
        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "day", GetString(t.Commands.Common.Prompts.DateTimeDay), GetString(t.Commands.Common.Prompts.DateTimeDay), 1, 2, true, $"{DateTime.UtcNow.Day}"));
        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "month", GetString(t.Commands.Common.Prompts.DateTimeMonth), GetString(t.Commands.Common.Prompts.DateTimeMonth), 1, 2, true, $"{DateTime.UtcNow.Month}"));
        modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "year", GetString(t.Commands.Common.Prompts.DateTimeYear), GetString(t.Commands.Common.Prompts.DateTimeYear), 1, 4, true, $"{DateTime.UtcNow.Year}"));


        var ModalResult = await PromptModalWithRetry(interaction, modal, false);

        if (ModalResult.TimedOut)
        {
            return new InteractionResult<DateTime>(new TimedOutException());
        }
        else if (ModalResult.Cancelled)
        {
            return new InteractionResult<DateTime>(new CancelException());
        }
        else if (ModalResult.Errored)
        {
            return new InteractionResult<DateTime>(ModalResult.Exception);
        }

        InteractionCreateEventArgs Response = ModalResult.Result;

        DateTime dateTime;

        try
        {
            if ((Response.Interaction.Data.Components.Any(x => x.CustomId == "hour") && !Response.Interaction.Data.Components.First(x => x.CustomId == "hour").Value.IsDigitsOnly()) ||
                (Response.Interaction.Data.Components.Any(x => x.CustomId == "minute") && !Response.Interaction.Data.Components.First(x => x.CustomId == "minute").Value.IsDigitsOnly()) ||
                (Response.Interaction.Data.Components.Any(x => x.CustomId == "day") && !Response.Interaction.Data.Components.First(x => x.CustomId == "day").Value.IsDigitsOnly()) ||
                (Response.Interaction.Data.Components.Any(x => x.CustomId == "month") && !Response.Interaction.Data.Components.First(x => x.CustomId == "month").Value.IsDigitsOnly()) ||
                (Response.Interaction.Data.Components.Any(x => x.CustomId == "year") && !Response.Interaction.Data.Components.First(x => x.CustomId == "year").Value.IsDigitsOnly()))
                throw new ArgumentException("Invalid date time");

            int hour = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("hour"));
            int minute = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("minute"));
            int day = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("day"));
            int month = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("month"));
            int year = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("year"));

            dateTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
        }
        catch (OverflowException)
        {
            return new InteractionResult<DateTime>(new InvalidOperationException());
        }
        catch (Exception ex)
        {
            return new InteractionResult<DateTime>(ex);
        }

        return new InteractionResult<DateTime>(dateTime);
    }
    #endregion

    public async Task<(Stream stream, int fileSize)> PromptForFileUpload(TimeSpan? timeOutOverride = null)
    {
        timeOutOverride ??= TimeSpan.FromMinutes(15);

        if (ctx.Bot.uploadInteractions.ContainsKey(ctx.User.Id))
        {
            if (ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.GetTotalSecondsUntil() > 0 && !ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`An upload interaction is already taking place. Please finish it beforehand.`",
                }.AsError(ctx)));

                throw new AlreadyAppliedException("");
            }

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
        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(ctx.ResponseMessage.Embeds[0]).WithFooter(ctx.ResponseMessage.Embeds[0]?.Footer?.Text + $" • {GetString(t.Commands.Common.InteractionTimeout)}").WithColor(DiscordColor.Gray)));

        if (Delete)
            Task.Delay(5000).ContinueWith(_ =>
            {
                if (!ctx.ResponseMessage?.Flags.Value.HasMessageFlag(MessageFlags.Ephemeral) ?? false)
                    ctx.ResponseMessage.DeleteAsync();
            });
    }

    public void DeleteOrInvalidate()
    {
        _ = RespondOrEdit($"✅ _{GetString(t.Commands.Common.InteractionFinished, true)}_");
        switch (ctx.CommandType)
        {
            case Enums.CommandType.ContextMenu:
            {
                _ = ctx.OriginalContextMenuContext.DeleteResponseAsync();
                break;
            }
            case Enums.CommandType.ApplicationCommand:
            {
                _ = ctx.OriginalInteractionContext.DeleteResponseAsync();
                break;
            }
            default:
            {
                _ = ctx.ResponseMessage.DeleteAsync();
                break;
            }
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

    public async Task<bool> CheckBotOwner()
    {
        if (!ctx.User.IsMaintenance(ctx.Bot.status))
        {
            SendBotOwnerError();
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
        Description = GetString(t.Commands.Common.Errors.NoMember)
    }.AsError(ctx));

    public void SendMaintenanceError()
        => _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = GetString(t.Commands.Common.Errors.Generic).Build(true, new TVar("Required", $"{ctx.CurrentUser.GetUsername()} Staff"))
        }.AsError(ctx));

    public void SendBotOwnerError()
    => _ = RespondOrEdit(new DiscordEmbedBuilder()
    {
        Description = GetString(t.Commands.Common.Errors.Generic).Build(true, new TVar("Required", $"<@{ctx.Bot.status.TeamOwner}>", false)),
    }.AsError(ctx));

    public void SendAdminError()
        => _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = GetString(t.Commands.Common.Errors.Generic).Build(true, new TVar("Required", "Administrator")),
        }.AsError(ctx));

    public void SendPermissionError(Permissions perms)
        => _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = GetString(t.Commands.Common.Errors.Generic).Build(true, new TVar("Required", perms.ToPermissionString())),
        }.AsError(ctx));

    public void SendVoiceStateError()
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = GetString(t.Commands.Common.Errors.VoiceChannel).Build(true),
        }.AsError(ctx)));

    public void SendUserBanError(BlacklistEntry entry)
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = GetString(t.Commands.Common.Errors.UserBan, true, new TVar("Reason", entry.Reason)),
        }.AsError(ctx)));

    public void SendGuildBanError(BlacklistEntry entry)
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = GetString(t.Commands.Common.Errors.GuildBan, true, new TVar("Reason", entry.Reason)),
        }.AsError(ctx)));

    public void SendSourceError(Enums.CommandType commandType)
        => _ = commandType switch
        {
            Enums.CommandType.ApplicationCommand => RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = GetString(t.Commands.Common.Errors.ExclusiveApp).Build(true),
            }.AsError(ctx)),
            Enums.CommandType.PrefixCommand => RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = GetString(t.Commands.Common.Errors.ExclusivePrefix).Build(true)
            }.AsError(ctx)),
            _ => throw new ArgumentException("Invalid Source defined."),
        };

    public void SendDataError()
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = GetString(t.Commands.Common.Errors.Data, true, new TVar("Command", $"{ctx.Prefix}data delete")),
        }.AsError(ctx)));

    public void SendDmError() 
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"📩 {GetString(t.Commands.Common.Errors.DirectMessage, true)}",
            ImageUrl = (ctx.User.Presence.ClientStatus.Mobile.HasValue ? "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif" : "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif")
        }.AsError(ctx)));
    
    public void SendDmRedirect() 
        => _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"📩 {GetString(t.Commands.Common.DirectMessageRedirect, true)}",
        }.AsSuccess(ctx)));

    public void SendOwnPermissionError(Permissions perms)
    {
        if (perms is Permissions.AccessChannels or Permissions.SendMessages or Permissions.EmbedLinks)
            return;

        _ = RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = GetString(t.Commands.Common.Errors.BotPermissions, true, new TVar("Required", perms.ToPermissionString()))
        }.AsError(ctx));
    }

    public void SendSyntaxError()
    {
        if (this.ctx.CommandType != Enums.CommandType.PrefixCommand)
            throw new ArgumentException("Syntax Error can only be generated for Prefix Commands.");

        var ctx = this.ctx.OriginalCommandContext;

        var embed = new DiscordEmbedBuilder
        {
            Description = $"**`{ctx.Prefix}{ctx.Command.Name}{(ctx.RawArgumentString != "" ? $" {ctx.RawArgumentString.SanitizeForCode().Replace("\\", "")}" : "")}` is not a valid way of using this command.**\nUse it like this instead: `{ctx.Prefix}{ctx.Command.GenerateUsage()}`\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
        }.AsError(this.ctx);

        _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).WithContent(this.ctx.User.Mention));
    }
    #endregion
}
