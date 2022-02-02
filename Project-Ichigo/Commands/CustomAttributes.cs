namespace Project_Ichigo.Attributes;

internal class CustomAttributes
{
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