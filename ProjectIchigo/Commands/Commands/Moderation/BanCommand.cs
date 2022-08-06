namespace ProjectIchigo.Commands;

internal class BanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.BanMembers) && await CheckOwnPermissions(Permissions.BanMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            string reason = (string)arguments["reason"];

            DiscordMember bMember = null;

            try
            {
                bMember = await victim.ConvertToMember(ctx.Guild);
            }
            catch { }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Banning {victim.UsernameWithDiscriminator} ({victim.Id})..`",
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
                if (ctx.Member.GetRoleHighestPosition() <= (bMember?.GetRoleHighestPosition() ?? -1))
                    throw new Exception();

                await ctx.Guild.BanMemberAsync(victim.Id, 7, $"{ctx.User.UsernameWithDiscriminator} banned user: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");

                embed.Color = EmbedColors.Success;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = $"✅ {victim.Mention} `was banned for '{(reason.IsNullOrWhiteSpace() ? "No reason provided" : reason).SanitizeForCodeBlock()}' by` {ctx.User.Mention}`.`";
            }
            catch (Exception)
            {
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = $"❌ {victim.Mention} `could not be banned.`";
            }

            await RespondOrEdit(embed);
        });
    }
}