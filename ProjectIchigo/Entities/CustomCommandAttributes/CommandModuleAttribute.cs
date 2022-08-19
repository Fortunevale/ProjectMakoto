namespace ProjectIchigo.Entities;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class CommandModuleAttribute : Attribute
{
    public readonly string ModuleString;

    public CommandModuleAttribute(string ModuleName)
    {
        ModuleString = ModuleName;
    }
}
