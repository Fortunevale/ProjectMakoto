// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ReactionRolesCommand;

internal sealed class RemoveAllCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.ReactionRoles;

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
                            this.SendSyntaxError();
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
                this.SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = this.GetString(CommandKey.RemovingAllReactionRoles)
            }.AsLoading(ctx, this.GetString(CommandKey.Title));

            _ = await this.RespondOrEdit(embed);
            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (!ctx.DbGuild.ReactionRoles.Any(x => x.MessageId == message.Id))
            {
                embed.Description = this.GetString(CommandKey.NoReactionRoles, true);
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, this.GetString(CommandKey.Title))));
                return;
            }

            foreach (var b in ctx.DbGuild.ReactionRoles.Where(x => x.MessageId == message.Id).ToList())
                ctx.DbGuild.ReactionRoles = ctx.DbGuild.ReactionRoles.Remove(x => x.MessageId.ToString(), b);

            _ = message.DeleteAllReactionsAsync();

            embed.Description = this.GetString(CommandKey.RemovedAllReactionRoles, true,
                new TVar("User", message?.Author.Mention ?? "`/`"),
                new TVar("Channel", message?.Channel.Mention ?? "`/`"));
            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, this.GetString(CommandKey.Title))));
        });
    }
}