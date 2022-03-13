namespace Project_Ichigo.Helpers;
internal class Resources
{
    public static readonly string Github = "https://cdn.discordapp.com/attachments/712761268393738301/893958382896173096/Github.png";
    public static readonly string QuestionMarkIcon = "https://cdn.discordapp.com/attachments/712761268393738301/899051918037504040/QuestionMark.png";

    public class LogIcons
    {
        public static readonly string Critical = "https://cdn.discordapp.com/attachments/712761268393738301/839905621377286184/Crit.png";
        public static readonly string Error = "https://cdn.discordapp.com/attachments/712761268393738301/839843996779675648/Error.png";
        public static readonly string Warning = "https://cdn.discordapp.com/attachments/712761268393738301/839843994850033664/Warning.png";
        public static readonly string Info = "https://cdn.discordapp.com/attachments/712761268393738301/890237551221284945/Info.png";
        public static readonly string Debug = "https://cdn.discordapp.com/attachments/712761268393738301/839854418861359114/Debug.png";
    }

    public class Regex
    {
        public static readonly string YouTubeUrl = @"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";
        public static readonly string Url = @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:\/~\+#]*[\w\-\@?^=%&amp;\/~\+#])?)";
    }

    public class AuditLogIcons
    {
        public static readonly string GuildUpdated = "https://cdn.discordapp.com/attachments/712761268393738301/846833791079415878/GuildUpdated.png";
        public static readonly string MessageDeleted = "https://cdn.discordapp.com/attachments/712761268393738301/839566709903720518/MessageRemoved.png";
        public static readonly string MessageEdited = "https://cdn.discordapp.com/attachments/712761268393738301/839566700328517703/MessageUpdated.png";
        public static readonly string QuestionMark = "https://cdn.discordapp.com/attachments/712761268393738301/899051918037504040/QuestionMark.png";
        public static readonly string UserAdded = "https://cdn.discordapp.com/attachments/712761268393738301/839566702190526514/UserAdded.png";
        public static readonly string UserBanned = "https://cdn.discordapp.com/attachments/712761268393738301/839911327056527380/UserBanned.png";
        public static readonly string UserBanRemoved = "https://cdn.discordapp.com/attachments/712761268393738301/846830397220323328/BanRemoved.png";
        public static readonly string UserKicked = "https://cdn.discordapp.com/attachments/712761268393738301/839911329229176842/UserKicked.png";
        public static readonly string UserLeft = "https://cdn.discordapp.com/attachments/712761268393738301/839566703477260348/UserRemoved.png";
        public static readonly string UserUpdated = "https://cdn.discordapp.com/attachments/712761268393738301/839566706032640088/UserUpdated.png";
        public static readonly string UserWarned = "https://cdn.discordapp.com/attachments/712761268393738301/839911324774301726/UserWarned.png";
    }

    public class StatusIndicators
    {
        public static string LoadingBlue => DiscordCircleLoading;
        public static string LoadingRed => DiscordCircleLoading;

        public static readonly string DiscordGenericLoading = "https://cdn.discordapp.com/attachments/906976602557145110/940100450403889172/L1.gif";
        public static readonly string DiscordBallsLoading = "https://cdn.discordapp.com/attachments/906976602557145110/940100451649613864/L2.gif";
        public static readonly string DiscordCircleLoading = "https://cdn.discordapp.com/attachments/906976602557145110/940100451213385778/L3.gif";
    }

    public class AccountIds
    {
        public static readonly ulong Disboard = 302050872383242240;
    }
}