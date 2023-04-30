namespace ProjectMakoto.Commands.Data;

internal class RequestCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForHeavy(ctx, true))
                return;

            if (ctx.DbUser.Data.LastDataRequest.GetTimespanSince() < TimeSpan.FromDays(14))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(t.Commands.Utility.Data.Request.TimeError).Replace("{RequestTimestamp}", $"`{ctx.DbUser.Data.LastDataRequest.ToTimestamp(TimestampFormat.ShortDateTime)}`").Replace("{WaitTimestamp}", $"`{ctx.DbUser.Data.LastDataRequest.AddDays(14).ToTimestamp(TimestampFormat.ShortDateTime)}`")}`"
                }.AsError(ctx));
                return;
            }

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`{GetString(t.Commands.Utility.Data.Request.Fetching)}`"
            }.AsLoading(ctx));

            RequestData requestData = new();

            if (ctx.Bot.users.ContainsKey(ctx.User.Id))
            {
                requestData.User = ctx.Bot.users[ctx.User.Id];
            }

            foreach (var guild in ctx.Bot.guilds)
            {
                if (guild.Value.Members.TryGetValue(ctx.User.Id, out Member member))
                {
                    requestData.GuildData.Add(guild.Key, member);
                }
            }

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData, Formatting.Indented)));

            switch (ctx.CommandType)
            {
                case Enums.CommandType.ApplicationCommand:
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(t.Commands.Utility.Data.Request.Confirm).Replace("{User}", ctx.User.UsernameWithDiscriminator)}`"
                    }.AsSuccess(ctx)).WithFile("userdata.json", stream));
                    ctx.DbUser.Data.LastDataRequest = DateTime.UtcNow;
                    break;
                }
                default:
                {
                    try
                    {
                        await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`{GetString(t.Commands.Utility.Data.Request.Confirm).Replace("{User}", ctx.User.UsernameWithDiscriminator)}`"
                        }.AsSuccess(ctx)).WithFile("userdata.json", stream));
                        ctx.DbUser.Data.LastDataRequest = DateTime.UtcNow;

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