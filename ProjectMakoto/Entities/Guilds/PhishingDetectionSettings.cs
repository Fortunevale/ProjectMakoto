// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Entities.Guilds;

public sealed class PhishingDetectionSettings(Bot bot, Guild parent) : RequiresParent<Guild>(bot, parent)
{
    [ColumnName("phishing_detect"), ColumnType(ColumnTypes.TinyInt), Default("1")]
    public bool DetectPhishing
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "phishing_detect", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "phishing_detect", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("phishing_warnonredirect"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool WarnOnRedirect
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "phishing_warnonredirect", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "phishing_warnonredirect", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("phishing_abuseipdb"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool AbuseIpDbReports
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "phishing_abuseipdb", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "phishing_abuseipdb", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("phishing_type"), ColumnType(ColumnTypes.TinyInt), Default("2")]
    public PhishingPunishmentType PunishmentType
    {
        get => (PhishingPunishmentType)this.Bot.DatabaseClient.GetValue<int>("guilds", "serverid", this.Parent.Id, "phishing_type", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "phishing_type", Convert.ToInt32(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }


    [ColumnName("phishing_reason"), ColumnType(ColumnTypes.Text), WithCollation, Default("%R")]
    public string CustomPunishmentReason
    {
        get => this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "phishing_reason", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "phishing_reason", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }


    [ColumnName("phishing_time"), ColumnType(ColumnTypes.BigInt), Default("1209600")]
    public TimeSpan CustomPunishmentLength
    {
        get => this.Bot.DatabaseClient.GetValue<TimeSpan>("guilds", "serverid", this.Parent.Id, "phishing_time", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "phishing_time", Convert.ToInt64(value.TotalSeconds), this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
