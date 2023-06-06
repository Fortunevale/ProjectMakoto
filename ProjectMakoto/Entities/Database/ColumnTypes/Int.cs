// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Database.ColumnTypes;

public sealed class Int : BaseColumn
{
    public Int(int MaxValue = 2147483647)
    {
        if (MaxValue > 2147483647)
            throw new ArgumentException($"The maximum size of a TinyInt is 2147483647");

        this.MaxValue = MaxValue;
    }

    public int MaxValue { get; private set; } = 2147483647;

    private int? _Value { get; set; }
    public int Value { get => this._Value ?? 0; set { this._Value = (value > this.MaxValue ? throw new ArgumentException($"The maximum size for this int is {this.MaxValue}") : value); } }

    public static implicit operator int(Int b) => b.Value;
    public static implicit operator Int(int v) => new() { Value = v };

    public override string ToString()
        => this.Value.ToString();

    public override object GetValue()
        => this.Value;
}
