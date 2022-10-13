namespace ProjectIchigo.Commands.Music;

internal class PlayCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckVoiceState() && await CheckOwnPermissions(Permissions.UseVoice) && await CheckOwnPermissions(Permissions.UseVoiceDetection));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string search = (string)arguments["search"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            if (search.IsNullOrWhiteSpace())
            {
                SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Preparing connection..`",
            }.SetLoading(ctx);
            await RespondOrEdit(embed);

            try
            {
                await new JoinCommand().ExecuteCommand(ctx, null);
            }
            catch (CancelException)
            {
                return;
            }

            var (Tracks, oriResult, Continue) = await MusicModuleAbstractions.GetLoadResult(ctx, search);

            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!Continue || !Tracks.IsNotNullAndNotEmpty())
                return;

            if (Tracks.Count > 1)
            {
                int added = 0;

                foreach (var b in Tracks)
                {
                    added++;
                    ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Add(new(b.Title, b.Uri.ToString(), b.Length, ctx.Guild, ctx.User));
                }

                embed.Description = $"`Queued {added} songs from `[`{oriResult.PlaylistInfo.Name}`]({search})`.`";

                embed.AddField(new DiscordEmbedField($"📜 Queue positions", $"{(ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count - added + 1)} - {ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count}", true));

                embed.SetSuccess(ctx);
                await ctx.BaseCommand.RespondOrEdit(embed);
            }
            else if (Tracks.Count == 1)
            {
                var track = Tracks[0];

                ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Add(new(track.Title, track.Uri.ToString(), track.Length, ctx.Guild, ctx.User));

                embed.Description = $"`Queued `[`{track.Title}`]({track.Uri})`.`";

                embed.AddField(new DiscordEmbedField($"📜 Queue position", $"{ctx.Bot.guilds[ctx.Guild.Id].MusicModule.SongQueue.Count}", true));
                embed.AddField(new DiscordEmbedField($"🔼 Uploaded by", $"{track.Author}", true));
                embed.AddField(new DiscordEmbedField($"🕒 Duration", $"{track.Length.GetHumanReadable(TimeFormat.MINUTES)}", true));

                embed.SetSuccess(ctx);
                await ctx.BaseCommand.RespondOrEdit(embed.Build());
            }
        });
    }
}