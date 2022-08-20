namespace ProjectIchigo.Commands;
internal class AvatarCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            victim ??= ctx.User;

            victim = await victim.GetFromApiAsync();

            var embed = new DiscordEmbedBuilder
            {
                ImageUrl = victim.AvatarUrl,
            }.SetInfo(ctx, $"{victim.UsernameWithDiscriminator}'s Avatar");

            DiscordMember member = null;

            try
            { member = await victim.ConvertToMember(ctx.Guild); }
            catch { }

            var ServerProfilePictureButton = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowServer", "Show Server Profile Picture", (string.IsNullOrWhiteSpace(member?.GuildAvatarHash)), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖥")));
            var ProfilePictureButton = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowProfile", "Show Profile Picture", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));

            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ServerProfilePictureButton);

            var msg = await RespondOrEdit(builder);

            CancellationTokenSource cancellationTokenSource = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            _ = Task.Delay(60000, cancellationTokenSource.Token).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                    ModifyToTimedOut(true);
                }
            });

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message?.Id == msg.Id && e.User.Id == ctx.User.Id)
                    {
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        _ = Task.Delay(60000, cancellationTokenSource.Token).ContinueWith(x =>
                        {
                            if (x.IsCompletedSuccessfully)
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                ModifyToTimedOut(true);
                            }
                        });

                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == ServerProfilePictureButton.CustomId)
                        {
                            embed.ImageUrl = member.GuildAvatarUrl;
                            _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ProfilePictureButton));
                        }
                        else if (e.Interaction.Data.CustomId == ProfilePictureButton.CustomId)
                        {
                            embed.ImageUrl = member.AvatarUrl;
                            _ = RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ServerProfilePictureButton));
                        }
                    }
                }).Add(ctx.Bot.watcher, ctx);
            }
        });
    }
}
