﻿namespace ProjectIchigo.Commands.Data;

internal class DeleteCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForHeavy(ctx, true))
                return;

            var Yes = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Common.Yes), false, new DiscordComponentEmoji(true.ToEmote(ctx.Bot)));
            var No = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(t.Common.No), false, new DiscordComponentEmoji(false.ToEmote(ctx.Bot)));

            if (ctx.Bot.objectedUsers.Contains(ctx.User.Id))
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(t.Commands.Data.Object.ProfileAlreadyDeleted)}`"
                }.AsBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

                var Menu1 = await ctx.WaitForButtonAsync();

                if (Menu1.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu1.GetCustomId() == Yes.CustomId)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(t.Commands.Data.Object.EnablingDataProcessing)}`"
                    }.AsBotLoading(ctx));

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
                            Description = $"`{GetString(t.Commands.Data.Object.EnablingDataProcessingError)}`"
                        }.AsBotError(ctx));
                        return;
                    }

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(t.Commands.Data.Object.EnablingDataProcessingSuccess)}`"
                    }.AsBotSuccess(ctx));
                }
                else
                {
                    DeleteOrInvalidate();
                }

                return;
            }

            if (ctx.DbUser.Data.DeletionRequested)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(t.Commands.Data.Object.DeletionAlreadyScheduled).Replace("{RequestTimestamp}", $"`{ctx.DbUser.Data.DeletionRequestDate.AddDays(-14).ToTimestamp()}`").Replace("{ScheduleTimestamp}", $"`{ctx.DbUser.Data.DeletionRequestDate.ToTimestamp()}`")}`"
                }.AsBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

                var Menu1 = await ctx.WaitForButtonAsync();

                if (Menu1.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu1.GetCustomId() == Yes.CustomId)
                {
                    ctx.DbUser.Data.DeletionRequested = false;
                    ctx.DbUser.Data.DeletionRequestDate = DateTime.MinValue;

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(t.Commands.Data.Object.DeletionScheduleReversed)}`"
                    }.AsBotSuccess(ctx));
                }
                else
                {
                    DeleteOrInvalidate();
                }

                return;
            }

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Data.Object.ObjectionDisclaimer).Build(true, true)
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
                    Description = $"**`{GetString(t.Commands.Data.Object.SecondaryConfirm)}`**"
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
                    ctx.DbUser.Data.DeletionRequestDate = DateTime.UtcNow.AddDays(14);
                    ctx.DbUser.Data.DeletionRequested = true;

                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`{GetString(t.Commands.Data.Object.ProfileDeletionScheduled)}`"
                    }.AsBotSuccess(ctx));
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