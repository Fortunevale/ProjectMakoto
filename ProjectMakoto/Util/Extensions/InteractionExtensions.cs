namespace ProjectMakoto.Util;

internal static class InteractionExtensions
{
    internal static string GetModalValueByCustomId(this DiscordInteraction interaction, string customId) 
        => interaction.Data.Components.First(x => x.CustomId == customId).Value;

    internal static async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this SharedCommandContext context, TimeSpan? timeOutOverride = null)
        => await context.Client.GetInteractivity().WaitForButtonAsync(context.ResponseMessage, context.User, timeOutOverride);
    
    internal static async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this InteractionContext context, TimeSpan? timeOutOverride = null)
        => await context.Client.GetInteractivity().WaitForButtonAsync(await context.GetOriginalResponseAsync(), context.User, timeOutOverride);
        
    internal static async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this ContextMenuContext context, TimeSpan? timeOutOverride = null)
        => await context.Client.GetInteractivity().WaitForButtonAsync(await context.GetOriginalResponseAsync(), context.User, timeOutOverride);
}