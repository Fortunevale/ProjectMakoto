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

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx))
                return;

            var transSource = bMessage.Content;
            transSource = RegexTemplates.Url.Replace(transSource, "");

            if (transSource.IsNullOrWhiteSpace())
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`There is no content to translate in the message you selected.`",
                }.AsError(ctx)));
                return;
            }

            HttpClient client = new();

            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.114 Safari/537.36");

            var GoogleButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Google (slow, accurate)", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1001098467550179469)));
            var LibreTranslateButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "LibreTranslate (fast, less accurate)", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1001098468602945598)));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"`What provider do you want to use?`",
            }.AsAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { GoogleButton, LibreTranslateButton }).AddComponents(MessageComponents.CancelButton));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(1));

            if (e.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == GoogleButton.CustomId)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Please select the language to translate from.`",
                }.AsAwaitingInput(ctx)));

                var SourceResult = await PromptCustomSelection(ctx.Bot.languageCodes.List.Select(x => new DiscordSelectComponentOption(x.Name, x.Code, null, (x.Code == ctx.Bot.users[ctx.User.Id].Translation.LastGoogleSource))).ToList(), "Select the Source Language..");

                if (SourceResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (SourceResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (SourceResult.Errored)
                {
                    throw SourceResult.Exception;
                }

                ctx.Bot.users[ctx.User.Id].Translation.LastGoogleSource = SourceResult.Result;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Okay! Translating from {SourceResult.Result}. Now select the language to translate to.`",
                }.AsAwaitingInput(ctx)));

                var TargetResult = await PromptCustomSelection(ctx.Bot.languageCodes.List.Where(x => x.Code != "auto").Select(x => new DiscordSelectComponentOption(x.Name, x.Code, null, (x.Code == ctx.Bot.users[ctx.User.Id].Translation.LastGoogleTarget))).ToList(), "Select the Target Language..");

                if (TargetResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (TargetResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (TargetResult.Errored)
                {
                    throw TargetResult.Exception;
                }

                ctx.Bot.users[ctx.User.Id].Translation.LastGoogleTarget = TargetResult.Result;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Translating..`",
                }.AsLoading(ctx)));

                var TranslationTask = ctx.Bot.translationClient.Translate_a(SourceResult.Result, TargetResult.Result, transSource);

                int PosInQueue = ctx.Bot.translationClient.Queue.Count;

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
                        }.AsLoading(ctx)));
                    }

                    Wait++;
                    await Task.Delay(1000);
                }

                var Translation = TranslationTask.Result;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"{Translation.Item1}",
                }.AsInfo(ctx, "", $"Translated from {(SourceResult.Result == "auto" ? $"{ctx.Bot.languageCodes.List.First(x => x.Code == Translation.Item2).Name} (Auto)" : ctx.Bot.languageCodes.List.First(x => x.Code == SourceResult.Result).Name)} to {ctx.Bot.languageCodes.List.First(x => x.Code == TargetResult.Result).Name} using Google")));
            }
            else if (e.GetCustomId() == LibreTranslateButton.CustomId)
            {
                var languagesResponse = await client.GetAsync($"http://{ctx.Bot.status.LoadedConfig.Secrets.LibreTranslateHost}/languages");

                var TranslationTargets = JsonConvert.DeserializeObject<List<LibreTranslateLanguage>>(await languagesResponse.Content.ReadAsStringAsync());

                var TranslationSources = TranslationTargets.ToList();
                TranslationSources.Insert(0, new LibreTranslateLanguage { code = "auto", name = "Auto Detect (experimental)" });

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Please select the language to translate from.`",
                }.AsAwaitingInput(ctx)));

                var SourceResult = await PromptCustomSelection(TranslationSources.Select(x => new DiscordSelectComponentOption(x.name, x.code, null, (x.code == ctx.Bot.users[ctx.User.Id].Translation.LastLibreTranslateSource))).ToList(), "Select the Source Language..");

                if (SourceResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (SourceResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (SourceResult.Errored)
                {
                    throw SourceResult.Exception;
                }

                ctx.Bot.users[ctx.User.Id].Translation.LastLibreTranslateSource = SourceResult.Result;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Okay! Translating from {SourceResult.Result}. Now select the language to translate to.`",
                }.AsAwaitingInput(ctx)));

                var TargetResult = await PromptCustomSelection(TranslationTargets.Select(x => new DiscordSelectComponentOption(x.name, x.code, null, (x.code == ctx.Bot.users[ctx.User.Id].Translation.LastLibreTranslateTarget))).ToList(), "Select the Target Language..");

                if (TargetResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (TargetResult.Cancelled)
                {
                    DeleteOrInvalidate();
                    return;
                }
                else if (TargetResult.Errored)
                {
                    throw TargetResult.Exception;
                }

                ctx.Bot.users[ctx.User.Id].Translation.LastLibreTranslateTarget = TargetResult.Result;

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"`Translating..`",
                }.AsLoading(ctx)));

                string query;

                using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "q", transSource },
                    { "source", SourceResult.Result },
                    { "target", TargetResult.Result },
                }))
                {
                    query = await content.ReadAsStringAsync();
                }

                var translateResponse = await client.PostAsync($"http://{ctx.Bot.status.LoadedConfig.Secrets.LibreTranslateHost}/translate?{query}", null);
                var parsedTranslation = JsonConvert.DeserializeObject<LibreTranslateTranslation>(await translateResponse.Content.ReadAsStringAsync());

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Description = $"{parsedTranslation.translatedText}",
                }.AsInfo(ctx, "", $"Translated from {(SourceResult.Result == "auto" ? $"{TranslationSources.First(x => x.code == parsedTranslation.detectedLanguage.language).name} ({parsedTranslation.detectedLanguage.confidence:N0}%)" : TranslationSources.First(x => x.code == SourceResult.Result).name)} to {TranslationTargets.First(x => x.code == TargetResult.Result).name} using LibreTranslate")));
            }
        });
    }
}