// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

public static class InteractionExtensions
{
    public static string GetModalValueByCustomId(this DiscordInteraction interaction, string customId)
        => interaction.Data.Components.First(x => x.CustomId == customId).Value;

    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this SharedCommandContext context, TimeSpan? timeOutOverride = null)
        => context.Client.GetInteractivity().WaitForButtonAsync(context.ResponseMessage, context.User, timeOutOverride);

    public static async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this InteractionContext context, TimeSpan? timeOutOverride = null)
        => await context.Client.GetInteractivity().WaitForButtonAsync(await context.GetOriginalResponseAsync(), context.User, timeOutOverride);

    public static async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this ContextMenuContext context, TimeSpan? timeOutOverride = null)
        => await context.Client.GetInteractivity().WaitForButtonAsync(await context.GetOriginalResponseAsync(), context.User, timeOutOverride);
}