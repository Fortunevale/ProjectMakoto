namespace ProjectIchigo.Commands.ActionLogCommand;

internal class ActionLogAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel == 0)
            return $"❌ `The actionlog is disabled.`";

        return $"{EmojiTemplates.GetChannel(ctx.Bot)} `Actionlog Channel                 ` : <#{ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel}>\n\n" +
               $"⚠ `Attempt gathering more details    ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.AttemptGettingMoreDetails.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `Join, Leaves & Kicks              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MembersModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `Nickname, Role, Membership Updates` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `User Profile Updates              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberProfileModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetMessage(ctx.Bot)} `Message Deletions                 ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageDeleted.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetMessage(ctx.Bot)} `Message Modifications             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `Role Updates                      ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.RolesModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `Bans & Unbans                     ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.BanlistModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetGuild(ctx.Bot)} `Server Modifications              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.GuildModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetChannel(ctx.Bot)} `Channel Modifications             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.ChannelsModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetVoiceState(ctx.Bot)} `Voice Channel Updates             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.VoiceStateUpdated.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetInvite(ctx.Bot)} `Invite Modifications              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.InvitesModified.ToEmote(ctx.Bot)}";
    }
}
