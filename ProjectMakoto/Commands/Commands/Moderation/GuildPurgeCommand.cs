// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class GuildPurgeCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageMessages) && await CheckPermissions(Permissions.ManageChannels) && await CheckOwnPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            int number = (int)arguments["number"];
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
                return;

            if (number is > 2000 or < 1)
            {
                SendSyntaxError();
                return;
            }

            var status_embed = new DiscordEmbedBuilder
            {
                Description = $"`Scanning all channels for messages sent by '{victim.GetUsername()}' ({victim.Id})..`"
            }.AsLoading(ctx, "Server Purge");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(status_embed));

            int currentProg = 0;
            int maxProg = ctx.Guild.Channels.Count;

            int allMsg = 0;
            Dictionary<ulong, List<DiscordMessage>> messages = new();

            foreach (var channel in ctx.Guild.Channels.Where(x => x.Value.Type is ChannelType.Text or ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.News))
            {
                allMsg = 0;
                foreach (var b in messages)
                    allMsg += b.Value.Count;

                currentProg++;

                status_embed.Description = $"`Scanning all channels for messages sent by '{victim.GetUsername()}' ({victim.Id})..`\n\n" +
                                            $"`Current Channel`: `({currentProg}/{maxProg})` {channel.Value.Mention} `({channel.Value.Id})`\n" +
                                            $"`Found Messages `: `{allMsg}`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(status_embed));

                int MessageInt = number;

                List<DiscordMessage> requested_messages = new();

                var pre_request = await channel.Value.GetMessagesAsync(1);

                if (pre_request.Count > 0)
                {
                    requested_messages.Add(pre_request[0]);
                    MessageInt -= 1;
                }

                while (true)
                {
                    if (pre_request.Count == 0)
                        break;

                    if (MessageInt <= 0)
                        break;

                    if (MessageInt > 100)
                    {
                        var current_request = await channel.Value.GetMessagesBeforeAsync(requested_messages.Last().Id, 100);

                        if (current_request.Count == 0)
                            break;

                        foreach (var b in current_request)
                            requested_messages.Add(b);

                        MessageInt -= 100;
                    }
                    else
                    {
                        var current_request = await channel.Value.GetMessagesBeforeAsync(requested_messages.Last().Id, MessageInt);

                        if (current_request.Count == 0)
                            break;

                        foreach (var b in current_request)
                            requested_messages.Add(b);

                        MessageInt -= MessageInt;
                    }
                }

                if (requested_messages.Count > 0)
                    foreach (var b in requested_messages.ToList())
                    {
                        if (b.Author.Id == victim.Id && b.CreationTimestamp.AddDays(14) > DateTime.UtcNow)
                        {
                            if (!messages.ContainsKey(channel.Key))
                                messages.Add(channel.Key, new List<DiscordMessage>());

                            messages[channel.Key].Add(b);
                        }
                    }
            }

            status_embed.Description = $"`Found {allMsg} messages sent by '{victim.GetUsername()}' ({victim.Id}). Deleting..`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(status_embed));

            currentProg = 0;
            maxProg = messages.Count;

            foreach (var channel in messages)
            {
                currentProg++;
                status_embed.Description = $"`Found {allMsg} messages sent by '{victim.GetUsername()}' ({victim.Id}). Deleting..`\n\n" +
                                            $"`Current Channel`: `({currentProg}/{maxProg})` <#{channel.Key}> `({channel.Key})`\n" +
                                            $"`Found Messages `: `{allMsg}`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(status_embed));

                await ctx.Guild.GetChannel(channel.Key).DeleteMessagesAsync(channel.Value);
            }

            status_embed.Description = $"`{allMsg} were found. {currentProg}/{maxProg} were deleted.`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(status_embed.AsSuccess(ctx, "Server Purge")));
        });
    }
}