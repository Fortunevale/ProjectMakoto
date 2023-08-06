// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;
internal sealed class BannerCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victim = (DiscordUser)arguments["victim"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            victim ??= ctx.User;

            victim = await victim.GetFromApiAsync();

            var embed = new DiscordEmbedBuilder
            {
                ImageUrl = victim.BannerUrl,
                Description = victim.BannerUrl.IsNullOrWhiteSpace() ? this.GetString(this.t.Commands.Utility.Banner.NoBanner, true) : ""
            }.AsInfo(ctx, this.GetString(this.t.Commands.Utility.Banner.Banner, false, new TVar("User", victim.GetUsernameWithIdentifier())));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            _ = await this.RespondOrEdit(builder);
        });
    }
}
