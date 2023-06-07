// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Music;

internal sealed class DisconnectCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null || conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.NotSameChannel, true),
                }.AsError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedDisconnectVotes.Contains(ctx.User.Id))
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.Disconnect.AlreadyVoted, true),
                }.AsError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedDisconnectVotes.Add(ctx.User.Id);

            if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedDisconnectVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
            {
                ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Dispose(ctx.Bot, ctx.Guild.Id, "Graceful Disconnect");
                ctx.Bot.guilds[ctx.Guild.Id].MusicModule = new(ctx.Bot.guilds[ctx.Guild.Id]);

                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = GetString(this.t.Commands.Music.Disconnect.Disconnected, true),
                }.AsSuccess(ctx));
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"`{GetGuildString(this.t.Commands.Music.Disconnect.VoteStarted, true)} ({ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedDisconnectVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
            }.AsAwaitingInput(ctx);

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            DiscordButtonComponent DisconnectVote = new(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetGuildString(this.t.Commands.Music.Disconnect.VoteButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⛔")));
            builder.AddComponents(DisconnectVote);

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

                        if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedDisconnectVotes.Contains(e.User.Id))
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ {GetString(this.t.Commands.Music.Disconnect.AlreadyVoted, true)}").AsEphemeral());
                            return;
                        }

                        var member = await e.User.ConvertToMember(ctx.Guild);

                        if (member.VoiceState is null || member.VoiceState.Channel.Id != conn.Channel.Id)
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ {GetString(this.t.Commands.Music.NotSameChannel, true)}").AsEphemeral());
                            return;
                        }

                        ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedDisconnectVotes.Add(e.User.Id);

                        if (ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedDisconnectVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                        {
                            ctx.Bot.guilds[ctx.Guild.Id].MusicModule.Dispose(ctx.Bot, ctx.Guild.Id, "Graceful Disconnect");
                            ctx.Bot.guilds[ctx.Guild.Id].MusicModule = new(ctx.Bot.guilds[ctx.Guild.Id]);

                            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = GetString(this.t.Commands.Music.Disconnect.Disconnected, true)
                            }.AsSuccess(ctx)));
                            return;
                        }

                        embed.Description = $"`{GetGuildString(this.t.Commands.Music.Disconnect.VoteStarted, true)} ({ctx.Bot.guilds[ctx.Guild.Id].MusicModule.collectedDisconnectVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                        await RespondOrEdit(embed.Build());
                    }
                }).Add(ctx.Bot);
            }
        });
    }
}