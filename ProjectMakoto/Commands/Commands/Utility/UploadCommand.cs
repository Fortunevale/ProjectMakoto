// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class UploadCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            Stream stream = (Stream)arguments["stream"];
            int filesize = (int)arguments["filesize"];

            if (ctx.DbUser.PendingUserUpload is null)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Utility.Upload.NoInteraction, true)
                }.AsError(ctx));
                return;
            }

            if (ctx.DbUser.PendingUserUpload.InteractionHandled)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Utility.Upload.AlreadyUploaded, true)
                }.AsError(ctx));
                return;
            }

            if (ctx.DbUser.PendingUserUpload.TimeOut.GetTotalSecondsUntil() < 0)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Utility.Upload.TimedOut, true,
                        new TVar("Timestamp", ctx.DbUser.PendingUserUpload.TimeOut.ToTimestamp()))
                }.AsError(ctx));
                ctx.DbUser.PendingUserUpload = null;
                return;
            }

            ctx.DbUser.PendingUserUpload.UploadedData = stream;
            ctx.DbUser.PendingUserUpload.FileSize = filesize;
            ctx.DbUser.PendingUserUpload.InteractionHandled = true;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(this.t.Commands.Utility.Upload.Uploaded, true)
            }.AsSuccess(ctx));

            await Task.Delay(500);
            DeleteOrInvalidate();
        });
    }
}