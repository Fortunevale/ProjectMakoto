namespace ProjectIchigo.Commands.BumpReminderCommand;

internal class BumpReminderCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (!ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.Enabled)
            return $"{Resources.Emojis.GetQuestionMark(ctx.Client, ctx.Bot)} `Bump Reminder Enabled` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote(ctx.Client)}";

        return $"{Resources.Emojis.GetQuestionMark(ctx.Client, ctx.Bot)} `Bump Reminder Enabled` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote(ctx.Client)}\n" +
               $"{Resources.Emojis.GetChannel(ctx.Client, ctx.Bot)} `Bump Reminder Channel` : <#{ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
               $"{Resources.Emojis.GetUser(ctx.Client, ctx.Bot)} `Bump Reminder Role   ` : <@&{ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminderSettings.RoleId})`";
    }
}
