namespace ProjectIchigo.Commands.VcCreator;

internal class NameCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            string newName = (string)arguments["newName"];
            DiscordChannel channel = ctx.Member.VoiceState.Channel;

            newName = (newName.IsNullOrWhiteSpace() ? $"{ctx.Member.DisplayName}'s Channel" : newName);

            if (!ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels.ContainsKey(channel.Id))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You're not in a channel created by the Voice Channel Creator.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].OwnerId != ctx.User.Id)
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You don't own this channel.`").AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].LastRename.GetTimespanSince() < TimeSpan.FromMinutes(5))
            {
                _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`You're on cooldown for renaming this channel. You can rename it again` {ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].LastRename.AddMinutes(5).ToTimestamp()}`.`").AsError(ctx));
                return;
            }

            foreach (var b in ctx.Bot.profanityList)
                newName = newName.Replace(b, new String('*', b.Length));

            ctx.Bot.guilds[ctx.Guild.Id].VcCreator.CreatedChannels[channel.Id].LastRename = DateTime.UtcNow;
            await channel.ModifyAsync(x => x.Name = newName.TruncateWithIndication(25));
            _ = await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`The channel has renamed to {newName.SanitizeForCode()}.`").AsSuccess(ctx));
        });
    }
}