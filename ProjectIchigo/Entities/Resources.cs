namespace ProjectIchigo.Entities;
internal class Resources
{
    public static readonly IReadOnlyList<Permissions> ProtectedPermissions = new List<Permissions>()
    {
        Permissions.Administrator,

        Permissions.MuteMembers,
        Permissions.DeafenMembers,
        Permissions.ModerateMembers,
        Permissions.KickMembers,
        Permissions.BanMembers,

        Permissions.ManageGuild,
        Permissions.ManageChannels,
        Permissions.ManageRoles,
        Permissions.ManageMessages,
        Permissions.ManageEvents,
        Permissions.ManageThreads,
        Permissions.ManageWebhooks,
        Permissions.ManageNicknames,

        Permissions.ViewAuditLog,
    };

    public static readonly DiscordButtonComponent CancelButton = new(ButtonStyle.Secondary, "cancel", "Cancel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));

    public class LogIcons
    {
        public static string Error => Resources.StatusIndicators.Error;
        public static string Warning => Resources.StatusIndicators.Warning;
        public static string Info => Resources.StatusIndicators.Success;
    }

    public class Regex
    {
        public static readonly string UserMention = @"((<@\d+>)|(<@!\d+>))";
        public static readonly string ChannelMention = @"(<#\d+>)";

        public static readonly string YouTubeUrl = @"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";
        public static readonly string DiscordChannelUrl = @"((https|http):\/\/(ptb\.|canary\.)?discord.com\/channels\/(\d+)\/(\d+)\/(\d+))";
        public static readonly string Url = @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:\/~\+#]*[\w\-\@?^=%&amp;\/~\+#])?)";
    }

    public class AuditLogIcons
    {
        public static readonly string QuestionMark = "https://cdn.discordapp.com/attachments/1005430437952356423/1005436838359617546/QuestionMark.png";

        public static readonly string GuildUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1005435381392625664/GuildUpdated.png";

        public static readonly string MessageDeleted = "https://media.discordapp.net/attachments/1005430437952356423/1005435466834772109/MessageRemoved.png";
        public static readonly string MessageEdited = "https://media.discordapp.net/attachments/1005430437952356423/1005435466507636777/MessageUpdated.png";

        public static readonly string InviteAdded = "https://media.discordapp.net/attachments/1005430437952356423/1005435357472493618/InviteAdded.png";
        public static readonly string InviteRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1005435357782880276/InviteRemoved.png";

        public static readonly string ChannelAdded = "https://media.discordapp.net/attachments/1005430437952356423/1005435375239581746/ChannelAdded.png";
        public static readonly string ChannelRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1005435375738683454/ChannelUpdated.png";
        public static readonly string ChannelModified = "https://media.discordapp.net/attachments/1005430437952356423/1005435376145551420/ChannelRemoved.png";

        public static readonly string VoiceStateUserJoined = "https://media.discordapp.net/attachments/1005430437952356423/1005435342440124447/VoiceStateUserJoined.png";
        public static readonly string VoiceStateUserLeft = "https://media.discordapp.net/attachments/1005430437952356423/1005435343362871387/VoiceStateUserLeft.png";
        public static readonly string VoiceStateUserUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1005435342901481472/VoiceStateUserUpdated.png";

        public static readonly string UserAdded = "https://media.discordapp.net/attachments/1005430437952356423/1005435423528591470/UserAdded.png";
        public static readonly string UserBanned = "https://media.discordapp.net/attachments/1005430437952356423/1005435424505856000/UserBanned.png";
        public static readonly string UserBanRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1005435423885111296/BanRemoved.png";
        public static readonly string UserKicked = "https://media.discordapp.net/attachments/1005430437952356423/1005435424191287457/UserKicked.png";
        public static readonly string UserLeft = "https://media.discordapp.net/attachments/1005430437952356423/1005435425541849168/UserRemoved.png";
        public static readonly string UserUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1005435425214709870/UserUpdated.png";
        public static readonly string UserWarned = "https://media.discordapp.net/attachments/1005430437952356423/1005435424833032243/UserWarned.png";
    }

    public class StatusIndicators
    {
        public static readonly string Loading = "https://cdn.discordapp.com/attachments/906976602557145110/940100451213385778/L3.gif";
        public static readonly string Success = "https://media.discordapp.net/attachments/1005430437952356423/1005430596589342790/CheckMark.png";
        public static readonly string Error = "https://media.discordapp.net/attachments/1005430437952356423/1005430597017141288/X.png";
        public static readonly string Warning = "https://media.discordapp.net/attachments/1005430437952356423/1005430597356892210/Warning.png";
    }

    public class AccountIds
    {
        public static readonly ulong Disboard = 302050872383242240;
    }
}