// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal sealed class SkipCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var session = lava.ConnectedSessions.Values.First(x => x.IsConnected);
            var conn = session.GetGuildPlayer(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.NotSameChannel, true),
                }.AsError(ctx));
                return;
            }

            if (ctx.DbGuild.MusicModule.collectedSkips.Contains(ctx.User.Id))
            {
                _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Skip.AlreadyVoted),
                }.AsError(ctx));
                return;
            }

            ctx.DbGuild.MusicModule.collectedSkips.Add(ctx.User.Id);

            if (ctx.DbGuild.MusicModule.collectedSkips.Count >= (conn.Channel.Users.Count - 1) * 0.51)
            {
                _ = await conn.StopAsync();

                _ = await this.RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Music.Skip.Skipped, true),
                }.AsSuccess(ctx));
                return;
            }

            var embed = new DiscordEmbedBuilder()
            {
                Description = $"`{this.GetGuildString(this.t.Commands.Music.Skip.VoteStarted)} ({ctx.DbGuild.MusicModule.collectedSkips.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
            }.AsAwaitingInput(ctx);

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            DiscordButtonComponent SkipSongVote = new(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetGuildString(this.t.Commands.Music.Skip.VoteButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏩")));
            _ = builder.AddComponents(SkipSongVote);

            _ = await this.RespondOrEdit(builder);

            _ = Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                    this.ModifyToTimedOut();
                }
            });

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    if (e.Message.Id == ctx.ResponseMessage.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (ctx.DbGuild.MusicModule.collectedSkips.Contains(e.User.Id))
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ {this.GetString(this.t.Commands.Music.Skip.AlreadyVoted, true)}").AsEphemeral());
                            return;
                        }

                        var member = await e.User.ConvertToMember(ctx.Guild);

                        if (member.VoiceState is null || member.VoiceState.Channel.Id != conn.Channel.Id)
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ {this.GetString(this.t.Commands.Music.NotSameChannel, true)}").AsEphemeral());
                            return;
                        }

                        ctx.DbGuild.MusicModule.collectedSkips.Add(e.User.Id);

                        if (ctx.DbGuild.MusicModule.collectedSkips.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                        {
                            _ = await conn.StopAsync();

                            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = this.GetString(this.t.Commands.Music.Skip.Skipped, true),
                            }.AsSuccess(ctx)));
                            return;
                        }

                        embed.Description = $"`{this.GetGuildString(this.t.Commands.Music.Skip.VoteStarted)} ({ctx.DbGuild.MusicModule.collectedSkips.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(SkipSongVote));
                    }
                }).Add(ctx.Bot);
            }
        });
    }
}