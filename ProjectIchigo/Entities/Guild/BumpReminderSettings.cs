namespace ProjectIchigo.Entities;

public class BumpReminderSettings
{
    private bool _Enabled { get; set; } = false;
    public bool Enabled { get => _Enabled; set { _Enabled = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private ulong _RoleId { get; set; } = 0;
    public ulong RoleId { get => _RoleId; set { _RoleId = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private ulong _ChannelId { get; set; } = 0;
    public ulong ChannelId { get => _ChannelId; set { _ChannelId = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private ulong _MessageId { get; set; } = 0;
    public ulong MessageId { get => _MessageId; set { _MessageId = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private ulong _PersistentMessageId { get; set; } = 0;
    public ulong PersistentMessageId { get => _PersistentMessageId; set { _PersistentMessageId = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private ulong _LastUserId { get; set; } = 0;
    public ulong LastUserId { get => _LastUserId; set { _LastUserId = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private DateTime _LastBump { get; set; } = DateTime.MinValue;
    public DateTime LastBump { get => _LastBump; set { _LastBump = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private DateTime _LastReminder { get; set; } = DateTime.MinValue;
    public DateTime LastReminder { get => _LastReminder; set { _LastReminder = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
    
    private int _BumpsMissed { get; set; } = 0;
    public int BumpsMissed { get => _BumpsMissed; set { _BumpsMissed = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}