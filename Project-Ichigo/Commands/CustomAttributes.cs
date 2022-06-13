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

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class PreventCommandDeletionAttribute : Attribute
    {
        public readonly bool PreventDeleteCommandMessage;

        public PreventCommandDeletionAttribute(bool PreventDeleteMessage = true)
        {
            this.PreventDeleteCommandMessage = PreventDeleteMessage;
        }
    }
}