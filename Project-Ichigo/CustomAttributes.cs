namespace Project_Ichigo.Attributes;

internal class CustomAttributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class CommandUsageAttribute : Attribute
    {
        public readonly string UsageString;

        public CommandUsageAttribute(string CommandUsage)
        {
            this.UsageString = CommandUsage;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class CommandModuleAttribute : Attribute
    {
        public readonly string ModuleString;

        public CommandModuleAttribute(string ModuleName)
        {
            this.ModuleString = ModuleName;
        }
    }
}