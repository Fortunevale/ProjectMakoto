﻿// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMakoto.Util.Initializers;
internal class ListLoader
{
    public static async Task Load(Bot bot)
    {
        bot.countryCodes = new();
        List<string[]> cc = JsonConvert.DeserializeObject<List<string[]>>(File.ReadAllText("Assets/Countries.json"));
        foreach (var b in cc)
        {
            bot.countryCodes.List.Add(b[2], new CountryCodes.CountryInfo
            {
                Name = b[0],
                ContinentCode = b[1],
                ContinentName = b[1].ToLower() switch
                {
                    "af" => "Africa",
                    "an" => "Antarctica",
                    "as" => "Asia",
                    "eu" => "Europe",
                    "na" => "North America",
                    "oc" => "Oceania",
                    "sa" => "South America",
                    _ => "Invalid Continent"
                }
            });
        }

        _logger.LogDebug("Loaded {Count} countries", bot.countryCodes.List.Count);

        bot.languageCodes = new();
        List<string[]> lc = JsonConvert.DeserializeObject<List<string[]>>(File.ReadAllText("Assets/Languages.json"));
        foreach (var b in lc)
        {
            bot.languageCodes.List.Add(new LanguageCodes.LanguageInfo
            {
                Code = b[0],
                Name = b[1],
            });
        }
        _logger.LogDebug("Loaded {Count} languages", bot.languageCodes.List.Count);

        bot.profanityList = JsonConvert.DeserializeObject<List<string>>(await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/zacanger/profane-words/master/words.json"));
        _logger.LogDebug("Loaded {Count} profanity words", bot.profanityList.Count);
    }
}
