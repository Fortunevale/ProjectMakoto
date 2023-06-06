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

            if (!ctx.Bot.uploadInteractions.ContainsKey(ctx.User.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Utility.Upload.NoInteraction, true)
                }.AsError(ctx));
                return;
            }

            if (ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Utility.Upload.AlreadyUploaded, true)
                }.AsError(ctx));
                return;
            }

            if (ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.GetTotalSecondsUntil() < 0)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Utility.Upload.TimedOut, true,
                        new TVar("Timestamp", ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.ToTimestamp()))
                }.AsError(ctx));
                ctx.Bot.uploadInteractions.Remove(ctx.User.Id);
                return;
            }

            ctx.Bot.uploadInteractions[ctx.User.Id].UploadedData = stream;
            ctx.Bot.uploadInteractions[ctx.User.Id].FileSize = filesize;
            ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled = true;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = GetString(this.t.Commands.Utility.Upload.Uploaded, true)
            }.AsSuccess(ctx));

            await Task.Delay(500);
            DeleteOrInvalidate();
        });
    }
}