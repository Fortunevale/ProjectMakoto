namespace ProjectIchigo.Commands;

internal class TranslateCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordMessage bMessage;

            if (arguments?.ContainsKey("message") ?? false)
            {
                bMessage = (DiscordMessage)arguments["message"];
            }
            else
            {
                switch (ctx.CommandType)
                {
                    case Enums.CommandType.PrefixCommand:
                    {
                        if (ctx.OriginalCommandContext.Message.ReferencedMessage is not null)
                        {
                            bMessage = ctx.OriginalCommandContext.Message.ReferencedMessage;
                        }
                        else
                        {
                            SendSyntaxError();
                            return;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException("Message expected");
                }
            }

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            HttpClient client = new HttpClient();

            var languagesResponse = await client.GetAsync($"http://{Secrets.Secrets.LibreTranslateHost}/languages");

            var TranslationTargets = JsonConvert.DeserializeObject<List<LibreTranslateLanguage>>(await languagesResponse.Content.ReadAsStringAsync());

            var TranslationSources = TranslationTargets.ToList();
            TranslationSources.Insert(0, new LibreTranslateLanguage { code = "auto", name = "Auto Detect (experimental)" });

            var TranslationSourcesSelect = new DiscordSelectComponent("Source Language", TranslationSources.Select(x => new DiscordSelectComponentOption(x.name, x.code, null, (x.code == "auto"))));
            var ConfirmButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Translate", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✒")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"`Please select the languages.`",
                Color = EmbedColors.AwaitingInput,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = ctx.Guild.IconUrl
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            }));

            string Source;

            try
            {
                var v = new List<DiscordSelectComponentOption>();

                foreach (var b in TranslationSources)
                {
                    DiscordEmoji flag_emote = null;
                    try
                    { flag_emote = DiscordEmoji.FromName(ctx.Client, $":flag_{b.code.ToLower()}:"); }
                    catch (Exception) { flag_emote = DiscordEmoji.FromUnicode("⬜"); }
                    v.Add(new DiscordSelectComponentOption(b.name, b.code, null, false, new DiscordComponentEmoji(flag_emote)));
                }

                Source = await PromptCustomSelection(v, "Select the Source Language..");
            }
            catch (ArgumentException)
            {
                ModifyToTimedOut();
                throw;
            }
            
            string Target;

            try
            {
                var v = new List<DiscordSelectComponentOption>();

                foreach (var b in TranslationTargets)
                {
                    DiscordEmoji flag_emote = null;
                    try
                    { flag_emote = DiscordEmoji.FromName(ctx.Client, $":flag_{b.code.ToLower()}:"); }
                    catch (Exception) { flag_emote = DiscordEmoji.FromUnicode("⬜"); }
                    v.Add(new DiscordSelectComponentOption(b.name, b.code, null, false, new DiscordComponentEmoji(flag_emote)));
                }

                Target = await PromptCustomSelection(v, "Select the Target Language..");
            }
            catch (ArgumentException)
            {
                ModifyToTimedOut();
                throw;
            }

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"`Translating..`",
                Color = EmbedColors.Processing,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            }));

            string query;

            using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "q", bMessage.Content },
                { "source", Source },
                { "target", Target },
            }))
            {
                query = await content.ReadAsStringAsync();
            }

            var translateResponse = await client.PostAsync($"http://{Secrets.Secrets.LibreTranslateHost}/translate?{query}", null);
            var parsedTranslation = JsonConvert.DeserializeObject<LibreTranslateTranslation>(await translateResponse.Content.ReadAsStringAsync());

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"{parsedTranslation.translatedText}",
                Color = EmbedColors.Info,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{bMessage.Author.UsernameWithDiscriminator} ({bMessage.Author.Id})",
                    IconUrl = bMessage.Author.AvatarUrl
                },
                Timestamp = bMessage.Timestamp,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Translated from {(Source == "auto" ? $"{TranslationSources.First(x => x.code == parsedTranslation.detectedLanguage.language).name} ({parsedTranslation.detectedLanguage.confidence.ToString("N0")}%)" : TranslationSources.First(x => x.code == Source).name)} to {TranslationTargets.First(x => x.code == Target).name} using LibreTranslate", IconUrl = "https://cdn.discordapp.com/attachments/906976602557145110/1000921551698399353/cba1464ba45e470db4ec853535218539cf5d4777.png" }
            }));
        });
    }
}