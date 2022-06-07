namespace Project_Ichigo.Objects;

public class JoinSettings
{
    private ulong _AutoAssignRoleId { get; set; } = 0;
    public ulong AutoAssignRoleId { get => _AutoAssignRoleId; set { _AutoAssignRoleId = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private ulong _JoinlogChannelId { get; set; } = 0;
    public ulong JoinlogChannelId { get => _JoinlogChannelId; set { _JoinlogChannelId = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private bool _AutoBanGlobalBans { get; set; } = true;
    public bool AutoBanGlobalBans { get => _AutoBanGlobalBans; set { _AutoBanGlobalBans = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    private bool _ReApplyRoles { get; set; } = false;
    public bool ReApplyRoles { get => _ReApplyRoles; set { _ReApplyRoles = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    private bool _ReApplyNickname { get; set; } = false;
    public bool ReApplyNickname { get => _ReApplyNickname; set { _ReApplyNickname = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}