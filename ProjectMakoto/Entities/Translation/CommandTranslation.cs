// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;
public sealed class CommandTranslation
{
    [JsonIgnore]
    public CommandTranslationType TranslatorType { get; set; }

    public int? Type;
    public SingleTranslationKey Names;
    public SingleTranslationKey? Descriptions;

    public CommandTranslation[]? Options;
    public CommandTranslation[]? Choices;
    public CommandTranslation[]? Groups;
    public CommandTranslation[]? Commands;
}

public enum CommandTranslationType
{
    Command = 0,
    Option = 1,
    Group = 2,
}