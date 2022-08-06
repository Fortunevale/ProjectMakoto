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
                Title = "",
                Description = $"`Unbanning {victim.UsernameWithDiscriminator} ({victim.Id})..`",
                Color = EmbedColors.Processing,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.Loading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            await RespondOrEdit(embed);

            try
            {
                await ctx.Guild.UnbanMemberAsync(victim);

                embed.Color = EmbedColors.Success;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = $"<@{victim.Id}> `{victim.UsernameWithDiscriminator}` was unbanned.";
            }
            catch (Exception)
            {
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = $"❌ `{victim.UsernameWithDiscriminator} ({victim.Id}) couldn't be unbanned.`";
            }

            await RespondOrEdit(embed);
        });
    }
}