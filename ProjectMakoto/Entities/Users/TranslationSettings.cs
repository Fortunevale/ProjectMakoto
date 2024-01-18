// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;

public sealed class TranslationSettings(Bot bot, User parent) : RequiresParent<User>(bot, parent)
{
    [ColumnName("last_google_source"), ColumnType(ColumnTypes.Text), Nullable]
    public string LastGoogleSource
    {
        get => this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Parent.Id, "last_google_source", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "last_google_source", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("last_google_target"), ColumnType(ColumnTypes.Text), Nullable]
    public string LastGoogleTarget
    {
        get => this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Parent.Id, "last_google_target", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "last_google_target", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("last_libretranslate_source"), ColumnType(ColumnTypes.Text), Nullable]
    public string LastLibreTranslateSource
    {
        get => this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Parent.Id, "last_libretranslate_source", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "last_libretranslate_source", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("last_libretranslate_target"), ColumnType(ColumnTypes.Text), Nullable]
    public string LastLibreTranslateTarget
    {
        get => this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Parent.Id, "last_libretranslate_target", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "last_libretranslate_target", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
