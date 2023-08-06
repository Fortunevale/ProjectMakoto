// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Data;

internal sealed class DeleteCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx, true))
                return;

            var Yes = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(this.t.Common.Yes), false, new DiscordComponentEmoji(true.ToEmote(ctx.Bot)));
            var No = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetString(this.t.Common.No), false, new DiscordComponentEmoji(false.ToEmote(ctx.Bot)));

            if (ctx.Bot.objectedUsers.Contains(ctx.User.Id))
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Utility.Data.Object.ProfileAlreadyDeleted, true)
                }.AsBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

                var Menu1 = await ctx.WaitForButtonAsync();

                if (Menu1.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }

                _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu1.GetCustomId() == Yes.CustomId)
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Utility.Data.Object.EnablingDataProcessing, true)
                    }.AsBotLoading(ctx));

                    try
                    {
                        _ = ctx.Bot.objectedUsers.Remove(ctx.User.Id);
                        await ctx.Bot.DatabaseClient._helper.DeleteRow(ctx.Bot.DatabaseClient.mainDatabaseConnection, "objected_users", "id", $"{ctx.User.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An exception occurred while trying to remove a user from the objection list", ex);

                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                        {
                            Description = this.GetString(this.t.Commands.Utility.Data.Object.EnablingDataProcessingError, true)
                        }.AsBotError(ctx));
                        return;
                    }

                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Utility.Data.Object.EnablingDataProcessingSuccess, true)
                    }.AsBotSuccess(ctx));
                }
                else
                {
                    this.DeleteOrInvalidate();
                }

                return;
            }

            if (ctx.DbUser.Data.DeletionRequested)
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = this.GetString(this.t.Commands.Utility.Data.Object.DeletionAlreadyScheduled, true,
                    new TVar("RequestTimestamp", ctx.DbUser.Data.DeletionRequestDate.AddDays(-14).ToTimestamp()),
                    new TVar("ScheduleTimestamp", ctx.DbUser.Data.DeletionRequestDate.ToTimestamp()))
                }.AsBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

                var Menu1 = await ctx.WaitForButtonAsync();

                if (Menu1.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }

                _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu1.GetCustomId() == Yes.CustomId)
                {
                    ctx.DbUser.Data.DeletionRequested = false;
                    ctx.DbUser.Data.DeletionRequestDate = DateTime.MinValue;

                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Utility.Data.Object.DeletionScheduleReversed, true)
                    }.AsBotSuccess(ctx));
                }
                else
                {
                    this.DeleteOrInvalidate();
                }

                return;
            }

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.Utility.Data.Object.ObjectionDisclaimer, true, true)
            }.AsBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

            var Menu = await ctx.WaitForButtonAsync();

            if (Menu.TimedOut)
            {
                this.ModifyToTimedOut();
                return;
            }

            _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Menu.GetCustomId() == Yes.CustomId)
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"**{this.GetString(this.t.Commands.Utility.Data.Object.SecondaryConfirm, true)}**"
                }.AsBotAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { No, Yes }));

                Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }

                _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu.GetCustomId() == Yes.CustomId)
                {
                    ctx.DbUser.Data.DeletionRequestDate = DateTime.UtcNow.AddDays(14);
                    ctx.DbUser.Data.DeletionRequested = true;

                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = this.GetString(this.t.Commands.Utility.Data.Object.ProfileDeletionScheduled, true)
                    }.AsBotSuccess(ctx));
                }
                else
                {
                    this.DeleteOrInvalidate();
                }
            }
            else
            {
                this.DeleteOrInvalidate();
            }
        });
    }
}