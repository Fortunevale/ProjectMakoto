// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class LanguageCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = $"{this.GetString(this.t.Commands.Utility.Language.Disclaimer, true)}\n" +
                              $"{this.GetString(this.t.Commands.Utility.Language.Response, true)}: `{(ctx.DbUser.OverrideLocale.IsNullOrWhiteSpace() ? (ctx.DbUser.CurrentLocale.IsNullOrWhiteSpace() ? "en (Default)" : $"{ctx.DbUser.CurrentLocale} (Discord)") : $"{ctx.DbUser.OverrideLocale} (Override)")}`"
            });

            List<DiscordStringSelectComponentOption> options = new();
            List<DiscordStringSelectComponentOption> newOptions = new();

            newOptions.Add(new DiscordStringSelectComponentOption("Disable Override", "_", this.GetString(this.t.Commands.Utility.Language.DisableOverride), false, DiscordEmoji.FromUnicode("❌").ToComponent()));

            options.Add(new DiscordStringSelectComponentOption("English", "en", "English"));
            options.Add(new DiscordStringSelectComponentOption("German", "de", "Deutsch"));
            options.Add(new DiscordStringSelectComponentOption("Indonesian", "id", "Bahasa Indonesia"));
            options.Add(new DiscordStringSelectComponentOption("Danish", "da", "Dansk"));
            options.Add(new DiscordStringSelectComponentOption("Spanish", "es-ES", "Español"));
            options.Add(new DiscordStringSelectComponentOption("French", "fr", "Français"));
            options.Add(new DiscordStringSelectComponentOption("Croatian", "hr", "Hrvatski"));
            options.Add(new DiscordStringSelectComponentOption("Italian", "it", "Italiano"));
            options.Add(new DiscordStringSelectComponentOption("Lithuanian", "lt", "Lietuviškai"));
            options.Add(new DiscordStringSelectComponentOption("Hungarian", "hu", "Magyar"));
            options.Add(new DiscordStringSelectComponentOption("Dutch", "nl", "Nederlands"));
            options.Add(new DiscordStringSelectComponentOption("Norwegian", "no", "Norsk"));
            options.Add(new DiscordStringSelectComponentOption("Polish", "pl", "Polski"));
            options.Add(new DiscordStringSelectComponentOption("Portuguese, Brazilian", "pt-BR", "Português do Brasil"));
            options.Add(new DiscordStringSelectComponentOption("Romanian, Romania", "ro", "Română"));
            options.Add(new DiscordStringSelectComponentOption("Finnish", "fi", "Suomi"));
            options.Add(new DiscordStringSelectComponentOption("Swedish", "sv-SE", "Svenska"));
            options.Add(new DiscordStringSelectComponentOption("Vietnamese", "vi", "Tiếng Việt"));
            options.Add(new DiscordStringSelectComponentOption("Turkish", "tr", "Türkçe"));
            options.Add(new DiscordStringSelectComponentOption("Czech", "cs", "Čeština"));
            options.Add(new DiscordStringSelectComponentOption("Greek", "el", "Ελληνικά"));
            options.Add(new DiscordStringSelectComponentOption("Bulgarian", "bg", "български"));
            options.Add(new DiscordStringSelectComponentOption("Russian", "ru", "Pусский"));
            options.Add(new DiscordStringSelectComponentOption("Ukrainian", "uk", "Українська"));
            options.Add(new DiscordStringSelectComponentOption("Hindi", "hi", "हिन्दी"));
            options.Add(new DiscordStringSelectComponentOption("Thai", "th", "ไทย"));
            options.Add(new DiscordStringSelectComponentOption("Chinese, China", "zh-CN", "中文"));
            options.Add(new DiscordStringSelectComponentOption("Japanese", "ja", "日本語"));
            options.Add(new DiscordStringSelectComponentOption("Chinese, Taiwan", "zh-TW", "繁體中文"));
            options.Add(new DiscordStringSelectComponentOption("Korean", "ko", "한국어"));

            foreach (var b in options)
                if (this.t.Progress.TryGetValue(b.Value, out var value))
                {
                    var perc = (value / (decimal)this.t.Progress["en"] * 100);
                    DiscordComponentEmoji emoji = null;

                    if (perc >= 100)
                        emoji = DiscordEmoji.FromUnicode("🟢").ToComponent();
                    else emoji = perc >= 85 ? DiscordEmoji.FromUnicode("🟡").ToComponent() : DiscordEmoji.FromUnicode("🔴").ToComponent();

                    newOptions.Add(new DiscordStringSelectComponentOption(b.Label, b.Value, b.Description.Insert(0, $"{perc.ToString("N1", CultureInfo.CreateSpecificCulture("en-US"))}% | "), false, emoji));
                }

            var SelectionResult = await this.PromptCustomSelection(newOptions, this.GetString(this.t.Commands.Utility.Language.Selector));

            if (SelectionResult.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }
            else if (SelectionResult.Cancelled)
            {
                this.DeleteOrInvalidate();
                return;
            }
            else if (SelectionResult.Errored)
            {
                throw SelectionResult.Exception;
            }

            switch (SelectionResult.Result)
            {
                case "_":
                {
                    ctx.DbUser.OverrideLocale = null;
                    break;
                }
                default:
                {
                    ctx.DbUser.OverrideLocale = SelectionResult.Result;
                    break;
                }
            }

            await this.ExecuteCommand(ctx, arguments);
            return;
        });
    }
}