namespace ProjectIchigo.Commands.ActionLogCommand;

internal class ActionLogAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.Channel == 0)
            return $"❌ `The actionlog is disabled.`";

        return $"`Actionlog Channel                 ` : <#{ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.Channel}>\n" +
                $"`Attempt gathering more details    ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails.BoolToEmote(ctx.Client)}\n" +
                $"`Join, Leaves & Kicks              ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.MembersModified.BoolToEmote(ctx.Client)}\n" +
                $"`Nickname, Role, Membership Updates` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.MemberModified.BoolToEmote(ctx.Client)}\n" +
                $"`User Profile Updates              ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.MemberProfileModified.BoolToEmote(ctx.Client)}\n" +
                $"`Message Deletions                 ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.MessageDeleted.BoolToEmote(ctx.Client)}\n" +
                $"`Message Modifications             ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.MessageModified.BoolToEmote(ctx.Client)}\n" +
                $"`Role Updates                      ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.RolesModified.BoolToEmote(ctx.Client)}\n" +
                $"`Bans & Unbans                     ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.BanlistModified.BoolToEmote(ctx.Client)}\n" +
                $"`Server Modifications              ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.GuildModified.BoolToEmote(ctx.Client)}\n" +
                $"`Channel Modifications             ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.ChannelsModified.BoolToEmote(ctx.Client)}\n" +
                $"`Voice Channel Updates             ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated.BoolToEmote(ctx.Client)}\n" +
                $"`Invite Modifications              ` : {ctx.Bot._guilds[ctx.Guild.Id].ActionLogSettings.InvitesModified.BoolToEmote(ctx.Client)}";
    }
}
