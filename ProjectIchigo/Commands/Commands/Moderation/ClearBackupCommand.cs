namespace ProjectIchigo.Commands;

internal class ClearBackupCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageRoles));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            if ((await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`{victim.UsernameWithDiscriminator} ({victim.Id}) is on the server and therefor their stored nickname and roles cannot be cleared.`",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = victim.AvatarUrl
                    },
                }.SetError(ctx));

                return;
            }

            if (!ctx.Bot.guilds[ctx.Guild.Id].Members.ContainsKey(victim.Id))
                ctx.Bot.guilds[ctx.Guild.Id].Members.Add(victim.Id, new(ctx.Bot.guilds[ctx.Guild.Id], victim.Id));

            ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].MemberRoles.Clear();
            ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].SavedNickname = "";

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Deleted stored nickname and roles for {victim.UsernameWithDiscriminator} ({victim.Id}).`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                }
            }.SetSuccess(ctx));
        });
    }
}