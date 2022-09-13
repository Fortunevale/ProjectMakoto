namespace ProjectIchigo.Commands.Data;

internal class ObjectCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForHeavy(ctx.Client, ctx, true))
                return;

            var Yes = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Yes", false, new DiscordComponentEmoji(true.ToEmote(ctx.Client)));
            var No = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "No", false, new DiscordComponentEmoji(false.ToEmote(ctx.Client)));

            if (ctx.Bot.objectedUsers.Contains(ctx.User.Id))
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`You already objected to having your data get processed. Do you want to reverse that decision?`"
                }.SetBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

                var Menu1 = await ctx.WaitForButtonAsync();

                if (Menu1.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu1.Result.Interaction.Data.CustomId == Yes.CustomId)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Okay, removing you from the objection list..`"
                    }.SetBotLoading(ctx));

                    try
                    {
                        ctx.Bot.objectedUsers.Remove(ctx.User.Id);
                        await ctx.Bot.databaseClient._helper.DeleteRow(ctx.Bot.databaseClient.mainDatabaseConnection, "objected_users", "id", $"{ctx.User.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An exception occurred while trying to remove a user from the objection list", ex);

                        await RespondOrEdit(new DiscordEmbedBuilder
                        {
                            Description = $"`I'm sorry but something went wrong while remove you from the objection list. This exception has been logged and will be fixed asap. Please retry in a few hours.`"
                        }.SetBotError(ctx));
                        return;
                    }

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Successfully removed you from the objection list.`"
                    }.SetBotSuccess(ctx));
                }
                else
                {
                    DeleteOrInvalidate();
                }

                return;
            }

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"`This action will delete all data related to your user account and object to further creation of an user account.`\n" +
                              $"**`In addition, this action will make the bot leave every server you own.`**\n" +
                              $"`This will prevent you from using any commands of the bot.`\n" +
                              $"`This will NOT delete data stored for guilds (see GuildData via '/data request').`\n\n" +
                              $"**`Are you sure you want to continue?`**"
            }.SetBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

            var Menu = await ctx.WaitForButtonAsync();

            if (Menu.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Menu.Result.Interaction.Data.CustomId == Yes.CustomId)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"**`Please confirm again, are you sure?`**"
                }.SetBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { No, Yes }));

                Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu.Result.Interaction.Data.CustomId == Yes.CustomId)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Okay, deleting your profile..`"
                    }.SetBotLoading(ctx));

                    try
                    {
                        ctx.Bot.users.Remove(ctx.User.Id);
                        await ctx.Bot.databaseClient._helper.DeleteRow(ctx.Bot.databaseClient.mainDatabaseConnection, "users", "userid", $"{ctx.User.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An exception occurred while trying to delete a user profile", ex);

                        await RespondOrEdit(new DiscordEmbedBuilder
                        {
                            Description = $"`I'm sorry but something went wrong while deleting your profile. This exception has been logged and will be fixed asap. Please retry in a few hours.`"
                        }.SetBotError(ctx));
                        return;
                    }

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Adding your account to objection list..`"
                    }.SetBotLoading(ctx));

                    ctx.Bot.objectedUsers.Add(ctx.User.Id);

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Successfully deleted your profile and added you to the objection list. You will no longer be able to run any commands. To re-allow user account creation, re-run this command.`"
                    }.SetBotSuccess(ctx));

                    foreach (var b in ctx.Client.Guilds.Where(x => x.Value.OwnerId == ctx.User.Id))
                    {
                        try { _logger.LogInfo($"Leaving guild '{b.Key}'.."); await b.Value.LeaveAsync(); } catch { }
                    }
                }
                else
                {
                    DeleteOrInvalidate();
                }
            }
            else
            {
                DeleteOrInvalidate();
            }
        });
    }
}