// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using DisCatSharp.Extensions.TwoFactorCommands.Enums;

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

    public async Task TransferCommand(SharedCommandContext ctx, Dictionary<string, object> arguments = null)
    {
        this.t = ctx.Bot.LoadedTranslations;
        this.ctx = ctx;

        ctx.Transferred = true;

        if (await this.BasePreExecutionCheck())
            await this.ExecuteCommand(this.ctx, arguments).Add(ctx.Bot, this.ctx);
    }

    public async Task ExecuteCommand(CommandContext ctx, Bot _bot, Dictionary<string, object> arguments = null)
    {
        this.ctx = new SharedCommandContext(this, ctx, _bot);
        this.t = _bot.LoadedTranslations;

        if (await this.BasePreExecutionCheck())
            await this.ExecuteCommand(this.ctx, arguments).Add(_bot, this.ctx);
    }

    public async Task ExecuteCommand(InteractionContext ctx, Bot _bot, Dictionary<string, object> arguments = null, bool Ephemeral = true, bool InitiateInteraction = true, bool InteractionInitiated = false)
    {
        this.ctx = new SharedCommandContext(this, ctx, _bot);
        this.t = _bot.LoadedTranslations;

        await Task.Run(async () =>
        {
            if (InitiateInteraction)
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    IsEphemeral = Ephemeral
                });

            this.ctx.RespondedToInitial = InitiateInteraction;

            if (InteractionInitiated)
                this.ctx.RespondedToInitial = true;

            if (await this.BasePreExecutionCheck())
                await this.ExecuteCommand(this.ctx, arguments).Add(_bot, this.ctx);
        }).Add(_bot, this.ctx);
    }

    public async Task ExecuteCommandWith2FA(InteractionContext ctx, Bot _bot, Dictionary<string, object> arguments = null)
    {
        this.ctx = new SharedCommandContext(this, ctx, _bot);
        this.t = _bot.LoadedTranslations;

        await Task.Run(async () =>
        {
            this.ctx.RespondedToInitial = false;

            if (!this.ctx.Bot.status.LoadedConfig.IsDev)
                if (!ctx.Client.CheckTwoFactorEnrollmentFor(ctx.User.Id))
                {
                    _ = ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                    {
                        Description = "`Please enroll in Two Factor Authentication via 'Enroll2FA'.`"
                    }.AsError(this.ctx)).AsEphemeral());
                    return;
                }
                else
                {
                    if (_bot.Users[ctx.User.Id].LastSuccessful2FA.GetTimespanSince() > TimeSpan.FromMinutes(3))
                    {
                        this.ctx.RespondedToInitial = true;
                        var tfa = await ctx.RequestTwoFactorAsync();

                        if (tfa.Result is TwoFactorResult.ValidCode or TwoFactorResult.InvalidCode)
                            await this.SwitchToEvent(tfa.ComponentInteraction);

                        if (tfa.Result != TwoFactorResult.ValidCode)
                        {
                            _ = this.RespondOrEdit(new DiscordMessageBuilder().WithContent("Invalid Code."));
                            return;
                        }
                        _bot.Users[ctx.User.Id].LastSuccessful2FA = DateTime.UtcNow;
                    }
                }

            if (!this.ctx.RespondedToInitial)
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    IsEphemeral = true
                });

            if (await this.BasePreExecutionCheck())
                await this.ExecuteCommand(this.ctx, arguments).Add(_bot, this.ctx);
        }).Add(_bot, this.ctx);
    }

    public async Task ExecuteCommand(ContextMenuContext ctx, Bot _bot, Dictionary<string, object> arguments = null, bool Ephemeral = true, bool InitiateInteraction = true, bool InteractionInitiated = false)
    {
        this.ctx = new SharedCommandContext(this, ctx, _bot);
        this.t = _bot.LoadedTranslations;

        await Task.Run(async () =>
        {
            if (InitiateInteraction)
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    IsEphemeral = Ephemeral
                });

            this.ctx.RespondedToInitial = InitiateInteraction;

            if (InteractionInitiated)
                this.ctx.RespondedToInitial = true;

            if (await this.BasePreExecutionCheck())
                await this.ExecuteCommand(this.ctx, arguments).Add(_bot, this.ctx);
        }).Add(_bot, this.ctx);
    }

    public async Task ExecuteCommand(ComponentInteractionCreateEventArgs ctx, DiscordClient client, string commandName, Bot _bot, Dictionary<string, object> arguments = null, bool Ephemeral = true, bool InitiateInteraction = true, bool InteractionInitiated = false)
    {
        this.ctx = new SharedCommandContext(this, ctx, client, commandName, _bot);
        this.t = _bot.LoadedTranslations;

        await Task.Run(async () =>
        {
            if (InitiateInteraction)
                await ctx.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    IsEphemeral = Ephemeral
                });

            this.ctx.RespondedToInitial = InitiateInteraction;

            if (InteractionInitiated)
                this.ctx.RespondedToInitial = true;

            if (await this.BasePreExecutionCheck())
                await this.ExecuteCommand(this.ctx, arguments).Add(_bot, this.ctx);
        }).Add(_bot, ctx);
    }

    private async Task<bool> BasePreExecutionCheck()
    {
        if (this.t is null)
        {
            _logger.LogWarn($"The translation were not set before the BasePreExecutionCheck()!");
            this.t = this.ctx.Bot.LoadedTranslations;
        }

        if (this.ctx.Bot.Users.ContainsKey(this.ctx.User.Id) && !this.ctx.User.Locale.IsNullOrWhiteSpace() && this.ctx.DbUser.CurrentLocale != this.ctx.User.Locale)
        {
            this.ctx.DbUser.CurrentLocale = this.ctx.User.Locale;
            _logger.LogDebug("Updated language for User '{User}' to '{Locale}'", this.ctx.User.Id, this.ctx.User.Locale);
        }

        if (this.ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Contains(this.ctx.ParentCommandName))
        {
            this.SendDisabledCommandError(this.ctx.ParentCommandName);
            return false;
        }

        if (this.ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Contains(this.ctx.CommandName))
        {
            this.SendDisabledCommandError(this.ctx.CommandName);
            return false;
        }

        if (!this.ctx.Channel.IsPrivate)
        {
            if (this.ctx.Bot.Guilds.ContainsKey(this.ctx.Guild.Id) && !this.ctx.Guild.PreferredLocale.IsNullOrWhiteSpace() && this.ctx.Bot.Guilds[this.ctx.Guild.Id].CurrentLocale != this.ctx.Guild.PreferredLocale)
            {
                this.ctx.Bot.Guilds[this.ctx.Guild.Id].CurrentLocale = this.ctx.Guild.PreferredLocale;
                _logger.LogDebug("Updated language for Guild '{Guild}' to '{Locale}'", this.ctx.Guild.Id, this.ctx.Guild.PreferredLocale);
            }

            if (!(await this.CheckOwnPermissions(Permissions.SendMessages)))
                return false;

            if (!(await this.CheckOwnPermissions(Permissions.EmbedLinks)))
                return false;

            if (!(await this.CheckOwnPermissions(Permissions.AddReactions)))
                return false;

            if (!(await this.CheckOwnPermissions(Permissions.AccessChannels)))
                return false;

            if (!(await this.CheckOwnPermissions(Permissions.AttachFiles)))
                return false;

            if (!(await this.CheckOwnPermissions(Permissions.ManageMessages)))
                return false;

            if (!(await this.BeforeExecution(this.ctx)))
                return false;
        }

        if ((this.ctx.Bot.objectedUsers.Contains(this.ctx.User.Id) || this.ctx.DbUser.Data.DeletionRequested) && this.ctx.CommandName != "data" && this.ctx.CommandName != "delete")
        {
            this.SendDataError();
            return false;
        }

        if (this.ctx.Bot.bannedUsers.TryGetValue(this.ctx.User.Id, out var blacklistedUserDetails))
        {
            this.SendUserBanError(blacklistedUserDetails);
            return false;
        }

        if (this.ctx.Bot.bannedGuilds.TryGetValue(this.ctx.Guild?.Id ?? 0, out var blacklistedGuildDetails))
        {
            this.SendGuildBanError(blacklistedGuildDetails);
            return false;
        }


        return !this.ctx.User.IsBot;
    }
    #endregion

    public async Task SwitchToEvent(ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
        {
            IsEphemeral = true
        });
        this.ctx.RespondedToInitial = true;
        this.ctx.OriginalComponentInteractionCreateEventArgs = e;
        this.ctx.CommandType = Enums.CommandType.Event;
    }

    #region RespondOrEdit
    public Task<DiscordMessage> RespondOrEdit(DiscordEmbed embed)
        => this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

    public Task<DiscordMessage> RespondOrEdit(DiscordEmbedBuilder embed)
        => this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.Build()));

    public Task<DiscordMessage> RespondOrEdit(string content)
        => this.RespondOrEdit(new DiscordMessageBuilder().WithContent(content));

    public async Task<DiscordMessage> RespondOrEdit(DiscordMessageBuilder discordMessageBuilder)
    {
        switch (this.ctx.CommandType)
        {
            case Enums.CommandType.ApplicationCommand:
            {
                DiscordWebhookBuilder discordWebhookBuilder = new();

                var files = new Dictionary<string, Stream>();

                foreach (var b in discordMessageBuilder.Files)
                    files.Add(b.Filename, b.Stream);

                _ = discordWebhookBuilder.AddComponents(discordMessageBuilder.Components);
                _ = discordWebhookBuilder.AddEmbeds(discordMessageBuilder.Embeds);
                _ = discordWebhookBuilder.AddFiles(files);
                discordWebhookBuilder.Content = discordMessageBuilder.Content;

                var msg = await this.ctx.OriginalInteractionContext.EditResponseAsync(discordWebhookBuilder);
                this.ctx.ResponseMessage = msg;
                return msg;
            }

            case Enums.CommandType.ContextMenu:
            {
                DiscordWebhookBuilder discordWebhookBuilder = new();

                var files = new Dictionary<string, Stream>();

                foreach (var b in discordMessageBuilder.Files)
                    files.Add(b.Filename, b.Stream);

                _ = discordWebhookBuilder.AddComponents(discordMessageBuilder.Components);
                _ = discordWebhookBuilder.AddEmbeds(discordMessageBuilder.Embeds);
                _ = discordWebhookBuilder.AddFiles(files);
                discordWebhookBuilder.Content = discordMessageBuilder.Content;

                var msg = await this.ctx.OriginalContextMenuContext.EditResponseAsync(discordWebhookBuilder);
                this.ctx.ResponseMessage = msg;
                return msg;
            }

            case Enums.CommandType.Event:
            {
                DiscordWebhookBuilder discordWebhookBuilder = new();

                var files = new Dictionary<string, Stream>();

                foreach (var b in discordMessageBuilder.Files)
                    files.Add(b.Filename, b.Stream);

                _ = discordWebhookBuilder.AddComponents(discordMessageBuilder.Components);
                _ = discordWebhookBuilder.AddEmbeds(discordMessageBuilder.Embeds);
                _ = discordWebhookBuilder.AddFiles(files);
                discordWebhookBuilder.Content = discordMessageBuilder.Content;

                var msg = await this.ctx.OriginalComponentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(discordWebhookBuilder);
                this.ctx.ResponseMessage = msg;
                return msg;
            }

            case Enums.CommandType.PrefixCommand:
            case Enums.CommandType.Custom:
            {
                if (this.ctx.ResponseMessage is not null)
                {
                    if ((discordMessageBuilder.Files?.Count ?? 0) > 0)
                        _ = discordMessageBuilder.KeepAttachments(false);

                    _ = await this.ctx.ResponseMessage.ModifyAsync(discordMessageBuilder);
                    this.ctx.ResponseMessage = await this.ctx.ResponseMessage.Refetch();

                    return this.ctx.ResponseMessage;
                }

                var msg = await this.ctx.Channel.SendMessageAsync(discordMessageBuilder);

                this.ctx.ResponseMessage = msg;

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
            new("CurrentCommand", this.ctx.Prefix + this.ctx.CommandName, false),
            new("Bot", this.ctx.CurrentUser.Mention, false),
            new("BotName", this.ctx.Client.CurrentApplication.Name, false),
            new("FullBot", this.ctx.CurrentUser.GetUsernameWithIdentifier(), false),
            new("BotDisplayName", this.ctx.CurrentUser.GetUsernameWithIdentifier(), false),
            new("User", this.ctx.User.Mention, false),
            new("UserName", this.ctx.User.GetUsername(), false),
            new("FullUser", this.ctx.User.GetUsernameWithIdentifier(), false),
            new("UserDisplayName", this.ctx.Member?.DisplayName ?? this.ctx.User.GetUsername(), false),
        };

    public string GetString(SingleTranslationKey key)
        => this.GetString(key, false, Array.Empty<TVar>());

    public string GetString(SingleTranslationKey key, params TVar[] vars)
        => this.GetString(key, false, vars);

    public string GetString(SingleTranslationKey key, bool Code = false, params TVar[] vars)
        => key.Get(this.ctx.DbUser).Build(Code, vars.Concat(this.GetDefaultVars()).ToArray());



    public string GetString(MultiTranslationKey key)
        => this.GetString(key, false, false, Array.Empty<TVar>());

    public string GetString(MultiTranslationKey key, params TVar[] vars)
        => this.GetString(key, false, false, vars);

    public string GetString(MultiTranslationKey key, bool Code = false, params TVar[] vars)
        => this.GetString(key, Code, false, vars);

    public string GetString(MultiTranslationKey key, bool Code = false, bool UseBoldMarker = false, params TVar[] vars)
        => key.Get(this.ctx.DbUser).Build(Code, UseBoldMarker, vars.Concat(this.GetDefaultVars()).ToArray());



    public string GetGuildString(SingleTranslationKey key)
        => this.GetGuildString(key, false, Array.Empty<TVar>());

    public string GetGuildString(SingleTranslationKey key, params TVar[] vars)
        => this.GetGuildString(key, false, vars);

    public string GetGuildString(SingleTranslationKey key, bool Code = false, params TVar[] vars)
        => key.Get(this.ctx.DbGuild).Build(Code, vars.Concat(this.GetDefaultVars()).ToArray());



    public string GetGuildString(MultiTranslationKey key)
        => this.GetGuildString(key, false, false, Array.Empty<TVar>());

    public string GetGuildString(MultiTranslationKey key, params TVar[] vars)
        => this.GetGuildString(key, false, false, vars);

    public string GetGuildString(MultiTranslationKey key, bool Code = false, params TVar[] vars)
        => this.GetGuildString(key, Code, false, vars);

    public string GetGuildString(MultiTranslationKey key, bool Code = false, bool UseBoldMarker = false, params TVar[] vars)
        => key.Get(this.ctx.DbGuild).Build(Code, UseBoldMarker, vars.Concat(this.GetDefaultVars()).ToArray());
    #endregion

    #region Selections
    public async Task<InteractionResult<DiscordRole>> PromptRoleSelection(RolePromptConfiguration configuration = null, TimeSpan? timeOutOverride = null)
    {
        configuration ??= new();
        timeOutOverride ??= TimeSpan.FromSeconds(120);

        var CreateNewButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.CreateRoleForMe), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
        var DisableButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), configuration.DisableOption ?? this.GetString(this.t.Commands.Common.Prompts.Disable), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));
        var EveryoneButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.SelectEveryone), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👥")));
        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.ConfirmSelection), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));


        var SelectionInteractionId = Guid.NewGuid().ToString();

        DiscordRole FinalSelection = null;

        var Selected = "";

        var FinishedSelection = false;
        var ExceptionOccurred = false;
        Exception ThrownException = null;

        async Task RefreshMessage()
        {
            var dropdown = new DiscordRoleSelectComponent(this.GetString(this.t.Commands.Common.Prompts.SelectARole), SelectionInteractionId, 1, 1, false);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(this.ctx.ResponseMessage.Embeds[0]).AsAwaitingInput(this.ctx)).AddComponents(dropdown).WithContent(this.ctx.ResponseMessage.Content);

            if (Selected.IsNullOrWhiteSpace())
                _ = ConfirmSelectionButton.Disable();
            else
                _ = ConfirmSelectionButton.Enable();

            List<DiscordComponent> components = new();

            if (!configuration.CreateRoleOption.IsNullOrWhiteSpace())
                components.Add(CreateNewButton);

            if (!configuration.DisableOption.IsNullOrWhiteSpace())
                components.Add(DisableButton);

            if (configuration.IncludeEveryone)
                components.Add(EveryoneButton);

            if (components.Count != 0)
                _ = builder.AddComponents(components);

            _ = builder.AddComponents(MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot), ConfirmSelectionButton);

            _ = await this.RespondOrEdit(builder);
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
                                var role = this.ctx.Guild.GetRole(Convert.ToUInt64(Selected));

                                if (role.IsManaged || this.ctx.Member.GetRoleHighestPosition() <= role.Position)
                                {
                                    Selected = "";
                                    _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AsEphemeral().WithContent($"❌ {this.GetString(this.t.Commands.Common.Prompts.SelectedRoleUnavailable, true)}"));
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
                            FinalSelection = this.ctx.Guild.EveryoneRole;
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == ConfirmSelectionButton.CustomId)
                        {
                            this.ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            FinalSelection = this.ctx.Guild.GetRole(Convert.ToUInt64(Selected));
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot).CustomId)
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

        this.ctx.Client.ComponentInteractionCreated += RunInteraction;

        while (!FinishedSelection && sw.Elapsed <= timeOutOverride)
        {
            await Task.Delay(100);
        }

        this.ctx.Client.ComponentInteractionCreated -= RunInteraction;

        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(this.ctx.ResponseMessage.Embeds[0]).WithContent(this.ctx.ResponseMessage.Content));

        if (ExceptionOccurred)
            return new InteractionResult<DiscordRole>(ThrownException);

        return sw.Elapsed >= timeOutOverride
            ? new InteractionResult<DiscordRole>(new TimedOutException())
            : new InteractionResult<DiscordRole>(FinalSelection);
    }

    public Task<InteractionResult<DiscordChannel>> PromptChannelSelection(ChannelType? channelType = null, ChannelPromptConfiguration configuration = null, TimeSpan? timeOutOverride = null)
        => this.PromptChannelSelection(((channelType is null || !channelType.HasValue) ? null : new ChannelType[] { channelType.Value }), configuration, timeOutOverride);

    public async Task<InteractionResult<DiscordChannel>> PromptChannelSelection(ChannelType[]? channelTypes = null, ChannelPromptConfiguration configuration = null, TimeSpan? timeOutOverride = null)
    {
        configuration ??= new();
        timeOutOverride ??= TimeSpan.FromSeconds(120);

        List<DiscordStringSelectComponentOption> FetchedChannels = new();

        var CreateNewButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.CreateChannelForMe), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
        var DisableButton = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), configuration.DisableOption ?? this.GetString(this.t.Commands.Common.Prompts.Disable), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));
        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.ConfirmSelection), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

        var SelectionInteractionId = Guid.NewGuid().ToString();

        DiscordChannel FinalSelection = null;

        var Selected = "";

        var FinishedSelection = false;
        var ExceptionOccurred = false;
        Exception ThrownException = null;

        async Task RefreshMessage()
        {
            var dropdown = new DiscordChannelSelectComponent(this.GetString(this.t.Commands.Common.Prompts.SelectAChannel), channelTypes, SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(this.ctx.ResponseMessage.Embeds[0]).AsAwaitingInput(this.ctx)).AddComponents(dropdown).WithContent(this.ctx.ResponseMessage.Content);

            if (Selected.IsNullOrWhiteSpace())
                _ = ConfirmSelectionButton.Disable();
            else
                _ = ConfirmSelectionButton.Enable();

            List<DiscordComponent> components = new();

            if (configuration.CreateChannelOption is not null)
                components.Add(CreateNewButton);

            if (!configuration.DisableOption.IsNullOrWhiteSpace())
                components.Add(DisableButton);

            if (components.Count > 0)
                _ = builder.AddComponents(components);

            _ = builder.AddComponents(MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot), ConfirmSelectionButton);

            _ = await this.RespondOrEdit(builder);
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
                            Selected = e.Values.First();
                            FetchedChannels = FetchedChannels.Select(x => new DiscordStringSelectComponentOption(x.Label, x.Value, x.Description, (x.Value == Selected), x.Emoji)).ToList();

                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == CreateNewButton.CustomId)
                        {
                            FinalSelection = await this.ctx.Guild.CreateChannelAsync(configuration.CreateChannelOption.Name, configuration.CreateChannelOption.ChannelType);
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == DisableButton.CustomId)
                        {
                            FinalSelection = null;
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == ConfirmSelectionButton.CustomId)
                        {
                            this.ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            FinalSelection = this.ctx.Guild.GetChannel(Convert.ToUInt64(Selected));
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot).CustomId)
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

        this.ctx.Client.ComponentInteractionCreated += RunInteraction;

        while (!FinishedSelection && sw.Elapsed <= timeOutOverride)
        {
            await Task.Delay(100);
        }

        this.ctx.Client.ComponentInteractionCreated -= RunInteraction;
        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(this.ctx.ResponseMessage.Embeds[0]).WithContent(this.ctx.ResponseMessage.Content));

        if (ExceptionOccurred)
            return new InteractionResult<DiscordChannel>(ThrownException);

        return sw.Elapsed >= timeOutOverride
            ? new InteractionResult<DiscordChannel>(new TimedOutException())
            : new InteractionResult<DiscordChannel>(FinalSelection);
    }

    public async Task<InteractionResult<string>> PromptCustomSelection(IEnumerable<DiscordStringSelectComponentOption> options, string? CustomPlaceHolder = null, TimeSpan? timeOutOverride = null)
    {
        timeOutOverride ??= TimeSpan.FromSeconds(120);
        CustomPlaceHolder ??= this.GetString(this.t.Commands.Common.Prompts.SelectAnOption);

        var ConfirmSelectionButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.ConfirmSelection), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

        var PrevPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
        var NextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

        var CurrentPage = 0;
        var SelectionInteractionId = Guid.NewGuid().ToString();

        string FinalSelection = null;

        var Selected = options.FirstOrDefault(x => x.Default, null)?.Value ?? "";

        var FinishedSelection = false;
        var ExceptionOccurred = false;
        Exception ThrownException = null;

        while (!Selected.IsNullOrWhiteSpace() && !options.Skip(CurrentPage * 25).Take(25).Any(x => x.Value == Selected))
        {
            if (!options.Skip(CurrentPage * 25).Take(25).Any())
            {
                CurrentPage = 0;
                break;
            }

            CurrentPage++;
        }

        async Task RefreshMessage()
        {
            var dropdown = new DiscordStringSelectComponent(CustomPlaceHolder, options.Skip(CurrentPage * 25).Take(25).Select(x => new DiscordStringSelectComponentOption(x.Label, x.Value, x.Description, (x.Value == Selected), x.Emoji)), SelectionInteractionId);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(this.ctx.ResponseMessage.Embeds[0]).AsAwaitingInput(this.ctx)).AddComponents(dropdown).WithContent(this.ctx.ResponseMessage.Content);

            NextPageButton.Disabled = options.Skip(CurrentPage * 25).Count() <= 25;
            PrevPageButton.Disabled = CurrentPage == 0;
            _ = builder.AddComponents(PrevPageButton, NextPageButton);

            if (Selected.IsNullOrWhiteSpace())
                _ = ConfirmSelectionButton.Disable();
            else
                _ = ConfirmSelectionButton.Enable();

            _ = builder.AddComponents(MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot), ConfirmSelectionButton);

            _ = await this.RespondOrEdit(builder);
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
                            Selected = e.Values.First();
                            await RefreshMessage();
                        }
                        else if (e.GetCustomId() == ConfirmSelectionButton.CustomId)
                        {
                            this.ctx.Client.ComponentInteractionCreated -= RunInteraction;

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
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot).CustomId)
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

        this.ctx.Client.ComponentInteractionCreated += RunInteraction;

        while (!FinishedSelection && sw.Elapsed <= timeOutOverride)
        {
            await Task.Delay(100);
        }

        this.ctx.Client.ComponentInteractionCreated -= RunInteraction;

        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(this.ctx.ResponseMessage.Embeds[0]).WithContent(this.ctx.ResponseMessage.Content));

        if (ExceptionOccurred)
            return new InteractionResult<string>(ThrownException);

        return sw.Elapsed >= timeOutOverride
            ? new InteractionResult<string>(new TimedOutException())
            : new InteractionResult<string>(FinalSelection);
    }
    #endregion

    #region Modals
    public Task<InteractionResult<ComponentInteractionCreateEventArgs>> PromptModalWithRetry(DiscordInteraction interaction, DiscordInteractionModalBuilder builder, bool ResetToOriginalEmbed = false, TimeSpan? timeOutOverride = null)
        => this.PromptModalWithRetry(interaction, builder, null, ResetToOriginalEmbed, timeOutOverride);

    public async Task<InteractionResult<ComponentInteractionCreateEventArgs>> PromptModalWithRetry(DiscordInteraction interaction, DiscordInteractionModalBuilder builder, DiscordEmbedBuilder customEmbed = null, bool ResetToOriginalEmbed = false, TimeSpan? timeOutOverride = null, bool open = true)
    {
        timeOutOverride ??= TimeSpan.FromMinutes(15);

        var oriEmbed = this.ctx.ResponseMessage.Embeds[0];

        var ReOpen = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.ReOpenModal), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔄")));

        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(customEmbed ?? new DiscordEmbedBuilder
        {
            Description = this.GetString(this.t.Commands.Common.Prompts.WaitingForModalResponse, true)
        }.AsAwaitingInput(this.ctx)).AddComponents(new List<DiscordComponent> { ReOpen, MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot) }));

        ComponentInteractionCreateEventArgs FinishedInteraction = null;

        var FinishedSelection = false;
        var ExceptionOccurred = false;
        var Cancelled = false;
        Exception ThrownException = null;

        if (open)
            await interaction.CreateInteractionModalResponseAsync(builder);

        this.ctx.Client.ComponentInteractionCreated += RunInteraction;

        async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (e.Message?.Id == this.ctx.ResponseMessage.Id && e.User.Id == this.ctx.User.Id)
                    {
                        if (e.GetCustomId() == builder.CustomId)
                        {
                            this.ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            FinishedInteraction = e;
                            FinishedSelection = true;
                        }
                        else if (e.GetCustomId() == ReOpen.CustomId)
                        {
                            await e.Interaction.CreateInteractionModalResponseAsync(builder);
                        }
                        else if (e.GetCustomId() == MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot).CustomId)
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
            }).Add(this.ctx.Bot, this.ctx);
        }

        var TimeoutSeconds = (int)(timeOutOverride.Value.TotalSeconds * 2);

        while (!FinishedSelection && !ExceptionOccurred && !Cancelled && TimeoutSeconds >= 0)
        {
            await Task.Delay(500);
            TimeoutSeconds--;
        }

        this.ctx.Client.ComponentInteractionCreated -= RunInteraction;

        if (ResetToOriginalEmbed)
            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(oriEmbed));

        if (ExceptionOccurred)
            return new InteractionResult<ComponentInteractionCreateEventArgs>(ThrownException);

        return TimeoutSeconds <= 0
            ? new InteractionResult<ComponentInteractionCreateEventArgs>(new TimeoutException())
            : new InteractionResult<ComponentInteractionCreateEventArgs>(FinishedInteraction);
    }


    public async Task<InteractionResult<TimeSpan>> PromptForTimeSpan(DiscordInteraction interaction, TimeSpan? MaxTime = null, TimeSpan? MinTime = null, TimeSpan? DefaultTime = null, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null)
    {
        _ = interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

        MinTime ??= TimeSpan.Zero;
        MaxTime ??= TimeSpan.FromDays(356);
        DefaultTime ??= TimeSpan.FromSeconds(30);
        timeOutOverride ??= TimeSpan.FromSeconds(300);

        var originalEmbed = ResetToOriginalEmbed ? this.ctx.ResponseMessage.Embeds : null;

        var removeSeconds = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "10s", false, "➖".UnicodeToEmoji().ToComponent());
        var removeSecond = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "1s", false, "➖".UnicodeToEmoji().ToComponent());
        var addSecond = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "1s", false, "➕".UnicodeToEmoji().ToComponent());
        var addSeconds = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "10s", false, "➕".UnicodeToEmoji().ToComponent());
        
        var removeMinutes = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "10m", false, "➖".UnicodeToEmoji().ToComponent());
        var removeMinute = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "1m", false, "➖".UnicodeToEmoji().ToComponent());
        var addMinute = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "1m", false, "➕".UnicodeToEmoji().ToComponent());
        var addMinutes = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "10m", false, "➕".UnicodeToEmoji().ToComponent());
        
        var removeHours = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "10h", false, "➖".UnicodeToEmoji().ToComponent());
        var removeHour = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "1h", false, "➖".UnicodeToEmoji().ToComponent());
        var addHour = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "1h", false, "➕".UnicodeToEmoji().ToComponent());
        var addHours = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "10h", false, "➕".UnicodeToEmoji().ToComponent());
        
        var removeDays = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "10d", false, "➖".UnicodeToEmoji().ToComponent());
        var removeDay = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "1d", false, "➖".UnicodeToEmoji().ToComponent());
        var addDay = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "1d", false, "➕".UnicodeToEmoji().ToComponent());
        var addDays = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "10d", false, "➕".UnicodeToEmoji().ToComponent());

        var setExact = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.ManuallyDefineTimespan), false, "🕒".UnicodeToEmoji().ToComponent());
        var confirmSelection = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.ConfirmSelection), false, "✅".UnicodeToEmoji().ToComponent());

        var previousSuccessSelected = DefaultTime!.Value;
        var currentSelectedTime = DefaultTime!.Value;

        Task UpdateMessage()
        {
            if (currentSelectedTime > MaxTime || MinTime > currentSelectedTime)
                currentSelectedTime = previousSuccessSelected;

            previousSuccessSelected = currentSelectedTime;

            var embed = new DiscordEmbedBuilder()
                .WithDescription($"`{this.GetString(this.t.Commands.Common.Prompts.CurrentTimespan)}`: `{currentSelectedTime.GetHumanReadable(TimeFormat.Days, TranslationUtil.GetTranslatedHumanReadableConfig(this.ctx.DbUser, this.ctx.Bot, true))}`")
                .AsAwaitingInput(this.ctx);

            return this.RespondOrEdit(new DiscordMessageBuilder()
                .AddEmbed(embed)
                .AddComponents(removeSeconds, removeSecond, addSecond, addSeconds)
                .AddComponents(removeMinutes, removeMinute, addMinute, addMinutes)
                .AddComponents(removeHours, removeHour, addHour, addHours)
                .AddComponents(removeDays, removeDay, addDay, addDays)
                .AddComponents(setExact, MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot), confirmSelection));
        }
        await UpdateMessage();

        var Finished = false;
        var Cancelled = false;
        var timeOut = Stopwatch.StartNew();

        async Task Interaction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Message?.Id == this.ctx.ResponseMessage?.Id)
                {
                    timeOut.Restart();

                    if (e.Id == removeSecond.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromSeconds(1));
                        await UpdateMessage();
                    }
                    else if (e.Id == removeSeconds.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromSeconds(10));
                        await UpdateMessage();
                    }
                    else if (e.Id == addSecond.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromSeconds(1));
                        await UpdateMessage();
                    }
                    else if (e.Id == addSeconds.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromSeconds(10));
                        await UpdateMessage();
                    }
                    else if (e.Id == removeMinute.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromMinutes(1));
                        await UpdateMessage();
                    }
                    else if (e.Id == removeMinutes.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromMinutes(10));
                        await UpdateMessage();
                    }
                    else if (e.Id == addMinute.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromMinutes(1));
                        await UpdateMessage();
                    }
                    else if (e.Id == addMinutes.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromMinutes(10));
                        await UpdateMessage();
                    }
                    else if (e.Id == removeHour.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromHours(1));
                        await UpdateMessage();
                    }
                    else if (e.Id == removeHours.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromHours(10));
                        await UpdateMessage();
                    }
                    else if (e.Id == addHour.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromHours(1));
                        await UpdateMessage();
                    }
                    else if (e.Id == addHours.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromHours(10));
                        await UpdateMessage();
                    }
                    else if (e.Id == removeDay.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromDays(1));
                        await UpdateMessage();
                    }
                    else if (e.Id == removeDays.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromDays(10));
                        await UpdateMessage();
                    }
                    else if (e.Id == addDay.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromDays(1));
                        await UpdateMessage();
                    }
                    else if (e.Id == addDays.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromDays(10));
                        await UpdateMessage();
                    }
                    else if (e.Id == confirmSelection.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        Finished = true;
                    }
                    else if (e.Id == MessageComponents.CancelButtonId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        Cancelled = true;
                    }
                    else if (e.Id == setExact.CustomId)
                    {

                        var modal = new DiscordInteractionModalBuilder().WithTitle(this.GetString(this.t.Commands.Common.Prompts.SelectATimeSpan)).WithCustomId(Guid.NewGuid().ToString());

                        if (MaxTime.Value.TotalDays >= 1)
                            _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "days", this.GetString(this.t.Commands.Common.Prompts.TimespanDays)
                                .Build(new TVar("Max", ((int)MaxTime.Value.TotalDays))), "0", 1, 3, true, $"{currentSelectedTime.Days}"));

                        if (MaxTime.Value.TotalHours >= 1)
                            _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "hours", this.GetString(this.t.Commands.Common.Prompts.TimespanHours)
                                .Build(new TVar("Max", (MaxTime.Value.TotalHours >= 24 ? "23" : $"{((int)MaxTime.Value.TotalHours)}"))), "0", 1, 2, true, $"{currentSelectedTime.Hours}"));

                        if (MaxTime.Value.TotalMinutes >= 1)
                            _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minutes", this.GetString(this.t.Commands.Common.Prompts.TimespanMinutes)
                                .Build(new TVar("Max", (MaxTime.Value.TotalMinutes >= 60 ? "59" : $"{((int)MaxTime.Value.TotalMinutes)}"))), $"0", 1, 2, true, $"{currentSelectedTime.Minutes}"));

                        _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "seconds", this.GetString(this.t.Commands.Common.Prompts.TimespanSeconds)
                            .Build(new TVar("Max", 59)), "0", 1, 2, true, $"{currentSelectedTime.Seconds}"));

                        var ModalResult = await this.PromptModalWithRetry(e.Interaction, modal, true, timeOutOverride.Value.Subtract(timeOut.Elapsed));

                        if (!ModalResult.Failed)
                        {
                            try
                            {
                                var Response = ModalResult.Result;
                                var modalLength = TimeSpan.FromSeconds(0);

                                if ((Response.Interaction.Data.Components.Any(x => x.CustomId == "seconds") && !Response.Interaction.Data.Components.First(x => x.CustomId == "seconds").Value.IsDigitsOnly()) ||
                                    (Response.Interaction.Data.Components.Any(x => x.CustomId == "minutes") && !Response.Interaction.Data.Components.First(x => x.CustomId == "minutes").Value.IsDigitsOnly()) ||
                                    (Response.Interaction.Data.Components.Any(x => x.CustomId == "hours") && !Response.Interaction.Data.Components.First(x => x.CustomId == "hours").Value.IsDigitsOnly()) ||
                                    (Response.Interaction.Data.Components.Any(x => x.CustomId == "days") && !Response.Interaction.Data.Components.First(x => x.CustomId == "days").Value.IsDigitsOnly()))
                                    throw new InvalidOperationException("Invalid TimeSpan");
                                var seconds = Response.Interaction.Data.Components.Any(x => x.CustomId == "seconds") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.CustomId == "seconds").Value)) : 0;
                                var minutes = Response.Interaction.Data.Components.Any(x => x.CustomId == "minutes") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.CustomId == "minutes").Value)) : 0;
                                var hours = Response.Interaction.Data.Components.Any(x => x.CustomId == "hours") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.CustomId == "hours").Value)) : 0;
                                var days = Response.Interaction.Data.Components.Any(x => x.CustomId == "days") ? Convert.ToDouble(Convert.ToUInt32(Response.Interaction.Data.Components.First(x => x.CustomId == "days").Value)) : 0;
                                modalLength = modalLength.Add(TimeSpan.FromSeconds(seconds));
                                modalLength = modalLength.Add(TimeSpan.FromMinutes(minutes));
                                modalLength = modalLength.Add(TimeSpan.FromHours(hours));
                                modalLength = modalLength.Add(TimeSpan.FromDays(days));

                                currentSelectedTime = modalLength;
                            }
                            catch { }
                        }

                        await UpdateMessage();
                    }
                }
            }).Add(this.ctx.Bot, this.ctx);
        }

        this.ctx.Client.ComponentInteractionCreated += Interaction;

        while (!Finished && !Cancelled && timeOut.ElapsedMilliseconds < timeOutOverride.Value.TotalMilliseconds)
            await Task.Delay(1000);

        this.ctx.Client.ComponentInteractionCreated -= Interaction;

        if (!Finished && !Cancelled && timeOut.ElapsedMilliseconds < timeOutOverride.Value.TotalMilliseconds)
            return new InteractionResult<TimeSpan>(new TimedOutException());
        
        if (Cancelled)
            return new InteractionResult<TimeSpan>(new CancelException());

        if (ResetToOriginalEmbed)
            _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(originalEmbed));

        return currentSelectedTime > MaxTime || currentSelectedTime < MinTime
            ? new InteractionResult<TimeSpan>(new InvalidOperationException("Invalid TimeSpan"))
            : new InteractionResult<TimeSpan>(currentSelectedTime);
    }

    public async Task<InteractionResult<DateTime>> PromptModalForDateTime(DiscordInteraction interaction, DateTime? defaultTime = null, bool ResetToOriginalEmbed = true, TimeSpan? timeOutOverride = null)
    {
        timeOutOverride ??= TimeSpan.FromMinutes(2);
        defaultTime ??= DateTime.UtcNow;

        var originalEmbed = ResetToOriginalEmbed ? this.ctx.ResponseMessage.Embeds : null;

        _ = interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

        var removeMinutes = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "10m", false, "➖".UnicodeToEmoji().ToComponent());
        var removeMinute = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "1m", false, "➖".UnicodeToEmoji().ToComponent());
        var addMinute = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "1m", false, "➕".UnicodeToEmoji().ToComponent());
        var addMinutes = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "10m", false, "➕".UnicodeToEmoji().ToComponent());

        var removeHours = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "10h", false, "➖".UnicodeToEmoji().ToComponent());
        var removeHour = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "1h", false, "➖".UnicodeToEmoji().ToComponent());
        var addHour = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "1h", false, "➕".UnicodeToEmoji().ToComponent());
        var addHours = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "10h", false, "➕".UnicodeToEmoji().ToComponent());

        var removeDays = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "10d", false, "➖".UnicodeToEmoji().ToComponent());
        var removeDay = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "1d", false, "➖".UnicodeToEmoji().ToComponent());
        var addDay = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "1d", false, "➕".UnicodeToEmoji().ToComponent());
        var addDays = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "10d", false, "➕".UnicodeToEmoji().ToComponent());

        var setExact = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.ManuallyDefineDateTime), false, "🕒".UnicodeToEmoji().ToComponent());
        var changeTimezone = new DiscordButtonComponent(ButtonStyle.Secondary, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.SelectTimezone), false, "🌐".UnicodeToEmoji().ToComponent());
        var confirmSelection = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Commands.Common.Prompts.ConfirmSelection), false, "✅".UnicodeToEmoji().ToComponent());

        var currentSelectedTime = defaultTime!.Value;

        Task UpdateMessage()
        {
            var embed = new DiscordEmbedBuilder()
                .WithDescription($"`{this.GetString(this.t.Commands.Common.Prompts.CurrentDateTime)}`: {currentSelectedTime.ToTimestamp()} ({currentSelectedTime.ToTimestamp(TimestampFormat.LongDateTime)})")
                .AsAwaitingInput(this.ctx);

            return this.RespondOrEdit(new DiscordMessageBuilder()
                .AddEmbed(embed)
                .AddComponents(removeMinutes, removeMinute, addMinute, addMinutes)
                .AddComponents(removeHours, removeHour, addHour, addHours)
                .AddComponents(removeDays, removeDay, addDay, addDays)
                .AddComponents(changeTimezone, setExact)
                .AddComponents(MessageComponents.GetCancelButton(this.ctx.DbUser, this.ctx.Bot), confirmSelection));
        }
        await UpdateMessage();

        var Finished = false;
        var Cancelled = false;
        var timeOut = Stopwatch.StartNew();

        async Task Interaction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Message?.Id == this.ctx.ResponseMessage?.Id)
                {
                    timeOut.Restart();

                    if (e.Id == removeMinute.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromMinutes(1));
                    }
                    else if (e.Id == removeMinutes.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromMinutes(10));
                    }
                    else if (e.Id == addMinute.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromMinutes(1));
                    }
                    else if (e.Id == addMinutes.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromMinutes(10));
                    }
                    else if (e.Id == removeHour.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromHours(1));
                    }
                    else if (e.Id == removeHours.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromHours(10));
                    }
                    else if (e.Id == addHour.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromHours(1));
                    }
                    else if (e.Id == addHours.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromHours(10));
                    }
                    else if (e.Id == removeDay.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromDays(1));
                    }
                    else if (e.Id == removeDays.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Subtract(TimeSpan.FromDays(10));
                    }
                    else if (e.Id == addDay.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromDays(1));
                    }
                    else if (e.Id == addDays.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        currentSelectedTime = currentSelectedTime.Add(TimeSpan.FromDays(10));
                    }
                    else if (e.Id == confirmSelection.CustomId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        Finished = true;
                        return;
                    }
                    else if (e.Id == MessageComponents.CancelButtonId)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        Cancelled = true;
                        return;
                    }
                    else if (e.Id == changeTimezone.CustomId)
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Common.Prompts.SelectTimezonePrompt, true)).AsAwaitingInput(this.ctx));

                        var promptResult = await this.PromptCustomSelection(TimeZoneInfo.GetSystemTimeZones()
                                .Select(x => new DiscordStringSelectComponentOption(x.DisplayName, x.Id, null, x.Id == (this.ctx.DbUser.Timezone ?? "UTC"))), null, timeOutOverride.Value.Subtract(timeOut.Elapsed));

                        if (promptResult.Failed)
                        {
                            await UpdateMessage();
                            return;
                        }

                        this.ctx.DbUser.Timezone = promptResult.Result;
                    }
                    else if (e.Id == setExact.CustomId)
                    {
                        var modalInteraction = e.Interaction;

                        if (this.ctx.DbUser.Timezone.IsNullOrWhiteSpace() || !TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id == this.ctx.DbUser.Timezone))
                        {
                            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Common.Prompts.SelectTimezonePrompt, true)).AsAwaitingInput(this.ctx));

                            var promptResult = await this.PromptCustomSelection(TimeZoneInfo.GetSystemTimeZones()
                                    .Select(x => new DiscordStringSelectComponentOption(x.DisplayName, x.Id, null, x.Id == (this.ctx.DbUser.Timezone ?? "UTC"))), null, timeOutOverride.Value.Subtract(timeOut.Elapsed));

                            if (promptResult.Failed)
                            {
                                await UpdateMessage();
                                return;
                            }

                            this.ctx.DbUser.Timezone = promptResult.Result;
                            modalInteraction = null;
                        }

                        var userTimezone = TimeZoneInfo.FindSystemTimeZoneById(this.ctx.DbUser.Timezone);
                        var userTime = TimeZoneInfo.ConvertTimeFromUtc(currentSelectedTime, userTimezone);

                        var modal = new DiscordInteractionModalBuilder().WithTitle(this.GetString(this.t.Commands.Common.Prompts.SelectADateTime)).WithCustomId(Guid.NewGuid().ToString());

                        _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "minute", this.GetString(this.t.Commands.Common.Prompts.DateTimeMinute), this.GetString(this.t.Commands.Common.Prompts.DateTimeMinute), 1, 2, true, $"{userTime.Minute}"));
                        _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "hour", this.GetString(this.t.Commands.Common.Prompts.DateTimeHour), this.GetString(this.t.Commands.Common.Prompts.DateTimeHour), 1, 2, true, $"{userTime.Hour}"));
                        _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "day", this.GetString(this.t.Commands.Common.Prompts.DateTimeDay), this.GetString(this.t.Commands.Common.Prompts.DateTimeDay), 1, 2, true, $"{userTime.Day}"));
                        _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "month", this.GetString(this.t.Commands.Common.Prompts.DateTimeMonth), this.GetString(this.t.Commands.Common.Prompts.DateTimeMonth), 1, 2, true, $"{userTime.Month}"));
                        _ = modal.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "year", this.GetString(this.t.Commands.Common.Prompts.DateTimeYear), this.GetString(this.t.Commands.Common.Prompts.DateTimeYear), 1, 4, true, $"{userTime.Year}"));

                        var ModalResult = await this.PromptModalWithRetry(modalInteraction, modal, null, false, timeOutOverride.Value.Subtract(timeOut.Elapsed), modalInteraction != null);

                        if (ModalResult.Errored)
                        {
                            await UpdateMessage();
                            return;
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

                            var hour = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("hour"));
                            var minute = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("minute"));
                            var day = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("day"));
                            var month = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("month"));
                            var year = Convert.ToInt32(Response.Interaction.GetModalValueByCustomId("year"));

                            dateTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified), userTimezone);
                        }
                        catch (Exception)
                        {
                            await UpdateMessage();
                            return;
                        }

                        currentSelectedTime = dateTime;
                    }
                    else
                    { return; }

                    await UpdateMessage();
                }
            }).Add(this.ctx.Bot, this.ctx);
        }

        this.ctx.Client.ComponentInteractionCreated += Interaction;

        while (!Finished && !Cancelled && timeOut.ElapsedMilliseconds < timeOutOverride.Value.TotalMilliseconds)
            await Task.Delay(1000);

        this.ctx.Client.ComponentInteractionCreated -= Interaction;

        if (!Finished && !Cancelled && timeOut.ElapsedMilliseconds < timeOutOverride.Value.TotalMilliseconds)
            return new InteractionResult<DateTime>(new TimedOutException());

        if (Cancelled)
            return new InteractionResult<DateTime>(new CancelException());

        if (ResetToOriginalEmbed)
            _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(originalEmbed));

        return new InteractionResult<DateTime>(currentSelectedTime);
    }
    #endregion

    public async Task<(Stream stream, int fileSize)> PromptForFileUpload(TimeSpan? timeOutOverride = null)
    {
        timeOutOverride ??= TimeSpan.FromMinutes(15);

        if (this.ctx.DbUser.PendingUserUpload is not null)
        {
            if (this.ctx.DbUser.PendingUserUpload.TimeOut.GetTotalSecondsUntil() > 0 && !this.ctx.DbUser.PendingUserUpload.InteractionHandled)
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`An upload interaction is already taking place. Please finish it beforehand.`",
                }.AsError(this.ctx)));

                throw new AlreadyAppliedException("");
            }

            this.ctx.DbUser.PendingUserUpload = null;
        }

        this.ctx.DbUser.PendingUserUpload = new UserUpload
        {
            TimeOut = DateTime.UtcNow.Add(timeOutOverride.Value)
        };

        while (this.ctx.DbUser.PendingUserUpload is not null && !this.ctx.DbUser.PendingUserUpload.InteractionHandled && this.ctx.DbUser.PendingUserUpload.TimeOut.GetTotalSecondsUntil() > 0)
        {
            await Task.Delay(500);
        }

        if (!this.ctx.DbUser.PendingUserUpload?.InteractionHandled ?? true)
            throw new ArgumentException("");

        var size = this.ctx.DbUser.PendingUserUpload.FileSize;
        var stream = this.ctx.DbUser.PendingUserUpload.UploadedData;

        this.ctx.DbUser.PendingUserUpload = null;
        return (stream, size);
    }

    #region FinishInteraction
    public void ModifyToTimedOut(bool Delete = false)
    {
        _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(this.ctx.ResponseMessage.Embeds[0]).WithFooter(this.ctx.ResponseMessage.Embeds[0]?.Footer?.Text + $" • {this.GetString(this.t.Commands.Common.InteractionTimeout)}").WithColor(DiscordColor.Gray)));

        if (Delete)
            _ = Task.Delay(5000).ContinueWith(_ =>
            {
                if (!this.ctx.ResponseMessage?.Flags.Value.HasMessageFlag(MessageFlags.Ephemeral) ?? false)
                    _ = this.ctx.ResponseMessage.DeleteAsync();
            });
    }

    public void DeleteOrInvalidate()
    {
        switch (this.ctx.CommandType)
        {
            case Enums.CommandType.ContextMenu:
            {
                _ = this.ctx.OriginalContextMenuContext.DeleteResponseAsync();
                break;
            }
            case Enums.CommandType.Event:
            {
                _ = this.ctx.OriginalComponentInteractionCreateEventArgs.Interaction.DeleteOriginalResponseAsync();
                break;
            }
            case Enums.CommandType.ApplicationCommand:
            {
                _ = this.ctx.OriginalInteractionContext.DeleteResponseAsync();
                break;
            }
            default:
            {
                _ = this.ctx.ResponseMessage?.DeleteAsync();
                break;
            }
        }
    }
    #endregion

    #region Checks
    public async Task<bool> CheckVoiceState()
    {
        if (this.ctx.Member.VoiceState is null)
        {
            this.SendVoiceStateError();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckMaintenance()
    {
        if (!this.ctx.User.IsMaintenance(this.ctx.Bot.status))
        {
            this.SendMaintenanceError();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckBotOwner()
    {
        if (!this.ctx.User.IsMaintenance(this.ctx.Bot.status))
        {
            this.SendBotOwnerError();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckAdmin()
    {
        if (!this.ctx.Member.IsAdmin(this.ctx.Bot.status))
        {
            this.SendAdminError();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckPermissions(Permissions perms)
    {
        if (!this.ctx.Member.Permissions.HasPermission(perms))
        {
            this.SendPermissionError(perms);
            return false;
        }

        return true;
    }

    public async Task<bool> CheckOwnPermissions(Permissions perms)
    {
        if (!this.ctx.CurrentMember.Permissions.HasPermission(perms))
        {
            this.SendOwnPermissionError(perms);
            return false;
        }

        return true;
    }

    public async Task<bool> CheckSource(Enums.CommandType commandType)
    {
        if (this.ctx.CommandType != commandType)
        {
            this.SendSourceError(commandType);
            return false;
        }

        return true;
    }
    #endregion

    #region ErrorTemplates

    public void SendDisabledCommandError(string disabledCommand)
        => _ = this.RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = this.GetString(this.t.Commands.Common.Errors.CommandDisabled, true, new TVar("Command", disabledCommand))
        }.AsError(this.ctx));

    public void SendNoMemberError()
    => _ = this.RespondOrEdit(new DiscordEmbedBuilder()
    {
        Description = this.GetString(this.t.Commands.Common.Errors.NoMember)
    }.AsError(this.ctx));

    public void SendMaintenanceError()
        => _ = this.RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = this.GetString(this.t.Commands.Common.Errors.Generic).Build(true, new TVar("Required", $"{this.ctx.CurrentUser.GetUsername()} Staff"))
        }.AsError(this.ctx));

    public void SendBotOwnerError()
    => _ = this.RespondOrEdit(new DiscordEmbedBuilder()
    {
        Description = this.GetString(this.t.Commands.Common.Errors.Generic).Build(true, new TVar("Required", $"<@{this.ctx.Bot.status.TeamOwner}>", false)),
    }.AsError(this.ctx));

    public void SendAdminError()
        => _ = this.RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = this.GetString(this.t.Commands.Common.Errors.Generic).Build(true, new TVar("Required", "Administrator")),
        }.AsError(this.ctx));

    public void SendPermissionError(Permissions perms)
        => _ = this.RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = this.GetString(this.t.Commands.Common.Errors.Generic).Build(true, new TVar("Required", perms.ToTranslatedPermissionString(this.ctx.DbUser, this.ctx.Bot))),
        }.AsError(this.ctx));

    public void SendVoiceStateError()
        => _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = this.GetString(this.t.Commands.Common.Errors.VoiceChannel).Build(true),
        }.AsError(this.ctx)));

    public void SendUserBanError(BanDetails entry)
        => _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = this.t.Commands.Common.Errors.UserBan.t["en"].Build(true, new TVar("Reason", entry.Reason)),
        }.AsError(this.ctx)));

    public void SendGuildBanError(BanDetails entry)
        => _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = this.GetString(this.t.Commands.Common.Errors.GuildBan, true, new TVar("Reason", entry.Reason)),
        }.AsError(this.ctx)));

    public void SendSourceError(Enums.CommandType commandType)
        => _ = commandType switch
        {
            Enums.CommandType.ApplicationCommand => this.RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = this.GetString(this.t.Commands.Common.Errors.ExclusiveApp).Build(true),
            }.AsError(this.ctx)),
            Enums.CommandType.PrefixCommand => this.RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = this.GetString(this.t.Commands.Common.Errors.ExclusivePrefix).Build(true)
            }.AsError(this.ctx)),
            _ => throw new ArgumentException("Invalid Source defined."),
        };

    public void SendDataError()
        => _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = this.GetString(this.t.Commands.Common.Errors.Data, true, new TVar("Command", $"{this.ctx.Prefix}data delete")),
        }.AsError(this.ctx)));

    public void SendDmError()
        => _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"📩 {this.GetString(this.t.Commands.Common.Errors.DirectMessage, true)}",
            ImageUrl = (this.ctx.User.Presence.ClientStatus.Mobile.HasValue ? "https://cdn.discordapp.com/attachments/1005430437952356423/1144961395515998238/34rhz83ghtzu3ght.gif" : "https://cdn.discordapp.com/attachments/1005430437952356423/1144964670197862400/et2grtzu2ghrzi52.gif")
        }.AsError(this.ctx)));

    public void SendDmRedirect()
        => _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
        {
            Description = $"📩 {this.GetString(this.t.Commands.Common.DirectMessageRedirect, true)}",
        }.AsSuccess(this.ctx)));

    public void SendOwnPermissionError(Permissions perms)
    {
        if (perms is Permissions.AccessChannels or Permissions.SendMessages or Permissions.EmbedLinks)
            return;

        _ = this.RespondOrEdit(new DiscordEmbedBuilder()
        {
            Description = this.GetString(this.t.Commands.Common.Errors.BotPermissions, true, new TVar("Required", perms.ToTranslatedPermissionString(this.ctx.DbUser, this.ctx.Bot)))
        }.AsError(this.ctx));
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

        _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).WithContent(this.ctx.User.Mention));
    }
    #endregion
}
