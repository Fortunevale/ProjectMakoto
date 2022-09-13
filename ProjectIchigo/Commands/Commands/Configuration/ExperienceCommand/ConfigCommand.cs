namespace ProjectIchigo.Commands.ExperienceCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = ExperienceCommandAbstractions.GetCurrentConfiguration(ctx)
            }.SetAwaitingInput(ctx, "Experience");

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var ToggleExperienceSystem = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].ExperienceSettings.UseExperience ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Experience System", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✨")));
            var ToggleBumperBoost = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Experience Boost for Bumpers", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏫")));

            await RespondOrEdit(builder
            .AddComponents(new List<DiscordComponent>
            {
                ToggleExperienceSystem,
                ToggleBumperBoost,
            })
            .AddComponents(MessageComponents.CancelButton));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == ToggleExperienceSystem.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].ExperienceSettings.UseExperience = !ctx.Bot.guilds[ctx.Guild.Id].ExperienceSettings.UseExperience;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ToggleBumperBoost.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder = !ctx.Bot.guilds[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}