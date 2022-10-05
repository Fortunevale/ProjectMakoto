﻿namespace ProjectIchigo.Commands.JoinCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = JoinCommandAbstractions.GetCurrentConfiguration(ctx)
            }.SetAwaitingInput(ctx, "Join Settings");

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var ToggleGlobalban = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Global Bans", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🌐")));
            var ChangeJoinlogChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Joinlog Channel", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👋")));
            var ChangeRoleOnJoin = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Change Role assigned on join", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
            var ToggleReApplyRoles = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.ReApplyRoles ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Role Re-Apply", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👥")));
            var ToggleReApplyName = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.ReApplyNickname ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Nickname Re-Apply", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

            await RespondOrEdit(builder
            .AddComponents(new List<DiscordComponent>
            {
                ToggleGlobalban,
                ToggleReApplyRoles,
                ToggleReApplyName,
            })
            .AddComponents(new List<DiscordComponent>
            {
                ChangeJoinlogChannel,
                ChangeRoleOnJoin,
            })
            .AddComponents(MessageComponents.CancelButton));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == ToggleGlobalban.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans = !ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.AutoBanGlobalBans;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ToggleReApplyRoles.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.ReApplyRoles = !ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.ReApplyRoles;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ToggleReApplyName.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.ReApplyNickname = !ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.ReApplyNickname;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ChangeJoinlogChannel.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(ChannelType.Text, new ChannelPromptConfiguration
                {
                    CreateChannelOption = new()
                    {
                        Name = "joinlog",
                        ChannelType = ChannelType.Text
                    },
                    DisableOption = "Disable Joinglog"
                });

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
                        await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any text channels in your server.`"));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.JoinlogChannelId = ChannelResult.Result is null ? 0 : ChannelResult.Result.Id;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == ChangeRoleOnJoin.CustomId)
            {
                var RoleResult = await PromptRoleSelection(new RolePromptConfiguration { CreateRoleOption = "AutoAssignedRole", DisableOption = "Disable Role on join" });

                if (RoleResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (RoleResult.Failed)
                {
                    if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any roles in your server.`"));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = RoleResult.Result is null ? 0 : RoleResult.Result.Id;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}