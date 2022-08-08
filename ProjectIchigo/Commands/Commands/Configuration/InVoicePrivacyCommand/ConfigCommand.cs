namespace ProjectIchigo.Commands.InVoicePrivacyCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = InVoicePrivacyCommandAbstractions.GetCurrentConfiguration(ctx)
            }.SetAwaitingInput(ctx, "In-Voice Text Channel Privacy");

            var ToggleDeletion = new DiscordButtonComponent((ctx.Bot._guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Message Deletion", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🗑")));
            var TogglePermission = new DiscordButtonComponent((ctx.Bot._guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Permission Protection", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📋")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                ToggleDeletion,
                TogglePermission
            })
            .AddComponents(Resources.CancelButton));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == ToggleDeletion.CustomId)
            {
                ctx.Bot._guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled = !ctx.Bot._guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == TogglePermission.CustomId)
            {
                ctx.Bot._guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled = !ctx.Bot._guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled;

                _ = Task.Run(async () =>
                {
                    if (ctx.Bot._guilds[ctx.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled)
                    {
                        if (!ctx.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                            return;

                        foreach (var b in ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                        {
                            _ = b.Value.AddOverwriteAsync(ctx.Guild.EveryoneRole, Permissions.None, Permissions.ReadMessageHistory | Permissions.SendMessages, "Enabled In-Voice Privacy");
                        }
                    }
                    else
                    {
                        if (!ctx.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                            return;

                        foreach (var b in ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                        {
                            _ = b.Value.DeleteOverwriteAsync(ctx.Guild.EveryoneRole, "Disabled In-Voice Privacy");
                        }
                    }
                });

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}