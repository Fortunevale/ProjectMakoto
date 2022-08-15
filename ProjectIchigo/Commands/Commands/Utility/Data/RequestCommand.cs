namespace ProjectIchigo.Commands.Data;

internal class RequestCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.User.Id].Cooldown.WaitForHeavy(ctx.Client, ctx, true))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = "`Fetching all your user data. This might take a moment..`"
            }.SetLoading(ctx));

            RequestData requestData = new();

            if (ctx.Bot._users.ContainsKey(ctx.User.Id))
            {
                requestData.User = ctx.Bot._users[ctx.User.Id];
            }

            foreach (var guild in ctx.Bot._guilds)
            {
                if (guild.Value.Members.ContainsKey(ctx.User.Id))
                {
                    requestData.GuildData.Add(guild.Key, guild.Value.Members[ctx.User.Id]);
                }
            }

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData, Formatting.Indented)));

            switch (ctx.CommandType)
            {
                case Enums.CommandType.ApplicationCommand:
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`Hello, {ctx.User.UsernameWithDiscriminator}. Here's your user data you requested. This may contain sensitive information.`"
                    }.SetSuccess(ctx)).WithFile("userdata.json", stream));
                    break;
                }
                default:
                {
                    try
                    {
                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`Hello, {ctx.User.UsernameWithDiscriminator}. Here's your user data you requested. This may contain sensitive information.`"
                        }.SetSuccess(ctx)).WithFile("userdata.json", stream));

                        await RespondOrEdit(new DiscordEmbedBuilder
                        {
                            Description = "`I sent your data via direct messages.`"
                        }.SetSuccess(ctx));
                    }
                    catch (DisCatSharp.Exceptions.UnauthorizedException)
                    {
                        var errorembed = new DiscordEmbedBuilder
                        {
                            Description = "`It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                            ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                        }.SetError(ctx);

                        if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                            errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                        await RespondOrEdit(errorembed);
                    }
                    break;
                }
            }
        });
    }
}