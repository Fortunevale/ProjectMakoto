namespace ProjectIchigo.Commands;

internal class SoftBanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.BanMembers) && await CheckOwnPermissions(Permissions.BanMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            int deleteMessageDays = (int)arguments["days"] > 7 ? 7 : (int)arguments["days"];
            if (deleteMessageDays < 0) deleteMessageDays = 0;
            string reason = (string)arguments["reason"];

            DiscordMember bMember = null;

            try
            {
                bMember = await victim.ConvertToMember(ctx.Guild);
            }
            catch { }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Soft banning {victim.UsernameWithDiscriminator} ({victim.Id})..`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                }
            }.AsLoading(ctx);
            await RespondOrEdit(embed);

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= (bMember?.GetRoleHighestPosition() ?? -1))
                    throw new Exception();

                await ctx.Guild.BanMemberAsync(victim.Id, deleteMessageDays, $"{ctx.User.UsernameWithDiscriminator} soft banned user: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");
                await ctx.Guild.UnbanMemberAsync(victim.Id, $"{ctx.User.UsernameWithDiscriminator} soft banned user: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");

                embed.Description = $"{victim.Mention} `was soft banned for '{(reason.IsNullOrWhiteSpace() ? "No reason provided" : reason).SanitizeForCode()}' by` {ctx.User.Mention}`.`";
                embed = embed.AsSuccess(ctx);
            }
            catch (Exception)
            {
                embed.Description = $"{victim.Mention} `could not be soft banned.`";
                embed = embed.AsError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}
