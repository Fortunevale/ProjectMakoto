// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class ManualBumpCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Moderation.ManualBump;

            if (!ctx.DbGuild.BumpReminder.Enabled)
            {
                _ = RespondOrEdit(new DiscordEmbedBuilder().WithDescription(GetString(CommandKey.NotSetUp, true)).AsError(ctx));
                return;
            }

            DiscordButtonComponent YesButton = new(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(this.t.Common.Yes), false, DiscordEmoji.FromUnicode("✅").ToComponent());

            await RespondOrEdit(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder().WithDescription(GetString(CommandKey.Warning, true)).AsWarning(ctx))
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(this.t.Common.No), false, DiscordEmoji.FromUnicode("❌").ToComponent()), YesButton));

            var e = await ctx.ResponseMessage.WaitForButtonAsync(ctx.User);

            if (e.TimedOut || e.GetCustomId() != YesButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }

            DiscordChannel channel = ctx.Guild.GetChannel(ctx.DbGuild.BumpReminder.ChannelId);

            ctx.DbGuild.BumpReminder.LastBump = DateTime.UtcNow;
            ctx.DbGuild.BumpReminder.LastReminder = DateTime.UtcNow;
            ctx.DbGuild.BumpReminder.BumpsMissed = 0;
            ctx.DbGuild.BumpReminder.LastUserId = 0;
            ctx.Bot.BumpReminder.ScheduleBump(ctx.Client, ctx.Guild.Id);

            _ = channel.DeleteMessageAsync(await channel.GetMessageAsync(ctx.DbGuild.BumpReminder.PersistentMessageId));
            DeleteOrInvalidate();
        });
    }
}