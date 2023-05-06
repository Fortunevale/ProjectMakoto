// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class FollowUpdatesCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageWebhooks) && await CheckOwnPermissions(Permissions.ManageWebhooks));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var channel = (FollowChannel)arguments["channel"];

            try
            {
                switch (channel)
                {
                    case FollowChannel.GithubUpdates:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GithubLog);
                        await b.FollowAsync(ctx.Channel);
                        break;
                    }
                    case FollowChannel.GlobalBans:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
                        await b.FollowAsync(ctx.Channel);
                        break;
                    }
                    case FollowChannel.News:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.News);
                        await b.FollowAsync(ctx.Channel);
                        break;
                    }
                }

                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Successfully followed {channel}.`",
                }.AsSuccess(ctx));
            }
            catch (Exception)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Could not follow {channel}.`",
                }.AsError(ctx));
            }
        });
    }
}