// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class InviteTrackerCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.InviteTracker;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                return $"{"📲".UnicodeToEmoji()} `{this.GetString(this.t.Commands.Config.InviteTracker.InviteTrackerEnabled)}`: {ctx.DbGuild.InviteTracker.Enabled.ToEmote(ctx.Bot)}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var Toggle = new DiscordButtonComponent((ctx.DbGuild.InviteTracker.Enabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleInviteTrackerButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📲")));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Toggle
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Toggle.CustomId)
            {
                ctx.DbGuild.InviteTracker.Enabled = !ctx.DbGuild.InviteTracker.Enabled;

                if (ctx.DbGuild.InviteTracker.Enabled)
                    _ = InviteTrackerEvents.UpdateCachedInvites(ctx.Bot, ctx.Guild);

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}