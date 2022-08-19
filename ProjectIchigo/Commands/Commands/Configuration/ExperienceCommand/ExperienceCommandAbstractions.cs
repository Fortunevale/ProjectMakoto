namespace ProjectIchigo.Commands.ExperienceCommand;

internal class ExperienceCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"✨ `Experience Enabled          `: {ctx.Bot.guilds[ctx.Guild.Id].ExperienceSettings.UseExperience.BoolToEmote(ctx.Client)}\n" +
               $"⏫ `Experience Boost for Bumpers`: {ctx.Bot.guilds[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder.BoolToEmote(ctx.Client)}";
    }
}
