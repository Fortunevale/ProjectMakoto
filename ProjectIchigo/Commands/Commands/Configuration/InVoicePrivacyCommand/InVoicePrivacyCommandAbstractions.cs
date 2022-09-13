namespace ProjectIchigo.Commands.InVoicePrivacyCommand;

internal class InVoicePrivacyCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"🗑 `Clear Messages on empty Voice Channel`: {ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled.ToEmote(ctx.Client)}\n" +
               $"📋 `Set Permissions on User Join         `: {ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled.ToEmote(ctx.Client)}";
    }
}
