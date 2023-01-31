namespace ProjectMakoto.Commands.LevelRewardsCommand;

internal class LevelRewardsCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        string str = "";
        if (ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Count != 0)
        {
            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.OrderBy(x => x.Level))
            {
                if (!ctx.Guild.Roles.ContainsKey(b.RoleId))
                {
                    ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Remove(b);
                    continue;
                }

                str += $"**Level**: `{b.Level}`\n" +
                        $"**Role**: <@&{b.RoleId}> (`{b.RoleId}`)\n" +
                        $"**Message**: `{b.Message}`\n";

                str += "\n\n";
            }
        }
        else
        {
            str = $"`No Level Rewards are set up.`";
        }

        return str;
    }
}