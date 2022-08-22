namespace ProjectIchigo.Entities;

internal class RegexTemplates
{
    public static readonly Regex UserMention = new(@"((<@\d+>)|(<@!\d+>))");
    public static readonly Regex ChannelMention = new(@"(<#\d+>)");

    public static readonly Regex SoundcloudUrl = new(@"^https?:\/\/(soundcloud\.com|snd\.sc)\/(.*)$");
    public static readonly Regex YouTubeUrl = new(@"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$");
    public static readonly Regex DiscordChannelUrl = new(@"((https|http):\/\/(ptb\.|canary\.)?discord.com\/channels\/(\d+)\/(\d+)\/(\d+))");
    public static readonly Regex Url = new(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:\/~\+#]*[\w\-\@?^=%&amp;\/~\+#])?)");
}
