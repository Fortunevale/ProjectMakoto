// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class StopCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var msg = await this.RespondOrEdit(new DiscordMessageBuilder().WithContent("Confirm?").AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, "Shutdown", "Confirm shutdown", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⛔")))));

            var x = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(1));

            if (x.TimedOut)
            {
                _ = await this.RespondOrEdit("_Interaction timed out._");
                return;
            }

            _ = await this.RespondOrEdit("Shutting down!");

            await ctx.Bot.ExitApplication(true);
        });
    }
}
