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

            var transSource = bMessage.Content;
            transSource = Regex.Replace(transSource, Resources.Regex.Url, "");

            if (transSource.IsNullOrWhiteSpace())
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`There is no content to translate in the message you selected.`",
                    Color = EmbedColors.Error,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = ctx.Guild.Name,
                        IconUrl = ctx.Guild.IconUrl
                    },
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow
                }));
                return;
            }

            HttpClient client = new();

            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.114 Safari/537.36");

            var GoogleButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Google (slow, accurate)", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1001098467550179469)));
            var LibreTranslateButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "LibreTranslate (fast, less accurate)", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1001098468602945598)));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"`What provider do you want to use?`",
                Color = EmbedColors.AwaitingInput,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Guild.Name,
                    IconUrl = ctx.Guild.IconUrl
                },
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow
            }).AddComponents(new List<DiscordComponent> { GoogleButton, LibreTranslateButton }).AddComponents(Resources.CancelButton));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(1));

            if (e.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == GoogleButton.CustomId)
            {
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
                    Source = await PromptCustomSelection(ctx.Bot._languageCodes.List.Select(x => new DiscordSelectComponentOption(x.Name, x.Code)).ToList(), "Select the Source Language..");
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    throw;
                }

                string Target;

                try
                {
                    Target = await PromptCustomSelection(ctx.Bot._languageCodes.List.Where(x => x.Code != "auto").Select(x => new DiscordSelectComponentOption(x.Name, x.Code)).ToList(), "Select the Target Language..");
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

                var TranslationTask = ctx.Bot._translationClient.Translate_a(Source, Target, transSource);

                int PosInQueue = ctx.Bot._translationClient.Queue.Count;

                bool Announced = false;
                int Wait = 0;

                while (!TranslationTask.IsCompleted)
                {
                    if (Wait > 3 && !Announced)
                    {
                        Announced = true;

                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                        {
                            Description = $"`Your translation request is currently in queue at position {PosInQueue}. This might take a moment to finish..`\n" +
                                          $"`Your request will be executed, approximately,` {Formatter.Timestamp(DateTime.UtcNow.AddSeconds((PosInQueue * 20) - 3))}`.`",
                            Color = EmbedColors.Processing,
                            Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = ctx.Guild.Name,
                                IconUrl = Resources.StatusIndicators.DiscordCircleLoading
                            },
                            Footer = ctx.GenerateUsedByFooter(),
                            Timestamp = DateTime.UtcNow
                        }));
                    }

                    Wait++;
                    await Task.Delay(1000);
                }

                var Translation = TranslationTask.Result;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"{Translation.Item1}",
                    Color = EmbedColors.Info,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = $"{bMessage.Author.UsernameWithDiscriminator} ({bMessage.Author.Id})",
                        IconUrl = bMessage.Author.AvatarUrl
                    },
                    Timestamp = bMessage.Timestamp,
                    Footer = ctx.GenerateUsedByFooter($"Translated from {(Source == "auto" ? $"{ctx.Bot._languageCodes.List.First(x => x.Code == Translation.Item2).Name} (Auto)" : ctx.Bot._languageCodes.List.First(x => x.Code == Source).Name)} to {ctx.Bot._languageCodes.List.First(x => x.Code == Target).Name} using Google", "https://cdn.discordapp.com/attachments/906976602557145110/1001112928226910228/2991148.png")
                }));
            }
            else if (e.Result.Interaction.Data.CustomId == LibreTranslateButton.CustomId)
            {
                var languagesResponse = await client.GetAsync($"http://{Secrets.Secrets.LibreTranslateHost}/languages");

                var TranslationTargets = JsonConvert.DeserializeObject<List<LibreTranslateLanguage>>(await languagesResponse.Content.ReadAsStringAsync());

                var TranslationSources = TranslationTargets.ToList();
                TranslationSources.Insert(0, new LibreTranslateLanguage { code = "auto", name = "Auto Detect (experimental)" });

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
                    Source = await PromptCustomSelection(TranslationSources.Select(x => new DiscordSelectComponentOption(x.name, x.code)).ToList(), "Select the Source Language..");
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut();
                    throw;
                }

                string Target;

                try
                {
                    Target = await PromptCustomSelection(TranslationTargets.Select(x => new DiscordSelectComponentOption(x.name, x.code)).ToList(), "Select the Target Language..");
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
                    { "q", transSource },
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
                    Footer = ctx.GenerateUsedByFooter($"Translated from {(Source == "auto" ? $"{TranslationSources.First(x => x.code == parsedTranslation.detectedLanguage.language).name} ({parsedTranslation.detectedLanguage.confidence:N0}%)" : TranslationSources.First(x => x.code == Source).name)} to {TranslationTargets.First(x => x.code == Target).name} using LibreTranslate", "https://cdn.discordapp.com/attachments/906976602557145110/1000921551698399353/cba1464ba45e470db4ec853535218539cf5d4777.png")
                }));
            }
        });
    }
}