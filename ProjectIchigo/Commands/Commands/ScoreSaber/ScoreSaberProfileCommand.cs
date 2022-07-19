namespace ProjectIchigo.Commands;

internal class ScoreSaberProfileCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string id = (string)arguments["id"];

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            bool AddLinkButton = true;

            if ((string.IsNullOrWhiteSpace(id) || id.Contains('@')))
            {
                DiscordUser user;

                try
                {
                    user = await ctx.Client.GetUserAsync(Convert.ToUInt64(Regex.Match(id, @"<@(\d*)>").Groups[1]));
                }
                catch (DisCatSharp.Exceptions.NotFoundException)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Score Saber • {ctx.Guild.Name}" },
                        Color = EmbedColors.Error,
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow,
                        Description = $"`The user you tagged does not exist.`"
                    }));
                    return;
                }

                if (!ctx.Bot._users.List.ContainsKey(user.Id))
                    ctx.Bot._users.List.Add(user.Id, new Users.Info(ctx.Bot));

                if (ctx.Bot._users.List[user.Id].ScoreSaber.Id != 0)
                {
                    id = ctx.Bot._users.List[user.Id].ScoreSaber.Id.ToString();
                    AddLinkButton = false;
                }
                else
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Score Saber • {ctx.Guild.Name}" },
                        Color = EmbedColors.Error,
                        Footer = ctx.GenerateUsedByFooter(),
                        Timestamp = DateTime.UtcNow,
                        Description = $"`This user has no Score Saber Profile linked to their Discord Account.`"
                    }));
                    return;
                }
            }

            await ScoreSaberCommandAbstractions.SendScoreSaberProfile(ctx, id, AddLinkButton);
        });
    }
}