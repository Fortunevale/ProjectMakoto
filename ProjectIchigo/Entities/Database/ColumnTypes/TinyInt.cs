namespace ProjectIchigo.Entities.Database.ColumnTypes;

public class TinyInt : BaseColumn
{
    public TinyInt(uint MaxValue = 127)
    {
        if (MaxValue > 127)
            throw new ArgumentException($"The maximum size of a TinyInt is 127");

        this.MaxValue = MaxValue;
    }

    public uint MaxValue { get; private set; }

    private int? _Value { get; set; }
    public int Value { get => _Value ?? throw new NullReferenceException(); set { _Value = (value > MaxValue ? throw new ArgumentException($"The maximum size for this int is {MaxValue}") : value); } }

    public static implicit operator int(TinyInt b) => b.Value;
    public static implicit operator uint(TinyInt b) => (uint)b.Value;
    public static implicit operator bool(TinyInt b) => b.Value == 1;
    public static implicit operator TinyInt(int v) => new() { Value = v };
    public static implicit operator TinyInt(sbyte v) => new() { Value = v };
    public static implicit operator TinyInt(uint v) => new() { Value = (int)v };
    public static implicit operator TinyInt(bool v) => new() { Value = v ? 1 : 0 };

    public override string ToString()
        => this.Value.ToString();

    public override object GetValue()
        => this.Value;
}
