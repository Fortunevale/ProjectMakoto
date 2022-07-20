namespace ProjectIchigo.Commands.InviteTrackerCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Invite Tracker • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = InviteTrackerCommandAbstractions.GetCurrentConfiguration(ctx)
            };

            var Toggle = new DiscordButtonComponent((ctx.Bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Invite Tracking", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📲")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                    Toggle
            })
            .AddComponents(Resources.CancelButton));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == Toggle.CustomId)
            {
                ctx.Bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled = !ctx.Bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled;

                if (ctx.Bot._guilds.List[ctx.Guild.Id].InviteTrackerSettings.Enabled)
                    _ = InviteTrackerEvents.UpdateCachedInvites(ctx.Bot, ctx.Guild);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}