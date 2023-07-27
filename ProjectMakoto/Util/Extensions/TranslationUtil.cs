// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

global using ProjectMakoto.Entities.Translation;

namespace ProjectMakoto.Util;

public static class TranslationUtil
{
    /// <inheritdoc cref="Build(string, bool, TVar[])">/>
    internal static string Build(this string str)
        => str.Build(false, null);

    /// <inheritdoc cref="Build(string, bool, TVar[])">/>
    internal static string Build(this string str, params TVar[] vars)
        => str.Build(false, vars);

    /// <summary>
    /// Build a translation string.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="Code">Whether to embed the string as inline code</param>
    /// <param name="vars">A list of variables to replace.</param>
    /// <returns></returns>
    internal static string Build(this string str, bool Code = false, params TVar[] vars)
    {
        if (str.IsNullOrEmpty())
            return str;

        if (Code && !str.StartsWith('_'))
            str = $"`{str}`";
        else
            Code = false;

        vars ??= Array.Empty<TVar>();

        foreach (var b in vars)
        {
            if (b.Replacement is null)
                _logger.LogWarn("TVar is null on ValueName {0}", b.ValName);

            var newText = b.Replacement?.ToString() ?? "";

            if (b.Replacement is EmbeddedLink embeddedLink)
            {
                newText = $"[{(Code ? $"`{embeddedLink.Text}`" : embeddedLink.Text)}]({embeddedLink.Url})";

                if (b.Sanitize)
                    newText = newText.SanitizeForCode();

                str = str.Replace($"{{{b.ValName}}}", $"`{newText}`");
                continue;
            }

            if (newText.StartsWith("<") && newText.EndsWith(">") && Code)
            {
                if (b.Sanitize)
                    newText = newText.SanitizeForCode();

                str = str.Replace($"{{{b.ValName}}}", $"`{newText}`");
                continue;
            }

            if (b.Sanitize)
                newText = newText.FullSanitize();

            str = str.Replace($"{{{b.ValName}}}", newText);
        }

        if (str.StartsWith("``"))
            str = str[1..];

        if (str.EndsWith("``"))
            str = str[..(str.Length - 1)];

        if (str.StartsWith("`<"))
            str = str[1..];
        
        if (str.StartsWith("`[") && vars.Any(x => x.Replacement is EmbeddedLink))
            str = str[1..];

        if (str.EndsWith(">`"))
            str = str[..(str.Length - 1)];

        return str;
    }

    /// <inheritdoc cref="Build(string[], bool, bool, TVar[])"/>
    internal static string Build(this string[] array)
        => array.Build(false, false, null);

    /// <summary>
    /// Builds a string array into a string, used for MultiTranslationKeys.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="Code">Whether to prefix and suffix ` on non-empty lines.</param>
    /// <param name="UseBoldMarker">Whether to make lines prefixing ** bold.</param>
    /// <returns></returns>
    internal static string Build(this string[] array, bool Code = false, bool UseBoldMarker = false, params TVar[] Tvars)
        => string.Join("\n", array.Select(x =>
        {
            bool boldLine = false;

            var y = x;

            if (y.StartsWith("**"))
            {
                boldLine = true;
                y = y.Remove(0, 2);
            }

            y = y.Build(Code, Tvars);

            if (x.IsNullOrWhiteSpace())
                return x;

            return $"{(boldLine ? "**" : "")}{y}{(boldLine ? "**" : "")}";
        }));

    /// <summary>
    /// Runs Replace on every string in a string array and returns the new array.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="old"></param>
    /// <param name="new"></param>
    /// <returns></returns>
    internal static string[] Replace(this string[] array, string old, object @new)
        => array.Select(x => x.Replace(old, @new)).ToArray();

    /// <summary>
    /// Calculates maximum character count for given list of translation keys.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="pairs"></param>
    /// <returns></returns>
    internal static int CalculatePadding(User user, params SingleTranslationKey[] pairs)
    {
        int pad = 0;

        foreach (var b in pairs)
        {
            var length = b.Get(user).Length;

            if (length > pad)
                pad = length;
        }

        return pad;
    }
}
