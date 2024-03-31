// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.PrefixCommands;

internal sealed class CustomArgumentConverter
{
    internal sealed class BoolConverter : IArgumentConverter<bool>
    {
        public async Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(1);

            if (value.ToLower() is "true" or "y" or "enable" or "allow" or "on")
                return true;
            else if (value.ToLower() is "false" or "n" or "disable" or "disallow" or "off")
                return false;

            throw new Exception($"Invalid Argument");
        }
    }

    internal sealed class AttachmentConverter : IArgumentConverter<DiscordAttachment>
    {
        public async Task<Optional<DiscordAttachment>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(1);

            if (!ctx.Message.Attachments?.Any() ?? true)
                throw new Exception("No attachment");

            return ctx.Message.Attachments[0];
        }
    }
}
