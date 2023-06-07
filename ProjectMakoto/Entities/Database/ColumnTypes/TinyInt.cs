// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Database.ColumnTypes;

public sealed class TinyInt : BaseColumn
{
    public TinyInt(uint MaxValue = 127)
    {
        if (MaxValue > 127)
            throw new ArgumentException($"The maximum size of a TinyInt is 127");

        this.MaxValue = MaxValue;
    }

    public uint MaxValue { get; private set; } = 127;

    private int? _Value { get; set; }
    public int Value { get => this._Value ?? 0; set { this._Value = (value > this.MaxValue ? throw new ArgumentException($"The maximum size for this int is {this.MaxValue}") : value); } }

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
