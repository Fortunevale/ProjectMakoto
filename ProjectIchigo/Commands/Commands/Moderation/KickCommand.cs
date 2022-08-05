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
                Color = EmbedColors.Processing,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = victim.AvatarUrl
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            };
            await RespondOrEdit(embed);

            try
            {
                if (ctx.Member.GetRoleHighestPosition() <= victim.GetRoleHighestPosition())
                    throw new Exception();

                await victim.RemoveAsync($"{ctx.User.UsernameWithDiscriminator} kicked user: {(reason.IsNullOrWhiteSpace() ? "No reason provided." : reason)}");

                embed.Color = EmbedColors.Success;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = $"<@{victim.Id}> `{victim.UsernameWithDiscriminator}` was kicked.\n\n" +
                                                        $"Reason: `{reason}`\n" +
                                                        $"Kicked by: {ctx.Member.Mention} `{ctx.Member.Username}#{ctx.Member.Discriminator}` (`{ctx.Member.Id}`)";
            }
            catch (Exception)
            {
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = $"❌ `{victim.UsernameWithDiscriminator} ({victim.Id}) couldn't be kicked.`";
            }

            await RespondOrEdit(embed);
        });
    }
}