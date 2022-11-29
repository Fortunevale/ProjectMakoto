using CommandType = ProjectIchigo.Enums.CommandType;

namespace ProjectIchigo.Commands;

internal class HelpCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var command_filter = (string)arguments["command"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            List<KeyValuePair<string, string>> Commands = new();

            var ApplicationCommandsList = ctx.Client.GetApplicationCommands().RegisteredCommands.First(x => x.Value?.Count > 0).Value;
            var PrefixCommandsList = ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).ToList();

            if (!command_filter.IsNullOrWhiteSpace())
            {
                switch (ctx.CommandType)
                {
                    case CommandType.ApplicationCommand:
                        if (ApplicationCommandsList.Any(x => x.Name.ToLower() == command_filter.ToLower() || (x.NameLocalizations?.Localizations?.Any(x => x.Value.ToLower() == command_filter.ToLower()) ?? false)))
                        {
                            var command = ApplicationCommandsList.First(x => x.Name.ToLower() == command_filter.ToLower() || (x.NameLocalizations?.Localizations?.Any(x => x.Value.ToLower() == command_filter.ToLower()) ?? false));

                            string commandName = command.NameLocalizations?.Localizations?.TryGetValue(ctx.User.Locale, out var localizedName) ?? false ? localizedName : command.Name;
                            var commandDescription = command.DescriptionLocalizations?.Localizations?.TryGetValue(ctx.User.Locale, out var localizedDescription) ?? false ? localizedDescription : command.Description;

                            var descBuilder = $"{GetString(t.commands.help.disclaimer)}\n\n" +
                                       $"`{ctx.Prefix}{command.GenerateUsage(ctx.User.Locale)}` - _{commandDescription}_\n";

                            if (command.Options.Any(x => x.Type == ApplicationCommandOptionType.SubCommand))
                            {
                                foreach (var b in command.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand))
                                {
                                    var optionDescription = b.DescriptionLocalizations?.Localizations?.TryGetValue(ctx.User.Locale, out var localizedOption) ?? false ? localizedOption : b.Description;

                                    descBuilder += $"`{ctx.Prefix}{commandName} {b.GenerateUsage(ctx.User.Locale)}` - _{optionDescription}_\n";
                                }
                            }

                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(descBuilder).AsBotInfo(ctx));
                            return;
                        }
                        else
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.commands.help.nocmd)}`").AsBotError(ctx));
                            return;
                        }
                    case CommandType.PrefixCommand:
                        if (PrefixCommandsList.Any(x => x.Value.Name.ToLower() == command_filter.ToLower()))
                        {
                            var command = PrefixCommandsList.First(x => x.Value.Name.ToLower() == command_filter.ToLower());

                            var desc = $"{GetString(t.commands.help.disclaimer)}\n\n" +
                                        $"`{ctx.Prefix}{command.Value.GenerateUsage()}` - _{command.Value.Description}{command.Value.Aliases.GenerateAliases()}_\n";

                            try
                            {
                                desc += string.Join("\n", ((CommandGroup)command.Value).Children.Select(x => $"`{ctx.Prefix}{x.Parent.Name} {x.GenerateUsage()}` - _{x.Description}{x.Aliases.GenerateAliases()}_"));
                            }
                            catch { }

                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(desc).AsBotInfo(ctx));
                            return;
                        }
                        else
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.commands.help.nocmd)}`").AsBotError(ctx));
                            return;
                        }
                }
            }

            foreach (var command in PrefixCommandsList)
            {
                if (command.Value.CustomAttributes.OfType<CommandModuleAttribute>() is null)
                    continue;

                var module = command.Value.CustomAttributes.OfType<CommandModuleAttribute>()?.FirstOrDefault()?.ModuleString ?? "Unknown";

                switch (module)
                {
                    case "configuration":
                        if (!ctx.Member.IsAdmin(ctx.Bot.status))
                            continue;
                        break;
                    case "maintenance":
                        if (!ctx.Member.IsMaintenance(ctx.Bot.status))
                            continue;
                        break;
                    case "hidden":
                        continue;
                    default:
                        break;
                }

                try
                {
                    var cmd = ApplicationCommandsList.First(x => x.Name.ToLower() == command.Key.ToLower());
                    var commandDescription = cmd.DescriptionLocalizations?.Localizations?.TryGetValue(ctx.User.Locale, out var localizedDescription) ?? false ? localizedDescription : cmd.Description;
                    var commandModuleName = module.ToLower() switch
                    {
                        "utility" => GetString(t.commands.modules.utility),
                        "social" => GetString(t.commands.modules.social),
                        "music" => GetString(t.commands.modules.music),
                        "moderation" => GetString(t.commands.modules.moderation),
                        "configuration" => GetString(t.commands.modules.configuration),
                        _ => module.FirstLetterToUpper(),
                    };
                    Commands.Add(new KeyValuePair<string, string>($"{commandModuleName}", $"{cmd.Mention} - _{commandDescription}_"));
                }
                catch { }
            }

            var Fields = Commands.PrepareEmbedFields();

            Dictionary<string, DiscordEmbedBuilder> discordEmbeds = new();

            foreach (var b in Fields)
            {
                if (!discordEmbeds.ContainsKey(b.Key))
                    discordEmbeds.Add(b.Key, new DiscordEmbedBuilder().WithDescription(GetString(t.commands.help.disclaimer)).AsBotInfo(ctx));

                if (!discordEmbeds[b.Key].Fields.Any())
                    discordEmbeds[b.Key].AddField(new DiscordEmbedField(GetString(t.commands.help.module).Replace("{Module}", b.Key), b.Value));
                else
                    discordEmbeds[b.Key].AddField(new DiscordEmbedField("󠂪 󠂪", b.Value));
            }

            int Page = 0;

            while (true)
            {
                var PreviousButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.common.previous_page), (Page <= 0), DiscordEmoji.FromUnicode("◀").ToComponent());
                var NextButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(t.common.next_page), (Page >= discordEmbeds.Count - 1), DiscordEmoji.FromUnicode("▶").ToComponent());

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(discordEmbeds.ElementAt(Page).Value).AddComponents(PreviousButton, NextButton));

                var Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu.GetCustomId() == PreviousButton.CustomId)
                {
                    Page--;
                    continue;
                }
                else
                {
                    Page++;
                    continue;
                }
            }
        });
    }
}