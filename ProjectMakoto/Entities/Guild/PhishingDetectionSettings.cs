// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class PhishingDetectionSettings
{
    public PhishingDetectionSettings(Guild guild)
    {
        this.Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _DetectPhishing { get; set; } = true;
    public bool DetectPhishing
    {
        get => this._DetectPhishing;
        set
        {
            this._DetectPhishing = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "phishing_detect", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private bool _WarnOnRedirect { get; set; } = false;
    public bool WarnOnRedirect
    {
        get => this._WarnOnRedirect;
        set
        {
            this._WarnOnRedirect = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "phishing_warnonredirect", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _AbuseIpDbReports { get; set; } = false;
    public bool AbuseIpDbReports
    {
        get => this._AbuseIpDbReports;
        set
        {
            this._AbuseIpDbReports = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "phishing_abuseipdb", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private PhishingPunishmentType _PunishmentType { get; set; } = PhishingPunishmentType.KICK;
    public PhishingPunishmentType PunishmentType
    {
        get => this._PunishmentType;
        set
        {
            this._PunishmentType = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "phishing_type", Convert.ToInt32(value), Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private string _CustomPunishmentReason { get; set; } = "%R";
    public string CustomPunishmentReason
    {
        get => this._CustomPunishmentReason;
        set
        {
            this._CustomPunishmentReason = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "phishing_reason", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private TimeSpan _CustomPunishmentLength { get; set; } = TimeSpan.FromDays(14);
    public TimeSpan CustomPunishmentLength
    {
        get => this._CustomPunishmentLength;
        set
        {
            this._CustomPunishmentLength = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "phishing_time", Convert.ToInt64(value.TotalSeconds), Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
