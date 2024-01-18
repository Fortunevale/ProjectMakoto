// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using CommandType = ProjectMakoto.Enums.CommandType;

namespace ProjectMakoto.Entities;

public sealed class SharedCommandContext
{
    public SharedCommandContext() { }

    public SharedCommandContext(BaseCommand cmd, CommandContext ctx, Bot _bot)
    {
        this.CommandType = CommandType.PrefixCommand;

        this.Member = ctx.Member;
        this.User = ctx.User;
        this.Guild = ctx.Guild;
        this.Channel = ctx.Channel;
        this.Client = ctx.Client;

        this.CurrentMember = ctx.Guild?.CurrentMember;
        this.CurrentUser = ctx.Client.CurrentUser;

        this.OriginalCommandContext = ctx;

        this.Bot = _bot;
        this.t = _bot.LoadedTranslations;

        this.Prefix = ctx.Prefix;
        this.CommandName = ctx.Command.Name;

        if (ctx.Command.Parent != null)
            this.CommandName = this.CommandName.Insert(0, $"{ctx.Command.Parent.Name} ");

        this._baseCommand = cmd;

        try
        {
            this.DbUser = _bot.Users[ctx.User.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }

        try
        {
            this.DbGuild = _bot.Guilds[ctx.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }
    }

    public SharedCommandContext(DiscordMessage message, Bot _bot, string CommandIdentifier)
    {
        this.CommandType = CommandType.Custom;

        this.Client = _bot.DiscordClient.GetShard(message.Guild);
        this.User = message.Author;
        this.Guild = message.Channel.Guild;
        this.Channel = message.Channel;

        this.CurrentMember = message.Channel?.Guild?.CurrentMember;
        this.CurrentUser = _bot.DiscordClient.CurrentUser;

        this.Bot = _bot;
        this.t = _bot.LoadedTranslations;

        this.CommandName = CommandIdentifier;

        this._baseCommand = new DummyCommand()
        {
            ctx = this,
            t = this.t
        };

        try
        {
            this.DbUser = _bot.Users[message.Author.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", message.Author.Id, ex);
        }

        try
        {
            this.DbGuild = _bot.Guilds[message.Channel.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{User}'\n{ex}", message.Channel.Guild.Id, ex);
        }
    }

    public SharedCommandContext(BaseCommand cmd, InteractionContext ctx, Bot _bot)
    {
        this.CommandType = CommandType.ApplicationCommand;

        this.Member = ctx.Member;
        this.User = ctx.User;
        this.Guild = ctx.Guild;
        this.Channel = ctx.Channel;
        this.Client = ctx.Client;

        this.CurrentMember = ctx.Guild?.CurrentMember;
        this.CurrentUser = ctx.Client.CurrentUser;

        this.OriginalInteractionContext = ctx;

        this.Prefix = "/";
        this.CommandName = ctx.FullCommandName;
        this.ParentCommandName = ctx.CommandName;

        this.Bot = _bot;
        this.t = _bot.LoadedTranslations;

        this._baseCommand = cmd;

        try
        {
            this.DbUser = _bot.Users[ctx.User.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }

        try
        {
            this.DbGuild = _bot.Guilds[ctx.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }
    }

    public SharedCommandContext(BaseCommand cmd, ComponentInteractionCreateEventArgs ctx, DiscordClient client, string commandName, Bot _bot)
    {
        this.CommandType = CommandType.Event;

        this.User = ctx.User;
        this.Guild = ctx.Guild;
        this.Channel = ctx.Channel;
        this.Client = client;

        try
        { if (ctx.Guild is not null) this.Member = ctx.User.ConvertToMember(ctx.Guild).GetAwaiter().GetResult(); }
        catch { }

        this.CurrentMember = ctx.Guild?.CurrentMember;
        this.CurrentUser = client.CurrentUser;

        this.OriginalComponentInteractionCreateEventArgs = ctx;

        this.Prefix = "/";
        this.CommandName = commandName;

        this.Bot = _bot;
        this.t = _bot.LoadedTranslations;

        this._baseCommand = cmd;

        try
        {
            this.DbUser = _bot.Users[ctx.User.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", ctx.User?.Id ?? 0, ex);
        }

        try
        {
            this.DbGuild = _bot.Guilds[ctx.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{Guild}'\n{ex}", ctx.Guild?.Id ?? 0, ex);
        }
    }

    public SharedCommandContext(BaseCommand cmd, ContextMenuContext ctx, Bot _bot)
    {
        this.CommandType = CommandType.ContextMenu;

        this.Member = ctx.Member;
        this.User = ctx.User;
        this.Guild = ctx.Guild;
        this.Channel = ctx.Channel;
        this.Client = ctx.Client;

        this.CurrentMember = ctx.Guild?.CurrentMember;
        this.CurrentUser = ctx.Client.CurrentUser;

        this.OriginalContextMenuContext = ctx;

        this.Prefix = "";
        this.CommandName = ctx.FullCommandName;
        this.ParentCommandName = ctx.CommandName;

        this.Bot = _bot;
        this.t = _bot.LoadedTranslations;

        this._baseCommand = cmd;

        try
        {
            this.DbUser = _bot.Users[ctx.User.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }

        try
        {
            this.DbGuild = _bot.Guilds[ctx.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }
    }

    /// <summary>
    /// Get's translations.
    /// </summary>
    public Translations t { get; set; }

    /// <summary>
    /// From what kind of source this command originated from.
    /// </summary>
    public CommandType CommandType { get; set; }

    /// <summary>
    /// The Command's Environment.
    /// </summary>
    public BaseCommand BaseCommand
        => this._baseCommand ?? new DummyCommand()
        {
            ctx = this,
            t = this.t,
        };

    private BaseCommand? _baseCommand { get; set; }

    /// <summary>
    /// What prefix was used to execute this command.
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// The name of the command used.
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    /// The name of the command used.
    /// </summary>
    public string ParentCommandName { get; set; }

    /// <summary>
    /// What Bot Instance was used to execute this command.
    /// </summary>
    public Bot Bot { get; set; }

    /// <summary>
    /// What DiscordClient was used to execute this command.
    /// </summary>
    public DiscordClient Client { get; set; }

    /// <inheritdoc cref="Client"/>
    public DiscordClient Discord
        => this.Client;


    /// <summary>
    /// The member that executed this command.
    /// </summary>
    public DiscordMember Member { get; set; }

    /// <summary>
    /// This user that executed this command.
    /// </summary>
    public DiscordUser User { get; set; }

    /// <summary>
    /// The user's database entry that executed this command.
    /// </summary>
    public User DbUser { get; set; }

    /// <summary>
    /// The current member the bot uses.
    /// </summary>
    public DiscordMember CurrentMember { get; set; }

    /// <summary>
    /// The current user the bot uses.
    /// </summary>
    public DiscordUser CurrentUser { get; set; }

    /// <summary>
    /// The guild this command was executed on.
    /// </summary>
    public DiscordGuild Guild { get; set; }

    /// <summary>
    /// The guild's database entry the command was executed on.
    /// </summary>
    public Guild DbGuild { get; set; }

    /// <summary>
    /// The channel this command was executed in.
    /// </summary>
    public DiscordChannel Channel { get; set; }

    /// <summary>
    /// Whether the bot already responded once. Only set if Type is ApplicationCommand or ContextMenu.
    /// </summary>
    public bool RespondedToInitial { get; set; }

    /// <summary>
    /// If the command was executed through another command.
    /// </summary>
    public bool Transferred { get; set; } = false;

    /// <summary>
    /// The message that's being used to interact with the user.
    /// </summary>
    public DiscordMessage ResponseMessage { get; set; }

    /// <summary>
    /// The original context.
    /// </summary>
    public ContextMenuContext OriginalContextMenuContext { get; set; }

    /// <summary>
    /// The original context.
    /// </summary>
    public CommandContext OriginalCommandContext { get; set; }

    /// <summary>
    /// The original context.
    /// </summary>
    public InteractionContext OriginalInteractionContext { get; set; }

    /// <summary>
    /// The original event args.
    /// </summary>
    public ComponentInteractionCreateEventArgs OriginalComponentInteractionCreateEventArgs { get; set; }

    /// <summary>
    /// The original interaction that started this command.
    /// </summary>
    public DiscordInteraction Interaction
        => this.CommandType switch
        {
            CommandType.ApplicationCommand => this.OriginalInteractionContext.Interaction,
            CommandType.Event => this.OriginalComponentInteractionCreateEventArgs.Interaction,
            CommandType.ContextMenu => this.OriginalContextMenuContext.Interaction,
            _ => null
        };
}
