// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using DisCatSharp.Extensions.TwoFactorCommands.Enums;

namespace ProjectMakoto.Commands;

internal class Disenroll2FAUserCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) 
        => await CheckMaintenance() && await CheckBotOwner();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (!ctx.Client.CheckTwoFactorEnrollmentFor(victim.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{victim.GetUsernameWithIdentifier()} is not enrolled in Two Factor Authentication.`").AsBotError(ctx));
                return;
            }

            ctx.Client.DisenrollTwoFactor(victim.Id);
            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Two Factor Authentication removed for {victim.GetUsername()}.`").AsBotSuccess(ctx));
        });
    }
}