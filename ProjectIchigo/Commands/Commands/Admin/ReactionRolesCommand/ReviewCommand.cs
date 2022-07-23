namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class ReviewCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                Color = EmbedColors.Loading,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = "`Loading Reaction Roles..`"
            });

            var messageCache = await ReactionRolesCommandAbstractions.CheckForInvalid(ctx);

            List<string> Desc = new();

            if (ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Count == 0)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = "`No reaction roles are set up.`"
                }.Build());
                return;
            }

            foreach (var b in ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles)
            {
                var channel = ctx.Guild.GetChannel(b.Value.ChannelId);
                var role = ctx.Guild.GetRole(b.Value.RoleId);
                var message = messageCache[b.Key];

                Desc.Add($"[`Message`]({message.JumpLink}) in {channel.Mention} `[#{channel.Name}]`\n" +
                        $"{b.Value.GetEmoji(ctx.Client)} - {role.Mention} `{role.Name}`");
            }

            List<string> Sections = new();
            string build = "";

            foreach (var b in Desc)
            {
                string curstr = $"{b}\n\n";

                if (build.Length + curstr.Length > 4096)
                {
                    Sections.Add(build);
                    build = "";
                }

                build += curstr;
            }

            if (build.Length > 0)
            {
                Sections.Add(build);
                build = "";
            }

            List<DiscordEmbed> embeds = Sections.Select(x => new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Reaction Roles • {ctx.Guild.Name}" },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = x
            }.Build()).ToList();

            await RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(embeds));
            return;
        });
    }
}