namespace ProjectIchigo.Commands;

internal class KickCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.KickMembers) && await CheckOwnPermissions(Permissions.KickMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordMember victim;
            string reason = (string)arguments["reason"];

            try
            {
                victim = await ((DiscordUser)arguments["victim"]).ConvertToMember(ctx.Guild);
            }
            catch (NotFoundException)
            {
                SendNoMemberError();
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Kicking {victim.UsernameWithDiscriminator} ({victim.Id})..`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
            }.SetLoading(ctx);
            await RespondOrEdit(embed);

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= victim.GetRoleHighestPosition())
                    throw new Exception();

                await victim.RemoveAsync($"{ctx.User.UsernameWithDiscriminator} kicked user: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");

                embed.Description = $"{victim.Mention} `was kicked for '{(reason.IsNullOrWhiteSpace() ? "No reason provided" : reason).SanitizeForCodeBlock()}' by` {ctx.User.Mention}`.`";
                embed = embed.SetSuccess(ctx);
            }
            catch (Exception)
            {
                embed.Description = $"{victim.Mention} `could not be kicked.`";
                embed.SetError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}