namespace ProjectIchigo.Commands.JoinCommand;

internal class JoinCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return  $"🌐 `Autoban Globally Banned Users`: {ctx.Bot.guilds[ctx.Guild.Id].Join.AutoBanGlobalBans.ToEmote(ctx.Client)}\n" +
                $"👋 `Joinlog Channel              `: {(ctx.Bot.guilds[ctx.Guild.Id].Join.JoinlogChannelId != 0 ? $"<#{ctx.Bot.guilds[ctx.Guild.Id].Join.JoinlogChannelId}>" : false.ToEmote(ctx.Client))}\n" +
                $"👤 `Role On Join                 `: {(ctx.Bot.guilds[ctx.Guild.Id].Join.AutoAssignRoleId != 0 ? $"<@&{ctx.Bot.guilds[ctx.Guild.Id].Join.AutoAssignRoleId}>" : false.ToEmote(ctx.Client))}\n" +
                $"👥 `Re-Apply Roles on Rejoin     `: {ctx.Bot.guilds[ctx.Guild.Id].Join.ReApplyRoles.ToEmote(ctx.Client)}\n" +
                $"💬 `Re-Apply Nickname on Rejoin  `: {ctx.Bot.guilds[ctx.Guild.Id].Join.ReApplyNickname.ToEmote(ctx.Client)}\n\n" +
                $"For security reasons, roles with any of the following permissions never get re-applied: {string.Join(", ", Resources.ProtectedPermissions.Select(x => $"`{x.ToPermissionString()}`"))}.\n\n" +
                $"In addition, if the user left the server 60+ days ago, neither roles nor nicknames will be re-applied.";
    }
}
