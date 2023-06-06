// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ActionLogCommand;

internal sealed class ActionLogAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var CommandKey = ctx.Bot.loadedTranslations.Commands.Config.ActionLog;

        if (ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel == 0)
            return $"❌ {CommandKey.ActionlogDisabled.Get(ctx.DbUser).Build(true)}";

        var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.InviteModifications, CommandKey.VoiceChannelUpdates, CommandKey.ChannelModifications, CommandKey.ServerModifications, CommandKey.BanUpdates,
            CommandKey.RoleUpdates, CommandKey.MessageModifications, CommandKey.MessageModifications, CommandKey.UserProfileUpdates, CommandKey.UserRoleUpdates, CommandKey.UserStateUpdates,
            CommandKey.AttemptGatheringMoreDetails, CommandKey.ActionLogChannel);

        return $"{EmojiTemplates.GetChannel(ctx.Bot)} `{CommandKey.ActionLogChannel.Get(ctx.DbUser).PadRight(pad)}` : <#{ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel}>\n\n" +
               $"⚠ `{CommandKey.AttemptGatheringMoreDetails.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.AttemptGettingMoreDetails.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.UserStateUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MembersModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.UserRoleUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.UserProfileUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberProfileModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetMessage(ctx.Bot)} `{CommandKey.MessageDeletions.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageDeleted.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetMessage(ctx.Bot)} `{CommandKey.MessageModifications.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.RoleUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.RolesModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.BanUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.BanlistModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetGuild(ctx.Bot)} `{CommandKey.ServerModifications.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.GuildModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetChannel(ctx.Bot)} `{CommandKey.ChannelModifications.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.ChannelsModified.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetVoiceState(ctx.Bot)} `{CommandKey.VoiceChannelUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.VoiceStateUpdated.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetInvite(ctx.Bot)} `{CommandKey.InviteModifications.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].ActionLog.InvitesModified.ToEmote(ctx.Bot)}";
    }
}
