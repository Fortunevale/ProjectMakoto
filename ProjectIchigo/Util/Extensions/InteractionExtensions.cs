namespace ProjectIchigo.Util;

internal static class InteractionExtensions
{
    internal static string GetModalValueByCustomId(this DiscordInteraction interaction, string customId) => interaction.Data.Components.First(x => x.CustomId == customId).Value;
}