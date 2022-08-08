namespace ProjectIchigo.Extensions;

internal static class PreMadeEmbedsExtensions
{
    public static DiscordEmbedBuilder SetLoading(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(ctx, CustomText);
        b.Author.IconUrl = Resources.StatusIndicators.Loading;

        b.Color = EmbedColors.Processing;
        b.Footer = ctx.GenerateUsedByFooter();
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    public static DiscordEmbedBuilder SetInfo(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(ctx, CustomText);

        b.Color = EmbedColors.Info;
        b.Footer = ctx.GenerateUsedByFooter();
        b.Timestamp = DateTime.UtcNow;

        return b;
    }
    
    public static DiscordEmbedBuilder SetAwaitingInput(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(ctx, CustomText);

        b.Color = EmbedColors.AwaitingInput;
        b.Footer = ctx.GenerateUsedByFooter();
        b.Timestamp = DateTime.UtcNow;

        return b;
    }
    
    public static DiscordEmbedBuilder SetError(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(ctx, CustomText);
        b.Author.IconUrl = Resources.StatusIndicators.Error;

        b.Color = EmbedColors.Error;
        b.Footer = ctx.GenerateUsedByFooter();
        b.Timestamp = DateTime.UtcNow;

        return b;
    }
    
    public static DiscordEmbedBuilder SetWarning(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(ctx, CustomText);
        b.Author.IconUrl = Resources.StatusIndicators.Warning;

        b.Color = EmbedColors.Warning;
        b.Footer = ctx.GenerateUsedByFooter();
        b.Timestamp = DateTime.UtcNow;

        return b;
    }
    
    public static DiscordEmbedBuilder SetSuccess(this DiscordEmbedBuilder b, SharedCommandContext ctx, string CustomText = "", string CustomFooterText = "")
    {
        b.Author = MakeDefaultAuthor(ctx, CustomText);
        b.Author.IconUrl = Resources.StatusIndicators.Success;

        b.Color = EmbedColors.Success;
        b.Footer = ctx.GenerateUsedByFooter();
        b.Timestamp = DateTime.UtcNow;

        return b;
    }

    private static DiscordEmbedBuilder.EmbedAuthor MakeDefaultAuthor(SharedCommandContext ctx, string CustomText = "") => new()
    {
        Name = $"{(CustomText.IsNullOrWhiteSpace() ? "" : $"{CustomText} • ")}{ctx.Guild.Name}",
        IconUrl = (ctx.Guild.IconHash.IsNullOrWhiteSpace() ? Resources.AuditLogIcons.QuestionMark : ctx.Guild.IconUrl)
    };

    public static DiscordEmbedBuilder.EmbedFooter GenerateUsedByFooter(this SharedCommandContext ctx, string addText = "", string customIcon = "")
    {
        return new DiscordEmbedBuilder.EmbedFooter
        {
            IconUrl = (!customIcon.IsNullOrWhiteSpace() ? customIcon : ctx.User.AvatarUrl),
            Text = $"Command used by {ctx.User.UsernameWithDiscriminator}{(string.IsNullOrEmpty(addText) ? "" : $" • {addText}")}"
        };
    }
    
    public static DiscordEmbedBuilder.EmbedFooter GenerateUsedByFooter(this CommandContext ctx, string addText = "")
    {
        return new DiscordEmbedBuilder.EmbedFooter
        {
            IconUrl = ctx.User.AvatarUrl,
            Text = $"Command used by {ctx.User.UsernameWithDiscriminator}{(string.IsNullOrEmpty(addText) ? "" : $" • {addText}")}"
        };
    }

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
            Description = $"**`{ctx.Prefix}{ctx.Command.Name}{CustomArguments}{(ctx.RawArgumentString != "" ? $" {ctx.RawArgumentString.SanitizeForCodeBlock().Replace("\\", "")}" : "")}` is not a valid way of using this command.**\nUse it like this instead: `{ctx.Prefix}{ctx.Command.GenerateUsage()}`\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
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
        return Usage;
    }

    public static string GenerateAliases(this IReadOnlyList<string> aliases) => $"{(aliases.Count > 0 ? $" (Aliases: `{string.Join("`, `", aliases)}`)" : "")}";
}