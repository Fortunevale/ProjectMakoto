// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Newtonsoft.Json.Linq;
using ProjectMakoto.Entities.Users;

namespace ProjectMakoto.Util.JsonSerializers;
public sealed class ReminderSnoozeMinifiedSerializer : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var cnv = (ReminderSnoozeButton)value;
        serializer.Serialize(writer, new object[] { cnv.Type, cnv.Description });
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var json = JArray.Load(reader);
        var objects = json.Values<object>().ToArray();

        return (PrivateButtonType)objects[0].ToInt32() != PrivateButtonType.ReminderSnooze
            ? throw new InvalidDataException()
            : (object)new ReminderSnoozeButton
        {
            Description = objects[1].ToString(),
        };
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(JsonConverter).IsAssignableFrom(objectType);
    }
}
