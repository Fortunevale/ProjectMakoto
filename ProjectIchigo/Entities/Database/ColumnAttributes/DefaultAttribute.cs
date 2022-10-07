namespace ProjectIchigo.Entities.Database.ColumnAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DefaultAttribute : Attribute
{
    public readonly string Default;

    public DefaultAttribute(string Default)
    {
        this.Default = Default;
    }
}
