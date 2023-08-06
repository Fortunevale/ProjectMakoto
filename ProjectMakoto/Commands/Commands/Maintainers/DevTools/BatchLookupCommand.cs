// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class BatchLookupCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var IDs = ((string)arguments["IDs"]).Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => x.ToUInt64()).ToList();

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Looking up {IDs.Count} users..`\n`{StringTools.GenerateASCIIProgressbar(0d, IDs.Count)}`").AsLoading(ctx));

            Dictionary<ulong, DiscordUser> fetched = new();

            for (var i = 0; i < IDs.Count; i++)
            {
                try
                {
                    fetched.Add(IDs[i], await ctx.Client.GetUserAsync(IDs[i]));
                }
                catch (Exception)
                {
                    fetched.Add(IDs[i], null);
                }

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Looking up {IDs.Count} users..`\n`{StringTools.GenerateASCIIProgressbar(i, IDs.Count)}`").AsLoading(ctx));
            }

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(string.Join("\n", fetched.Select(x => $"{(x.Value is null ? $"❌ `Failed to fetch '{x.Key}'`" : $"✅ {x.Value.Mention} `{x.Value.GetUsernameWithIdentifier()}` (`{x.Value.Id}`)")}"))).AsSuccess(ctx));
        });
    }
}