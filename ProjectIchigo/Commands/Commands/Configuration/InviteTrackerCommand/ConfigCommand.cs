﻿namespace ProjectIchigo.Commands.InviteTrackerCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = InviteTrackerCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, "Invite Tracker");

            var Toggle = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].InviteTracker.Enabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Invite Tracking", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📲")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Toggle
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Toggle.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].InviteTracker.Enabled = !ctx.Bot.guilds[ctx.Guild.Id].InviteTracker.Enabled;

                if (ctx.Bot.guilds[ctx.Guild.Id].InviteTracker.Enabled)
                    _ = InviteTrackerEvents.UpdateCachedInvites(ctx.Bot, ctx.Guild);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}