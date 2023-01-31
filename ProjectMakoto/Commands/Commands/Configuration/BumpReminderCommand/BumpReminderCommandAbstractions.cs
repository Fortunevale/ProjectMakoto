namespace ProjectMakoto.Commands.BumpReminderCommand;

internal class BumpReminderCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (!ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled)
            return $"{EmojiTemplates.GetQuestionMark(ctx.Bot)} `Bump Reminder Enabled` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled.ToEmote(ctx.Bot)}";

        return $"{EmojiTemplates.GetQuestionMark(ctx.Bot)} `Bump Reminder Enabled` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetChannel(ctx.Bot)} `Bump Reminder Channel` : <#{ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId})`\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `Bump Reminder Role   ` : <@&{ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId})`";
    }
}
