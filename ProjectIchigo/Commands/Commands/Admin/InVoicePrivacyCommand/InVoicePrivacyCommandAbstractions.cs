namespace ProjectIchigo.Commands.InVoicePrivacyCommand;

internal class InVoicePrivacyCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"`Clear Messages on empty Voice Channel`: {ctx.Bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled.BoolToEmote(ctx.Client)}\n" +
               $"`Set Permissions on User Join         `: {ctx.Bot._guilds.List[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled.BoolToEmote(ctx.Client)}";
    }
}
