// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class AfkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string reason = (string)arguments["reason"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            if (reason.Length > 128)
            {
                SendSyntaxError();
                return;
            }

            ctx.DbUser.AfkStatus.Reason = reason.FullSanitize();
            ctx.DbUser.AfkStatus.TimeStamp = DateTime.UtcNow;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"{ctx.User.Mention} {GetString(t.Commands.Social.Afk.SetAfk, true)}"
            }.AsSuccess(ctx, GetString(t.Commands.Social.Afk.Title)));
            await Task.Delay(10000);
            _ = ctx.ResponseMessage.DeleteAsync();
        });
    }
}