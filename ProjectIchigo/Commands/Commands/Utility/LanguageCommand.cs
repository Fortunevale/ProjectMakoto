namespace ProjectIchigo.Commands;

internal class LanguageCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            await RespondOrEdit(new DiscordEmbedBuilder()
            {
                Description = $"`{GetString(t.Commands.Language.Disclaimer)}`\n`{GetString(t.Commands.Language.Response)}`: `{(ctx.Bot.users[ctx.User.Id].OverrideLocale.IsNullOrWhiteSpace() ? (ctx.Bot.users[ctx.User.Id].CurrentLocale.IsNullOrWhiteSpace() ? "en (Default)" : $"{ctx.Bot.users[ctx.User.Id].CurrentLocale} (Discord)") : $"{ctx.Bot.users[ctx.User.Id].OverrideLocale} (Override)")}`"
            });

            List<DiscordStringSelectComponentOption> options = new();

            options.Add(new DiscordStringSelectComponentOption("Disable Override", "_", GetString(t.Commands.Language.DisableOverride)));
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

            var SelectionResult = await PromptCustomSelection(options, "Select a new language");

            if (SelectionResult.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }
            else if (SelectionResult.Cancelled)
            {
                DeleteOrInvalidate();
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
                    ctx.Bot.users[ctx.User.Id].OverrideLocale = null;
                    break;
                }
                default:
                {
                    ctx.Bot.users[ctx.User.Id].OverrideLocale = SelectionResult.Result;
                    break;
                }
            }

            await ExecuteCommand(ctx, arguments);
            return;
        });
    }
}