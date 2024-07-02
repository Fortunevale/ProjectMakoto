// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class PurgeCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckPermissions(Permissions.ManageMessages) && await this.CheckOwnPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var number = (int)arguments["number"];
            var victim = (DiscordUser)arguments["user"];

            var CommandKey = this.t.Commands.Moderation.Purge;

            try
            {
                if (ctx.CommandType == Enums.CommandType.PrefixCommand)
                    await ctx.OriginalCommandContext.Message.DeleteAsync();
            }
            catch { }

            if (number is > 2000 or < 1)
            {
                this.SendSyntaxError();
                return;
            }

            var FailedToDeleteAmount = 0;

            if (number > 100)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.Fetching, true, new TVar("Count", number)))
                    .AsLoading(ctx));

                var fetchedMessages = (await ctx.Channel.GetMessagesAsync(100)).ToList();

                if (fetchedMessages.Any(x => x.Id == ctx.ResponseMessage.Id))
                    _ = fetchedMessages.Remove(fetchedMessages.First(x => x.Id == ctx.ResponseMessage.Id));

                while (fetchedMessages.Count <= number)
                {
                    var fetch = fetchedMessages.Count + 100 <= number
                        ? await ctx.Channel.GetMessagesBeforeAsync(fetchedMessages.Last().Id, 100)
                        : await ctx.Channel.GetMessagesBeforeAsync(fetchedMessages.Last().Id, number - fetchedMessages.Count);

                    if (fetch.Any())
                        fetchedMessages.AddRange(fetch);
                    else
                        break;
                }

                if (victim is not null)
                    foreach (var b in fetchedMessages.Where(x => x.Author.Id != victim.Id).ToList())
                        _ = fetchedMessages.Remove(b);

                var failedDeletion = 0;

                foreach (var b in fetchedMessages.Where(x => x.CreationTimestamp < DateTime.UtcNow.AddDays(-14)).ToList())
                {
                    _ = fetchedMessages.Remove(b);
                    FailedToDeleteAmount++;
                    failedDeletion++;
                }

                if (fetchedMessages.Count > 0)
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription(this.GetString(CommandKey.Fetched, true, new TVar("Count", fetchedMessages.Count)))
                        .AsError(ctx));
                }
                else
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription(this.GetString(CommandKey.NoMessages, true))
                        .AsError(ctx));
                    return;
                }

                var total = fetchedMessages.Count;
                var deleted = 0;

                List<Task> deletionOperations = new();

                try
                {
                    while (fetchedMessages.Count != 0)
                    {
                        var currentDeletion = fetchedMessages.Take(100);

                        deletionOperations.Add(ctx.Channel.DeleteMessagesAsync(currentDeletion).ContinueWith(task =>
                        {
                            if (task.IsCompletedSuccessfully)
                                deleted += currentDeletion.Count();
                            else
                                failedDeletion += currentDeletion.Count();
                        }));

                        foreach (var b in currentDeletion.ToList())
                            _ = fetchedMessages.Remove(b);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to delete messages");
                    throw;
                }

                while (!deletionOperations.All(x => x.IsCompleted))
                {
                    _ = await this.RespondOrEdit($"`{StringTools.GenerateASCIIProgressbar(deleted, total)} {MathTools.CalculatePercentage(deleted, total),3}%`");
                }

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().
                    WithDescription($"{this.GetString(CommandKey.Deleted, true, new TVar("Count", deleted))}\n{this.GetString(CommandKey.Failed, true, new TVar("Count", FailedToDeleteAmount))}")
                    .AsSuccess(ctx));
                return;
            }
            else
            {
                var bMessages = (await ctx.Channel.GetMessagesAsync(number)).ToList();

                if (victim is not null)
                {
                    foreach (var b in bMessages.Where(x => x.Author.Id != victim.Id).ToList())
                    {
                        _ = bMessages.Remove(b);
                    }
                }

                foreach (var b in bMessages.Where(x => x.CreationTimestamp < DateTime.UtcNow.AddDays(-14)).ToList())
                {
                    _ = bMessages.Remove(b);
                    FailedToDeleteAmount++;
                }

                if (bMessages.Count > 0)
                    await ctx.Channel.DeleteMessagesAsync(bMessages);

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().
                WithDescription($"{this.GetString(CommandKey.Deleted, true, new TVar("Count", bMessages.Count))}\n{this.GetString(CommandKey.Failed, true, new TVar("Count", FailedToDeleteAmount))}")
                .AsSuccess(ctx));
            }
        });
    }
}