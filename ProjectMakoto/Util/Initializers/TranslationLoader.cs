// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util.Initializers;
internal sealed class TranslationLoader
{
    internal static async Task Load(Bot _bot)
    {
        _bot.LoadedTranslations = JsonConvert.DeserializeObject<Translations>(File.ReadAllText("Translations/strings.json"), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
        _logger.LogDebug("Loaded translations");

        Dictionary<string, int> CalculateTranslationProgress(object? obj)
        {
            if (obj is null)
            {
                _logger.LogWarn("A Translation Group was not loaded.");
                return new Dictionary<string, int>();
            }

            Dictionary<string, int> counts = new();

            Type objType = obj.GetType();
            FieldInfo[] fields = objType.GetFields();

            foreach (FieldInfo field in fields)
            {
                object fieldValue = field.GetValue(obj);
                var elems = fieldValue as IList;
                if (elems is not null)
                {
                    foreach (var item in elems)
                    {
                        foreach (var b in CalculateTranslationProgress(item))
                        {
                            if (!counts.ContainsKey(b.Key))
                                counts.Add(b.Key, 0);

                            counts[b.Key] += b.Value;
                        }
                    }
                }
                else
                {
                    if (field.FieldType.Assembly == objType.Assembly)
                    {
                        if (field.FieldType == typeof(SingleTranslationKey))
                        {
                            foreach (var b in ((SingleTranslationKey)fieldValue).t)
                            {
                                if (!counts.ContainsKey(b.Key))
                                    counts.Add(b.Key, 0);

                                counts[b.Key]++;
                            }
                        }
                        else if (field.FieldType == typeof(MultiTranslationKey))
                        {
                            foreach (var b in ((MultiTranslationKey)fieldValue).t)
                            {
                                if (!counts.ContainsKey(b.Key))
                                    counts.Add(b.Key, 0);

                                counts[b.Key]++;
                            }
                        }

                        foreach (var b in CalculateTranslationProgress(fieldValue))
                        {
                            if (!counts.ContainsKey(b.Key))
                                counts.Add(b.Key, 0);

                            counts[b.Key] += b.Value;
                        }
                    }
                    else
                    {
                        if (field.FieldType == typeof(SingleTranslationKey))
                        {
                            foreach (var b in ((SingleTranslationKey)fieldValue).t)
                            {
                                if (!counts.ContainsKey(b.Key))
                                    counts.Add(b.Key, 0);

                                counts[b.Key]++;
                            }
                        }
                        else if (field.FieldType == typeof(MultiTranslationKey))
                        {
                            foreach (var b in ((MultiTranslationKey)fieldValue).t)
                            {
                                if (!counts.ContainsKey(b.Key))
                                    counts.Add(b.Key, 0);

                                counts[b.Key]++;
                            }
                        }
                    }
                }
            }

            return counts;
        }
        _bot.LoadedTranslations.Progress = CalculateTranslationProgress(_bot.LoadedTranslations);
        _logger.LogDebug("Loaded translations: {0}", string.Join("; ", _bot.LoadedTranslations.Progress.Select(x => $"{x.Key}:{x.Value}")));
    }
}
