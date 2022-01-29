namespace Project_Ichigo.Commands.User;
internal class UserCommands : BaseCommandModule
{
    [Command("help"),
    CommandModule("user"),
    Description("Displays help")]
    public async Task Help(CommandContext ctx)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                List<KeyValuePair<string, string>> Commands = new();


                Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "user")
                    .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                    $"{x.Value.GenerateUsage()}` - " +
                    $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("User Commands", x)).ToList());

                Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "music")
                    .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                    $"{x.Value.GenerateUsage()}` - " +
                    $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Music Commands", x)).ToList());

                Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "mod")
                    .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                    $"{x.Value.GenerateUsage()}` - " +
                    $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Mod Commands", x)).ToList());

                if (ctx.Member.IsAdmin())
                    Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "admin")
                        .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                        $"{x.Value.GenerateUsage()}` - " +
                        $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Admin Commands", x)).ToList());

                if (ctx.Member.IsMaintenance())
                    Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "maintainence")
                        .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                        $"{x.Value.GenerateUsage()}` - " +
                        $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Maintenance Commands", x)).ToList());

                if (ctx.Member.IsMaintenance())
                    Commands.AddRange(ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).Where(x => x.Value.CustomAttributes.OfType<CommandModuleAttribute>() is not null && x.Value.CustomAttributes.OfType<CommandModuleAttribute>().FirstOrDefault().ModuleString == "hidden")
                        .Select(x => $"`{ctx.Prefix}{x.Value.Name}" +
                        $"{x.Value.GenerateUsage()}` - " +
                        $"_{x.Value.Description}{(x.Value.Aliases.Count > 0 ? $" (Aliases: `{String.Join("`, `", x.Value.Aliases)}`)" : "")}_").Select(x => new KeyValuePair<string, string>("Hidden Commands", x)).ToList());

                var Fields = Commands.PrepareEmbedFields();
                var Embeds = Fields.Select(x => new KeyValuePair<string, string>(x.Key, x.Value
                    .Replace("##Prefix##", ctx.Prefix)
                    .Replace("##n##", "\n")))
                    .ToList().PrepareEmbeds("", "All available commands will be listed below.\nArguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n" +
                                                "**Do not include the brackets when using commands, they're merely an indicator for requirement.**");

                try
                {
                    foreach (var b in Embeds)
                        await ctx.Member.SendMessageAsync(embed: b.WithAuthor(ctx.Guild.Name, "", ctx.Guild.IconUrl).WithFooter($"Command used by {ctx.Member.UsernameWithDiscriminator}").WithTimestamp(DateTime.UtcNow).WithColor(new DiscordColor("#36FFFF")).Build());

                    var successembed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Description = ":mailbox_with_mail: `You got mail! Please check your dm's.`",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                            IconUrl = ctx.Member.AvatarUrl
                        },
                        Timestamp = DateTime.UtcNow,
                        Color = DiscordColor.Green
                    };

                    await ctx.Channel.SendMessageAsync(embed: successembed);
                }
                catch (DisCatSharp.Exceptions.UnauthorizedException)
                {
                    var errorembed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Description = ":x: `It seems i can't dm you. Please make sure you have the server's direct messages on and you don't have me blocked.`",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                            IconUrl = ctx.Member.AvatarUrl
                        },
                        Timestamp = DateTime.UtcNow,
                        Color = DiscordColor.DarkRed,
                        ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                    };

                    if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                        errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                    await ctx.Channel.SendMessageAsync(embed: errorembed);
                }
                catch (Exception ex)
                {
                    LogError($"{ex}");

                    var errorembed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = ctx.Guild.Name,
                            IconUrl = ctx.Guild.IconUrl
                        },
                        Description = ":x: `An unhandled exception occured while trying to send you a direct message. This error has been logged, please try again later.`",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}",
                            IconUrl = ctx.Member.AvatarUrl
                        },
                        Timestamp = DateTime.UtcNow,
                        Color = DiscordColor.DarkRed,
                        ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867133233984569364/1q3uUtPAUU_1.gif"
                    };

                    if (ctx.User.Presence.ClientStatus.Mobile.HasValue)
                        errorembed.ImageUrl = "https://cdn.discordapp.com/attachments/712761268393738301/867143225868681226/1q3uUtPAUU_4.gif";

                    await ctx.Channel.SendMessageAsync(embed: errorembed);
                }
            }
            catch (Exception ex)
            {
                LogError($"{ex}");
            }
        });
    }
}
