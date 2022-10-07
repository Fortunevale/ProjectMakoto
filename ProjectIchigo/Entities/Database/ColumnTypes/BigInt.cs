namespace ProjectIchigo.Entities.Database.ColumnTypes;

public class BigInt : BaseColumn
{
    public BigInt(long MaxValue = 9223372036854775807)
    {
        if (MaxValue > 9223372036854775807)
            throw new ArgumentException($"The maximum size of a BigInt is 9223372036854775807");

        this.MaxValue = MaxValue;
    }

    public long MaxValue { get; private set; }

    private long? _Value { get; set; }
    public long Value { get => _Value ?? throw new NullReferenceException(); set { _Value = (value.ToInt64() > MaxValue ? throw new ArgumentException($"The maximum size for this int is {MaxValue}") : value); } }

    public static implicit operator long(BigInt b) => b.Value;
    public static implicit operator ulong(BigInt b) => (ulong)b.Value;
    public static implicit operator BigInt(long v) => new() { Value = v };
    public static implicit operator BigInt(ulong v) => new() { Value = (long)v };

    public override string ToString()
        => this.Value.ToString();

    public override object GetValue()
        => this.Value;
}
