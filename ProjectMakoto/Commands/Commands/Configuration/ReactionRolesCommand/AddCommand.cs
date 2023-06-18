// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ReactionRolesCommand;

internal sealed class AddCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            DiscordMessage message;

            var embed = new DiscordEmbedBuilder
            {
                Description = "`Adding reaction role..`"
            }.AsLoading(ctx, "Reaction Roles");

            await RespondOrEdit(embed);

            DiscordRole role_parameter;
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

            if (arguments?.ContainsKey("role_parameter") ?? false)
            {
                role_parameter = (DiscordRole)arguments["role_parameter"];
            }
            else
            {
                switch (ctx.CommandType)
                {
                    case Enums.CommandType.ContextMenu:
                    {
                        embed.Description = $"`Please select the role you want this reaction role to assign below.`";
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsAwaitingInput(ctx, "Reaction Roles")));
                        var RoleResult = await PromptRoleSelection();

                        if (RoleResult.TimedOut)
                        {
                            ModifyToTimedOut();
                            return;
                        }
                        else if (RoleResult.Cancelled)
                        {
                            DeleteOrInvalidate();
                            return;
                        }
                        else if (RoleResult.Failed)
                        {
                            if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                            {
                                await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any roles in your server.`"));
                                await Task.Delay(3000);
                                return;
                            }

                            throw RoleResult.Exception;
                        }

                        role_parameter = RoleResult.Result;
                        break;
                    }
                    default:
                        throw new ArgumentException("Interaction expected");
                }
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
                        embed.Description = $"`Please react with the emoji you want to use to the target message.`";
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsAwaitingInput(ctx, "Reaction Roles")));

                        var emoji_wait = await ctx.Client.GetInteractivity().WaitForReactionAsync(x => x.Channel.Id == ctx.Channel.Id && x.User.Id == ctx.User.Id && x.Message.Id == message.Id, TimeSpan.FromMinutes(2));

                        if (emoji_wait.TimedOut)
                        {
                            ModifyToTimedOut();
                            return;
                        }

                        try
                        { _ = emoji_wait.Result.Message.DeleteReactionAsync(emoji_wait.Result.Emoji, ctx.User); }
                        catch { }

                        emoji_parameter = emoji_wait.Result.Emoji;

                        if (emoji_parameter.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji_parameter.Id))
                        {
                            embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Reaction Roles")));
                            return;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Interaction expected");
                }
            }

            embed.Author.IconUrl = ctx.Guild.IconUrl;

            if (ctx.DbGuild.ReactionRoles.Count > 100)
            {
                embed.Description = $"`You've reached the limit of 100 reaction roles per guild. You cannot add more reaction roles unless you remove one.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Reaction Roles")));
                return;
            }

            if (emoji_parameter.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji_parameter.Id))
            {
                embed.Description = $"`The bot has no access to this emoji. Any emoji of this server and built-in discord emojis should work.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Reaction Roles")));
                return;
            }

            if (ctx.DbGuild.ReactionRoles.Any(x => (x.Key == message.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName())))
            {
                embed.Description = $"`The specified emoji has already been used for a reaction role on the selected message.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Reaction Roles")));
                return;
            }

            if (ctx.DbGuild.ReactionRoles.Any(x => x.Value.RoleId == role_parameter.Id))
            {
                embed.Description = $"`The specified role is already being used in another reaction role.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Reaction Roles")));
                return;
            }

            await message.CreateReactionAsync(emoji_parameter);

            ctx.DbGuild.ReactionRoles.Add(new KeyValuePair<ulong, Entities.ReactionRoleEntry>(message.Id, new Entities.ReactionRoleEntry
            {
                ChannelId = message.Channel.Id,
                RoleId = role_parameter.Id,
                EmojiId = emoji_parameter.Id,
                EmojiName = emoji_parameter.GetUniqueDiscordName()
            }));

            embed.Description = $"`Added role` {role_parameter.Mention} `to message sent by` {message.Author.Mention} `in` {message.Channel.Mention} `with emoji` {emoji_parameter} `.`";
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, "Reaction Roles")));
        });
    }
}