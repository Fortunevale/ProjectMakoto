namespace ProjectIchigo.Entities;

internal class TaskInfo
{
    internal TaskInfo(Task task)
    {
        this.task = task;
    }
    
    internal TaskInfo(Task task, CommandContext ctx = null)
    {
        this.CommandContext = ctx;
        this.task = task;
    }

    internal TaskInfo(Task task, InteractionContext ctx = null)
    {
        this.InteractionContext = ctx;
        this.task = task;
    }
    
    internal TaskInfo(Task task, SharedCommandContext ctx = null)
    {
        this.SharedCommandContext = ctx;
        this.task = task;
    }
    
    internal TaskInfo(Task task, ContextMenuContext ctx = null)
    {
        this.ContextMenuContext = ctx;
        this.task = task;
    }

    internal string uuid { get; private set; } = Guid.NewGuid().ToString();
    internal CommandContext? CommandContext { get; private set; } = null;
    internal InteractionContext? InteractionContext { get; private set; } = null;
    internal SharedCommandContext? SharedCommandContext { get; private set; } = null;
    internal ContextMenuContext? ContextMenuContext { get; private set; } = null;
    internal Task task { get; private set; }
    internal DateTime CreationTimestamp { get; private set; } = DateTime.UtcNow;
}
