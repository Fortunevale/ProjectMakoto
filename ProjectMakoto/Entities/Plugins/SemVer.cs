// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Plugins;

public sealed class SemVer : IComparable<SemVer>
{
    public SemVer(int major, int minor, int patch)
    {
        this.Major = major;
        this.Minor = minor;
        this.Patch = patch;
    }

    public SemVer(string v)
    {
        var regex = RegexTemplates.SemVer.Match(v);

        if (!regex.Success)
            throw new ArgumentException("Input string in incorrect format.");

        this.Major = regex.Groups[1].Value.ToInt32();
        this.Minor = regex.Groups[2].Value.ToInt32();
        this.Patch = regex.Groups[3].Value.ToInt32();
    }

    public int Major { get; init; }
    public int Minor { get; init; }
    public int Patch { get; init; }

    public override string ToString()
        => $"{this.Major}.{this.Minor}.{this.Patch}";

    public int CompareTo(SemVer? other)
    {
        if (other is null)
            return 1;

        return ((int)this).CompareTo((int)other);
    }

    public static implicit operator string(SemVer v)
        => $"{v.Major}.{v.Minor}.{v.Patch}";

    public static implicit operator int(SemVer v)
        => (v.Major * 1000) + (v.Minor * 100) + v.Patch;

    public static implicit operator SemVer(string v)
        => new(v);
}
