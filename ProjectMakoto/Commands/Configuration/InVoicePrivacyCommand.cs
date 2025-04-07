// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Configuration;

internal sealed class InVoicePrivacyCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.InVoicePrivacy;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.InVoicePrivacy;

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.ClearMessagesOnLeave, CommandKey.SetPermissions);

                return $"{"🗑".UnicodeToEmoji()} `{CommandKey.ClearMessagesOnLeave.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.InVoiceTextPrivacy.ClearTextEnabled.ToEmote(ctx.Bot)}\n" +
                       $"{"📋".UnicodeToEmoji()} `{CommandKey.SetPermissions.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.InVoiceTextPrivacy.SetPermissionsEnabled.ToEmote(ctx.Bot)}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var ToggleDeletion = new DiscordButtonComponent((ctx.DbGuild.InVoiceTextPrivacy.ClearTextEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleMessageDeletionButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
            var TogglePermission = new DiscordButtonComponent((ctx.DbGuild.InVoiceTextPrivacy.SetPermissionsEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.TogglePermissionProtectionButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📋")));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                ToggleDeletion,
                TogglePermission
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ToggleDeletion.CustomId)
            {
                ctx.DbGuild.InVoiceTextPrivacy.ClearTextEnabled = !ctx.DbGuild.InVoiceTextPrivacy.ClearTextEnabled;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == TogglePermission.CustomId)
            {
                ctx.DbGuild.InVoiceTextPrivacy.SetPermissionsEnabled = !ctx.DbGuild.InVoiceTextPrivacy.SetPermissionsEnabled;

                _ = Task.Run(async () =>
                {
                    if (ctx.DbGuild.InVoiceTextPrivacy.SetPermissionsEnabled)
                    {
                        if (!ctx.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                            return;

                        foreach (var b in ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                        {
                            _ = b.Value.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.None, Permissions.ReadMessageHistory | Permissions.SendMessages, this.GetGuildString(CommandKey.EnabledInVoicePrivacy));
                        }
                    }
                    else
                    {
                        if (!ctx.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                            return;

                        foreach (var b in ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                        {
                            _ = b.Value.DeleteOverwriteAsync(ctx.Guild.EveryoneRole, this.GetGuildString(CommandKey.DisabledInVoicePrivacy));
                        }
                    }
                });

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}