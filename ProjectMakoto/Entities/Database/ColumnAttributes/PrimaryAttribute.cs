namespace ProjectMakoto.Entities.Database.ColumnAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PrimaryAttribute : Attribute
{
    public readonly bool Primary;

    public PrimaryAttribute(bool Primary = true)
    {
        this.Primary = Primary;
    }
}
