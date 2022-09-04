namespace ProjectIchigo.Commands;

internal class PurgeCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageMessages) && await CheckOwnPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            int number = (int)arguments["number"];
            DiscordUser victim = (DiscordUser)arguments["victim"];

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
                var embed = new DiscordEmbedBuilder
                {
                    Description = $"`Fetching {number} messages..`",
                }.SetLoading(ctx);
                await RespondOrEdit(embed);

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
                    embed.Description = $"`Fetched {fetchedMessages.Count} messages. Deleting..`";
                    await RespondOrEdit(embed);
                }
                else
                {
                    embed.Description = $"`No messages were found with the specified filter.`";
                    await RespondOrEdit(embed.SetError(ctx));
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
                    _logger.LogError($"Failed to delete messages", ex);
                    embed.Description = $"`An error occurred trying to delete the specified messages. The error has been reported, please try again in a few hours.`";
                    await RespondOrEdit(embed.SetError(ctx));
                    return;
                }

                while (!deletionOperations.All(x => x.IsCompleted))
                {
                    await Task.Delay(1000);
                    embed.Description = $"`Deleted {deleted}/{total} messages..`";
                    await RespondOrEdit(embed);
                }

                embed.Description = $"`Successfully deleted {deleted} messages`\n{(FailedToDeleteAmount > 0 ? $"`Failed to delete {FailedToDeleteAmount} messages because they we're more than 14 days old.`" : "")}";

                await RespondOrEdit(embed.SetSuccess(ctx));
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

                var embed = new DiscordEmbedBuilder
                {
                    Description = $"`Deleted {bMessages.Count} messages.`\n{(FailedToDeleteAmount > 0 ? $"`Failed to delete {FailedToDeleteAmount} messages because they we're more than 14 days old.`" : "")}",
                };
                await RespondOrEdit((FailedToDeleteAmount > 0 ? embed.SetError(ctx) : embed.SetSuccess(ctx)));
            }
        });
    }
}