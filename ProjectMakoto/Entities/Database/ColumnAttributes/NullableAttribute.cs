namespace ProjectMakoto.Entities.Database.ColumnAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NullableAttribute : Attribute
{
    public readonly bool Nullable;

    public NullableAttribute(bool Nullable = true)
    {
        this.Nullable = Nullable;
    }
}
