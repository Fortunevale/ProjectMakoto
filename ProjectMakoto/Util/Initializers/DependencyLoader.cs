// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using FFMpegCore;

namespace ProjectMakoto.Util.Initializers;
internal static class DependencyLoader
{
    public static async Task Load(Bot bot)
    {
        if (!GenericExtensions.TryGetFileInfo("ffmpeg", out var ffmpegInfo))
            throw new FileNotFoundException("Please install ffmpeg.", "ffmpeg");

        GlobalFFOptions.Configure(new FFOptions
        {
            BinaryFolder = ffmpegInfo.Directory.FullName,
            TemporaryFilesFolder = Path.GetTempPath(),
        });
    }
}
