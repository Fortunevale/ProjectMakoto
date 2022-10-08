namespace ProjectIchigo.Entities.Database.ColumnTypes;

public class Int : BaseColumn
{
    public Int(int MaxValue = 2147483647)
    {
        if (MaxValue > 2147483647)
            throw new ArgumentException($"The maximum size of a TinyInt is 2147483647");

        this.MaxValue = MaxValue;
    }

    public int MaxValue { get; private set; } = 2147483647;

    private int? _Value { get; set; }
    public int Value { get => _Value ?? 0; set { _Value = (value > MaxValue ? throw new ArgumentException($"The maximum size for this int is {MaxValue}") : value); } }

    public static implicit operator int(Int b) => b.Value;
    public static implicit operator Int(int v) => new() { Value = v };

    public override string ToString()
        => this.Value.ToString();

    public override object GetValue()
        => this.Value;
}
