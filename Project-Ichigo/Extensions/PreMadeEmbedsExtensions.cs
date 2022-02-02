namespace Project_Ichigo.Extensions;

internal static class PreMadeEmbedsExtensions
{
    public static async Task<DiscordMessage> SendMaintenanceError(this CommandContext ctx)
    {
        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = ctx.Guild.IconUrl,
                Name = ctx.Guild.Name
            },
            Title = "",
            Description = "You dont have permissions to use this command. You need to be <@411950662662881290> to use this command.",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{ctx.User.Username}#{ctx.User.Discriminator} attempted to use \"{ctx.Prefix}{ctx.Command.Name}\"",
                IconUrl = ctx.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = new DiscordColor("#ff6666")
        };

        var msg = await ctx.Channel.SendMessageAsync(embed: embed, content: ctx.User.Mention);

        return msg;
    }

    public static async Task<DiscordMessage> SendAdminError(this CommandContext ctx)
    {
        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = ctx.Guild.IconUrl,
                Name = ctx.Guild.Name
            },
            Title = "",
            Description = $"You dont have permissions to use this command. You need to be `Admin` to use this command.",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{ctx.User.Username}#{ctx.User.Discriminator} attempted to use \"{ctx.Prefix}{ctx.Command.Name}\"",
                IconUrl = ctx.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = new DiscordColor("#ff6666")
        };

        var msg = await ctx.Channel.SendMessageAsync(embed: embed, content: ctx.User.Mention);

        return msg;
    }

    public static async Task<DiscordMessage> SendModError(this CommandContext ctx)
    {
        DiscordEmbedBuilder embed = new()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = ctx.Guild.IconUrl,
                Name = ctx.Guild.Name
            },
            Title = "",
            Description = $"You dont have permissions to use this command. You need to be `Mod` to use this command.",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"{ctx.User.Username}#{ctx.User.Discriminator} attempted to use \"{ctx.Prefix}{ctx.Command.Name}\"",
                IconUrl = ctx.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = new DiscordColor("#ff6666")
        };

        var msg = await ctx.Channel.SendMessageAsync(embed: embed, content: ctx.User.Mention);

        return msg;
    }

    public static async Task<DiscordMessage> SendSyntaxError(this CommandContext ctx)
    {
        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = ctx.Guild.IconUrl,
                Name = ctx.Guild.Name
            },
            Title = "",
            Description = $"**`{ctx.Prefix}{ctx.Command.Name}{(ctx.RawArgumentString != "" ? $" {ctx.RawArgumentString.Replace("`", "").Replace("\\", "")}" : "")}` is not a valid way of using this command.**\nUse it like this instead: `{ctx.Prefix}{ctx.Command.Name}{ctx.Command.GenerateUsage()}`\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Command used by {ctx.User.Username}#{ctx.User.Discriminator}",
                IconUrl = ctx.User.AvatarUrl
            },
            Timestamp = DateTime.UtcNow,
            Color = new DiscordColor("#ff6666")
        };

        if (ctx.Client.GetCommandsNext()
            .RegisteredCommands[ ctx.Command.Name ].Overloads
            .First().Arguments
            .First().Type.Name is "DiscordUser" or "DiscordMember")
            embed.Description += "\n\n_Tip: Make sure you copied the user id and not a server, channel or message id._";

        var msg = await ctx.Channel.SendMessageAsync(embed: embed, content: ctx.User.Mention);

        return msg;
    }

    public static string GenerateUsage(this Command cmd)
    {
        string Usage = "";

        if (cmd.Overloads.Count > 0)
        {
            foreach (var b in cmd.Overloads.First().Arguments)
            {
                Usage += $" ";

                if (b.IsOptional)
                    Usage += "[";
                else
                    Usage += "<";

                if (b.Description != null && b.Description != "")
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
}