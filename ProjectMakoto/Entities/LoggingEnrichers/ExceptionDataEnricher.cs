// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Serilog.Core;

namespace ProjectMakoto.Entities.LoggingEnrichers;
public class ExceptionDataEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Exception == null ||
            logEvent.Exception.Data == null ||
            logEvent.Exception.Data.Count == 0)
            return;

        var dataDictionary = logEvent.Exception.Data
            .Cast<DictionaryEntry>()
            .Where(e => e.Key is string)
            .ToDictionary(e => (string)e.Key, e => e.Value);

        var property = propertyFactory.CreateProperty("ExceptionData", dataDictionary, destructureObjects: true);

        logEvent.AddPropertyIfAbsent(property);
    }
}
