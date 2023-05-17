﻿// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ExperienceCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Config.Experience;

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = ExperienceCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var ToggleExperienceSystem = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].Experience.UseExperience ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleExperienceButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✨")));
            var ToggleBumperBoost = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].Experience.BoostXpForBumpReminder ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleExperienceBoostButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏫")));

            await RespondOrEdit(builder
            .AddComponents(new List<DiscordComponent>
            {
                ToggleExperienceSystem,
                ToggleBumperBoost,
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ToggleExperienceSystem.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].Experience.UseExperience = !ctx.Bot.guilds[ctx.Guild.Id].Experience.UseExperience;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ToggleBumperBoost.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].Experience.BoostXpForBumpReminder = !ctx.Bot.guilds[ctx.Guild.Id].Experience.BoostXpForBumpReminder;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}