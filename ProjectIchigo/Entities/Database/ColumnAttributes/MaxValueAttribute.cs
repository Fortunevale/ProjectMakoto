namespace ProjectIchigo.Entities.Database.ColumnAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MaxValueAttribute : Attribute
{
    public readonly long MaxValue;

    public MaxValueAttribute(long MaxValue)
    {
        this.MaxValue = MaxValue;
    }
}
