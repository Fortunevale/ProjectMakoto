// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Database.ColumnAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PrimaryAttribute : Attribute
{
    public readonly bool Primary;

    public PrimaryAttribute(bool Primary = true)
    {
        this.Primary = Primary;
    }
}
