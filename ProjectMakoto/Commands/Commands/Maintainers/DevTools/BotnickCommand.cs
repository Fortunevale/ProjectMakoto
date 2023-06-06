// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class BotnickCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string newNickname = (string)arguments["newNickname"];

            try
            {
                await ctx.Guild.CurrentMember.ModifyAsync(x => x.Nickname = newNickname);

                if (newNickname.IsNullOrWhiteSpace())
                    await RespondOrEdit($"My nickname on this server has been reset.");
                else
                    await RespondOrEdit($"My nickname on this server has been changed to **{newNickname}**.");
            }
            catch (Exception)
            {
                await RespondOrEdit($"My nickname could not be changed.");
            }
        });
    }
}
