namespace Project_Ichigo.Objects;

internal class TaskInfo
{
    internal string uuid { get; set; }
    internal CommandContext? ctx { get; set; } = null;
    internal Task task { get; set; }
}
