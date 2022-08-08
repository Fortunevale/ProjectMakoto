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

        public static readonly string GuildUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1005611232172199957/GuildUpdated.png";

        public static readonly string MessageDeleted = "https://media.discordapp.net/attachments/1005430437952356423/1005611256637575238/MessageRemoved.png";
        public static readonly string MessageEdited = "https://media.discordapp.net/attachments/1005430437952356423/1005611256289435798/MessageUpdated.png";

        public static readonly string InviteAdded = "https://media.discordapp.net/attachments/1005430437952356423/1005611245157752963/InviteAdded.png";
        public static readonly string InviteRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1005611245577191454/InviteRemoved.png";

        public static readonly string ChannelAdded = "https://media.discordapp.net/attachments/1005430437952356423/1005611228661555290/ChannelAdded.png";
        public static readonly string ChannelRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1005611229181640794/ChannelRemoved.png";
        public static readonly string ChannelModified = "https://media.discordapp.net/attachments/1005430437952356423/1005611228229545984/ChannelUpdated.png";

        public static readonly string VoiceStateUserJoined = "https://media.discordapp.net/attachments/1005430437952356423/1005444860091838494/VoiceStateUserJoined.png";
        public static readonly string VoiceStateUserLeft = "https://media.discordapp.net/attachments/1005430437952356423/1005444860385435668/VoiceStateUserLeft.png";
        public static readonly string VoiceStateUserUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1005444860695810149/VoiceStateUserUpdated.png";

        public static readonly string UserAdded = "https://media.discordapp.net/attachments/1005430437952356423/1005611280578641920/UserAdded.png";
        public static readonly string UserBanned = "https://media.discordapp.net/attachments/1005430437952356423/1005611281052614656/UserBanned.png";
        public static readonly string UserBanRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1005611280222142545/BanRemoved.png";
        public static readonly string UserKicked = "https://media.discordapp.net/attachments/1005430437952356423/1005611281367171233/UserKicked.png";
        public static readonly string UserLeft = "https://media.discordapp.net/attachments/1005430437952356423/1005611281715318876/UserRemoved.png";
        public static readonly string UserUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1005611282298306640/UserUpdated.png";
        public static readonly string UserWarned = "https://media.discordapp.net/attachments/1005430437952356423/1005611279534268416/UserWarned.png";
    }

    public class StatusIndicators
    {
        public static readonly string Loading = "https://cdn.discordapp.com/attachments/906976602557145110/940100451213385778/L3.gif";
        public static readonly string Success = "https://cdn.discordapp.com/attachments/1005430437952356423/1005552726857486427/CheckMark_Icon.png";
        public static readonly string Error = "https://cdn.discordapp.com/attachments/1005430437952356423/1005544462384103465/Error_Icon.png";
        public static readonly string Warning = "https://media.discordapp.net/attachments/1005430437952356423/1005430597356892210/Warning.png";
    }

    public class Emojis
    {
        public static DiscordEmoji GetCheckboxTickedRed(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.CheckboxTickedRedEmojiId);
        public static DiscordEmoji GetCheckboxUntickedRed(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.CheckboxUntickedRedEmojiId);
        public static DiscordEmoji GetCheckboxTickedBlue(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.CheckboxTickedBlueEmojiId);
        public static DiscordEmoji GetCheckboxUntickedBlue(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.CheckboxUntickedBlueEmojiId);
        public static DiscordEmoji GetCheckboxTickedGreen(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.CheckboxTickedGreenEmojiId);
        public static DiscordEmoji GetCheckboxUntickedGreen(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.CheckboxUntickedGreenEmojiId);

        public static DiscordEmoji GetQuestionMark(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.QuestionMarkEmojiId);

        public static DiscordEmoji GetGuild(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.GuildEmojiId);
        public static DiscordEmoji GetChannel(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.ChannelEmojiId);
        public static DiscordEmoji GetUser(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.UserEmojiId);
        public static DiscordEmoji GetVoiceState(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.VoiceStateEmojiId);
        public static DiscordEmoji GetMessage(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.MessageEmojiId);
        public static DiscordEmoji GetInvite(DiscordClient client, Bot bot) => DiscordEmoji.FromGuildEmote(client, bot._status.LoadedConfig.InviteEmojiId);
    }
}