// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;
public class TranslationReportSettings : RequiresParent<User>
{
    public TranslationReportSettings(Bot bot, User parent) : base(bot, parent)
    {
    }

    private int _AcceptedTOS { get; set; } = 0;
    public int AcceptedTOS
    {
        get => this._AcceptedTOS;
        set
        {
            this._AcceptedTOS = value;
            _ = this.Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "translationreport_accepted_tos", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private DateTime _FirstRequestTime { get; set; } = DateTime.MinValue;
    public DateTime FirstRequestTime
    {
        get => this._FirstRequestTime;
        set
        {
            this._FirstRequestTime = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "translationreport_ratelimit_first", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private int _RequestCount { get; set; } = 0;
    public int RequestCount
    {
        get => this._RequestCount;
        set
        {
            this._RequestCount = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "translationreport_ratelimit_count", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
