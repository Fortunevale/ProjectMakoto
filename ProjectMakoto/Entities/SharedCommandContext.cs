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
        CommandName = ctx.CommandName;

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
        CommandName = ctx.CommandName;

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
}
