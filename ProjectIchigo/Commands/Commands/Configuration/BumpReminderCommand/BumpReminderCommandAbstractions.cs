namespace ProjectIchigo.Commands.BumpReminderCommand;

internal class BumpReminderCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (!ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.Enabled)
            return $"{EmojiTemplates.GetQuestionMark(ctx.Client, ctx.Bot)} `Bump Reminder Enabled` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.Enabled.ToEmote(ctx.Client)}";

        return $"{EmojiTemplates.GetQuestionMark(ctx.Client, ctx.Bot)} `Bump Reminder Enabled` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.Enabled.ToEmote(ctx.Client)}\n" +
               $"{EmojiTemplates.GetChannel(ctx.Client, ctx.Bot)} `Bump Reminder Channel` : <#{ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
               $"{EmojiTemplates.GetUser(ctx.Client, ctx.Bot)} `Bump Reminder Role   ` : <@&{ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.RoleId})`";
    }
}
