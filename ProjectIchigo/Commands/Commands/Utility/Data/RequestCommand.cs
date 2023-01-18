namespace ProjectIchigo.Commands.Data;

internal class RequestCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForHeavy(ctx.Client, ctx, true))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`{GetString(t.Commands.Data.Request.Fetching)}`"
            }.AsLoading(ctx));

            RequestData requestData = new();

            if (ctx.Bot.users.ContainsKey(ctx.User.Id))
            {
                requestData.User = ctx.Bot.users[ctx.User.Id];
            }

            foreach (var guild in ctx.Bot.guilds)
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
                        Description = $"`{GetString(t.Commands.Data.Request.Confirm).Replace("{User}", ctx.User.UsernameWithDiscriminator)}`"
                    }.AsSuccess(ctx)).WithFile("userdata.json", stream));
                    break;
                }
                default:
                {
                    try
                    {
                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`{GetString(t.Commands.Data.Request.Confirm).Replace("{User}", ctx.User.UsernameWithDiscriminator)}`"
                        }.AsSuccess(ctx)).WithFile("userdata.json", stream));

                        SendDmRedirect();
                    }
                    catch (DisCatSharp.Exceptions.UnauthorizedException)
                    {
                        SendDmError();
                    }
                    break;
                }
            }
        });
    }
}