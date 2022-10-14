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

            var ApplicationCommandsList = ctx.Client.GetApplicationCommands().RegisteredCommands.First(x => x.Value?.Count > 0);
            var PrefixCommandsList = ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).ToList();

            if (!command_filter.IsNullOrWhiteSpace())
            {
                if (PrefixCommandsList.Any(x => x.Value.Name.ToLower() == command_filter.ToLower()))
                {
                    var command = PrefixCommandsList.First(x => x.Value.Name.ToLower() == command_filter.ToLower());

                    var desc = $"Arguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n" +
                                            $"**Do not include the brackets when using commands, they're merely an indicator for requirement.**\n\n" +
                                            $"`{ctx.Prefix}{command.Value.GenerateUsage()}` - _{command.Value.Description}{command.Value.Aliases.GenerateAliases()}_\n";

                    try
                    {
                        desc += string.Join("\n", ((CommandGroup)command.Value).Children.Select(x => $"`{ctx.Prefix}{x.Parent.Name} {x.GenerateUsage()}` - _{x.Description}{x.Aliases.GenerateAliases()}_"));
                    }
                    catch { }

                    await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(desc).SetBotInfo(ctx));
                    return;
                }
                else
                {
                    await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`No such command found.`").SetBotError(ctx));
                    return;
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

                if (ctx.CommandType == Enums.CommandType.PrefixCommand)
                {
                    Commands.Add(new KeyValuePair<string, string>($"{module.FirstLetterToUpper()}", $"`{ctx.Prefix}{command.Value.GenerateUsage()}` - _{command.Value.Description}{command.Value.Aliases.GenerateAliases()}_"));
                }
                else
                {
                    try
                    {
                        var cmd = ApplicationCommandsList.Value.First(x => x.Name.ToLower() == command.Key.ToLower());
                        Commands.Add(new KeyValuePair<string, string>($"{module.FirstLetterToUpper()}", $"{cmd.Mention} - _{command.Value.Description}_"));
                    }
                    catch { }
                }
            }

            var Fields = Commands.PrepareEmbedFields();

            Dictionary<string, DiscordEmbedBuilder> discordEmbeds = new();

            foreach (var b in Fields)
            {
                if (!discordEmbeds.ContainsKey(b.Key))
                    discordEmbeds.Add(b.Key, new DiscordEmbedBuilder().WithDescription("Arguments wrapped in `[]` are optional while arguments wrapped in `<>` are required.\n" +
                                            "**Do not include the brackets when using commands, they're merely an indicator for requirement.**").SetBotInfo(ctx));

                if (!discordEmbeds[b.Key].Fields.Any())
                    discordEmbeds[b.Key].AddField(new DiscordEmbedField($"{b.Key.FirstLetterToUpper()} Commands", b.Value));
                else
                    discordEmbeds[b.Key].AddField(new DiscordEmbedField("󠂪 󠂪", b.Value));
            }

            int Page = 0;

            while (true)
            {
                var PreviousButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Previous Page", (Page <= 0), DiscordEmoji.FromUnicode("◀").ToComponent());
                var NextButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Next Page", (Page >= discordEmbeds.Count - 1), DiscordEmoji.FromUnicode("▶").ToComponent());

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(discordEmbeds.ElementAt(Page).Value).AddComponents(PreviousButton, NextButton));

                var Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }

                _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (Menu.Result.GetCustomId() == PreviousButton.CustomId)
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