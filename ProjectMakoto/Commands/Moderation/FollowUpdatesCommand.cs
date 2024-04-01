// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class FollowUpdatesCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await this.CheckPermissions(Permissions.ManageWebhooks) && await this.CheckOwnPermissions(Permissions.ManageWebhooks));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var channel = (FollowChannel)arguments["channel"];

            var CommandKey = this.t.Commands.Moderation.FollowUpdates;

            try
            {
                switch (channel)
                {
                    case FollowChannel.GithubUpdates:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GithubLog);
                        _ = await b.FollowAsync(ctx.Channel);
                        break;
                    }
                    case FollowChannel.GlobalBans:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GlobalBanAnnouncements);
                        _ = await b.FollowAsync(ctx.Channel);
                        break;
                    }
                    case FollowChannel.News:
                    {
                        var b = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.News);
                        _ = await b.FollowAsync(ctx.Channel);
                        break;
                    }
                }

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(CommandKey.Followed, true, new TVar("Channel", channel)),
                }.AsSuccess(ctx));
            }
            catch (Exception)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = this.GetString(CommandKey.Failed, true, new TVar("Channel", channel)),
                }.AsError(ctx));
            }
        });
    }
}