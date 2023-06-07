// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Database.ColumnTypes;

public sealed class LongText : BaseColumn
{
    public LongText(long MaxLength = 4294967296)
    {
        if (MaxLength > 4294967296)
            throw new ArgumentException("The maximum size of a LongText is 4294967296");

        this.MaxLength = MaxLength;
    }

    public long MaxLength { get; private set; } = 4294967296;

    private string? _Value { get; set; }
    public string Value { get => this._Value.IsNullOrEmpty() ? "" : this._Value; set { this._Value = (value.Length.ToInt64() > this.MaxLength ? throw new ArgumentException($"The maximum length for this string is {this.MaxLength}") : value); } }

    public static implicit operator string(LongText b) => b.Value;
    public static implicit operator LongText(string v) => new() { Value = v };

    public override string ToString()
        => this.Value;

    public override object GetValue()
        => this.Value;
}
