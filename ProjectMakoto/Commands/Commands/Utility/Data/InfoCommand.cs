// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Data;

internal sealed class InfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx, true))
                return;

            if (ctx.Bot.RawFetchedPrivacyPolicy.IsNullOrWhiteSpace())
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Utility.Data.Policy.NoPolicy, true, new TVar("Bot", ctx.CurrentUser.GetUsername())),
                }.AsBotError(ctx));
                return;
            }

            var RawPolicy = ctx.Bot.RawFetchedPrivacyPolicy.Replace("#", "");

            var PolicyStrings = RawPolicy.ReplaceLineEndings("\n").Split("\n\n").ToList();

            var Title = "";
            List<DiscordEmbed> embeds = new();

            for (var i = 0; i < PolicyStrings.Count; i++)
            {
                if (i == 0)
                {
                    Title = PolicyStrings[i];
                    continue;
                }

                embeds.Add(new DiscordEmbedBuilder
                {
                    Title = (i == 1 ? Title : ""),
                    Description = PolicyStrings[i]
                });
            }

            try
            {
                foreach (var b in embeds)
                    _ = await ctx.User.SendMessageAsync(b);

                this.SendDmRedirect();
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                this.SendDmError();
            }
        });
    }
}