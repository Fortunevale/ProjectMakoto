// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class GuildPurgeCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckPermissions(Permissions.ManageMessages) && await this.CheckPermissions(Permissions.ManageChannels) && await this.CheckOwnPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Moderation.GuildPurge;

            var number = (int)arguments["number"];
            var victim = (DiscordUser)arguments["user"];

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            if (number is > 2000 or < 1)
            {
                this.SendSyntaxError();
                return;
            }

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder().
                WithDescription(this.GetString(CommandKey.Scanning, true, new TVar("Victim", victim.Mention)))
                .AsLoading(ctx)));

            var currentProg = 0;
            var maxProg = ctx.Guild.Channels.Count;

            var allMsg = 0;
            Dictionary<ulong, List<DiscordMessage>> channelList = new();

            foreach (var channel in ctx.Guild.Channels.Where(x => x.Value.Type is ChannelType.Text or ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.News or ChannelType.Voice))
            {
                allMsg = 0;
                foreach (var b in channelList)
                    allMsg += b.Value.Count;

                currentProg++;

                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder().
                    WithDescription($"{this.GetString(CommandKey.Scanning, true, new TVar("Victim", victim.Mention))}\n" +
                                    $"`{StringTools.GenerateASCIIProgressbar(currentProg, maxProg)} {MathTools.CalculatePercentage(currentProg, maxProg),3}%`")
                    .AsLoading(ctx)));

                var MessageInt = number;

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
                            if (!channelList.ContainsKey(channel.Key))
                                channelList.Add(channel.Key, new List<DiscordMessage>());

                            channelList[channel.Key].Add(b);
                        }
                    }
            }

            foreach (var channel in channelList)
                foreach (var message in channel.Value.ToList())
                    if (message.CreationTimestamp.GetTimespanSince() > TimeSpan.FromDays(14))
                        _ = channel.Value.Remove(message);

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                .WithDescription($"{this.GetString(CommandKey.Deleting, true, new TVar("Victim", victim.Mention), new TVar("Count", allMsg))}\n" +
                                 $"`{StringTools.GenerateASCIIProgressbar(currentProg, maxProg)} {MathTools.CalculatePercentage(currentProg, maxProg)}%`")
                .AsLoading(ctx)));

            currentProg = 0;
            maxProg = 0;

            foreach (var channel in channelList)
                maxProg += channel.Value.Count;

            foreach (var channel in channelList)
            {
                try
                {
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                        .WithDescription($"{this.GetString(CommandKey.Deleting, true, new TVar("Victim", victim.Mention), new TVar("Count", allMsg))}\n" +
                                         $"`{StringTools.GenerateASCIIProgressbar(currentProg, maxProg)} {MathTools.CalculatePercentage(currentProg, maxProg)}%`")
                        .AsLoading(ctx)));

                    while (channel.Value.Count > 0)
                    {
                        var msgs = channel.Value.Take(100).ToList();
                        await ctx.Guild.GetChannel(channel.Key).DeleteMessagesAsync(msgs);
                        channel.Value.RemoveRange(0, msgs.Count);
                        currentProg += msgs.Count;
                    }
                }
                catch { }
            }

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                .WithDescription(this.GetString(CommandKey.Ended, true, new TVar("Victim", victim.Mention), new TVar("Min", currentProg), new TVar("Max", maxProg), new TVar("ChannelCount", channelList.Count)))
                .AsSuccess(ctx)));
        });
    }
}