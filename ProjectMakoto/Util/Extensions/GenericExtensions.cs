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
    public static bool TryGetFileInfo(string fileName, out FileInfo file)
    {
        if (File.Exists(fileName))
        {
            file = new FileInfo(fileName);
            return true;
        }

        var environmentVariables = Environment.GetEnvironmentVariables().ConvertToDictionary<string, string>();
        var paths = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32S or PlatformID.Win32Windows or PlatformID.Win32NT or PlatformID.WinCE => environmentVariables.First(x => x.Key.ToLower() == "path").Value.Split(';'),
            PlatformID.Unix => environmentVariables.First(x => x.Key.ToLower() == "path").Value.Split(':'),
            _ => throw new NotImplementedException(),
        };

        foreach (var path in paths)
        {
            var currentFilePath = Path.Combine(path, fileName);
            if (File.Exists(currentFilePath))
            {
                file = new FileInfo(currentFilePath);
                return true;
            }

            currentFilePath += ".exe";

            if (File.Exists(currentFilePath))
            {
                file = new FileInfo(currentFilePath);
                return true;
            }
        }

        file = null;
        return false;
    }

    public static Dictionary<T1, T2> ConvertToDictionary<T1, T2>(this IDictionary iDic)
    {
        var dic = new Dictionary<T1, T2>();
        var enumerator = iDic.GetEnumerator();
        while (enumerator.MoveNext())
        {
            dic[(T1)enumerator.Key] = (T2)enumerator.Value;
        }
        return dic;
    }

    public static T[] Add<T>(this T[] array, T addObject)
        => array.Append(addObject).ToArray();
    
    public static T[] AddRange<T>(this T[] array, IEnumerable<T> addObjects)
        => array.Concat(addObjects).ToArray();

    public static T[] Update<T>(this T[] array, Func<T, string> equalPredicate, T newObject)
        => array.Where(x => equalPredicate.Invoke(x) != equalPredicate.Invoke(newObject)).Append(newObject).ToArray();
    
    public static T[] Remove<T>(this T[] array, Func<T, string> equalPredicate, T removeObject)
        => array.Where(x => equalPredicate.Invoke(x) != equalPredicate.Invoke(removeObject)).ToArray();

    public static string TruncateWithIndication(this string value, int maxLength, string customString = "..")
    {
        return string.IsNullOrEmpty(value)
            ? value
            : value.Length <= maxLength ? value : $"{value[..(maxLength - customString.Length)]}{customString}";
    }

    internal static Exception AddData(this Exception exception, string key, object? data)
    {
        exception.Data.Add(key, data);
        return exception;
    }

    internal static bool ContainsTask(this IReadOnlyList<ScheduledTask>? tasks, string type, ulong snowflake, string id) 
        => tasks.Where(x =>
        {
            if (x.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                return false;

            if (scheduledTaskIdentifier.Type != type)
                return false;

            return scheduledTaskIdentifier.Snowflake == snowflake;
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
                _logger.Log(LogLevel.Trace, $"String {{str}} logged: {additionalInfo}", str);
                break;
            default:
                throw new NotImplementedException("The specified log level is not implemented");
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
        var order = 0;
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
        var enc = Encoding.UTF8;
        var result = SHA256.HashData(enc.GetBytes(value));

        foreach (var b in result)
            _ = Sb.Append(b.ToString("x2"));

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

    internal static string TruncateAt(this string str, params char[] chars)
        => str.TruncateAt(false, chars);

    internal static string TruncateAt(this string str, params string[] strings)
        => str.TruncateAt(false, strings);

    internal static string TruncateAt(this string str, bool Reverse, params char[] chars)
    {
        if (!chars.IsNotNullAndNotEmpty() || !chars.Any(x => str.Contains(x)))
            return str;

        var indexes = chars.Select(x => new KeyValuePair<char, int>(x, !Reverse ? str.IndexOf(x) : str.LastIndexOf(x))).ToList();

        return str[..(!Reverse ? indexes.Min(x => x.Value) : indexes.Max(x => x.Value))];
    }

    internal static string TruncateAt(this string str, bool Reverse, params string[] strings)
    {
        if (!strings.IsNotNullAndNotEmpty() || !strings.Any(x => str.Contains(x)))
            return str;

        var indexes = strings.Select(x => new KeyValuePair<string, int>(x, !Reverse ? str.IndexOf(x) : str.LastIndexOf(x))).ToList();

        return str[..(!Reverse ? indexes.Min(x => x.Value) : indexes.Max(x => x.Value))];
    }

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