namespace ProjectIchigo.Commands.Music;

internal class DisconnectCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
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

            if (ctx.Bot._guilds[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Contains(ctx.User.Id))
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`You already voted to disconnect the bot.`",
                }.SetError(ctx));
                return;
            }

            ctx.Bot._guilds[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Add(ctx.User.Id);

            if (ctx.Bot._guilds[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
            {
                ctx.Bot._guilds[ctx.Guild.Id].Lavalink.Dispose(ctx.Bot, ctx.Guild.Id, "Graceful Disconnect");
                ctx.Bot._guilds[ctx.Guild.Id].Lavalink = new(ctx.Bot._guilds[ctx.Guild.Id]);

                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"`The bot was disconnected.`",
                }.SetSuccess(ctx));
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = $"`You voted to disconnect the bot. ({ctx.Bot._guilds[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
            }.SetAwaitingInput(ctx);

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            DiscordButtonComponent DisconnectVote = new(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Vote to disconnect", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⛔")));
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

                        if (ctx.Bot._guilds[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Contains(e.User.Id))
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent($"❌ `You already voted to disconnect the bot.`").AsEphemeral());
                            return;
                        }

                        var member = await e.User.ConvertToMember(ctx.Guild);

                        if (member.VoiceState is null || member.VoiceState.Channel.Id != conn.Channel.Id)
                        {
                            _ = e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("❌ `You aren't in the same channel as the bot.`").AsEphemeral());
                            return;
                        }

                        ctx.Bot._guilds[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Add(e.User.Id);

                        if (ctx.Bot._guilds[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                        {
                            ctx.Bot._guilds[ctx.Guild.Id].Lavalink.Dispose(ctx.Bot, ctx.Guild.Id, "Graceful Disconnect");
                            ctx.Bot._guilds[ctx.Guild.Id].Lavalink = new(ctx.Bot._guilds[ctx.Guild.Id]);

                            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
                            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).DisconnectAsync();

                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = $"`The bot was disconnected.`",
                            }.SetSuccess(ctx)));
                            return;
                        }

                        embed.Description = $"`You voted to disconnect the bot. ({ctx.Bot._guilds[ctx.Guild.Id].Lavalink.collectedDisconnectVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                        await RespondOrEdit(embed.Build());
                    }
                }).Add(ctx.Bot._watcher);
            }
        });
    }
}