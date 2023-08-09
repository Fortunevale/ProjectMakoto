// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal sealed class RegexTemplates
{
    public static readonly Regex UserMention = new(@"((<@(\d+)>)|(<@!(\d+)>))", RegexOptions.Compiled);
    public static readonly Regex ChannelMention = new(@"(<#\d+>)", RegexOptions.Compiled);

    public static readonly Regex BandcampUrl = new(@"(https?:\/\/)?([\d|\w]+)\.bandcamp\.com\/?.*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex SoundcloudUrl = new(@"^https?:\/\/(soundcloud\.com|snd\.sc)\/(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex YouTubeUrl = new(@"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex DiscordChannelUrl = new(@"((https|http):\/\/(ptb\.|canary\.)?discord.com\/channels\/(\d+)\/(\d+)\/(\d+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex Url = new(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static readonly Regex Token = new(@"(mfa\.[a-z0-9_-]{20,})|((?<botid>[a-z0-9_-]{23,28})\.(?<creation>[a-z0-9_-]{6,7})\.(?<enc>[a-z0-9_-]{27,}))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex GitHubUrl = new(@"https:\/\/github\.com\/([^ \/]*)\/([^ \/]*)(\/blob\/([^ \/]*))?\/([^ #]*)#(L\d*)(-(L\d*))?", RegexOptions.IgnoreCase);
    public static readonly Regex Ip = new(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");

    public static readonly Regex Code = new(@"(?:```)(?:cs)?((.|\n)*)(?:```)", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex AllowedNickname = new(@"[^a-zA-Z0-9 _\-!.,:;#+*~´`?^°<>|""§$%&\/\\()={\[\]}²³€@_]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static readonly Regex GitHubRepoUrl = new(@"https:\/\/github\.com\/([^\/]*)\/([^\/]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex SemVer = new(@"^(\d*)\.(\d*)\.(\d*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
}
