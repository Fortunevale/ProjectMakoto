namespace ProjectIchigo.Commands.Music;

internal class ClearQueueCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`The bot is not in a voice channel.`",
                }.SetError(ctx));
                return;
            }

            if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`You aren't in the same channel as the bot.`",
                }.SetError(ctx));
                return;
            }

            if (ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Contains(ctx.User.Id))
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`You already voted to clear the current queue.`",
                }.SetError(ctx));
                return;
            }

            ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Add(ctx.User.Id);

            if (ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
            {
                ctx.Bot.guilds[ctx.Guild.Id].Lavalink.SongQueue.Clear();
                ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Clear();

                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`The queue was cleared.`",
                }.SetSuccess(ctx));
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"`You voted to clear the current queue. ({ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
            }.SetAwaitingInput(ctx);

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            DiscordButtonComponent DisconnectVote = new(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Vote to clear the current queue", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
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

                        if (ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Contains(e.User.Id))
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ `You already voted to clear the current queue.`").AsEphemeral());
                            return;
                        }

                        var member = await e.User.ConvertToMember(ctx.Guild);

                        if (member.VoiceState is null || member.VoiceState.Channel.Id != conn.Channel.Id)
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("❌ `You aren't in the same channel as the bot.`").AsEphemeral());
                            return;
                        }

                        ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Add(e.User.Id);

                        if (ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                        {
                            ctx.Bot.guilds[ctx.Guild.Id].Lavalink.SongQueue.Clear();
                            ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Clear();

                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = $"`The queue was cleared.`",
                            }.SetSuccess(ctx)));
                            return;
                        }

                        embed.Description = $"`You voted to clear the current queue. ({ctx.Bot.guilds[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                        await RespondOrEdit(embed.Build());
                    }
                }).Add(ctx.Bot.watcher);
            }
        });
    }
}