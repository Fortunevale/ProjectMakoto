using CommandType = ProjectIchigo.Enums.CommandType;

namespace ProjectIchigo.Entities;

internal class SharedCommandContext
{
    internal SharedCommandContext(CommandContext ctx, Bot _bot)
    {
        CommandType = CommandType.PrefixCommand;

        Member = ctx.Member;
        User = ctx.User;
        Guild = ctx.Guild;
        Channel = ctx.Channel;
        Client = ctx.Client;

        OriginalCommandContext = ctx;

        Bot = _bot;
    }
    
    internal SharedCommandContext(InteractionContext ctx, Bot _bot)
    {
        CommandType = CommandType.ApplicationCommand;

        Member = ctx.Member;
        User = ctx.User;
        Guild = ctx.Guild;
        Channel = ctx.Channel;
        Client = ctx.Client;

        OriginalInteractionContext = ctx;

        Bot = _bot;
    }

    public CommandType CommandType { get; set; }

    public Bot Bot { get; set; }

    public DiscordMember Member { get; set; }
    public DiscordUser User { get; set; }
    public DiscordGuild Guild { get; set; }
    public DiscordChannel Channel { get; set; }
    public DiscordClient Client { get; set; }

    public DiscordMessage ResponseMessage { get; set; }

    public CommandContext OriginalCommandContext { get; set; }
    public InteractionContext OriginalInteractionContext { get; set; }
}
