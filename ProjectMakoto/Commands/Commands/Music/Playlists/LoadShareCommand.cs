// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Playlists;

internal sealed class LoadShareCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            ulong userid = (ulong)arguments["userid"];
            string id = ((string)arguments["id"])
                .Replace("/", "")
                .Replace("\\", "")
                .Replace(">", "")
                .Replace("<", "")
                .Replace("|", "")
                .Replace(":", "")
                .Replace("&", "");

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = GetString(t.Commands.Music.Playlists.LoadShare.Loading, true)
            }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title));
            await RespondOrEdit(embed);

            if (!Directory.Exists("PlaylistShares"))
                Directory.CreateDirectory("PlaylistShares");

            if (!Directory.Exists($"PlaylistShares/{userid}") || !File.Exists($"PlaylistShares/{userid}/{id}.json"))
            {
                embed.Description = GetString(t.Commands.Music.Playlists.LoadShare.NotFound);
                embed.AsError(ctx, GetString(t.Commands.Music.Playlists.Title));
                await RespondOrEdit(embed.Build());
                return;
            }

            var user = await ctx.Client.GetUserAsync(userid);

            var rawJson = File.ReadAllText($"PlaylistShares/{userid}/{id}.json");
            var ImportJson = JsonConvert.DeserializeObject<UserPlaylist>((rawJson is null or "null" or "" ? "[]" : rawJson), new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });

            var pad = TranslationUtil.CalculatePadding(ctx.DbUser, t.Commands.Music.Playlists.LoadShare.PlaylistName, t.Commands.Music.Playlists.Tracks, t.Commands.Music.Playlists.LoadShare.CreatedBy);

            embed.AsInfo(ctx, GetString(t.Commands.Music.Playlists.Title));
            embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = ImportJson.PlaylistThumbnail };
            embed.Color = (ImportJson.PlaylistColor is "#FFFFFF" or null or "" ? EmbedColors.Info : new DiscordColor(ImportJson.PlaylistColor.IsValidHexColor()));
            embed.Description = $"{GetString(t.Commands.Music.Playlists.LoadShare.Found, true)}\n\n" +
                                $"`{GetString(t.Commands.Music.Playlists.LoadShare.PlaylistName).PadRight(pad)}`: `{ImportJson.PlaylistName}`\n" +
                                $"`{GetString(t.Commands.Music.Playlists.Tracks).PadRight(pad)}`: `{ImportJson.List.Count}`\n" +
                                $"`{GetString(t.Commands.Music.Playlists.LoadShare.CreatedBy).PadRight(pad)}`: {user.Mention} `{user.GetUsernameWithIdentifier()} ({user.Id})`";

            DiscordButtonComponent Confirm = new(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Commands.Music.Playlists.LoadShare.ImportButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📥")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { Confirm, MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot) }));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(1));

            if (e.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);


            if (e.GetCustomId() == Confirm.CustomId)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.Playlists.LoadShare.Importing, true),
                }.AsLoading(ctx, GetString(t.Commands.Music.Playlists.Title))));

                if (ctx.DbUser.UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = GetString(t.Commands.Music.Playlists.PlayListLimit, true, new TVar("Count", 10)),
                    }.AsError(ctx, GetString(t.Commands.Music.Playlists.Title))));
                    return;
                }

                ctx.DbUser.UserPlaylists.Add(ImportJson);

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.Playlists.LoadShare.Imported, true, new TVar("Name", ImportJson.PlaylistName)),
                }.AsSuccess(ctx, GetString(t.Commands.Music.Playlists.Title))));
            }
            else
            {
                DeleteOrInvalidate();
            }
        });
    }
}