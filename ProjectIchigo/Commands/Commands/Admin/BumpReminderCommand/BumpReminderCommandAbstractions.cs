namespace ProjectIchigo.Commands.BumpReminderCommand;

internal class BumpReminderCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (!ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.Enabled)
            return $"`Bump Reminder Enabled` : {ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote(ctx.Client)}";

        return $"`Bump Reminder Enabled` : {ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.Enabled.BoolToEmote(ctx.Client)}\n" +
            $"`Bump Reminder Channel` : <#{ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.ChannelId}> `({ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.ChannelId})`\n" +
            $"`Bump Reminder Role   ` : <@&{ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.RoleId}> `({ctx.Bot._guilds[ctx.Guild.Id].BumpReminderSettings.RoleId})`";
    }
}
