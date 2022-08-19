namespace ProjectIchigo.Commands;

internal class ScoreSaberProfileCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string id = (string)arguments["id"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            bool AddLinkButton = true;

            if ((string.IsNullOrWhiteSpace(id) || id.Contains('@')))
            {
                DiscordUser user;

                try
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        if (Regex.IsMatch(id, @"<@((!?)(\d*))>", RegexOptions.ExplicitCapture))
                            user = await ctx.Client.GetUserAsync(Convert.ToUInt64(Regex.Match(id, @"<@((!?)(\d*))>", RegexOptions.ExplicitCapture).Groups[3].Value));
                        else
                        {
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = $"`Invalid input.`"
                            }.SetError(ctx, "Score Saber")));
                            return;
                        }
                    }
                    else
                        user = ctx.User;
                }
                catch (DisCatSharp.Exceptions.NotFoundException)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`The user you tagged does not exist.`"
                    }.SetError(ctx, "Score Saber")));
                    return;
                }

                if (ctx.Bot.users[user.Id].ScoreSaber.Id != 0)
                {
                    id = ctx.Bot.users[user.Id].ScoreSaber.Id.ToString();
                    AddLinkButton = false;
                }
                else
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`This user has no Score Saber Profile linked to their Discord Account.`"
                    }.SetError(ctx, "Score Saber")));
                    return;
                }
            }

            await ScoreSaberCommandAbstractions.SendScoreSaberProfile(ctx, id, AddLinkButton);
        });
    }
}