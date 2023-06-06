// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Database.ColumnTypes;

public sealed class BigInt : BaseColumn
{
    public BigInt(long MaxValue = 9223372036854775807)
    {
        if (MaxValue > 9223372036854775807)
            throw new ArgumentException($"The maximum size of a BigInt is 9223372036854775807");

        this.MaxValue = MaxValue;
    }

    public long MaxValue { get; private set; } = 9223372036854775807;

    private long? _Value { get; set; }
    public long Value { get => this._Value ?? 0; set { this._Value = (value.ToInt64() > this.MaxValue ? throw new ArgumentException($"The maximum size for this int is {this.MaxValue}") : value); } }

    public static implicit operator long(BigInt b) => b.Value;
    public static implicit operator ulong(BigInt b) => (ulong)b.Value;
    public static implicit operator BigInt(long v) => new() { Value = v };
    public static implicit operator BigInt(ulong v) => new() { Value = (long)v };

    public override string ToString()
        => this.Value.ToString();

    public override object GetValue()
        => this.Value;
}
