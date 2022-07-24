namespace ProjectIchigo.Entities;

public class JoinSettings
{
    public JoinSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private ulong _AutoAssignRoleId { get; set; } = 0;
    public ulong AutoAssignRoleId 
    { 
        get => _AutoAssignRoleId; 
        set 
        { 
            _AutoAssignRoleId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "auto_assign_role_id", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private ulong _JoinlogChannelId { get; set; } = 0;
    public ulong JoinlogChannelId 
    { 
        get => _JoinlogChannelId; 
        set 
        { 
            _JoinlogChannelId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "joinlog_channel_id", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private bool _AutoBanGlobalBans { get; set; } = true;
    public bool AutoBanGlobalBans 
    { 
        get => _AutoBanGlobalBans; 
        set 
        { 
            _AutoBanGlobalBans = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "autoban_global_ban", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }



    private bool _ReApplyRoles { get; set; } = false;
    public bool ReApplyRoles 
    { 
        get => _ReApplyRoles; 
        set 
        { 
            _ReApplyRoles = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "reapplyroles", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }



    private bool _ReApplyNickname { get; set; } = false;
    public bool ReApplyNickname 
    { 
        get => _ReApplyNickname; 
        set 
        { 
            _ReApplyNickname = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "reapplynickname", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}