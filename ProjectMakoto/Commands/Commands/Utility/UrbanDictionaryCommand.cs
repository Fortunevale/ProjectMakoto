﻿// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class UrbanDictionaryCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.User.Id].Cooldown.WaitForModerate(ctx, true))
                return;

            string term = (string)arguments["term"];

            if (!ctx.Channel.IsNsfw && ctx.CommandType != Enums.CommandType.ApplicationCommand)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Utility.UrbanDictionary.AdultContentError, true)
                }.AsError(ctx));
                return;
            }

            var Yes = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Common.Yes), false, new DiscordComponentEmoji(true.ToEmote(ctx.Bot)));
            var No = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(t.Common.No), false, new DiscordComponentEmoji(false.ToEmote(ctx.Bot)));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Utility.UrbanDictionary.AdultContentWarning, true)
            }.AsAwaitingInput(ctx)).AddComponents(new List<DiscordComponent> { Yes, No }));

            var Menu = await ctx.WaitForButtonAsync();

            if (Menu.TimedOut)
            {
                ModifyToTimedOut();
                return;
            }

            _ = Menu.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Menu.GetCustomId() == Yes.CustomId)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.Utility.UrbanDictionary.LookingUp, true,
                        new TVar("Term", term))
                }.AsLoading(ctx));

                if (term.IsNullOrWhiteSpace())
                {
                    await RespondOrEdit(new DiscordEmbedBuilder
                    {
                        Description = GetString(t.Commands.Utility.UrbanDictionary.LookupFail, true,
                            new TVar("Term", term))
                    }.AsError(ctx));
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
                        Description = GetString(t.Commands.Utility.UrbanDictionary.LookupFail, true, 
                            new TVar("Term", term))
                    }.AsError(ctx));
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
                        Description = GetString(t.Commands.Utility.UrbanDictionary.NotExist, true, new TVar("Term", term))
                    }.AsError(ctx));
                    return;
                }

                var embeds = Definitions.Take(3).Select(x => new DiscordEmbedBuilder
                {
                    Title = $"**{x.word.Replace("**", "")}** - {GetString(t.Commands.Utility.UrbanDictionary.WrittenBy, new TVar("Author", x.author))}",
                    Description = $"**{GetString(t.Commands.Utility.UrbanDictionary.Definition)}**\n\n" +
                                  $"{x.definition.Replace("[", "").Replace("]", "")}\n\n" +
                                  $"**{GetString(t.Commands.Utility.UrbanDictionary.Example)}**\n\n" +
                                  $"{x.example.Replace("[", "").Replace("]", "")}\n\n" +
                                  $"👍 `{x.thumbs_up}` | 👎 `{x.thumbs_down}` | 🕒 {Formatter.Timestamp(x.written_on, TimestampFormat.LongDateTime)}",
                    Url = x.permalink
                }.AsInfo(ctx).Build()).ToList();

                await RespondOrEdit(new DiscordMessageBuilder().AddEmbeds(embeds));
            }
            else
            {
                DeleteOrInvalidate();
            }            
        });
    }
}