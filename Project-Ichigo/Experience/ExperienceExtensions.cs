namespace Project_Ichigo.Experience;

internal static class ExperienceExtensions
{
    internal static int CalculateMessageExperience(this DiscordMessage message)
    {
        if (message.Content.Length > 0)
        {
            if (Regex.IsMatch(message.Content, @"^(-|>>|;;|\$|\!|\!d|owo |/)"))
                return 0;
        }

        int Points = 1;

        if (message.ReferencedMessage is not null)
            Points += 2;

        if (message.Attachments is not null && string.IsNullOrWhiteSpace(message.Content))
            Points -= 1;

        if (Regex.IsMatch(message.Content, Resources.Regex.Url))
        {
            string ModifiedString = Regex.Replace(message.Content, Resources.Regex.Url, "");

            if (ModifiedString.Length > 10)
                Points += 1;

            if (ModifiedString.Length > 25)
                Points += 1;

            if (ModifiedString.Length > 50)
                Points += 1;

            if (ModifiedString.Length > 75)
                Points += 1;
        }
        else
        {
            if (message.Content.Length > 10)
                Points += 1;

            if (message.Content.Length > 25)
                Points += 1;

            if (message.Content.Length > 50)
                Points += 1;

            if (message.Content.Length > 75)
                Points += 1;
        }

        return Points;
    }

    internal static async void ModifyExperience(this DiscordUser user, DiscordGuild guild, ServerInfo server, int Amount) => (await user.ConvertToMember(guild)).ModifyExperience(guild, server, Amount);

    internal static async void ModifyExperience(this DiscordMember user, DiscordGuild guild, ServerInfo server, int Amount)
    {
        if (user.IsBot)
            return;

        if (!server.Servers[guild.Id].ExperienceSettings.UseExperience)
            return;

        if (server.Servers[guild.Id].Members[user.Id].Experience is > (long.MaxValue - 10000) or < (long.MinValue + 10000))
        {
            LogWarn($"Member '{user.Id}' on '{guild.Id}' is within 10000 points of the experience limit. Resetting.");
            server.Servers[guild.Id].Members[user.Id].Experience = 1;
        }

        server.Servers[guild.Id].Members[user.Id].Experience += Amount;
    }
}
