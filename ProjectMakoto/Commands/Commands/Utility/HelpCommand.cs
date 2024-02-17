// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class HelpCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var command_filter = (string)arguments["command"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            List<KeyValuePair<string, string>> Commands = new();
            var PrefixCommandsList = ctx.Client.GetCommandsNext().RegisteredCommands.GroupBy(x => x.Value.Name).Select(x => x.First()).ToList();

            var ApplicationCommandsList = ctx.Client.GetApplicationCommands().RegisteredCommands.First(x => x.Value?.Count > 0).Value;

            foreach (var appCommand in ApplicationCommandsList
                .OrderByDescending(x => x.ContainingType?.GetCustomAttribute<ModulePriorityAttribute>()?.Priority ?? 0))
            {
                try
                {
                    var module = appCommand.ContainingType.Name.Replace("AppCommands", "").ToLower();

                    if (appCommand.ContainingType.Namespace != "ProjectMakoto.ApplicationCommands")
                        module = ctx.Bot.Plugins[ctx.Bot.PluginCommands.First(plugin => plugin.Value.Any(cmd => cmd.Name == appCommand.Name)).Key].Name;

                    switch (module)
                    {
                        case "configuration":
                            if (!ctx.Member.IsAdmin(ctx.Bot.status))
                                continue;
                            break;
                        case "maintenance":
                            if (!ctx.User.IsMaintenance(ctx.Bot.status))
                                continue;
                            break;
                        case "hidden":
                            continue;
                        default:
                            break;
                    }

                    try
                    {
                        var commandKey = this.t.CommandList.FirstOrDefault(localized => localized.Names.Any(x => x.Value == appCommand.Name), null);

                        string commandName;
                        string commandDescription;
                        string commandUsage;

                        if (commandKey is not null)
                        {
                            commandName = this.GetString(commandKey.Names);
                            commandDescription = this.GetString(commandKey.Descriptions);
                            commandUsage = string.Join(" ", commandKey.Options?.Select(x => $"<{this.GetString(x.Names).FirstLetterToUpper()}>") ?? new List<string>());
                        }
                        else
                        {
                            commandName = appCommand.Name;
                            commandDescription = appCommand.Description;
                            commandUsage = string.Join(" ", appCommand.Options?.Select(x => $"<{x.Name.FirstLetterToUpper()}>") ?? new List<string>());
                        }

                        if (command_filter is not null)
                            if (!(commandKey?.Names.Any(x => x.Value.Contains(command_filter, StringComparison.InvariantCultureIgnoreCase)) ?? false) && !commandName.Contains(command_filter, StringComparison.InvariantCultureIgnoreCase))
                                continue;

                        string commandMention;

                        if (appCommand.Options?.Any(x => x.Type == ApplicationCommandOptionType.SubCommand) ?? false)
                            commandMention = $"`/{commandName}`";
                        else commandMention = appCommand.Type != ApplicationCommandType.ChatInput ? $"`{commandName}`" : appCommand.Mention;

                        Command? prefixCommand;

                        if (PrefixCommandsList.Any(x => x.Value.Name.Equals(appCommand.Name, StringComparison.CurrentCultureIgnoreCase)))
                            prefixCommand = PrefixCommandsList.First(x => x.Value.Name.Equals(appCommand.Name, StringComparison.CurrentCultureIgnoreCase)).Value;
                        else prefixCommand = appCommand.CustomAttributes.Any(x => x is PrefixCommandAlternativeAttribute)
                            ? PrefixCommandsList
                                .First(x => x.Value.Name.ToLower() == ((PrefixCommandAlternativeAttribute)appCommand.CustomAttributes
                                    .First(x => x is PrefixCommandAlternativeAttribute)).PrefixCommand.ToLower().TruncateAt(' ')).Value
                            : null;

                        var commandModuleName = module.ToLower() switch
                        {
                            "utility" => this.GetString(this.t.Commands.ModuleNames.Utility),
                            "social" => this.GetString(this.t.Commands.ModuleNames.Social),
                            "music" => this.GetString(this.t.Commands.ModuleNames.Music),
                            "moderation" => this.GetString(this.t.Commands.ModuleNames.Moderation),
                            "configuration" => this.GetString(this.t.Commands.ModuleNames.Configuration),
                            _ => module.FirstLetterToUpper(),
                        };
                        
                        var TypeEmoji = appCommand.Type switch
                        {
                            ApplicationCommandType.ChatInput => EmojiTemplates.GetSlashCommand(ctx.Bot),
                            ApplicationCommandType.Message => EmojiTemplates.GetMessageCommand(ctx.Bot),
                            ApplicationCommandType.User => EmojiTemplates.GetUserCommand(ctx.Bot),
                            _ => throw new NotImplementedException(),
                        };

                        Commands.Add(new KeyValuePair<string, string>($"{commandModuleName}",
                            $"{TypeEmoji}{((prefixCommand is null) ? EmojiTemplates.GetPrefixCommandDisabled(ctx.Bot) : EmojiTemplates.GetPrefixCommandEnabled(ctx.Bot))} {commandMention}{(commandUsage.IsNullOrWhiteSpace() ? "" : $"`{commandUsage}`")}{(commandDescription.IsNullOrWhiteSpace() ? "" : $" - _{commandDescription}_")}"));
                    
                        foreach (var subCmd in appCommand.Options?.Where(x => x.Type == ApplicationCommandOptionType.SubCommand) ?? new List<DiscordApplicationCommandOption>())
                        {
                            var subKey = commandKey?.Commands.FirstOrDefault(localized => localized.Names.Any(x => x.Value == subCmd.Name), null);

                            string subName;
                            string subDescription;
                            string subUsage;

                            if (subKey is not null)
                            {
                                subName = $"{commandName} {this.GetString(subKey.Names)}";
                                subDescription = this.GetString(subKey.Descriptions);
                                subUsage = string.Join(" ", subKey.Options?.Select(x => $"<{this.GetString(x.Names).FirstLetterToUpper()}>") ?? new List<string>());
                            }
                            else
                            {
                                subName = $"{commandName} {subCmd.Name}";
                                subDescription = subCmd.Description;
                                subUsage = string.Join(" ", subCmd.Options?.Select(x => $"<{x.Name.FirstLetterToUpper()}>") ?? new List<string>());
                            }

                            Command? subPrefixCommand = null;

                            if (prefixCommand is CommandGroup group)
                                subPrefixCommand = group.Children.FirstOrDefault(x => x.Name == subCmd.Name);

                            Commands.Add(new KeyValuePair<string, string>($"{commandModuleName}",
                            $"{EmojiTemplates.GetInVisible(ctx.Bot)}{TypeEmoji}{(subPrefixCommand is null ? EmojiTemplates.GetPrefixCommandDisabled(ctx.Bot) : EmojiTemplates.GetPrefixCommandEnabled(ctx.Bot))} `/{subName}`‍{(subUsage.IsNullOrWhiteSpace() ? "" : $"`{subUsage}`")}{(subDescription.IsNullOrWhiteSpace() ? "" : $" - _{subDescription}_")}"));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to generate help", ex);
                    }
                }
                catch { }
            }

            if (Commands.Count == 0)
                throw new NullReferenceException();

            var Fields = Commands.PrepareEmbedFields();

            var discordEmbeds = Fields.PrepareEmbeds(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.Help.Disclaimer)).AsInfo(ctx), true);

            var Page = 0;

            while (true)
            {
                var PreviousButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Common.PreviousPage), (Page <= 0), DiscordEmoji.FromUnicode("◀").ToComponent());
                var NextButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(this.t.Common.NextPage), (Page >= discordEmbeds.Count - 1), DiscordEmoji.FromUnicode("▶").ToComponent());

                var builder = new DiscordMessageBuilder().WithEmbed(discordEmbeds.ElementAt(Page));

                if (!PreviousButton.Disabled || !NextButton.Disabled)
                    _ = builder.AddComponents(PreviousButton, NextButton);

                _ = await this.RespondOrEdit(builder);

                if (PreviousButton.Disabled && NextButton.Disabled)
                    return;

                var Menu = await ctx.WaitForButtonAsync();

                if (Menu.TimedOut)
                {
                    this.ModifyToTimedOut();
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