namespace ProjectMakoto.Commands.ExperienceCommand;

internal class ExperienceCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"✨ `Experience Enabled          `: {ctx.Bot.guilds[ctx.Guild.Id].Experience.UseExperience.ToEmote(ctx.Bot)}\n" +
               $"⏫ `Experience Boost for Bumpers`: {ctx.Bot.guilds[ctx.Guild.Id].Experience.BoostXpForBumpReminder.ToEmote(ctx.Bot)}";
    }
}
