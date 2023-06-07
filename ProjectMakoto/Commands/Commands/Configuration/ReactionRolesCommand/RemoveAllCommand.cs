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
                Description = "`Removing all reaction roles..`"
            }.AsLoading(ctx, "Reaction Roles");

            await RespondOrEdit(embed);
            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Any(x => x.Key == message.Id))
            {
                embed.Description = $"`The specified message doesn't contain any reaction roles.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Reaction Roles")));
                return;
            }

            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Where(x => x.Key == message.Id).ToList())
                ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(b);

            _ = message.DeleteAllReactionsAsync();

            embed.Description = $"`Removed all reaction roles from message sent by` {message.Author.Mention} `in` {message.Channel.Mention} `.`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, "Reaction Roles")));
        });
    }
}