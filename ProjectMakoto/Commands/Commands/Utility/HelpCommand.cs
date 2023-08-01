// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using CommandType = ProjectMakoto.Enums.CommandType;

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
                .OrderByDescending(x => ((ModulePriorityAttribute)x.ContainingType?.GetCustomAttribute<ModulePriorityAttribute>()).Priority))
            {
                try
                {
                    string module = appCommand.ContainingType.Name.Replace("AppCommands", "").ToLower();

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
                        var commandKey = t.Commands.CommandList.First(localized => localized.Names.Any(x => x.Value == appCommand.Name));

                        var commandName = GetString(commandKey.Names);
                        var commandDescription = GetString(commandKey.Descriptions);
                        var commandUsage = string.Join(" ", commandKey.Options?.Select(x => $"<{GetString(x.Names).FirstLetterToUpper()}>") ?? new List<string>());

                        if (command_filter is not null)
                            if (!commandKey.Names.Any(x => x.Value.Contains(command_filter, StringComparison.InvariantCultureIgnoreCase)))
                                continue;

                        string commandMention;

                        if (appCommand.Options?.Any(x => x.Type == ApplicationCommandOptionType.SubCommand) ?? false)
                            commandMention = $"`/{commandName}`";
                        else if (appCommand.Type != ApplicationCommandType.ChatInput)
                            commandMention = $"`{commandName}`";
                        else
                            commandMention = appCommand.Mention;

                        Command? prefixCommand;

                        if (PrefixCommandsList.Any(x => x.Value.Name.ToLower() == appCommand.Name.ToLower()))
                            prefixCommand = PrefixCommandsList.First(x => x.Value.Name.ToLower() == appCommand.Name.ToLower()).Value;
                        else if (appCommand.CustomAttributes.Any(x => x is PrefixCommandAlternativeAttribute))
                            prefixCommand = PrefixCommandsList
                                .First(x => x.Value.Name.ToLower() == ((PrefixCommandAlternativeAttribute)appCommand.CustomAttributes
                                    .First(x => x is PrefixCommandAlternativeAttribute)).PrefixCommand.ToLower().TruncateAt(' ')).Value;
                        else
                            prefixCommand = null;

                        var commandModuleName = module.ToLower() switch
                        {
                            "utility" => GetString(this.t.Commands.ModuleNames.Utility),
                            "social" => GetString(this.t.Commands.ModuleNames.Social),
                            "music" => GetString(this.t.Commands.ModuleNames.Music),
                            "moderation" => GetString(this.t.Commands.ModuleNames.Moderation),
                            "configuration" => GetString(this.t.Commands.ModuleNames.Configuration),
                            _ => module.FirstLetterToUpper(),
                        };
                        
                        DiscordEmoji TypeEmoji = appCommand.Type switch
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
                            var subKey = commandKey.Commands.First(localized => localized.Names.Any(x => x.Value == subCmd.Name));

                            var subName = $"{commandName} {GetString(subKey.Names)}";
                            var subDescription = GetString(subKey.Descriptions);
                            var subUsage = string.Join(" ", subKey.Options?.Select(x => $"<{GetString(x.Names).FirstLetterToUpper()}>") ?? new List<string>());

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

            if (!Commands.Any())
                throw new NullReferenceException();

            var Fields = Commands.PrepareEmbedFields();

            List<DiscordEmbedBuilder> discordEmbeds = Fields.PrepareEmbeds(new DiscordEmbedBuilder().WithDescription(GetString(this.t.Commands.Utility.Help.Disclaimer)).AsBotInfo(ctx), true);

            int Page = 0;

            while (true)
            {
                var PreviousButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(this.t.Common.PreviousPage), (Page <= 0), DiscordEmoji.FromUnicode("◀").ToComponent());
                var NextButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(this.t.Common.NextPage), (Page >= discordEmbeds.Count - 1), DiscordEmoji.FromUnicode("▶").ToComponent());

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(discordEmbeds.ElementAt(Page)).AddComponents(PreviousButton, NextButton));

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