// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class ScoreSaberProfileCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var id = (string)arguments["profile"];

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var AddLinkButton = true;

            if ((string.IsNullOrWhiteSpace(id) || id.Contains('@')))
            {
                DiscordUser user;

                try
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        if (Regex.IsMatch(id, @"<@((!?)(\d*))>", RegexOptions.ExplicitCapture))
                            user = await ctx.Client.GetUserAsync(Convert.ToUInt64(Regex.Match(id, @"<@((!?)(\d*))>").Groups[3].Value));
                        else
                        {
                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = this.GetString(this.t.Commands.ScoreSaber.Profile.InvalidInput, true)
                            }.AsError(ctx, "Score Saber")));
                            return;
                        }
                    }
                    else
                        user = ctx.User;
                }
                catch (DisCatSharp.Exceptions.NotFoundException)
                {
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.ScoreSaber.Profile.NoUser, true)
                    }.AsError(ctx, "Score Saber")));
                    return;
                }

                if (ctx.Bot.Users[user.Id].ScoreSaber.Id != 0)
                {
                    id = ctx.Bot.Users[user.Id].ScoreSaber.Id.ToString();
                    AddLinkButton = false;
                }
                else
                {
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.ScoreSaber.Profile.NoProfile, true)
                    }.AsError(ctx, "Score Saber")));
                    return;
                }
            }

            await ScoreSaberCommandAbstractions.SendScoreSaberProfile(ctx, id, AddLinkButton);
        });
    }
}