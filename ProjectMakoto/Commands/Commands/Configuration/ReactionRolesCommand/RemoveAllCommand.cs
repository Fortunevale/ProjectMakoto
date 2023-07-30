// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ReactionRolesCommand;

internal sealed class RemoveAllCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Config.ReactionRoles;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            DiscordMessage message;

            if (arguments?.ContainsKey("message") ?? false)
            {
                message = (DiscordMessage)arguments["message"];
            }
            else
            {
                switch (ctx.CommandType)
                {
                    case Enums.CommandType.PrefixCommand:
                    {
                        if (ctx.OriginalCommandContext.Message.ReferencedMessage is not null)
                        {
                            message = ctx.OriginalCommandContext.Message.ReferencedMessage;
                        }
                        else
                        {
                            SendSyntaxError();
                            return;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Message expected");
                }
            }

            if (message is null)
            {
                SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = GetString(CommandKey.RemovingAllReactionRoles)
            }.AsLoading(ctx, GetString(CommandKey.Title));

            await RespondOrEdit(embed);
            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!ctx.DbGuild.ReactionRoles.Any(x => x.Key == message.Id))
            {
                embed.Description = GetString(CommandKey.NoReactionRoles, true);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, GetString(CommandKey.Title))));
                return;
            }

            foreach (var b in ctx.DbGuild.ReactionRoles.Where(x => x.Key == message.Id).ToList())
                ctx.DbGuild.ReactionRoles.Remove(b);

            _ = message.DeleteAllReactionsAsync();

            embed.Description = GetString(CommandKey.RemovedAllReactionRoles, true,
                new TVar("User", message?.Author.Mention ?? "`/`"),
                new TVar("Channel", message?.Channel.Mention ?? "`/`"));
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, GetString(CommandKey.Title))));
        });
    }
}