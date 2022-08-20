namespace ProjectIchigo.Commands.ActionLogCommand;

internal class ActionLogAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.Channel == 0)
            return $"❌ `The actionlog is disabled.`";

        return $"{Resources.Emojis.GetChannel(ctx.Client, ctx.Bot)} `Actionlog Channel                 ` : <#{ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.Channel}>\n\n" +
               $"⚠ `Attempt gathering more details    ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetUser(ctx.Client, ctx.Bot)} `Join, Leaves & Kicks              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MembersModified.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetUser(ctx.Client, ctx.Bot)} `Nickname, Role, Membership Updates` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MemberModified.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetUser(ctx.Client, ctx.Bot)} `User Profile Updates              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MemberProfileModified.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetMessage(ctx.Client, ctx.Bot)} `Message Deletions                 ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MessageDeleted.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetMessage(ctx.Client, ctx.Bot)} `Message Modifications             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MessageModified.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetUser(ctx.Client, ctx.Bot)} `Role Updates                      ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.RolesModified.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetUser(ctx.Client, ctx.Bot)} `Bans & Unbans                     ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.BanlistModified.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetGuild(ctx.Client, ctx.Bot)} `Server Modifications              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.GuildModified.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetChannel(ctx.Client, ctx.Bot)} `Channel Modifications             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.ChannelsModified.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetVoiceState(ctx.Client, ctx.Bot)} `Voice Channel Updates             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetInvite(ctx.Client, ctx.Bot)} `Invite Modifications              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.InvitesModified.BoolToEmote(ctx.Client)}";
    }
}
