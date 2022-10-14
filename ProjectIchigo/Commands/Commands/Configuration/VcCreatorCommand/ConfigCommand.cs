﻿namespace ProjectIchigo.Commands.VcCreatorCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckAdmin() && await CheckOwnPermissions(Permissions.ManageChannels));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = VcCreatorCommandAbstractions.GetCurrentConfiguration(ctx)
            }.SetInfo(ctx, "Voice Channel Creator");

            var SetChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set Voice Channel Creator", false, EmojiTemplates.GetChannel(ctx.Client, ctx.Bot).ToComponent());

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                SetChannel
            })
            .AddComponents(MessageComponents.CancelButton));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == SetChannel.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(ChannelType.Voice, new ChannelPromptConfiguration { DisableOption = "Disable Voice Channel Creator" });

                if (ChannelResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (ChannelResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ChannelResult.Failed)
                {
                    if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any voice channels in your server.`"));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                DiscordOverwrite present = null;
                if (ChannelResult.Result?.Parent?.PermissionOverwrites.Any(x => (x.Type == OverwriteType.Role) && (x.Id == ctx.Guild.EveryoneRole.Id)) ?? false)
                    present = ChannelResult.Result.Parent.PermissionOverwrites.First(x => (x.Type == OverwriteType.Role) && (x.Id == ctx.Guild.EveryoneRole.Id));

                var Category = ChannelResult.Result?.Parent ?? await ctx.Guild.CreateChannelAsync("Voice Channel Creator", ChannelType.Category);
                await ChannelResult.Result?.ModifyAsync(x => { x.Name = "➕ Create new Channel"; x.Parent = Category; x.PermissionOverwrites = new List<DiscordOverwriteBuilder>() { new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole) { Allowed = (present?.Allowed ?? Permissions.None), Denied = (present?.Denied ?? Permissions.None) | Permissions.ReadMessageHistory | Permissions.UseVoiceDetection | Permissions.Speak } }; });

                ctx.Bot.guilds[ctx.Guild.Id].VcCreator.Channel = ChannelResult.Result?.Id ?? 0;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}