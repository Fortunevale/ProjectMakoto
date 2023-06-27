// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Microsoft.Extensions.Logging;

namespace ProjectMakoto.Util;

internal static class GenericExtensions
{
    internal static bool ContainsTask(this IReadOnlyList<InternalSheduler.ScheduledTask>? tasks, string type, ulong snowflake, string id) 
        => tasks.Where(x =>
        {
            if (x.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                return false;

            if (scheduledTaskIdentifier.Type != type)
                return false;

            if (scheduledTaskIdentifier.Snowflake != snowflake)
                return false;

            return true;
        }).Any(x => ((ScheduledTaskIdentifier)x.CustomData).Id == id);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
    internal static string Log(this string str, CustomLogLevel lvl, string additionalInfo)
    {
        switch (lvl)
        {
            case CustomLogLevel.None:
                _logger.LogNone($"String {{0}} logged: {additionalInfo}", str);
                break;
            case CustomLogLevel.Fatal:
                _logger.LogFatal($"String {{0}} logged: {additionalInfo}", str);
                break;
            case CustomLogLevel.Error:
                _logger.LogError($"String {{0}} logged: {additionalInfo}", str);
                break;
            case CustomLogLevel.Warn:
                _logger.LogWarn($"String {{0}} logged: {additionalInfo}", str);
                break;
            case CustomLogLevel.Info:
                _logger.LogInfo($"String {{0}} logged: {additionalInfo}", str);
                break;
            case CustomLogLevel.Debug:
                _logger.LogDebug($"String {{0}} logged: {additionalInfo}", str);
                break;
            case CustomLogLevel.Debug2:
                _logger.LogDebug2($"String {{0}} logged: {additionalInfo}", str);
                break;
            case CustomLogLevel.Trace:
                _logger.LogTrace($"String {{0}} logged: {additionalInfo}", str);
                break;
            case CustomLogLevel.Trace2:
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"String {{0}} logged: {additionalInfo}", str);
                break;
            default:
                break;
        }
        return str;
    }

    internal static string FileSizeToHumanReadable(this int size)
        => GetHumanReadableSize((long)size);

    internal static string FileSizeToHumanReadable(this uint size)
    => GetHumanReadableSize((long)size);

    internal static string FileSizeToHumanReadable(this long size)
        => GetHumanReadableSize((long)size);

    internal static string FileSizeToHumanReadable(this ulong size)
    => GetHumanReadableSize((long)size);

    private static string GetHumanReadableSize(this long size)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return String.Format("{0:0.##} {1}", size, sizes[order]);
    }

    internal static string GetSHA256(this string value)
    {
        StringBuilder Sb = new();
        Encoding enc = Encoding.UTF8;
        byte[] result = SHA256.HashData(enc.GetBytes(value));

        foreach (byte b in result)
            Sb.Append(b.ToString("x2"));

        return Sb.ToString();
    }

    internal static bool TryGetCustomAttribute<T>(this PropertyInfo collection, Type type, out T? attribute)
    {
        var objects = collection.GetCustomAttributes(false);
        if (objects.Any(x => x.GetType() == type))
        {
            attribute = (T)objects.First(x => x.GetType() == type);
            return true;
        }

        attribute = default;
        return false;
    }

    internal static string IsValidHexColor(this string str, string Default = "#FFFFFF")
        => !str.IsNullOrWhiteSpace() && Regex.IsMatch(str, @"^(#([a-fA-f0-9]{6}))$") ? str : Default;

    internal static string ToHex(this DiscordColor c)
        => ColorTools.ToHex(c.R, c.G, c.B);

    internal static string SanitizeForCode(this string str)
        => str.Replace("`", "´");

    internal static string FullSanitize(this string str)
    {
        var proc = str;

        proc = proc.Replace("`", "´");

        try
        { proc = RegexTemplates.UserMention.Replace(proc, ""); }
        catch { }
        try
        { proc = RegexTemplates.ChannelMention.Replace(proc, ""); }
        catch { }

        proc = proc.Replace("@everyone", "");
        proc = proc.Replace("@here", "");

        return Formatter.Sanitize(proc);
    }

    internal static Stream ToStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
