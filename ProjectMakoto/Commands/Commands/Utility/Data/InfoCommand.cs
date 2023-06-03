// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Data;

internal class InfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx, true))
                return;

            if (ctx.Bot.RawFetchedPrivacyPolicy.IsNullOrWhiteSpace())
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Utility.Data.Policy.NoPolicy, true, new TVar("Bot", ctx.CurrentUser.GetUsername())),
                }.AsBotError(ctx));
                return;
            }

            var RawPolicy = ctx.Bot.RawFetchedPrivacyPolicy.Replace("#", "");

            List<string> PolicyStrings = RawPolicy.ReplaceLineEndings("\n").Split("\n\n").ToList();

            string Title = "";
            List<DiscordEmbed> embeds = new();

            for (int i = 0; i < PolicyStrings.Count; i++)
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
                    await ctx.User.SendMessageAsync(b);

                SendDmRedirect();
            }
            catch (DisCatSharp.Exceptions.UnauthorizedException)
            {
                SendDmError();
            }
        });
    }
}