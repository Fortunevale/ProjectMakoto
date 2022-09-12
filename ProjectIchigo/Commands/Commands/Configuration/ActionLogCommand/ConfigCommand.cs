namespace ProjectIchigo.Commands.ActionLogCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = ActionLogAbstractions.GetCurrentConfiguration(ctx)
            }.SetAwaitingInput(ctx, "Actionlog");

            var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), $"Disable Actionlog", (ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
            var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), $"{(ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.Channel == 0 ? "Set Channel" : "Change Channel")}", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeFilter = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), $"Change Filter", (ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📣")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                { Disable }
            })
            .AddComponents(new List<DiscordComponent>
            {
                { ChangeChannel },
                { ChangeFilter }
            }).AddComponents(MessageComponents.CancelButton));

            var Button = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Button.Result.Interaction.Data.CustomId == Disable.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings = new(ctx.Bot.guilds[ctx.Guild.Id]);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.Result.Interaction.Data.CustomId == ChangeChannel.CustomId)
            {
                try
                {
                    var channel = await PromptChannelSelection(ChannelType.Text, true, "actionlog", ChannelType.Text);

                    await channel.ModifyAsync(x => x.PermissionOverwrites = new List<DiscordOverwriteBuilder>
                    {
                        new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole) { Denied = Permissions.All },
                        new DiscordOverwriteBuilder(ctx.Member) { Allowed = Permissions.All },
                    });

                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.Channel = channel.Id;

                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                catch (NullReferenceException)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder().SetError(ctx).WithDescription("`Could not find any text channels in your server.`"));
                    await Task.Delay(3000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
            }
            else if (Button.Result.Interaction.Data.CustomId == ChangeFilter.CustomId)
            {
                try
                {
                    var Selections = new List<DiscordSelectComponentOption>
                    {
                        new DiscordSelectComponentOption("Attempt gathering more details", "attempt_further_detail", "This option may sometimes be inaccurate.", ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠"))),
                        new DiscordSelectComponentOption("Join, Leaves & Kicks", "log_members_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MembersModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Nickname, Role Updates", "log_member_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MemberModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("User Profile Updates", "log_memberprofile_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MemberProfileModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Message Deletions", "log_message_deleted", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MessageDeleted, new DiscordComponentEmoji(EmojiTemplates.GetMessage(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Message Modifications'", "log_message_updated", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MessageModified, new DiscordComponentEmoji(EmojiTemplates.GetMessage(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Role Updates", "log_roles_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.RolesModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Bans & Unbans", "log_banlist_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.BanlistModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Server Modifications", "log_guild_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.GuildModified, new DiscordComponentEmoji(EmojiTemplates.GetGuild(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Channel Modifications", "log_channels_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.ChannelsModified, new DiscordComponentEmoji(EmojiTemplates.GetChannel(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Voice Channel Updates", "log_voice_state", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated, new DiscordComponentEmoji(EmojiTemplates.GetVoiceState(ctx.Client, ctx.Bot))),
                        new DiscordSelectComponentOption("Invite Modifications", "log_invites_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.InvitesModified, new DiscordComponentEmoji(EmojiTemplates.GetInvite(ctx.Client, ctx.Bot))),
                    };

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new DiscordSelectComponent("No options selected.", Selections, Guid.NewGuid().ToString(), 0, Selections.Count, false)));

                    var e = await ctx.Client.GetInteractivity().WaitForSelectAsync(ctx.ResponseMessage, x => x.User.Id == ctx.User.Id, TimeSpan.FromMinutes(2));

                    if (e.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }

                    _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    List<string> selected = e.Result.Values.ToList();

                    if (!ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails && selected.Contains("attempt_further_detail"))
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription($"`The option 'Attempt gathering more details' may sometimes be inaccurate. Please make sure to double check the audit log on serious concerns.`\n\n" +
                                            $"Continuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(5))}..").SetWarning(ctx, "Actionlog")));
                        await Task.Delay(5000);
                    }

                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.AttemptGettingMoreDetails = selected.Contains("attempt_further_detail");

                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MembersModified = selected.Contains("log_members_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MemberModified = selected.Contains("log_member_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MemberProfileModified = selected.Contains("log_memberprofile_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MessageDeleted = selected.Contains("log_message_deleted");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.MessageModified = selected.Contains("log_message_updated");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.RolesModified = selected.Contains("log_roles_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.BanlistModified = selected.Contains("log_banlist_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.GuildModified = selected.Contains("log_guild_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.ChannelsModified = selected.Contains("log_channels_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.VoiceStateUpdated = selected.Contains("log_voice_state");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLogSettings.InvitesModified = selected.Contains("log_invites_modified");

                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }
            }
            else if (Button.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}