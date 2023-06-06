// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.InVoicePrivacyCommand;

internal sealed class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Config.InVoicePrivacy;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = InVoicePrivacyCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            var ToggleDeletion = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacy.ClearTextEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleMessageDeletionButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
            var TogglePermission = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacy.SetPermissionsEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.TogglePermissionProtectionButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📋")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                ToggleDeletion,
                TogglePermission
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ToggleDeletion.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacy.ClearTextEnabled = !ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacy.ClearTextEnabled;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == TogglePermission.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacy.SetPermissionsEnabled = !ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacy.SetPermissionsEnabled;

                _ = Task.Run(async () =>
                {
                    if (ctx.Bot.guilds[ctx.Guild.Id].InVoiceTextPrivacy.SetPermissionsEnabled)
                    {
                        if (!ctx.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                            return;

                        foreach (var b in ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                        {
                            _ = b.Value.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.None, Permissions.ReadMessageHistory | Permissions.SendMessages, GetGuildString(CommandKey.EnabledInVoicePrivacy));
                        }
                    }
                    else
                    {
                        if (!ctx.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                            return;

                        foreach (var b in ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                        {
                            _ = b.Value.DeleteOverwriteAsync(ctx.Guild.EveryoneRole, GetGuildString(CommandKey.DisabledInVoicePrivacy));
                        }
                    }
                });

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}