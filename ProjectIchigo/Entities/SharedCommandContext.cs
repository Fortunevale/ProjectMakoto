using CommandType = ProjectIchigo.Enums.CommandType;

namespace ProjectIchigo.Entities;

internal class SharedCommandContext
{
    internal SharedCommandContext(BaseCommand cmd, CommandContext ctx, Bot _bot)
    {
        CommandType = CommandType.PrefixCommand;

        Member = ctx.Member;
        User = ctx.User;
        Guild = ctx.Guild;
        Channel = ctx.Channel;
        Client = ctx.Client;

        OriginalCommandContext = ctx;

        Bot = _bot;

        Prefix = ctx.Prefix;
        CommandName = ctx.Command.Name;

        BaseCommand = cmd;
    }
    
    internal SharedCommandContext(DiscordMessage message, Bot _bot)
    {
        CommandType = CommandType.Custom;

        User = message.Author;
        Guild = message.Channel.Guild;
        Channel = message.Channel;

        Bot = _bot;

        BaseCommand = new DummyCommand()
        {
            Context = this
        };
    }
    
    internal SharedCommandContext(BaseCommand cmd, InteractionContext ctx, Bot _bot)
    {
        CommandType = CommandType.ApplicationCommand;

        Member = ctx.Member;
        User = ctx.User;
        Guild = ctx.Guild;
        Channel = ctx.Channel;
        Client = ctx.Client;

        OriginalInteractionContext = ctx;

        Prefix = "/";
        CommandName = ctx.CommandName;

        Bot = _bot;

        BaseCommand = cmd;
    }

    public CommandType CommandType { get; set; }

    public Bot Bot { get; set; }

    public string Prefix { get; set; }
    public string CommandName { get; set; }

    public DiscordMember Member { get; set; }
    public DiscordUser User { get; set; }
    public DiscordGuild Guild { get; set; }
    public DiscordChannel Channel { get; set; }
    public DiscordClient Client { get; set; }

    public DiscordMessage ResponseMessage { get; set; }

    public BaseCommand BaseCommand { get; set; }

    public CommandContext OriginalCommandContext { get; set; }
    public InteractionContext OriginalInteractionContext { get; set; }
}
