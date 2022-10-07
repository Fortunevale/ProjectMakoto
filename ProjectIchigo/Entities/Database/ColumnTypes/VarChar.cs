namespace ProjectIchigo.Entities.Database.ColumnTypes;

public class VarChar : BaseColumn
{
    public VarChar(long MaxLength = 65535)
    {
        if (MaxLength > 65535)
            throw new ArgumentException("The maximum size of a VarChar is 65535");

        this.MaxLength = MaxLength;
    }

    public long MaxLength { get; private set; }

    private string? _Value { get; set; }
    public string? Value { get => _Value; set { _Value = (value.Length.ToInt64() > MaxLength ? throw new ArgumentException($"The maximum length for this string is {MaxLength}") : value); } }

    public static implicit operator string(VarChar b) => b.Value;
    public static implicit operator VarChar(string v) => new() { Value = v };

    public override string ToString()
        => this.Value;

    public override object GetValue()
        => this.Value;
}
