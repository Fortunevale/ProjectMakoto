﻿namespace ProjectIchigo.Commands.Playlists;

internal class LoadShareCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            ulong userid = (ulong)arguments["userid"];
            string id = (string)arguments["id"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"`Loading the shared playlists..`"
            }.SetLoading(ctx, "Playlists");
            await RespondOrEdit(embed);

            if (!Directory.Exists("PlaylistShares"))
                Directory.CreateDirectory("PlaylistShares");

            if (!Directory.Exists($"PlaylistShares/{userid}") || !File.Exists($"PlaylistShares/{userid}/{id}.json"))
            {
                embed.Description = "`The specified sharecode couldn't be found.`";
                embed.SetError(ctx, "Playlists");
                await RespondOrEdit(embed.Build());
                return;
            }

            var user = await ctx.Client.GetUserAsync(userid);

            var rawJson = File.ReadAllText($"PlaylistShares/{userid}/{id}.json");
            var ImportJson = JsonConvert.DeserializeObject<UserPlaylist>((rawJson is null or "null" or "" ? "[]" : rawJson), new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });

            embed.SetInfo(ctx, "Playlists");
            embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = ImportJson.PlaylistThumbnail };
            embed.Color = (ImportJson.PlaylistColor is "#FFFFFF" or null or "" ? EmbedColors.Info : new DiscordColor(ImportJson.PlaylistColor.IsValidHexColor()));
            embed.Description = "`Playlist found! Please check details of the playlist below and confirm or deny whether you want to import this playlist.`\n\n" +
                               $"`Playlist Name`: `{ImportJson.PlaylistName}`\n" +
                               $"`Tracks       `: `{ImportJson.List.Count}`\n" +
                               $"`Created by   `: {user.Mention} `{user.UsernameWithDiscriminator} ({user.Id})`";

            DiscordButtonComponent Confirm = new(ButtonStyle.Success, Guid.NewGuid().ToString(), "Import this playlist", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📥")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { Confirm, Resources.CancelButton }));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(1));

            if (e.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);


            if (e.Result.Interaction.Data.CustomId == Confirm.CustomId)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Importing playlist..`",
                }.SetLoading(ctx, "Playlists")));

                if (ctx.Bot.users[ctx.Member.Id].UserPlaylists.Count >= 10)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Description = $"`You already have 10 Playlists stored. Please delete one to create a new one.`",
                    }.SetError(ctx, "Playlists")));
                    return;
                }

                ctx.Bot.users[ctx.Member.Id].UserPlaylists.Add(ImportJson);

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`The playlist '{ImportJson.PlaylistName}' has been added to your playlists.`",
                }.SetSuccess(ctx, "Playlists")));
            }
            else
            {
                DeleteOrInvalidate();
            }
        });
    }
}