namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class ReviewCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = "`Loading Reaction Roles..`"
            }.SetLoading(ctx, "Reaction Roles"));

            var messageCache = await ReactionRolesCommandAbstractions.CheckForInvalid(ctx);

            List<string> Desc = new();

            if (ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Count == 0)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = "`No reaction roles are set up.`"
                }.SetInfo(ctx, "Reaction Roles"));
                return;
            }

            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles)
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
                Description = x
            }.SetInfo(ctx, "Reaction Roles").Build()).ToList();

            await RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(embeds));
            return;
        });
    }
}