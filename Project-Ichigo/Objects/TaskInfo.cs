namespace Project_Ichigo.Objects;

internal class TaskInfo
{
    internal TaskInfo(Task task, CommandContext ctx = null)
    {
        this.ctx = ctx;
        this.task = task;
    }

    internal string uuid { get; private set; } = Guid.NewGuid().ToString();
    internal CommandContext? ctx { get; private set; } = null;
    internal Task task { get; private set; }
    internal DateTime CreationTimestamp { get; private set; } = DateTime.UtcNow;
}
