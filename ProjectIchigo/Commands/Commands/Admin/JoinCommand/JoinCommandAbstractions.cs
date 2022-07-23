namespace ProjectIchigo.Commands.JoinCommand;
internal class JoinCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return  $"`Autoban Globally Banned Users` : {ctx.Bot._guilds[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans.BoolToEmote(ctx.Client)}\n" +
                $"`Joinlog Channel              ` : {(ctx.Bot._guilds[ctx.Guild.Id].JoinSettings.JoinlogChannelId != 0 ? $"<#{ctx.Bot._guilds[ctx.Guild.Id].JoinSettings.JoinlogChannelId}>" : false.BoolToEmote(ctx.Client))}\n" +
                $"`Role On Join                 ` : {(ctx.Bot._guilds[ctx.Guild.Id].JoinSettings.AutoAssignRoleId != 0 ? $"<@&{ctx.Bot._guilds[ctx.Guild.Id].JoinSettings.AutoAssignRoleId}>" : false.BoolToEmote(ctx.Client))}\n" +
                $"`Re-Apply Roles on Rejoin     ` : {ctx.Bot._guilds[ctx.Guild.Id].JoinSettings.ReApplyRoles.BoolToEmote(ctx.Client)}\n" +
                $"`Re-Apply Nickname on Rejoin  ` : {ctx.Bot._guilds[ctx.Guild.Id].JoinSettings.ReApplyNickname.BoolToEmote(ctx.Client)}\n\n" +
                $"For security reasons, roles with any of the following permissions never get re-applied: {string.Join(", ", Resources.ProtectedPermissions.Select(x => $"`{x.ToPermissionString()}`"))}.\n\n" +
                $"In addition, if the user left the server 60+ days ago, neither roles nor nicknames will be re-applied.";
    }
}
