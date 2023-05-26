// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;
internal class LeaderboardCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            int ShowAmount = (int)arguments["ShowAmount"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            if (!ctx.Bot.guilds[ctx.Guild.Id].Experience.UseExperience)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Utility.Leaderboard.Disabled, true,
                        new TVar("Command", $"{ctx.Prefix}experiencesettings config"))
                }.AsError(ctx, GetString(t.Commands.Utility.Leaderboard.Title)));
                return;
            }

            if (ShowAmount is > 50 or < 3)
            {
                SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Utility.Leaderboard.Fetching, true),
            }.AsLoading(ctx, GetString(t.Commands.Utility.Leaderboard.Title));

            await RespondOrEdit(embed: embed);

            int count = 0;

            int currentuserplacement = 0;

            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].Members.OrderByDescending(x => x.Value.Experience.Points))
            {
                currentuserplacement++;
                if (b.Key == ctx.User.Id)
                    break;
            }

            var members = await ctx.Guild.GetAllMembersAsync();

            List<KeyValuePair<string, string>> Board = new();

            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].Members.OrderByDescending(x => x.Value.Experience.Points))
            {
                try
                {
                    if (!members.Any(x => x.Id == b.Key))
                        continue;

                    DiscordMember bMember = members.First(x => x.Id == b.Key);

                    if (bMember is null)
                        continue;

                    if (bMember.IsBot)
                        continue;

                    if (b.Value.Experience.Points <= 1)
                        break;

                    count++;

                    Board.Add(new KeyValuePair<string, string>("󠂪 󠂪 ", $"**{count.ToEmotes()}**. <@{b.Key}> `{bMember.GetUsernameWithIdentifier()}` ({GetString(t.Commands.Utility.Leaderboard.Level, true, new TVar("Level", b.Value.Experience.Level), new TVar("Points", b.Value.Experience.Points))}"));

                    if (count >= ShowAmount)
                        break;
                }
                catch { }
            }

            var fields = Board.PrepareEmbedFields();

            foreach (var field in fields)
                embed.AddField(new DiscordEmbedField(field.Key, field.Value));

            if (count != 0)
            {
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Description = GetString(t.Commands.Utility.Leaderboard.Placement, new TVar("Placement", currentuserplacement));
                await RespondOrEdit(embed.AsInfo(ctx, GetString(t.Commands.Utility.Leaderboard.Title)));
            }
            else
            {
                embed.Description = $":no_entry_sign: {GetString(t.Commands.Utility.Leaderboard.NoPoints, true)}";
                await RespondOrEdit(embed.AsInfo(ctx, GetString(t.Commands.Utility.Leaderboard.Title)));
            }
        });
    }
}
