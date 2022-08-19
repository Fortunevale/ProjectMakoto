using CommandType = ProjectIchigo.Enums.CommandType;

namespace ProjectIchigo.Entities;

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
    }

    public SharedCommandContext(DiscordMessage message, Bot _bot)
    {
        CommandType = CommandType.Custom;

        User = message.Author;
        Guild = message.Channel.Guild;
        Channel = message.Channel;

        CurrentMember = message.Channel?.Guild?.CurrentMember;
        CurrentUser = _bot.discordClient.CurrentUser;

        Bot = _bot;

        BaseCommand = new DummyCommand()
        {
            ctx = this
        };
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
    /// The channel this command was exectued in.
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
