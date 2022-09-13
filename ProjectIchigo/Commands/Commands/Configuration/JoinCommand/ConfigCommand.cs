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
                try
                {
                    var channel = await PromptChannelSelection(ChannelType.Text, true, "joinlog", ChannelType.Text, true, "Disable Joinlog");

                    if (channel is null)
                        ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.JoinlogChannelId = 0;
                    else
                        ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.JoinlogChannelId = channel.Id;

                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                }
                catch (NullReferenceException)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any text channels in your server.`"));
                    await Task.Delay(3000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (e.Result.Interaction.Data.CustomId == ChangeRoleOnJoin.CustomId)
            {
                try
                {
                    var role = await PromptRoleSelection(true, "AutoAssignedRole", true, "Disable Role on join");

                    if (role is null)
                        ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = 0;
                    else
                        ctx.Bot.guilds[ctx.Guild.Id].JoinSettings.AutoAssignRoleId = role.Id;

                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (e.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}