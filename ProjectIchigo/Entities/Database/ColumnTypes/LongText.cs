namespace ProjectIchigo.Entities.Database.ColumnTypes;

public class LongText : BaseColumn
{
    public LongText(long MaxLength = 4294967296)
    {
        if (MaxLength > 4294967296)
            throw new ArgumentException("The maximum size of a LongText is 4294967296");

        this.MaxLength = MaxLength;
    }

    public long MaxLength { get; private set; }

    private string? _Value { get; set; }
    public string? Value { get => _Value; set { _Value = (value.Length.ToInt64() > MaxLength ? throw new ArgumentException($"The maximum length for this string is {MaxLength}") : value); } }

    public static implicit operator string(LongText b) => b.Value;
    public static implicit operator LongText(string v) => new() { Value = v };

    public override string ToString()
        => this.Value;

    public override object GetValue()
        => this.Value;
}
