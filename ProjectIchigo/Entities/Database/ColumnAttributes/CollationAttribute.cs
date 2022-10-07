namespace ProjectIchigo.Entities.Database.ColumnAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CollationAttribute : Attribute
{
    public readonly string Collation;

    public CollationAttribute(string Collation)
    {
        this.Collation = Collation;
    }
}
