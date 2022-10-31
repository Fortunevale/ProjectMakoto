namespace ProjectIchigo.Commands.Data;

internal class DeleteCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForHeavy(ctx.Client, ctx, true))
                return;

            var Yes = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Yes", false, new DiscordComponentEmoji(true.ToEmote(ctx.Bot)));
            var No = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "No", false, new DiscordComponentEmoji(false.ToEmote(ctx.Bot)));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"`This action will delete all data related to your user account. This includes, but is not limited to: Playlists, Settings, Url Submissions.`\n" +
                              $"`This will NOT delete data stored for guilds (see GuildData via '/data request').`\n\n" +
                              $"**`Are you sure you want to continue?`**"
            }.AsBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

            var Menu = await ctx.WaitForButtonAsync();

            if (Menu.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Menu.GetCustomId() == Yes.CustomId)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"**`Please confirm again, are you sure?`**"
                }.AsBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { No, Yes }));

                Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu.GetCustomId() == Yes.CustomId)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Okay, deleting your profile..`"
                    }.AsGenericLoading(ctx));

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
                        }.AsBotError(ctx));
                        return;
                    }

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Successfully deleted your profile.`"
                    }.AsSuccess(ctx));
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