// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Database.ColumnTypes;

public sealed class VarChar : BaseColumn
{
    public VarChar(long MaxLength = 65535)
    {
        if (MaxLength > 65535)
            throw new ArgumentException("The maximum size of a VarChar is 65535");

        this.MaxLength = MaxLength;
    }

    public long MaxLength { get; private set; } = 65535;

    private string? _Value { get; set; }
    public string Value { get => this._Value.IsNullOrEmpty() ? "" : this._Value; set { this._Value = (value.Length.ToInt64() > this.MaxLength ? throw new ArgumentException($"The maximum length for this string is {this.MaxLength}") : value); } }

    public static implicit operator string(VarChar b) => b.Value;
    public static implicit operator VarChar(string v) => new() { Value = v };

    public override string ToString()
        => this.Value;

    public override object GetValue()
        => this.Value;
}
