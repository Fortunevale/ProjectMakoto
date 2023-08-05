// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class VcCreatorCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckAdmin() && await CheckOwnPermissions(Permissions.ManageChannels));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Config.VcCreator;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                return $"{EmojiTemplates.GetChannel(ctx.Bot)} `{GetString(CommandKey.Title)}`: {(ctx.DbGuild.VcCreator.Channel == 0 ? false.ToEmote(ctx.Bot) : $"<#{ctx.DbGuild.VcCreator.Channel}>")}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsInfo(ctx, GetString(CommandKey.Title));

            var SetChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.SetVcCreator), false, EmojiTemplates.GetChannel(ctx.Bot).ToComponent());

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                SetChannel
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == SetChannel.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(ChannelType.Voice, new ChannelPromptConfiguration { DisableOption = GetString(CommandKey.DisableVcCreator) });

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
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(CommandKey.NoChannels, true)));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                IReadOnlyList<DiscordOverwrite> present = ChannelResult.Result.Parent.PermissionOverwrites;

                var Category = ChannelResult.Result?.Parent ?? await ctx.Guild.CreateChannelAsync(GetString(CommandKey.Title), ChannelType.Category);
                await ChannelResult.Result?.ModifyAsync(x => { x.Name = $"➕ {GetGuildString(CommandKey.CreateNewChannel)}"; x.Parent = Category; x.PermissionOverwrites = ChannelResult.Result.Parent.PermissionOverwrites.Merge(ctx.Guild.EveryoneRole, Permissions.None, Permissions.ReadMessageHistory | Permissions.UseVoiceDetection | Permissions.Speak); });

                ctx.DbGuild.VcCreator.Channel = ChannelResult.Result?.Id ?? 0;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}