// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;

public sealed class ReminderItem
{
    public string UUID { get; set; } = Guid.NewGuid().ToString();

    public string CreationPlace { get; set; }

    private string _Description { get; set; }
    public string Description
    {
        get => this._Description;
        set
        {
            if (value.Length > 512)
                throw new ArgumentException("The description cannot be longer than 512 characters.");

            this._Description = value;
        }
    }

    public DateTime DueTime { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
