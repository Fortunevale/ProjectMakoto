namespace ProjectIchigo.Commands.Music;

internal class ClearQueueCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckVoiceState();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn is null)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"❌ `The bot is not in a voice channel.`",
                    Color = EmbedColors.Error,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            if (conn.Channel.Id != ctx.Member.VoiceState.Channel.Id)
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"❌ `You aren't in the same channel as the bot.`",
                    Color = EmbedColors.Error,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            if (ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Contains(ctx.User.Id))
            {
                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"❌ `You already voted to clear the current queue.`",
                    Color = EmbedColors.Error,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Add(ctx.User.Id);

            if (ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
            {
                ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Clear();
                ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Clear();

                await RespondOrEdit(embed: new DiscordEmbedBuilder
                {
                    Description = $"✅ `The queue was cleared.`",
                    Color = EmbedColors.Success,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            DiscordEmbedBuilder embed = new()
            {
                Description = $"❓ `You voted to clear the current queue. ({ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`",
                Color = EmbedColors.AwaitingInput,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = ctx.Guild.IconUrl
                },
                Footer = ctx.GenerateUsedByFooter()
            };

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

                        if (ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Contains(e.User.Id))
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

                        ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Add(e.User.Id);

                        if (ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count >= (conn.Channel.Users.Count - 1) * 0.51)
                        {
                            ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.SongQueue.Clear();
                            ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Clear();

                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = $"✅ `The queue was cleared.`",
                                Color = EmbedColors.Success,
                                Author = new DiscordEmbedBuilder.EmbedAuthor
                                {
                                    Name = ctx.Guild.Name,
                                    IconUrl = ctx.Guild.IconUrl
                                },
                                Footer = ctx.GenerateUsedByFooter(),
                                Timestamp = DateTime.UtcNow
                            }));
                            return;
                        }

                        embed.Description = $"❓ `You voted to clear the current queue. ({ctx.Bot._guilds.List[ctx.Guild.Id].Lavalink.collectedClearQueueVotes.Count}/{Math.Ceiling((conn.Channel.Users.Count - 1.0) * 0.51)})`";
                        await RespondOrEdit(embed.Build());
                    }
                }).Add(ctx.Bot._watcher);
            }
        });
    }
}