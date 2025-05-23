// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Data;

internal sealed class RequestCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx, true))
                return;

            if (ctx.DbUser.Data.LastDataRequest.GetTimespanSince() < TimeSpan.FromDays(14))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Utility.Data.Request.TimeError, true,
                        new TVar("RequestTimestamp", ctx.DbUser.Data.LastDataRequest.ToTimestamp(TimestampFormat.ShortDateTime)),
                        new TVar("WaitTimestamp", ctx.DbUser.Data.LastDataRequest.AddDays(14).ToTimestamp(TimestampFormat.ShortDateTime)))
                }.AsError(ctx));
                return;
            }

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Utility.Data.Request.Fetching, true)
            }.AsLoading(ctx));

            RequestData requestData = new();

            if (ctx.Bot.Users.ContainsKey(ctx.User.Id))
            {
                requestData.User = ctx.DbUser;
            }

            foreach (var guild in ctx.Bot.Guilds)
            {
                if (guild.Value.Members.TryGetValue(ctx.User.Id, out var member))
                {
                    requestData.GuildData.Add(guild.Key, member);
                }
            }

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestData, Formatting.Indented)));

            switch (ctx.CommandType)
            {
                case Enums.CommandType.ApplicationCommand:
                {
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Utility.Data.Request.Confirm, true)
                    }.AsSuccess(ctx)).WithFile("userdata.json", stream));
                    ctx.DbUser.Data.LastDataRequest = DateTime.UtcNow;
                    break;
                }
                default:
                {
                    try
                    {
                        _ = await ctx.User.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = this.GetString(this.t.Commands.Utility.Data.Request.Confirm, true)
                        }.AsSuccess(ctx)).WithFile("userdata.json", stream));
                        ctx.DbUser.Data.LastDataRequest = DateTime.UtcNow;

                        this.SendDmRedirect();
                    }
                    catch (DisCatSharp.Exceptions.UnauthorizedException)
                    {
                        this.SendDmError();
                    }
                    break;
                }
            }
        });
    }
}