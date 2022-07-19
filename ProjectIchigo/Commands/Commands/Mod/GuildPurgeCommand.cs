﻿namespace ProjectIchigo.Commands;

internal class GuildPurgeCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageMessages) && await CheckPermissions(Permissions.ManageChannels) && await CheckOwnPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            int number = (int)arguments["number"];
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            if (number is > 2000 or < 1)
            {
                SendSyntaxError();
                return;
            }

            var status_embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Server Purge • {ctx.Guild.Name}" },
                Color = EmbedColors.Processing,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`Scanning all channels for messages sent by '{victim.UsernameWithDiscriminator}' ({victim.Id})..`"
            };

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

                status_embed.Description = $"`Scanning all channels for messages sent by '{victim.UsernameWithDiscriminator}' ({victim.Id})..`\n\n" +
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

            status_embed.Description = $"`Found {allMsg} messages sent by '{victim.UsernameWithDiscriminator}' ({victim.Id}). Deleting..`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(status_embed));

            currentProg = 0;
            maxProg = messages.Count;

            foreach (var channel in messages)
            {
                currentProg++;
                status_embed.Description = $"`Found {allMsg} messages sent by '{victim.UsernameWithDiscriminator}' ({victim.Id}). Deleting..`\n\n" +
                                            $"`Current Channel`: `({currentProg}/{maxProg})` <#{channel.Key}> `({channel.Key})`\n" +
                                            $"`Found Messages `: `{allMsg}`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(status_embed));

                await ctx.Guild.GetChannel(channel.Key).DeleteMessagesAsync(channel.Value);
            }

            status_embed.Description = $"`Finished operation.`";
            status_embed.Color = EmbedColors.Success;
            status_embed.Author.IconUrl = Resources.LogIcons.Info;
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(status_embed));
        });
    }
}