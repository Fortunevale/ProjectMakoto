// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal class SkipCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.NotSameChannel, true),
                }.AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedSkips.Contains(ctx.User.Id))
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.Skip.AlreadyVoted),
                }.AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedSkips.Add(ctx.User.Id);

            if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedSkips.Count >= (conn.Channel.Users.Count - 1) * 0.51)
            {
                await conn.StopAsync();

                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Music.Skip.Skipped, true),
                }.AsSuccess(ctx));
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"`{GetGuildString(t.Commands.Music.Skip.VoteStarted)} ({ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedSkips.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
            }.AsAwaitingInput(ctx);

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            DiscordButtonComponent SkipSongVote = new(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetGuildString(t.Commands.Music.Skip.VoteButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⏩")));
            builder.AddComponents(SkipSongVote);

            await RespondOrEdit(builder);

            _ = Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                    ModifyToTimedOut();
                }
            });

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message.Id == ctx.ResponseMessage.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedSkips.Contains(e.User.Id))
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ {GetString(t.Commands.Music.Skip.AlreadyVoted, true)}").AsEphemeral());
                            return;
                        }

                        var member = await e.User.ConvertToMember(ctx.Guild);

                        if (member.VoiceState is null || member.VoiceState.Channel.Id != conn.Channel.Id)
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ {GetString(t.Commands.Music.NotSameChannel, true)}").AsEphemeral());
                            return;
                        }

                        ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedSkips.Add(e.User.Id);

                        if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedSkips.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                        {
                            await conn.StopAsync();

                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = GetString(t.Commands.Music.Skip.Skipped),
                            }.AsSuccess(ctx)));
                            return;
                        }

                        embed.Description = $"`{GetGuildString(t.Commands.Music.Skip.VoteStarted)} ({ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedSkips.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                        await RespondOrEdit(embed.Build());
                    }
                }).Add(ctx.Bot.watcher);
            }
        });
    }
}