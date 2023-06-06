// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class JoinSettings
{
    public JoinSettings(Guild guild)
    {
        this.Parent = guild;
    }

    private Guild Parent { get; set; }



    private ulong _AutoAssignRoleId { get; set; } = 0;
    public ulong AutoAssignRoleId
    {
        get => this._AutoAssignRoleId;
        set
        {
            this._AutoAssignRoleId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "auto_assign_role_id", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private ulong _JoinlogChannelId { get; set; } = 0;
    public ulong JoinlogChannelId
    {
        get => this._JoinlogChannelId;
        set
        {
            this._JoinlogChannelId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "joinlog_channel_id", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private bool _AutoBanGlobalBans { get; set; } = true;
    public bool AutoBanGlobalBans
    {
        get => this._AutoBanGlobalBans;
        set
        {
            this._AutoBanGlobalBans = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "autoban_global_ban", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }



    private bool _ReApplyRoles { get; set; } = false;
    public bool ReApplyRoles
    {
        get => this._ReApplyRoles;
        set
        {
            this._ReApplyRoles = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "reapplyroles", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }



    private bool _ReApplyNickname { get; set; } = false;
    public bool ReApplyNickname
    {
        get => this._ReApplyNickname;
        set
        {
            this._ReApplyNickname = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "reapplynickname", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}