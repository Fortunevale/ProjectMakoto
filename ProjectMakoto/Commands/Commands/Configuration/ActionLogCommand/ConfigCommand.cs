namespace ProjectMakoto.Commands.ActionLogCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = ActionLogAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, "Actionlog");

            var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), $"Disable Actionlog", (ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
            var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), $"{(ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel == 0 ? "Set Channel" : "Change Channel")}", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeFilter = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), $"Change Filter", (ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📣")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                { Disable }
            })
            .AddComponents(new List<DiscordComponent>
            {
                { ChangeChannel },
                { ChangeFilter }
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Button.GetCustomId() == Disable.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].ActionLog = new(ctx.Bot.guilds[ctx.Guild.Id]);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeChannel.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(ChannelType.Text, new ChannelPromptConfiguration
                {
                    CreateChannelOption = new()
                    {
                        Name = "actionlog",
                        ChannelType = ChannelType.Text
                    }
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
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any text channels in your server.`"));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                await ChannelResult.Result.ModifyAsync(x => x.PermissionOverwrites = new List<DiscordOverwriteBuilder>
                {
                    new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole) { Denied = Permissions.All },
                    new DiscordOverwriteBuilder(ctx.Member) { Allowed = Permissions.All },
                });

                ctx.Bot.guilds[ctx.Guild.Id].ActionLog.Channel = ChannelResult.Result.Id;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeFilter.CustomId)
            {
                try
                {
                    var Selections = new List<DiscordStringSelectComponentOption>
                    {
                        new DiscordStringSelectComponentOption("Attempt gathering more details", "attempt_further_detail", "This option may sometimes be inaccurate.", ctx.Bot.guilds[ctx.Guild.Id].ActionLog.AttemptGettingMoreDetails, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠"))),
                        new DiscordStringSelectComponentOption("Join, Leaves & Kicks", "log_members_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MembersModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Nickname, Role Updates", "log_member_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption("User Profile Updates", "log_memberprofile_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberProfileModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Message Deletions", "log_message_deleted", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageDeleted, new DiscordComponentEmoji(EmojiTemplates.GetMessage(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Message Modifications'", "log_message_updated", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageModified, new DiscordComponentEmoji(EmojiTemplates.GetMessage(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Role Updates", "log_roles_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.RolesModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Bans & Unbans", "log_banlist_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.BanlistModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Server Modifications", "log_guild_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.GuildModified, new DiscordComponentEmoji(EmojiTemplates.GetGuild(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Channel Modifications", "log_channels_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.ChannelsModified, new DiscordComponentEmoji(EmojiTemplates.GetChannel(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Voice Channel Updates", "log_voice_state", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.VoiceStateUpdated, new DiscordComponentEmoji(EmojiTemplates.GetVoiceState(ctx.Bot))),
                        new DiscordStringSelectComponentOption("Invite Modifications", "log_invites_modified", null, ctx.Bot.guilds[ctx.Guild.Id].ActionLog.InvitesModified, new DiscordComponentEmoji(EmojiTemplates.GetInvite(ctx.Bot))),
                    };

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new DiscordStringSelectComponent("No options selected.", Selections, Guid.NewGuid().ToString(), 0, Selections.Count, false)));

                    var e = await ctx.Client.GetInteractivity().WaitForSelectAsync(ctx.ResponseMessage, x => x.User.Id == ctx.User.Id, ComponentType.StringSelect, TimeSpan.FromMinutes(2));

                    if (e.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }

                    _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    List<string> selected = e.Result.Values.ToList();

                    if (!ctx.Bot.guilds[ctx.Guild.Id].ActionLog.AttemptGettingMoreDetails && selected.Contains("attempt_further_detail"))
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.WithDescription($"`The option 'Attempt gathering more details' may sometimes be inaccurate. Please make sure to double check the audit log on serious concerns.`\n\n" +
                                            $"Continuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(5))}..").AsWarning(ctx, "Actionlog")));
                        await Task.Delay(5000);
                    }

                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.AttemptGettingMoreDetails = selected.Contains("attempt_further_detail");

                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MembersModified = selected.Contains("log_members_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberModified = selected.Contains("log_member_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MemberProfileModified = selected.Contains("log_memberprofile_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageDeleted = selected.Contains("log_message_deleted");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.MessageModified = selected.Contains("log_message_updated");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.RolesModified = selected.Contains("log_roles_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.BanlistModified = selected.Contains("log_banlist_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.GuildModified = selected.Contains("log_guild_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.ChannelsModified = selected.Contains("log_channels_modified");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.VoiceStateUpdated = selected.Contains("log_voice_state");
                    ctx.Bot.guilds[ctx.Guild.Id].ActionLog.InvitesModified = selected.Contains("log_invites_modified");

                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }
            }
            else if (Button.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}