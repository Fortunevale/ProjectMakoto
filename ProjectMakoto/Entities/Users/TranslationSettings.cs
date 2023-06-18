// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class TranslationSettings : RequiresParent<User>
{
    public TranslationSettings(Bot bot, User parent) : base(bot, parent)
    {
    }

    private string _LastGoogleSource { get; set; } = "";
    public string LastGoogleSource
    {
        get => this._LastGoogleSource;
        set
        {
            this._LastGoogleSource = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "last_google_source", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private string _LastGoogleTarget { get; set; } = "";
    public string LastGoogleTarget
    {
        get => this._LastGoogleTarget;
        set
        {
            this._LastGoogleTarget = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "last_google_target", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private string _LastLibreTranslateSource { get; set; } = "";
    public string LastLibreTranslateSource
    {
        get => this._LastLibreTranslateSource;
        set
        {
            this._LastLibreTranslateSource = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "last_libretranslate_source", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private string _LastLibreTranslateTarget { get; set; } = "";
    public string LastLibreTranslateTarget
    {
        get => this._LastLibreTranslateTarget;
        set
        {
            this._LastLibreTranslateTarget = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "last_libretranslate_target", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
