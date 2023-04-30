global using ProjectMakoto.Entities.Translation;

namespace ProjectMakoto.Util;

public static class TranslationUtil
{
    /// <inheritdoc cref="Build(string, bool, TVar[])">/>
    internal static string Build(this string str)
        => str.Build(false, true, null);

    /// <inheritdoc cref="Build(string, bool, TVar[])">/>
    internal static string Build(this string str, params TVar[] vars)
        => str.Build(false, true, vars);

    /// <inheritdoc cref="Build(string, bool, TVar[])">/>
    internal static string Build(this string str, bool Code = false, params TVar[] vars)
        => str.Build(Code, true, vars);

    /// <summary>
    /// Build a translation string.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="Code">Whether to embed the string as inline code</param>
    /// <param name="vars">A list of variables to replace.</param>
    /// <returns></returns>
    internal static string Build(this string str, bool Code = false, bool Sanitize = true, params TVar[] vars)
    {
        if (str.IsNullOrEmpty())
            return str;

        if (Code)
            str = $"`{str}`";

        foreach (var b in vars)
        {
            var newText = b.Replacement.ToString();

            if (newText.StartsWith("<") && newText.EndsWith(">") && Code)
            {
                if (Sanitize)
                    newText = newText.SanitizeForCode();

                str = str.Replace($"{{{b.ValName}}}", $"`{newText}`");
                continue;
            }

            if (Sanitize)
                newText = newText.Sanitize();

            str = str.Replace($"{{{b.ValName}}}", newText);
        }

        return str;
    }

    /// <inheritdoc cref="Build(string[], bool, bool, TVar[])"/>
    internal static string Build(this string[] array)
        => array.Build(false, false, true, null);

    /// <inheritdoc cref="Build(string[], bool, bool, TVar[])"/>
    internal static string Build(this string[] array, bool Code = false, params TVar[] Tvars)
        => array.Build(Code, false, true, Tvars);

    /// <summary>
    /// Builds a string array into a string, used for MultiTranslationKeys.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="Code">Whether to prefix and suffix ` on non-empty lines.</param>
    /// <param name="UseBoldMarker">Whether to make lines prefixing ** bold.</param>
    /// <returns></returns>
    internal static string Build(this string[] array, bool Code = false, bool UseBoldMarker = false, bool Sanitize = true, params TVar[] Tvars)
        => string.Join("\n", array.Select(x =>
        {
            var y = x.Build(Code, Sanitize, Tvars);

            if (x.IsNullOrWhiteSpace())
                return x;

            bool boldLine = false;
            
            if (y.StartsWith("**"))
            {
                boldLine = true;
                y = y.Remove(0, 2);
            }

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
