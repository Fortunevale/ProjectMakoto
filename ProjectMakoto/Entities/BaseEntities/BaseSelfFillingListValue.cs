// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class BaseSelfFillingListValue<T> : RequiresBotReference
{
    public BaseSelfFillingListValue(Bot bot, ulong key) : base(bot)
    {
        this.Id = key;
    }

    internal ulong Id { get; set; }

    public virtual T Convert(BaseSelfFillingListValue<T> oldValue)
        => throw new NullReferenceException();

    public static implicit operator BaseSelfFillingListValue<T>(Guild a) => a;
    public static implicit operator BaseSelfFillingListValue<T>(User a) => a;
}
