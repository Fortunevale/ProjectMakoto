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
        public static readonly string QuestionMark = "https://media.discordapp.net/attachments/1005430437952356423/1006675199577567262/QuestionMark.png";

        public static readonly string GuildUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1006675562384850954/GuildUpdated.png";

        public static readonly string MessageDeleted = "https://media.discordapp.net/attachments/1005430437952356423/1006675627799220244/MessageRemoved.png";
        public static readonly string MessageEdited = "https://media.discordapp.net/attachments/1005430437952356423/1006675628122198016/MessageUpdated.png";

        public static readonly string InviteAdded = "https://media.discordapp.net/attachments/1005430437952356423/1006675471569801287/InviteAdded.png";
        public static readonly string InviteRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1006675471859196054/InviteRemoved.png";

        public static readonly string ChannelAdded = "https://media.discordapp.net/attachments/1005430437952356423/1006675281718804521/ChannelAdded.png";
        public static readonly string ChannelRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1006675282905813092/ChannelRemoved.png";
        public static readonly string ChannelModified = "https://media.discordapp.net/attachments/1005430437952356423/1006675283266514974/ChannelUpdated.png";

        public static readonly string VoiceStateUserJoined = "https://media.discordapp.net/attachments/1005430437952356423/1006676083871076512/VoiceStateUserJoined.png";
        public static readonly string VoiceStateUserLeft = "https://media.discordapp.net/attachments/1005430437952356423/1006676084235968724/VoiceStateUserLeft.png";
        public static readonly string VoiceStateUserUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1006676084659601508/VoiceStateUserUpdated.png";

        public static readonly string UserAdded = "https://media.discordapp.net/attachments/1005430437952356423/1006675756056850492/UserAdded.png";
        public static readonly string UserBanned = "https://media.discordapp.net/attachments/1005430437952356423/1006675756534997072/UserBanned.png";
        public static readonly string UserBanRemoved = "https://media.discordapp.net/attachments/1005430437952356423/1006675754588839936/BanRemoved.png";
        public static readonly string UserKicked = "https://media.discordapp.net/attachments/1005430437952356423/1006675757222858873/UserKicked.png";
        public static readonly string UserLeft = "https://media.discordapp.net/attachments/1005430437952356423/1006675757726171268/UserRemoved.png";
        public static readonly string UserUpdated = "https://media.discordapp.net/attachments/1005430437952356423/1006675758053331044/UserUpdated.png";
        public static readonly string UserWarned = "https://media.discordapp.net/attachments/1005430437952356423/1006675758405664798/UserWarned.png";
    }

    public class StatusIndicators
    {
        public static readonly string Loading = "https://media.discordapp.net/attachments/1005430437952356423/1006676441343201370/Loading.gif";
        public static readonly string Success = "https://media.discordapp.net/attachments/1005430437952356423/1006676420770136114/CheckMark_Icon.png";
        public static readonly string Error = "https://media.discordapp.net/attachments/1005430437952356423/1006676421546098698/Error_Icon.png";
        public static readonly string Warning = "https://media.discordapp.net/attachments/1005430437952356423/1006676420388470911/Warning.png";
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