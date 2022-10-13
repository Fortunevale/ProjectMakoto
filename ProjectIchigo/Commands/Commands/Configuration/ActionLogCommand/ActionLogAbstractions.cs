namespace ProjectIchigo.Commands.ActionLogCommand;

internal class ActionLogAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel == 0)
            return $"❌ `The actionlog is disabled.`";

        return $"{EmojiTemplates.GetChannel(ctx.Client, ctx.Bot)} `Actionlog Channel                 ` : <#{ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel}>\n\n" +
               $"⚠ `Attempt gathering more details    ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.AttemptGettingMoreDetails.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Client, ctx.Bot)} `Join, Leaves & Kicks              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MembersModified.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Client, ctx.Bot)} `Nickname, Role, Membership Updates` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberModified.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Client, ctx.Bot)} `User Profile Updates              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberProfileModified.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetMessage(ctx.Client, ctx.Bot)} `Message Deletions                 ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageDeleted.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetMessage(ctx.Client, ctx.Bot)} `Message Modifications             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageModified.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Client, ctx.Bot)} `Role Updates                      ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.RolesModified.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Client, ctx.Bot)} `Bans & Unbans                     ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.BanlistModified.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetGuild(ctx.Client, ctx.Bot)} `Server Modifications              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.GuildModified.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetChannel(ctx.Client, ctx.Bot)} `Channel Modifications             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.ChannelsModified.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetVoiceState(ctx.Client, ctx.Bot)} `Voice Channel Updates             ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.VoiceStateUpdated.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetInvite(ctx.Client, ctx.Bot)} `Invite Modifications              ` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.InvitesModified.ToEmote(ctx.Client)}";
    }
}
