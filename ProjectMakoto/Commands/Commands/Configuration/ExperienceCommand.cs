// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class ExperienceCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.Experience;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.Experience;

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.ExperienceEnabled, CommandKey.ExperienceBoostForBumpers);

                return $"{"✨".UnicodeToEmoji()} `{CommandKey.ExperienceEnabled.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Experience.UseExperience.ToEmote(ctx.Bot)}\n" +
                       $"{"⏫".UnicodeToEmoji()} `{CommandKey.ExperienceBoostForBumpers.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Experience.BoostXpForBumpReminder.ToEmote(ctx.Bot)}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder()
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var ToggleExperienceSystem = new DiscordButtonComponent((ctx.DbGuild.Experience.UseExperience ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleExperienceButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✨")));
            var ToggleBumperBoost = new DiscordButtonComponent((ctx.DbGuild.Experience.BoostXpForBumpReminder ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleExperienceBoostButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏫")));

            _ = await this.RespondOrEdit(builder
            .AddComponents(new List<DiscordComponent>
            {
                ToggleExperienceSystem,
                ToggleBumperBoost,
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ToggleExperienceSystem.CustomId)
            {
                ctx.DbGuild.Experience.UseExperience = !ctx.DbGuild.Experience.UseExperience;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ToggleBumperBoost.CustomId)
            {
                ctx.DbGuild.Experience.BoostXpForBumpReminder = !ctx.DbGuild.Experience.BoostXpForBumpReminder;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                this.DeleteOrInvalidate();
            }
        });
    }
}