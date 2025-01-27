// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMakoto.Commands;
public class AutocompleteProviders
{
    public sealed class HelpAutoComplete : IAutocompleteProvider
    {
        private static readonly string[] separator = new string[] { "-", "_" };

        public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
        {
            try
            {
                var bot = ((Bot)ctx.Services.GetService(typeof(Bot)));

                var filteredCommands = bot.DiscordClient.GetShard(ctx.Guild).GetCommandList(bot)
                    .Where(x => x.Name.Contains(ctx.FocusedOption.Value.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    .Where(x => !x.DefaultMemberPermissions.HasValue || ctx.Member.Permissions.HasPermission(x.DefaultMemberPermissions.Value))
                    .Where(x => x.Type == ApplicationCommandType.ChatInput)
                    .Take(25);

                var options = filteredCommands
                    .Select(x => new DiscordApplicationCommandAutocompleteChoice(string.Join("-", x.Name.Split(separator, StringSplitOptions.None)
                        .Select(x => x.FirstLetterToUpper())), x.Name))
                    .ToList();
                return options.AsEnumerable();
            }
            catch (Exception)
            {
                return new List<DiscordApplicationCommandAutocompleteChoice>().AsEnumerable();
            }
        }
    }

    public sealed class ReportTranslationAutoComplete : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
        {
            try
            {
                switch ((ReportTranslationType)Enum.Parse(typeof(ReportTranslationType), ctx.Options.First(x => x.Name == "affected_type").RawValue))
                {
                    case ReportTranslationType.Miscellaneous:
                        return new List<DiscordApplicationCommandAutocompleteChoice>();
                    case ReportTranslationType.Command:
                        return await new HelpAutoComplete().Provider(ctx);
                    case ReportTranslationType.Event:
                    {
                        var filteredTypes = Assembly.GetAssembly(this.GetType()).GetTypes()
                            .Where(t => String.Equals(t.Namespace, "ProjectMakoto.Events", StringComparison.Ordinal))
                            .Where(t => !t.Name.StartsWith('<'))
                            .Where(x => x.Name.Contains(ctx.FocusedOption.Value.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            .Take(25);

                        var options = filteredTypes
                            .Select(x => new DiscordApplicationCommandAutocompleteChoice(x.Name.Replace("Events", ""), x.FullName))
                            .ToList();

                        return options;
                    }
                    default:
                        return new List<DiscordApplicationCommandAutocompleteChoice>();
                }
            }
            catch (Exception)
            {
                return new List<DiscordApplicationCommandAutocompleteChoice>();
            }
        }
    }
}
