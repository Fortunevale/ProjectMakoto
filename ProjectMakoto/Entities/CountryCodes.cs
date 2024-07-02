// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class CountryCodes
{
    internal CountryCodes() { }

    public IReadOnlyDictionary<string, CountryInfo> List
        => this._List.AsReadOnly();

    internal Dictionary<string, CountryInfo> _List { get; set; } = new();

    public sealed class CountryInfo
    {
        internal CountryInfo() { }

        public string Name { get; internal set; }
        public string ContinentCode { get; internal set; }
        public string ContinentName { get; internal set; }
    }
}