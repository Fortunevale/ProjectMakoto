// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;
internal sealed class RankCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victim = (DiscordUser)arguments["user"];

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            if (!ctx.DbGuild.Experience.UseExperience)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Utility.Leaderboard.Disabled, true,
                        new TVar("Command", $"{ctx.Prefix}experiencesettings config"))
                }.AsError(ctx, this.GetString(this.t.Commands.Utility.Rank.Title)));
                return;
            }

            victim ??= ctx.User;

            victim = await victim.GetFromApiAsync();

            var current = (long)Math.Floor((decimal)(ctx.DbGuild.Members[victim.Id].Experience.Points - ctx.Bot.ExperienceHandler.CalculateLevelRequirement(ctx.DbGuild.Members[victim.Id].Experience.Level - 1)));
            var max = (long)Math.Floor((decimal)(ctx.Bot.ExperienceHandler.CalculateLevelRequirement(ctx.DbGuild.Members[victim.Id].Experience.Level) - ctx.Bot.ExperienceHandler.CalculateLevelRequirement(ctx.DbGuild.Members[victim.Id].Experience.Level - 1)));

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"{(victim.Id == ctx.User.Id ? this.GetString(this.t.Commands.Utility.Rank.Self, new TVar("Level", ctx.DbGuild.Members[victim.Id].Experience.Level.ToEmotes()), new TVar("Points", ctx.DbGuild.Members[victim.Id].Experience.Points.ToString("N0", CultureInfo.GetCultureInfo("en-US")))) : this.GetString(this.t.Commands.Utility.Rank.Other, new TVar("User", victim.Mention), new TVar("Level", ctx.DbGuild.Members[victim.Id].Experience.Level.ToEmotes()), new TVar("Points", ctx.DbGuild.Members[victim.Id].Experience.Points.ToString("N0", CultureInfo.GetCultureInfo("en-US")))))}\n\n" +
                              $"**{this.GetString(this.t.Commands.Utility.Rank.Progress, new TVar("Level", (ctx.DbGuild.Members[victim.Id].Experience.Level + 1).ToEmotes()))}**\n" +
                              $"`{Math.Floor((decimal)((decimal)((decimal)current / (decimal)max) * 100)).ToString().Replace(",", ".")}%` " +
                              $"`{StringTools.GenerateASCIIProgressbar(current, max, 44)}` " +
                              $"`{current}/{max} XP`",
            }.AsInfo(ctx, this.GetString(this.t.Commands.Utility.Rank.Title)));
        });
    }
}
