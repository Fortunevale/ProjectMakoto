// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

global using Xorog.Logger;
global using Xorog.UniversalExtensions;
global using Xorog.UniversalExtensions.Entities;
global using static TranslationSourceGenerator.Log;
global using static Xorog.Logger.Logger;
global using static Xorog.UniversalExtensions.UniversalExtensions;
global using static Xorog.UniversalExtensions.UniversalExtensionsEnums;
global using Xorog.Logger.Enums;
global using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace TranslationSourceGenerator;

internal class Program
{
    public string SourceOrigin = """"
// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class Translations
{
    public Dictionary<string, int> Progress = new();

    // InsertPoint
}
"""";

    public string SourceDirectory = "";

    public string StringsJson
        => Path.Combine(SourceDirectory, "Translations", "strings.json");
    
    public string TranslationCs
        => Path.Combine(SourceDirectory, "Entities", "Translation", "Translations.cs");

    static void Main(string[] args)
    {
        new Program().Execute(args).GetAwaiter().GetResult();
    }

    public async Task Execute(string[] args)
    {
        if (!Directory.Exists($"{new FileInfo(Assembly.GetExecutingAssembly().FullName).Directory.FullName}/logs"))
            Directory.CreateDirectory($"{new FileInfo(Assembly.GetExecutingAssembly().FullName).Directory.FullName}/logs");

        _logger = StartLogger($"{new FileInfo(Assembly.GetExecutingAssembly().FullName).Directory.FullName}/logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", LogLevel.DEBUG, DateTime.UtcNow.AddDays(-3), false);

        _ = Task.Run(async () =>
        {
            this.SourceDirectory = args[0];

            _logger.LogDebug("Project Makoto Translation Source Generator started up.");
            _logger.LogDebug("Source Directory: {0}", this.SourceDirectory);
            _logger.LogDebug("Strings.json Location: {0}", this.StringsJson);
            _logger.LogDebug("Translation.cs Location: {0}", this.TranslationCs);
            DateTime lastModify = new();
            while (true)
            {
                try
                {
                    FileInfo fileInfo = new(this.StringsJson);

                    if (lastModify != fileInfo.LastWriteTimeUtc)
                    {
                        try
                        {
                            string Insert = "";

                            _logger.LogDebug("Translation file updated. Updating Translations.cs..");

                            JObject jsonFile = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(StringsJson));

                            void RecursiveHandle(JObject token, string ParentPath, int depth)
                            {
                                foreach (var item in token)
                                {
                                    var className = $"{item.Key.First().ToString().ToLower()}{item.Key.Remove(0, 1)}";

                                    switch (className)
                                    {
                                        case "object":
                                            className = $"@{className}";
                                            break;
                                        default:
                                            break;
                                    }

                                    var fieldName = $"{item.Key.FirstLetterToUpper()}";
                                    var entryPoint = $"{ParentPath}/{className}";

                                    int InsertPosition = 0;
                                    if (!ParentPath.IsNullOrWhiteSpace())
                                    {
                                        var IndexPath = $"// {ParentPath} InsertPoint";
                                        InsertPosition = Insert.IndexOf(IndexPath) + IndexPath.Length;
                                    }

                                    switch (item.Value.Type)
                                    {
                                        case JTokenType.Object:
                                        {
                                            bool containsLocaleCode = false;
                                            bool localeCodeIsArray = false;
                                            foreach (var subItem in item.Value.ToObject<JObject>())
                                                if (subItem.Key == "en")
                                                {
                                                    containsLocaleCode = true;
                                                    localeCodeIsArray = subItem.Value.Type == JTokenType.Array;
                                                    break;
                                                }

                                            if (containsLocaleCode)
                                            {
                                                if (!localeCodeIsArray)
                                                {
                                                    _logger.LogDebug("Found SingleKey '{0}'", item.Key);
                                                    Insert = Insert.Insert(InsertPosition, $"\n{new string(' ', depth * 4)}public SingleTranslationKey {item.Key};");
                                                }
                                                else
                                                {
                                                    _logger.LogDebug("Found MultiKey '{0}'", item.Key);
                                                    Insert = Insert.Insert(InsertPosition, $"\n{new string(' ', depth * 4)}public MultiTranslationKey {item.Key};");
                                                }
                                                continue;
                                            }
                                            else
                                            {
                                                _logger.LogDebug("Found Group '{0}'", item.Key);

                                                Insert = Insert.Insert(InsertPosition, $"\n{new string(' ', depth * 4)}public {className} {fieldName};\n" +
                                                    $"{new string(' ', depth * 4)}public class {className}\n" +
                                                    $"{new string(' ', depth * 4)}{{\n" +
                                                    $"{new string(' ', depth * 4)}// {entryPoint} InsertPoint\n" +
                                                    $"{new string(' ', depth * 4)}}}\n");
                                            }

                                            RecursiveHandle(item.Value.ToObject<JObject>(), entryPoint, depth + 1);
                                            break;
                                        }
                                        default:
                                            break;
                                    }
                                }
                            }
                            RecursiveHandle(jsonFile, "", 1);

                            Insert = string.Join("\n", Insert.Split("\n").Where(x => !x.Contains("InsertPoint")));

                            File.WriteAllText(TranslationCs, SourceOrigin.Replace("// InsertPoint", Insert));

                            _logger.LogDebug("Updated Translations.cs.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Failed to update Translations.cs.", ex);
                        }
                    }

                    lastModify = fileInfo.LastWriteTimeUtc;

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to watch file", ex);
                    await Task.Delay(10000);
                }
            }
        });

        await Task.Delay(-1);
    }
}
