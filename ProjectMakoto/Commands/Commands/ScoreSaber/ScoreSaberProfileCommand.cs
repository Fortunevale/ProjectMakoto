namespace ProjectMakoto.Commands;

internal class ScoreSaberProfileCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string id = (string)arguments["id"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
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
                            user = await ctx.Client.GetUserAsync(Convert.ToUInt64(Regex.Match(id, @"<@((!?)(\d*))>").Groups[3].Value));
                        else
                        {
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = $"`{GetString(t.Commands.ScoreSaber.Profile.InvalidInput)}`"
                            }.AsError(ctx, "Score Saber")));
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
                        Description = $"`{GetString(t.Commands.ScoreSaber.Profile.NoUser)}`"
                    }.AsError(ctx, "Score Saber")));
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
                        Description = $"`{GetString(t.Commands.ScoreSaber.Profile.NoProfile)}`"
                    }.AsError(ctx, "Score Saber")));
                    return;
                }
            }

            await ScoreSaberCommandAbstractions.SendScoreSaberProfile(ctx, id, AddLinkButton);
        });
    }
}