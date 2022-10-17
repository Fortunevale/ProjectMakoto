namespace ProjectIchigo.Commands;

internal class RemoveTimeoutCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ModerateMembers) && await CheckOwnPermissions(Permissions.ModerateMembers));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordMember victim;

            try
            {
                victim = await ((DiscordUser)arguments["victim"]).ConvertToMember(ctx.Guild);
            }
            catch (DisCatSharp.Exceptions.NotFoundException)
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
                Description = $"`Removing timeout for {victim.UsernameWithDiscriminator} ({victim.Id})..`",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
            }.AsLoading(ctx);
            await RespondOrEdit(embed: embed);

            try
            {
                await victim.RemoveTimeoutAsync();
                embed.Description = $"`Removed timeout for {victim.UsernameWithDiscriminator} ({victim.Id}).`";
                embed = embed.AsSuccess(ctx);
            }
            catch (Exception)
            {
                embed.Description = $"`Couldn't remove timeout for {victim.UsernameWithDiscriminator} ({victim.Id}).`";
                embed = embed.AsError(ctx);
            }

            await RespondOrEdit(embed);
        });
    }
}