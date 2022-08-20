namespace ProjectIchigo.Commands;

internal class UrbanDictionaryCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForModerate(ctx.Client, ctx, true))
                return;

            string term = (string)arguments["term"];

            if (!ctx.Channel.IsNsfw)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Urban Dictionary can potentionally contain Adult Content. Please run this command within a channel marked as Age-Restricted.`"
                }.SetError(ctx));
                return;
            }

            var Yes = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Yes", false, new DiscordComponentEmoji(true.BoolToEmote(ctx.Client)));
            var No = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "No", false, new DiscordComponentEmoji(false.BoolToEmote(ctx.Client)));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = $"`Urban Dictionary can potentionally contain Adult Content. Continue?`"
            }.SetAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

            var Menu = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User);

            if (Menu.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Menu.Result.Interaction.Data.CustomId == Yes.CustomId)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Looking up '{term}'..`"
                }.SetLoading(ctx));

                if (term.IsNullOrWhiteSpace())
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Failed to look up the term '{term}'`"
                    }.SetError(ctx));
                    return;
                }

                HttpClient client = new();

                string query;

                using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "term", term },
                }))
                {
                    query = await content.ReadAsStringAsync();
                }

                var Response = await client.GetAsync($"https://api.urbandictionary.com/v0/define?{query}");

                if (!Response.IsSuccessStatusCode)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`Failed to look up the term '{term}': {Response.StatusCode}`"
                    }.SetError(ctx));
                    return;
                }

                List<UrbanDictionary.List> Definitions = null;

                try
                {
                    UrbanDictionary rawDefinitions = JsonConvert.DeserializeObject<UrbanDictionary>(await Response.Content.ReadAsStringAsync());
                    Definitions = rawDefinitions.list.ToList();
                    Definitions.Sort((a, b) => b.RatingRatio.CompareTo(a.RatingRatio));
                }
                catch (Exception ex)
                {
                    _logger.LogError("a", ex);
                }

                if (!Definitions.IsNotNullAndNotEmpty())
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = $"`The term '{term}' does not exist on Urban Dictionary.`"
                    }.SetError(ctx));
                    return;
                }

                var embeds = Definitions.Take(3).Select(x => new DiscordEmbedBuilder
                {
                    Title = $"**{x.word.Replace("**", "")}** - Written by {x.author}",
                    Description = $"**Definition**\n\n" +
                                  $"{x.definition.Replace("[", "").Replace("]", "")}\n\n" +
                                  $"**Example**\n\n" +
                                  $"{x.example.Replace("[", "").Replace("]", "")}\n\n" +
                                  $"👍 `{x.thumbs_up}` | 👎 `{x.thumbs_down}` | 🕒 {Formatter.Timestamp(x.written_on, TimestampFormat.LongDateTime)}",
                    Url = x.permalink
                }.SetInfo(ctx).Build()).ToList();

                await RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(embeds));
            }
            else
            {
                DeleteOrInvalidate();
            }            
        });
    }
}