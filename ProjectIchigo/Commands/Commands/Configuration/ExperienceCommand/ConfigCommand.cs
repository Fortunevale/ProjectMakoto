namespace ProjectIchigo.Commands.ExperienceCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = ExperienceCommandAbstractions.GetCurrentConfiguration(ctx)
            }.SetAwaitingInput(ctx, "Experience");

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var ToggleExperienceSystem = new DiscordButtonComponent((ctx.Bot._guilds[ctx.Guild.Id].ExperienceSettings.UseExperience ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Experience System", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✨")));
            var ToggleBumperBoost = new DiscordButtonComponent((ctx.Bot._guilds[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Experience Boost for Bumpers", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏫")));

            await RespondOrEdit(builder
            .AddComponents(new List<DiscordComponent>
            {
                ToggleExperienceSystem,
                ToggleBumperBoost,
            })
            .AddComponents(Resources.CancelButton));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == ToggleExperienceSystem.CustomId)
            {
                ctx.Bot._guilds[ctx.Guild.Id].ExperienceSettings.UseExperience = !ctx.Bot._guilds[ctx.Guild.Id].ExperienceSettings.UseExperience;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ToggleBumperBoost.CustomId)
            {
                ctx.Bot._guilds[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder = !ctx.Bot._guilds[ctx.Guild.Id].ExperienceSettings.BoostXpForBumpReminder;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}