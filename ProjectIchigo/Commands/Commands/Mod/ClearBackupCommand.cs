namespace ProjectIchigo.Commands;

internal class ClearBackupCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageRoles));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            if ((await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`{victim.UsernameWithDiscriminator} ({victim.Id}) is on the server and therefor their stored nickname and roles cannot be cleared.`",
                    Color = EmbedColors.Error,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = victim.AvatarUrl
                    },
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });

                return;
            }

            if (!ctx.Bot._guilds.List.ContainsKey(ctx.Guild.Id))
                ctx.Bot._guilds.List.Add(ctx.Guild.Id, new Guilds.ServerSettings());

            if (!ctx.Bot._guilds.List[ctx.Guild.Id].Members.ContainsKey(victim.Id))
                ctx.Bot._guilds.List[ctx.Guild.Id].Members.Add(victim.Id, new());

            ctx.Bot._guilds.List[ctx.Guild.Id].Members[victim.Id].MemberRoles.Clear();
            ctx.Bot._guilds.List[ctx.Guild.Id].Members[victim.Id].SavedNickname = "";

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Deleted stored nickname and roles for {victim.UsernameWithDiscriminator} ({victim.Id}).`",
                Color = EmbedColors.StrongPunishment,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = ctx.Guild.IconUrl
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            });
        });
    }
}