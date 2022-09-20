namespace ProjectIchigo.Entities;

internal class RegexTemplates
{
    public static readonly Regex UserMention = new(@"((<@\d+>)|(<@!\d+>))", RegexOptions.Compiled);
    public static readonly Regex ChannelMention = new(@"(<#\d+>)", RegexOptions.Compiled);

    public static readonly Regex BandcampUrl = new(@"(https?:\/\/)?([\d|\w]+)\.bandcamp\.com\/?.*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex SoundcloudUrl = new(@"^https?:\/\/(soundcloud\.com|snd\.sc)\/(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex YouTubeUrl = new(@"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex DiscordChannelUrl = new(@"((https|http):\/\/(ptb\.|canary\.)?discord.com\/channels\/(\d+)\/(\d+)\/(\d+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex Url = new(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:\/~\+#]*[\w\-\@?^=%&amp;\/~\+#])?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex Token = new(@"(mfa\.[a-z0-9_-]{20,})|((?<botid>[a-z0-9_-]{23,28})\.(?<creation>[a-z0-9_-]{6,7})\.(?<enc>[a-z0-9_-]{27,}))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex GitHubUrl = new(@"https:\/\/github\.com\/([^ \/]*)\/([^ \/]*)(\/blob\/([^ \/]*))?\/([^ #]*)#(L\d*)(-(L\d*))?", RegexOptions.IgnoreCase);
    public static readonly Regex Ip = new(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");
}
