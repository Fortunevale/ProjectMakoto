// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class PurgeCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageMessages) && await CheckOwnPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            int number = (int)arguments["number"];
            DiscordUser victim = (DiscordUser)arguments["victim"];

            var CommandKey = this.t.Commands.Moderation.Purge;

            try
            {
                if (ctx.CommandType == Enums.CommandType.PrefixCommand)
                    await ctx.OriginalCommandContext.Message.DeleteAsync();
            }
            catch { }

            if (number is > 2000 or < 1)
            {
                SendSyntaxError();
                return;
            }

            int FailedToDeleteAmount = 0;

            if (number > 100)
            {
                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.Fetching, true, new TVar("Count", number)))
                    .AsLoading(ctx));

                List<DiscordMessage> fetchedMessages = (await ctx.Channel.GetMessagesAsync(100)).ToList();

                if (fetchedMessages.Any(x => x.Id == ctx.ResponseMessage.Id))
                    fetchedMessages.Remove(fetchedMessages.First(x => x.Id == ctx.ResponseMessage.Id));

                while (fetchedMessages.Count <= number)
                {
                    IReadOnlyList<DiscordMessage> fetch;

                    if (fetchedMessages.Count + 100 <= number)
                        fetch = await ctx.Channel.GetMessagesBeforeAsync(fetchedMessages.Last().Id, 100);
                    else
                        fetch = await ctx.Channel.GetMessagesBeforeAsync(fetchedMessages.Last().Id, number - fetchedMessages.Count);

                    if (fetch.Any())
                        fetchedMessages.AddRange(fetch);
                    else
                        break;
                }

                if (victim is not null)
                    foreach (var b in fetchedMessages.Where(x => x.Author.Id != victim.Id).ToList())
                        fetchedMessages.Remove(b);

                int failedDeletion = 0;

                foreach (var b in fetchedMessages.Where(x => x.CreationTimestamp < DateTime.UtcNow.AddDays(-14)).ToList())
                {
                    fetchedMessages.Remove(b);
                    FailedToDeleteAmount++;
                    failedDeletion++;
                }

                if (fetchedMessages.Count > 0)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription(GetString(CommandKey.Fetched, true, new TVar("Count", fetchedMessages.Count)))
                        .AsError(ctx));
                }
                else
                {
                    await RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription(GetString(CommandKey.NoMessages, true))
                        .AsError(ctx));
                    return;
                }

                int total = fetchedMessages.Count;
                int deleted = 0;

                List<Task> deletionOperations = new();

                try
                {
                    while (fetchedMessages.Any())
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
                            fetchedMessages.Remove(b);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to delete messages", ex);
                    throw;
                }

                while (!deletionOperations.All(x => x.IsCompleted))
                {
                    await RespondOrEdit($"`{UniversalExtensions.GenerateASCIIProgressbar(deleted, total)} {UniversalExtensions.CalculatePercentage(deleted, total),3}%`");
                }

                await RespondOrEdit(new DiscordEmbedBuilder().
                    WithDescription($"{GetString(CommandKey.Deleted, true, new TVar("Count", deleted))}\n{GetString(CommandKey.Failed, true, new TVar("Count", FailedToDeleteAmount))}")
                    .AsSuccess(ctx));
                return;
            }
            else
            {
                List<DiscordMessage> bMessages = (await ctx.Channel.GetMessagesAsync(number)).ToList();

                if (victim is not null)
                {
                    foreach (var b in bMessages.Where(x => x.Author.Id != victim.Id).ToList())
                    {
                        bMessages.Remove(b);
                    }
                }

                foreach (var b in bMessages.Where(x => x.CreationTimestamp < DateTime.UtcNow.AddDays(-14)).ToList())
                {
                    bMessages.Remove(b);
                    FailedToDeleteAmount++;
                }

                if (bMessages.Count > 0)
                    await ctx.Channel.DeleteMessagesAsync(bMessages);

                await RespondOrEdit(new DiscordEmbedBuilder().
                WithDescription($"{GetString(CommandKey.Deleted, true, new TVar("Count", bMessages.Count))}\n{GetString(CommandKey.Failed, true, new TVar("Count", FailedToDeleteAmount))}")
                .AsSuccess(ctx));
            }
        });
    }
}