// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ReactionRolesCommand;

internal sealed class RemoveCommand : BaseCommand
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

            var embed = new DiscordEmbedBuilder
            {
                Description = GetString(CommandKey.RemovingReactionRole, true)
            }.AsLoading(ctx, GetString(CommandKey.Title));

            await RespondOrEdit(embed);

            DiscordEmoji emoji_parameter;

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

            if (arguments?.ContainsKey("emoji_parameter") ?? false)
            {
                emoji_parameter = (DiscordEmoji)arguments["emoji_parameter"];
            }
            else
            {
                switch (ctx.CommandType)
                {
                    case Enums.CommandType.ContextMenu:
                    {
                        embed.Description = GetString(CommandKey.ReactWithEmojiToRemove, true);
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsAwaitingInput(ctx, GetString(CommandKey.Title))));

                        var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == message.Id, TimeSpan.FromMinutes(2));

                        if (emoji_wait.TimedOut)
                        {
                            ModifyToTimedOut();
                            return;
                        }

                        emoji_parameter = emoji_wait.Result.Emoji;
                        break;
                    }
                    default:
                        throw new ArgumentException("Interaction expected");
                }
            }

            if (!ctx.DbGuild.ReactionRoles.Any(x => x.Key == message.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName()))
            {
                embed.Description = GetString(CommandKey.NoReactionRoleFound);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, GetString(CommandKey.Title))));
                return;
            }

            var obj = ctx.DbGuild.ReactionRoles.First(x => x.Key == message.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName());

            var role = ctx.Guild.GetRole(obj.Value.RoleId);
            var channel = ctx.Guild.GetChannel(obj.Value.ChannelId);
            var reactionMessage = await channel.GetMessageAsync(obj.Key);
            _ = reactionMessage.DeleteReactionsEmojiAsync(obj.Value.GetEmoji(ctx.Client));

            ctx.DbGuild.ReactionRoles.Remove(obj);

            embed.Description = GetString(CommandKey.RemovedReactionRole, true,
                new TVar("Role", role.Mention),
                new TVar("User", reactionMessage?.Author.Mention ?? "`/`"),
                new TVar("Channel", reactionMessage?.Channel.Mention ?? "`/`"),
                new TVar("Emoji", obj.Value.GetEmoji(ctx.Client)));
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, GetString(CommandKey.Title))));
        });
    }
}