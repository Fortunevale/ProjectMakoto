// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

internal static class PreMadeEmbedsExtensions
{
    public static DiscordEmbedBuilder AsLoading(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsLoading(b, ctx.Guild, ctx.User, ctx.DbUser, CustomText, CustomFooterText);
    
    public static DiscordEmbedBuilder AsLoading(this DiscordEmbedBuilder b, DiscordGuild guild, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(guild, CustomText);
        b.Author.IconUrl = StatusIndicatorIcons.Loading;

        b.Color = EmbedColors.Processing;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsBotLoading(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsBotLoading(b, ctx.Client, ctx.User, ctx.DbUser, CustomText, CustomFooterText);
    
    public static DiscordEmbedBuilder AsBotLoading(this DiscordEmbedBuilder b, DiscordClient client, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultBotAuthor(client, CustomText);
        b.Author.IconUrl = StatusIndicatorIcons.Loading;

        b.Color = EmbedColors.Processing;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsInfo(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsInfo(b, ctx.Guild, ctx.User, ctx.DbUser, CustomText, CustomFooterText);
    
    public static DiscordEmbedBuilder AsInfo(this DiscordEmbedBuilder b, DiscordGuild guild, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(guild, CustomText);

        b.Color = EmbedColors.Info;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsBotInfo(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsBotInfo(b, ctx.Client, ctx.User, ctx.DbUser, CustomText, CustomFooterText);

    public static DiscordEmbedBuilder AsBotInfo(this DiscordEmbedBuilder b, DiscordClient client, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultBotAuthor(client, CustomText);

        b.Color = EmbedColors.Info;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsAwaitingInput(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsAwaitingInput(b, ctx.Guild, ctx.User, ctx.DbUser, CustomText, CustomFooterText);
    
    public static DiscordEmbedBuilder AsAwaitingInput(this DiscordEmbedBuilder b, DiscordGuild guild, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(guild, CustomText);

        b.Color = EmbedColors.AwaitingInput;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsBotAwaitingInput(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsBotAwaitingInput(b, ctx.Client, ctx.User, ctx.DbUser, CustomText, CustomFooterText);
    
    public static DiscordEmbedBuilder AsBotAwaitingInput(this DiscordEmbedBuilder b, DiscordClient client, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultBotAuthor(client, CustomText);

        b.Color = EmbedColors.AwaitingInput;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsError(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsError(b, ctx.Guild, ctx.User, ctx.DbUser, CustomText, CustomFooterText);
    
    public static DiscordEmbedBuilder AsError(this DiscordEmbedBuilder b, DiscordGuild guild, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(guild, CustomText);
        b.Author.IconUrl = StatusIndicatorIcons.Error;

        b.Color = EmbedColors.Error;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsBotError(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsBotError(b, ctx.Client, ctx.User, ctx.DbUser, CustomText, CustomFooterText);
    
    public static DiscordEmbedBuilder AsBotError(this DiscordEmbedBuilder b, DiscordClient client, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultBotAuthor(client, CustomText);
        b.Author.IconUrl = StatusIndicatorIcons.Error;

        b.Color = EmbedColors.Error;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsWarning(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsWarning(b, ctx.Guild, ctx.User, ctx.DbUser, CustomText, CustomFooterText);

    public static DiscordEmbedBuilder AsWarning(this DiscordEmbedBuilder b, DiscordGuild guild, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(guild, CustomText);
        b.Author.IconUrl = StatusIndicatorIcons.Warning;

        b.Color = EmbedColors.Warning;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsBotWarning(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
    => AsBotWarning(b, ctx.Client, ctx.User, ctx.DbUser, CustomText, CustomFooterText);

    public static DiscordEmbedBuilder AsBotWarning(this DiscordEmbedBuilder b, DiscordClient client, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultBotAuthor(client, CustomText);
        b.Author.IconUrl = StatusIndicatorIcons.Warning;

        b.Color = EmbedColors.Warning;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsSuccess(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsSuccess(b, ctx.Guild, ctx.User, ctx.DbUser, CustomText, CustomFooterText);
    
    public static DiscordEmbedBuilder AsSuccess(this DiscordEmbedBuilder b, DiscordGuild guild, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(guild, CustomText);
        b.Author.IconUrl = StatusIndicatorIcons.Success;

        b.Color = EmbedColors.Success;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder AsBotSuccess(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
        => AsBotSuccess(b, ctx.Client, ctx.User, ctx.DbUser, CustomText, CustomFooterText);

    public static DiscordEmbedBuilder AsBotSuccess(this DiscordEmbedBuilder b, DiscordClient client, DiscordUser user, User dbUser, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultBotAuthor(client, CustomText);
        b.Author.IconUrl = StatusIndicatorIcons.Success;

        b.Color = EmbedColors.Success;
        b.Footer = user.GenerateUsedByFooter(dbUser, CustomFooterText);
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    private static DiscordEmbedBuilder.EmbedAuthor MakeDefaultAuthor(DiscordGuild guild, string CustomText = "") => new()
    {
        Name = $"{(CustomText.IsNullOrWhiteSpace() ? "" : $"{CustomText} • ")}{guild.Name}",
        IconUrl = (guild.IconHash.IsNullOrWhiteSpace() ? AuditLogIcons.QuestionMark : guild.IconUrl)
    };

    private static DiscordEmbedBuilder.EmbedAuthor MakeDefaultBotAuthor(DiscordClient client, string CustomText = "") => new()
    {
        Name = client.CurrentUser.Username,
        IconUrl = client.CurrentUser.AvatarUrl
    };

    public static DiscordEmbedBuilder.EmbedFooter GenerateUsedByFooter(this SharedCommandContext ctx, string addText = "", string customIcon = "")
        => ctx.User.GenerateUsedByFooter(ctx.DbUser, addText, customIcon);

    public static DiscordEmbedBuilder.EmbedFooter GenerateUsedByFooter(this DiscordUser user, User dbUser, string addText = "", string customIcon = "") 
        => new()
        {
            IconUrl = (!customIcon.IsNullOrWhiteSpace() ? customIcon : user.AvatarUrl),
            Text = $"{Bot.loadedTranslations.Commands.Common.UsedByFooter.Get(dbUser).Build(new TVar("User", user.GetUsername()))}{(string.IsNullOrEmpty(addText) ? "" : $" • {addText}")}"
        };

    public static DiscordEmbedBuilder.EmbedFooter GenerateUsedByFooter(this CommandContext ctx, string addText = "", string customIcon = "")
        => new()
        {
            IconUrl = (!customIcon.IsNullOrWhiteSpace() ? customIcon : ctx.User.AvatarUrl),
            Text = $"{Bot.loadedTranslations.Commands.Common.UsedByFooter.Get(ctx.User).Build(new TVar("User", ctx.User.GetUsername()))}{(string.IsNullOrEmpty(addText) ? "" : $" • {addText}")}"
        };

    public static async Task<DiscordMessage> SendCommandGroupHelp(this IReadOnlyList<Command> cmds, CommandContext ctx, string CustomText = "", string CustomImageUrl = "", string CustomParentName = "")
    {
        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = ctx.Guild.IconUrl,
                Name = $"{(CustomParentName.IsNullOrWhiteSpace() ? cmds[0].Parent.Name.FirstLetterToUpper() : CustomParentName)} Command Help • {ctx.Guild.Name}"
            },
            Description = $"{string.Join("\n", cmds.Select(x => $"`{ctx.Prefix}{x.Parent.Name} {x.GenerateUsage()}` - _{x.Description}{x.Aliases.GenerateAliases()}_"))}\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
            Footer = ctx.GenerateUsedByFooter(),
            Timestamp = DateTime.UtcNow,
            Color = EmbedColors.Info
        };

        if (!string.IsNullOrWhiteSpace(CustomText))
            embed.Description += CustomText;
        
        if (!string.IsNullOrWhiteSpace(CustomImageUrl))
            embed.ImageUrl += CustomImageUrl;

        var msg = await ctx.Channel.SendMessageAsync(embed: embed, content: ctx.User.Mention);

        return msg;
    }

    public static async Task<DiscordMessage> SendSyntaxError(this CommandContext ctx, string CustomArguments = "")
    {
        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = ctx.Guild.IconUrl,
                Name = ctx.Guild.Name
            },
            Title = "",
            Description = $"**`{ctx.Prefix}{ctx.Command.Name}{CustomArguments}{(ctx.RawArgumentString != "" ? $" {ctx.RawArgumentString.SanitizeForCode().Replace("\\", "")}" : "")}` is not a valid way of using this command.**\nUse it like this instead: `{ctx.Prefix}{ctx.Command.GenerateUsage()}`\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
            Footer = ctx.GenerateUsedByFooter(),
            Timestamp = DateTime.UtcNow,
            Color = EmbedColors.Error
        };

        if (ctx.Client.GetCommandsNext()
            .RegisteredCommands[ ctx.Command.Name ].Overloads[0].Arguments[0].Type.Name is "DiscordUser" or "DiscordMember")
            embed.Description += "\n\n_Tip: Make sure you copied the user id and not a server, channel or message id._";

        var msg = await ctx.Channel.SendMessageAsync(embed: embed, content: ctx.User.Mention);

        return msg;
    }

    public static string GenerateUsage(this Command cmd)
    {
        string Usage = cmd.Name;

        if (cmd.Overloads.Count > 0)
        {
            foreach (var b in cmd.Overloads[0].Arguments)
            {
                Usage += $" ";

                if (b.IsOptional)
                    Usage += "[";
                else
                    Usage += "<";

                if (b.Description is not null and not "")
                    Usage += b.Description;
                else
                    Usage += b.Type.Name;

                if (b.IsOptional)
                    Usage += "]";
                else
                    Usage += ">";
            }

            Usage = Usage.Replace("DiscordUser", "@User")
                         .Replace("DiscordMember", "@Member")
                         .Replace("DiscordChannel", "#Channel")
                         .Replace("DiscordRole", "@Role")
                         .Replace("Boolean", "true/false")
                         .Replace("Int32", "Number")
                         .Replace("Int64", "Number")
                         .Replace("String", "Text");
        }
        return Usage.SanitizeForCode();
    }

    public static string GenerateUsage(this DiscordApplicationCommand cmd, string? locale = null)
    {
        string Usage = cmd.NameLocalizations?.Localizations?.TryGetValue(locale ?? "-", out var localizedName) ?? false ? localizedName : cmd.Name;

        if (cmd.Options.Count > 0)
        {
            foreach (var b in cmd.Options.Where(x => x.Type is not ApplicationCommandOptionType.SubCommand and not ApplicationCommandOptionType.SubCommandGroup))
            {
                var localizedUsage = b.NameLocalizations?.Localizations?.TryGetValue(locale ?? "-", out var localizedOption) ?? false ? localizedOption : b.Name;

                Usage += " ";

                if (!b.Required)
                    Usage += $"[{localizedUsage}]";
                else
                    Usage += $"<{localizedUsage}>";
            }
        }
        return Usage.SanitizeForCode();
    }
    
    public static string GenerateUsage(this DiscordApplicationCommandOption cmd, string? locale = null)
    {
        string Usage = cmd.NameLocalizations?.Localizations?.TryGetValue(locale ?? "-", out var localizedName) ?? false ? localizedName : cmd.Name;

        if (cmd.Options?.Count > 0)
        {
            foreach (var b in cmd.Options.Where(x => x.Type is not ApplicationCommandOptionType.SubCommand and not ApplicationCommandOptionType.SubCommandGroup))
            {
                var localizedUsage = b.NameLocalizations?.Localizations?.TryGetValue(locale ?? "-", out var localizedOption) ?? false ? localizedOption : b.Name;

                Usage += " ";

                if (!b.Required)
                    Usage += $"[{localizedUsage}]";
                else
                    Usage += $"<{localizedUsage}>";
            }
        }
        return Usage.SanitizeForCode();
    }

    public static string GenerateAliases(this IReadOnlyList<string> aliases) => $"{(aliases.Count > 0 ? $" (Aliases: `{string.Join("`, `", aliases)}`)" : "")}";
}