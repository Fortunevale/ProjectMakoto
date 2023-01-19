namespace ProjectIchigo.Commands.Data;

internal class InfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForHeavy(ctx.Client, ctx, true))
                return;

            if (ctx.Bot.RawFetchedPrivacyPolicy.IsNullOrWhiteSpace())
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(t.Commands.Data.Policy.NoPolicy).Replace("{Bot}", ctx.CurrentUser.Username)}`",
                }.AsBotError(ctx));
                return;
            }

            var RawPolicy = ctx.Bot.RawFetchedPrivacyPolicy.Replace("#", "");

            List<string> PolicyStrings = RawPolicy.ReplaceLineEndings("\n").Split("\n\n").ToList();

            string Title = "";
            List<DiscordEmbed> embeds = new();

            for (int i = 0; i < PolicyStrings.Count; i++)
            {
                if (i == 0)
                {
                    Title = PolicyStrings[i];
                    continue;
                }

                embeds.Add(new DiscordEmbedBuilder
                {
                    Title = (i == 1 ? Title : ""),
                    Description = PolicyStrings[i]
                });
            }

            try
            {
                foreach (var b in embeds)
                    await ctx.Member.SendMessageAsync(b);

                SendDmRedirect();
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                SendDmError();
            }
        });
    }
}