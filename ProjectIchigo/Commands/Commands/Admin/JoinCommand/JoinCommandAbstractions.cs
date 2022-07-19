namespace ProjectIchigo.Commands.JoinCommand;
internal class JoinCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx, Bot _bot)
    {
        return $"`Autoban Globally Banned Users` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans.BoolToEmote(ctx.Client)}\n" +
                $"`Joinlog Channel              ` : {(_bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId != 0 ? $"<#{_bot._guilds.List[ctx.Guild.Id].JoinSettings.JoinlogChannelId}>" : false.BoolToEmote(ctx.Client))}\n" +
                $"`Role On Join                 ` : {(_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId != 0 ? $"<@&{_bot._guilds.List[ctx.Guild.Id].JoinSettings.AutoAssignRoleId}>" : false.BoolToEmote(ctx.Client))}\n" +
                $"`Re-Apply Roles on Rejoin     ` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyRoles.BoolToEmote(ctx.Client)}\n" +
                $"`Re-Apply Nickname on Rejoin  ` : {_bot._guilds.List[ctx.Guild.Id].JoinSettings.ReApplyNickname.BoolToEmote(ctx.Client)}\n\n" +
                $"For security reasons, roles with any of the following permissions never get re-applied: {string.Join(", ", Resources.ProtectedPermissions.Select(x => $"`{x.ToPermissionString()}`"))}.\n\n" +
                $"In addition, if the user left the server 60+ days ago, neither roles nor nicknames will be re-applied.";
    }
}
