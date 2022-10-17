namespace ProjectIchigo.Commands;

internal class UnbanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.BanMembers) && await CheckOwnPermissions(Permissions.BanMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Unbanning {victim.UsernameWithDiscriminator} ({victim.Id})..`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
            }.AsLoading(ctx);
            await RespondOrEdit(embed);

            try
            {
                await ctx.Guild.UnbanMemberAsync(victim);

                embed.Description = $"<@{victim.Id}> `{victim.UsernameWithDiscriminator}` was unbanned.";
                embed = embed.AsSuccess(ctx);
            }
            catch (Exception)
            {
                embed.Description = $"`{victim.UsernameWithDiscriminator} ({victim.Id}) couldn't be unbanned.`";
                embed = embed.AsError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}