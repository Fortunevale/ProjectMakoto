namespace ProjectMakoto.Entities;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class CommandModuleAttribute : Attribute
{
    public readonly string ModuleString;

    public CommandModuleAttribute(string ModuleName)
    {
        this.ModuleString = ModuleName;
    }
}
