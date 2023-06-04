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

public class SharedCommandContext
{
    public SharedCommandContext(BaseCommand cmd, CommandContext ctx, Bot _bot)
    {
        CommandType = CommandType.PrefixCommand;

        Member = ctx.Member;
        User = ctx.User;
        Guild = ctx.Guild;
        Channel = ctx.Channel;
        Client = ctx.Client;

        CurrentMember = ctx.Guild?.CurrentMember;
        CurrentUser = ctx.Client.CurrentUser;

        OriginalCommandContext = ctx;

        Bot = _bot;

        Prefix = ctx.Prefix;
        CommandName = ctx.Command.Name;

        if (ctx.Command.Parent != null)
            CommandName = CommandName.Insert(0, $"{ctx.Command.Parent.Name} ");

        BaseCommand = cmd;

        try
        {
            DbUser = _bot.users[ctx.User.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }
        
        try
        {
            DbGuild = _bot.guilds[ctx.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }
    }

    public SharedCommandContext(DiscordMessage message, Bot _bot, string CommandIdentifier)
    {
        CommandType = CommandType.Custom;

        User = message.Author;
        Guild = message.Channel.Guild;
        Channel = message.Channel;

        CurrentMember = message.Channel?.Guild?.CurrentMember;
        CurrentUser = _bot.discordClient.CurrentUser;

        Bot = _bot;

        CommandName = CommandIdentifier;

        BaseCommand = new DummyCommand()
        {
            ctx = this
        };

        try
        {
            DbUser = _bot.users[message.Author.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", message.Author.Id, ex);
        }

        try
        {
            DbGuild = _bot.guilds[message.Channel.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{User}'\n{ex}", message.Channel.Guild.Id, ex);
        }
    }

    public SharedCommandContext(BaseCommand cmd, InteractionContext ctx, Bot _bot)
    {
        CommandType = CommandType.ApplicationCommand;

        Member = ctx.Member;
        User = ctx.User;
        Guild = ctx.Guild;
        Channel = ctx.Channel;
        Client = ctx.Client;

        CurrentMember = ctx.Guild?.CurrentMember;
        CurrentUser = ctx.Client.CurrentUser;

        OriginalInteractionContext = ctx;

        Prefix = "/";
        CommandName = ctx.FullCommandName;

        Bot = _bot;

        BaseCommand = cmd;

        try
        {
            DbUser = _bot.users[ctx.User.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }

        try
        {
            DbGuild = _bot.guilds[ctx.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }
    }

    public SharedCommandContext(BaseCommand cmd, ComponentInteractionCreateEventArgs ctx, DiscordClient client, string commandName, Bot _bot)
    {
        CommandType = CommandType.Event;

        User = ctx.User;
        Guild = ctx.Guild;
        Channel = ctx.Channel;
        Client = client;

        try { if (ctx.Guild is not null) Member = ctx.User.ConvertToMember(ctx.Guild).GetAwaiter().GetResult(); } catch { }

        CurrentMember = ctx.Guild?.CurrentMember;
        CurrentUser = client.CurrentUser;

        OriginalComponentInteractionCreateEventArgs = ctx;

        Prefix = "/";
        CommandName = commandName;

        Bot = _bot;

        BaseCommand = cmd;

        try
        {
            DbUser = _bot.users[ctx.User.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", ctx.User?.Id ?? 0, ex);
        }

        try
        {
            DbGuild = _bot.guilds[ctx.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{Guild}'\n{ex}", ctx.Guild?.Id ?? 0, ex);
        }
    }

    public SharedCommandContext(BaseCommand cmd, ContextMenuContext ctx, Bot _bot)
    {
        CommandType = CommandType.ContextMenu;

        Member = ctx.Member;
        User = ctx.User;
        Guild = ctx.Guild;
        Channel = ctx.Channel;
        Client = ctx.Client;

        CurrentMember = ctx.Guild?.CurrentMember;
        CurrentUser = ctx.Client.CurrentUser;

        OriginalContextMenuContext = ctx;

        Prefix = "";
        CommandName = ctx.FullCommandName;

        Bot = _bot;

        BaseCommand = cmd;

        try
        {
            DbUser = _bot.users[ctx.User.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database user entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }

        try
        {
            DbGuild = _bot.guilds[ctx.Guild.Id];
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Unable to fetch database guild entry for '{User}'\n{ex}", ctx.User.Id, ex);
        }
    }

    /// <summary>
    /// From what kind of source this command originated from.
    /// </summary>
    public CommandType CommandType { get; set; }

    /// <summary>
    /// The Command's Environment.
    /// </summary>
    public BaseCommand BaseCommand { get; set; }

    /// <summary>
    /// What prefix was used to execute this command.
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// The name of the command used.
    /// </summary>
    public string CommandName { get; set; }

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
        => Client;
    

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
            CommandType.ApplicationCommand => OriginalInteractionContext.Interaction,
            CommandType.Event => OriginalComponentInteractionCreateEventArgs.Interaction,
            CommandType.ContextMenu => OriginalContextMenuContext.Interaction,
            _ => null
        };
}
