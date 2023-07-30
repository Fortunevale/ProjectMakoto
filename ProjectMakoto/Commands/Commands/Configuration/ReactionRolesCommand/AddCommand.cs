// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Commands.ReactionRolesCommand;

internal sealed class AddCommand : BaseCommand
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
                Description = GetString(CommandKey.AddingReactionRole, true)
            }.AsLoading(ctx, GetString(CommandKey.Title));

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
                        embed.Description = GetString(CommandKey.SelectRolePrompt, true);
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsAwaitingInput(ctx, GetString(CommandKey.Title))));
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
                                await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(CommandKey.NoRoles, true)));
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
                        embed.Description = GetString(CommandKey.ReactWithEmoji, true);
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsAwaitingInput(ctx, GetString(CommandKey.Title))));

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
                            embed.Description = GetString(CommandKey.NoAccessToEmoji);
                            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, GetString(CommandKey.Title))));
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
                embed.Description = GetString(CommandKey.ReactionRoleLimitReached, true);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, GetString(CommandKey.Title))));
                return;
            }

            if (emoji_parameter.Id != 0 && !ctx.Guild.Emojis.ContainsKey(emoji_parameter.Id))
            {
                embed.Description = GetString(CommandKey.NoAccessToEmoji, true);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, GetString(CommandKey.Title))));
                return;
            }

            if (ctx.DbGuild.ReactionRoles.Any(x => (x.Key == message.Id && x.Value.EmojiName == emoji_parameter.GetUniqueDiscordName())))
            {
                embed.Description = GetString(CommandKey.EmojiAlreadyUsed, true);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, GetString(CommandKey.Title))));
                return;
            }

            if (ctx.DbGuild.ReactionRoles.Any(x => x.Value.RoleId == role_parameter.Id))
            {
                embed.Description = GetString(CommandKey.RoleAlreadyUsed, true);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, GetString(CommandKey.Title))));
                return;
            }

            await message.CreateReactionAsync(emoji_parameter);

            ctx.DbGuild.ReactionRoles.Add(new KeyValuePair<ulong, ReactionRoleEntry>(message.Id, new()
            {
                ChannelId = message.Channel.Id,
                RoleId = role_parameter.Id,
                EmojiId = emoji_parameter.Id,
                EmojiName = emoji_parameter.GetUniqueDiscordName()
            }));

            embed.Description = GetString(CommandKey.AddedReactionRole, true, 
                new TVar("Role", role_parameter.Mention),
                new TVar("User", message.Author.Mention),
                new TVar("Channel", message.Channel.Mention),
                new TVar("Emoji", emoji_parameter));
            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, GetString(CommandKey.Title))));
        });
    }
}