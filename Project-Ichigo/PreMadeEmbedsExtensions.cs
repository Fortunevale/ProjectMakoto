namespace Project_Ichigo;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

internal static class PreMadeEmbedsExtensions
{
    public static DiscordEmbedBuilder GenerateMaintenanceError(this CommandContext ctx)
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

        return embed;
    }

    public static DiscordEmbedBuilder GenerateAdminError(this CommandContext ctx)
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

        return embed;
    }

    public static DiscordEmbedBuilder GenerateModError(this CommandContext ctx)
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

        return embed;
    }

    public static DiscordEmbedBuilder CreateSyntaxError(this CommandContext ctx)
    {
        if (ctx.Client.GetCommandsNext().RegisteredCommands.ContainsKey(ctx.Command.Name) && ctx.Client.GetCommandsNext().RegisteredCommands.FirstOrDefault(x => x.Key == ctx.Command.Name).Value.CustomAttributes.OfType<CommandUsageAttribute>().FirstOrDefault() is not null)
        {
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = ctx.Guild.IconUrl,
                    Name = ctx.Guild.Name
                },
                Title = "",
                Description = $"**`{ctx.Prefix}{ctx.Command.Name}{(ctx.RawArgumentString != "" ? $" {ctx.RawArgumentString.Replace("`", "").Replace("\\", "")}" : "")}` is not a valid way of using this command.**\nUse it like this instead: `{ctx.Prefix}{ctx.Command.Name} {ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].CustomAttributes.OfType<CommandUsageAttribute>().FirstOrDefault().UsageString}`\n\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n**Do not include the brackets when using commands, they're merely an indicator for requirement.**",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.User.Username}#{ctx.User.Discriminator}",
                    IconUrl = ctx.User.AvatarUrl
                },
                Timestamp = DateTime.UtcNow,
                Color = new DiscordColor("#ff6666")
            };

            if (ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].CustomAttributes.OfType<CommandUsageAttribute>().FirstOrDefault().UsageString.Contains("@User"))
                embed.Description += "\n\n_Tip: You might've accidentally copied a message id or channel id instead of the user id._";

            try
            {
                return embed;

                //string RoleRequired = ctx.Client.GetCommandsNext().RegisteredCommands[ctx.Command.Name].CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString;

                //if (RoleRequired == "user" || RoleRequired == "music")
                //{
                //    return embed;
                //}
                //if (RoleRequired == "mod")
                //{
                //    if (ctx.Guild.Members[ctx.User.Id].IsMod())
                //    {
                //        return embed;
                //    }
                //    else
                //    {
                //        return GenerateModError(ctx);
                //    }
                //}
                //else if (RoleRequired == "admin")
                //{
                //    if (ctx.Guild.Members[ctx.User.Id].IsAdmin())
                //    {
                //        return embed;
                //    }
                //    else
                //    {
                //        return GenerateAdminError(ctx);
                //    }
                //}
                //else if (RoleRequired == "maintainence")
                //{
                //    if (ctx.Guild.Members[ ctx.User.Id ].IsMaintenance())
                //    {
                //        return embed;
                //    }
                //    else
                //    {
                //        return GenerateMaintenanceError(ctx);
                //    }
                //}
                //else
                //{
                //    LogWarn($"Specified Command Module wasn't found: {RoleRequired}");
                //    return embed;
                //}
            }
            catch (Exception ex)
            {
                LogError($"Failed to check roles of user {ctx.User.Username}#{ctx.User.Discriminator}: {ex}");
                return embed;
            }
        }
        else
        {
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = ctx.Guild.IconUrl,
                    Name = ctx.Guild.Name
                },
                Title = "",
                Description = $"You didn't use the command right. However, there was no command syntax available.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Command used by {ctx.User.Username}#{ctx.User.Discriminator}",
                    IconUrl = ctx.User.AvatarUrl
                },
                Timestamp = DateTime.UtcNow,
                Color = new DiscordColor("#ff6666")
            };

            return embed;
        }
    }
}