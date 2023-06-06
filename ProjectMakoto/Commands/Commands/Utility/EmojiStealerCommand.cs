// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class EmojiStealerCommand : BaseCommand
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

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            HttpClient client = new();

            var embed = new DiscordEmbedBuilder
            {
                Description = GetString(t.Commands.Utility.EmojiStealer.DownloadingPre, true),
            }.AsLoading(ctx);
            await RespondOrEdit(embed);

            Dictionary<ulong, EmojiEntry> SanitizedEmoteList = new();
            MemoryStream zipFileStream = new();
            bool FinishedInteraction = false;

            var Emotes = bMessage.Content.GetEmotes();

            foreach (var b in Emotes)
                SanitizedEmoteList.Add(b.Item1, new EmojiEntry
                {
                    Name = b.Item2,
                    Animated = b.Item3,
                    EntryType = EmojiType.EMOJI
                });

            if (!Emotes.Any() && (bMessage.Stickers is null || bMessage.Stickers.Count == 0))
            {
                embed.Description = GetString(t.Commands.Utility.EmojiStealer.NoEmojis, true);
                await RespondOrEdit(embed.AsError(ctx));
                return;
            }

            string guid = Guid.NewGuid().ToString().MakeValidFileName();

            try
            {
                if (SanitizedEmoteList.Count > 0)
                {
                    embed.Description = GetString(t.Commands.Utility.EmojiStealer.DownloadingEmojis, true, new TVar("Count", SanitizedEmoteList.Count));
                    await RespondOrEdit(embed);

                    foreach (var b in SanitizedEmoteList.ToList())
                    {
                        try
                        {
                            var EmoteStream = await client.GetStreamAsync($"https://cdn.discordapp.com/emojis/{b.Key}.{(b.Value.Animated ? "gif" : "png")}");

                            string NameExists = "";
                            int NameExistsInt = 1;

                            string Name = $"{b.Value.Name}{NameExists}.{(b.Value.Animated ? "gif" : "png")}".MakeValidFileName('_');

                            while (SanitizedEmoteList.Any(x => x.Value.Data.Name == Name))
                            {
                                NameExistsInt++;
                                NameExists = $" ({NameExistsInt})";

                                Name = $"{b.Value.Name}{NameExists}.{(b.Value.Animated ? "gif" : "png")}".MakeValidFileName('_');
                            }

                            b.Value.Data.Name = Name;
                            EmoteStream.CopyTo(b.Value.Data.Stream);
                            b.Value.Data.Stream.Position = 0;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Failed to download an emote", ex);
                                
                            SanitizedEmoteList.Remove(b.Key);
                        }
                    }
                }

                if (bMessage.Stickers.Count > 0)
                {
                    embed.Description = GetString(t.Commands.Utility.EmojiStealer.DownloadingStickers, true, new TVar("Count", bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()).Count()));
                    await RespondOrEdit(embed);

                    foreach (var b in bMessage.Stickers.GroupBy(x => x.Url).Select(x => x.First()))
                    {
                        var newEntry = new EmojiEntry
                        {
                            Animated = false,
                            Name = b.Name,
                            Description = b.Description,
                            Emoji = "🤖".UnicodeToEmoji(),
                            StickerFormat = b.FormatType,
                            EntryType = EmojiType.STICKER
                        };

                        try
                        {
                            var StickerStream = await client.GetStreamAsync(b.Url);

                            string NameExists = "";
                            int NameExistsInt = 1;

                            string Name = $"{b.Name}{NameExists}.png".MakeValidFileName('_');

                            while (SanitizedEmoteList.Any(x => x.Value.Data.Name == Name))
                            {
                                NameExistsInt++;
                                NameExists = $" ({NameExistsInt})";

                                Name = $"{newEntry.Name}{NameExists}.png".MakeValidFileName('_');
                            }

                            newEntry.Data.Name = Name;
                            StickerStream.CopyTo(newEntry.Data.Stream);
                            newEntry.Data.Stream.Position = 0;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Failed to download an emote", ex);

                            SanitizedEmoteList.Remove(b.Id);
                        }

                        SanitizedEmoteList.Add(b.Id, newEntry);
                    }
                }

                if (SanitizedEmoteList.Count == 0)
                {
                    embed.Description = GetString(t.Commands.Utility.EmojiStealer.NoSuccessfulDownload, true);
                    await RespondOrEdit(embed.AsError(ctx));

                    return;
                }

                string emojiText = "";

                if (SanitizedEmoteList.Any(x => x.Value.EntryType == EmojiType.EMOJI))
                    emojiText += GetString(t.Commands.Utility.EmojiStealer.Emoji);

                if (SanitizedEmoteList.Any(x => x.Value.EntryType == EmojiType.STICKER))
                    emojiText += $"{(emojiText.Length > 0 ? $" & {GetString(t.Commands.Utility.EmojiStealer.Sticker)}" : GetString(t.Commands.Utility.EmojiStealer.Sticker))}";

                embed.Description = GetString(t.Commands.Utility.EmojiStealer.ReceivePrompt, true, new TVar("Type", emojiText));
                embed.AsAwaitingInput(ctx);

                bool IncludeStickers = false;

                if (!SanitizedEmoteList.Any(x => x.Value.EntryType == EmojiType.EMOJI))
                    IncludeStickers = true;

                var IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", GetString(t.Commands.Utility.EmojiStealer.ToggleStickers), !SanitizedEmoteList.Any(x => x.Value.EntryType == EmojiType.EMOJI), new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));

                var AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", GetString(t.Commands.Utility.EmojiStealer.AddEmojisToServer), !ctx.Member.Permissions.HasPermission(Permissions.ManageGuildExpressions), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                var ZipPrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "ZipPrivateMessage", GetString(t.Commands.Utility.EmojiStealer.DirectMessageZip), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖥")));
                var SinglePrivateMessageButton = new DiscordButtonComponent(ButtonStyle.Primary, "SinglePrivateMessage", GetString(t.Commands.Utility.EmojiStealer.DirectMessageSingle), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📱")));

                var SendHereButton = new DiscordButtonComponent(ButtonStyle.Secondary, "SendHere", GetString(t.Commands.Utility.EmojiStealer.CurrentChatZip), !(ctx.Member.Permissions.HasPermission(Permissions.AttachFiles)), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

                var builder = new DiscordMessageBuilder().WithEmbed(embed);

                if (SanitizedEmoteList.Any(x => x.Value.EntryType == EmojiType.STICKER))
                    builder.AddComponents(IncludeStickersButton);

                builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

                await RespondOrEdit(builder);

                CancellationTokenSource cancellationTokenSource = new();

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                _ = Task.Delay(60000, cancellationTokenSource.Token).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        FinishedInteraction = true;

                        ModifyToTimedOut();
                    }
                });

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                            {
                                cancellationTokenSource.Cancel();
                                cancellationTokenSource = new();

                                _ = Task.Delay(60000, cancellationTokenSource.Token).ContinueWith(x =>
                                {
                                    if (x.IsCompletedSuccessfully)
                                    {
                                        ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                        ModifyToTimedOut();
                                    }
                                });

                                _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                if (e.GetCustomId() == AddToServerButton.CustomId)
                                {
                                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                    cancellationTokenSource.Cancel();

                                    if (!ctx.Member.Permissions.HasPermission(Permissions.ManageGuildExpressions))
                                    {
                                        SendPermissionError(Permissions.ManageGuildExpressions);
                                        return;
                                    }

                                    if (!ctx.CurrentMember.Permissions.HasPermission(Permissions.ManageGuildExpressions))
                                    {
                                        SendOwnPermissionError(Permissions.ManageGuildExpressions);
                                        return;
                                    }

                                    bool DiscordWarning = false;

                                    embed.Description = GetString(t.Commands.Utility.EmojiStealer.AddEmojisToServerLoading, true,
                                        new TVar("Min", 0),
                                        new TVar("Max", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.EntryType == EmojiType.EMOJI).Count())));
                                    embed.AsLoading(ctx);
                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                    for (int i = 0; i < SanitizedEmoteList.Count; i++)
                                    {
                                        try
                                        {
                                            Task task;

                                            switch (SanitizedEmoteList.ElementAt(i).Value.EntryType)
                                            {
                                                case EmojiType.STICKER:
                                                {
                                                    var sticker = SanitizedEmoteList.ElementAt(i).Value;

                                                    task = ctx.Guild.CreateStickerAsync(sticker.Name, sticker.Description ?? sticker.Name, sticker.Emoji, sticker.Data.Stream, sticker.StickerFormat);

                                                    int WaitSeconds = 0;

                                                    while (task.Status == TaskStatus.WaitingForActivation)
                                                    {
                                                        WaitSeconds++;

                                                        if (WaitSeconds > 10 && !DiscordWarning)
                                                        {
                                                            embed.Description = GetString(t.Commands.Utility.EmojiStealer.AddStickersToServerLoading, true,
                                                                new TVar("Min", 0),
                                                                new TVar("Max", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.EntryType == EmojiType.EMOJI).Count()))) +
                                                                                $"\n{GetString(t.Commands.Utility.EmojiStealer.AddToServerLoadingNotice)}";
                                                            await RespondOrEdit(embed);

                                                            DiscordWarning = true;
                                                        }
                                                        await Task.Delay(1000);
                                                    }

                                                    if (task.IsFaulted)
                                                        throw task.Exception.InnerException;
                                                    break;
                                                }
                                                case EmojiType.EMOJI:
                                                {
                                                    var emoji = SanitizedEmoteList.ElementAt(i);

                                                    task = ctx.Guild.CreateEmojiAsync(SanitizedEmoteList.ElementAt(i).Value.Name, emoji.Value.Data.Stream);

                                                    int WaitSeconds = 0;

                                                    while (task.Status == TaskStatus.WaitingForActivation)
                                                    {
                                                        WaitSeconds++;

                                                        if (WaitSeconds > 10 && !DiscordWarning)
                                                        {
                                                            embed.Description = GetString(t.Commands.Utility.EmojiStealer.AddEmojisToServerLoading, true,
                                                                new TVar("Min", 0),
                                                                new TVar("Max", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.EntryType == EmojiType.EMOJI).Count()))) +
                                                                                $"\n{GetString(t.Commands.Utility.EmojiStealer.AddToServerLoadingNotice)}";
                                                            await RespondOrEdit(embed);

                                                            DiscordWarning = true;
                                                        }
                                                        await Task.Delay(1000);
                                                    }

                                                    if (task.IsFaulted)
                                                        throw task.Exception.InnerException;
                                                    break;
                                                }
                                                default:
                                                    throw new NotImplementedException();
                                            }
                                        }
                                        catch (DisCatSharp.Exceptions.BadRequestException ex)
                                        {
                                            var regex = Regex.Match(ex.WebResponse.Response.Replace("\\", ""), "((\"code\": )(\\d*))");

                                            if (regex.Groups[3].Value == "30008")
                                            {
                                                embed.Description = GetString(t.Commands.Utility.EmojiStealer.NoMoreRoom, true, new TVar("Count", i));
                                                embed.AsError(ctx);
                                                await RespondOrEdit(embed);
                                                return;
                                            }
                                            else
                                                throw;
                                        }
                                    }

                                    embed.Description = GetString(t.Commands.Utility.EmojiStealer.SuccessAdded, true,
                                        new TVar("Count", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.EntryType == EmojiType.EMOJI).Count())));
                                    embed.AsSuccess(ctx);
                                    await RespondOrEdit(embed);
                                    return;
                                }
                                else if (e.GetCustomId() == SinglePrivateMessageButton.CustomId)
                                {
                                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                    cancellationTokenSource.Cancel();

                                    embed.Description = GetString(t.Commands.Utility.EmojiStealer.SendingDm, true, new TVar("Type", emojiText));
                                    embed.AsLoading(ctx);
                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                    try
                                    {
                                        var totalCount = IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.EntryType == EmojiType.EMOJI).Count();

                                        for (int i = 0; i < SanitizedEmoteList.Count; i++)
                                        {
                                            if (!IncludeStickers)
                                                if (SanitizedEmoteList.ElementAt(i).Value.EntryType != EmojiType.EMOJI)
                                                    continue;

                                            var current = SanitizedEmoteList.ElementAt(i);
                                            current.Value.Data.Stream.Seek(0, SeekOrigin.Begin);

                                            var currentFilename = $"{current.Value.Name}.{(current.Value.Animated == true ? "gif" : "png")}";

                                            await ctx.User.SendMessageAsync(new DiscordMessageBuilder()
                                                .WithContent($"`{i + 1}/{totalCount}` `{currentFilename}`")
                                                .WithFile($"{currentFilename}", current.Value.Data.Stream));
                                            await Task.Delay(1000);
                                        }

                                        await ctx.User.SendMessageAsync(new DiscordMessageBuilder().WithContent(GetString(t.Commands.Utility.EmojiStealer.SuccessDm, new TVar("Type", emojiText))));
                                    }
                                    catch (DisCatSharp.Exceptions.UnauthorizedException)
                                    {
                                        SendDmError();
                                        return;
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }

                                    embed.Description = GetString(t.Commands.Utility.EmojiStealer.SuccessDmMain, true,
                                        new TVar("Count", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.EntryType == EmojiType.EMOJI).Count())),
                                        new TVar("Type", emojiText));
                                    await RespondOrEdit(embed.AsSuccess(ctx));
                                    return;
                                }
                                else if (e.GetCustomId() == ZipPrivateMessageButton.CustomId || e.GetCustomId() == SendHereButton.CustomId)
                                {
                                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                    cancellationTokenSource.Cancel();

                                    embed.Description = GetString(t.Commands.Utility.EmojiStealer.PreparingZip, true);
                                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsLoading(ctx)));

                                    using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, true))
                                    {
                                        for (int i = 0; i < SanitizedEmoteList.Count; i++)
                                        {
                                            if (!IncludeStickers)
                                                if (SanitizedEmoteList.ElementAt(i).Value.EntryType != EmojiType.EMOJI)
                                                    continue;

                                            var current = SanitizedEmoteList.ElementAt(i);
                                            var newEntry = archive.CreateEntry(current.Value.Data.Name);
                                            using (var entryStream = newEntry.Open())
                                                await current.Value.Data.Stream.CopyToAsync(entryStream);
                                        }
                                    }

                                    zipFileStream.Seek(0, SeekOrigin.Begin);

                                    if (e.GetCustomId() == ZipPrivateMessageButton.CustomId)
                                    {
                                        embed.Description = GetString(t.Commands.Utility.EmojiStealer.SendingZipDm, true);
                                        await RespondOrEdit(embed);

                                        try
                                        {
                                            zipFileStream.Seek(0, SeekOrigin.Begin);
                                            await ctx.User.SendMessageAsync(new DiscordMessageBuilder().WithFile($"Emojis.zip", zipFileStream).WithContent(GetString(t.Commands.Utility.EmojiStealer.SuccessDm, new TVar("Type", emojiText))));
                                        }
                                        catch (DisCatSharp.Exceptions.UnauthorizedException)
                                        {
                                            SendDmError();
                                            return;
                                        }
                                        catch (Exception)
                                        {
                                            throw;
                                        }

                                        embed.Description = GetString(t.Commands.Utility.EmojiStealer.SuccessDmMain, true,
                                            new TVar("Count", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.EntryType == EmojiType.EMOJI).Count())),
                                            new TVar("Type", emojiText));
                                        await RespondOrEdit(embed.AsSuccess(ctx));
                                    }
                                    else if (e.GetCustomId() == SendHereButton.CustomId)
                                    {
                                        if (!ctx.Member.Permissions.HasPermission(Permissions.AttachFiles))
                                        {
                                            SendPermissionError(Permissions.AttachFiles);
                                            return;
                                        }

                                        embed.Description = GetString(t.Commands.Utility.EmojiStealer.SendingZipChat, true);
                                        await RespondOrEdit(embed);

                                        embed.Description = GetString(t.Commands.Utility.EmojiStealer.SuccessChat, true,
                                            new TVar("Count", (IncludeStickers ? SanitizedEmoteList.Count : SanitizedEmoteList.Where(x => x.Value.EntryType == EmojiType.EMOJI).Count())),
                                            new TVar("Type", emojiText));

                                        zipFileStream.Seek(0, SeekOrigin.Begin);
                                        await RespondOrEdit(new DiscordMessageBuilder().WithFile($"Emotes.zip", zipFileStream).WithEmbed(embed.AsSuccess(ctx)));
                                    }
                                    return;
                                }
                                else if (e.GetCustomId() == IncludeStickersButton.CustomId)
                                {
                                    IncludeStickers = !IncludeStickers;

                                    if (!IncludeStickers)
                                    {
                                        if (!SanitizedEmoteList.Any(x => x.Value.EntryType == EmojiType.EMOJI))
                                            IncludeStickers = true;
                                    }

                                    IncludeStickersButton = new DiscordButtonComponent((IncludeStickers ? ButtonStyle.Success : ButtonStyle.Danger), "ToggleStickers", GetString(t.Commands.Utility.EmojiStealer.ToggleStickers), !SanitizedEmoteList.Any(x => x.Value.EntryType == EmojiType.EMOJI), new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, (ulong)(IncludeStickers ? 970278964755038248 : 970278964079767574))));
                                    AddToServerButton = new DiscordButtonComponent(ButtonStyle.Success, "AddToServer", (IncludeStickers ? GetString(t.Commands.Utility.EmojiStealer.AddEmojisAndStickerToServer) : GetString(t.Commands.Utility.EmojiStealer.AddEmojisToServer)), !ctx.Member.Permissions.HasPermission(Permissions.ManageGuildExpressions), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));

                                    var builder = new DiscordMessageBuilder().WithEmbed(embed);

                                    if (SanitizedEmoteList.Any(x => x.Value.EntryType == EmojiType.STICKER))
                                        builder.AddComponents(IncludeStickersButton);

                                    builder.AddComponents(new List<DiscordComponent> { AddToServerButton, ZipPrivateMessageButton, SinglePrivateMessageButton, SendHereButton });

                                    await RespondOrEdit(builder);
                                }
                            }
                        }
                        finally
                        {
                            if (e.GetCustomId() != IncludeStickersButton.CustomId)
                                FinishedInteraction = true;
                        }
                    }).Add(ctx.Bot.watcher, ctx);
                }
            }
            finally
            {
                while (!FinishedInteraction)
                    await Task.Delay(1000);

                try { await zipFileStream.DisposeAsync(); } catch { }
                foreach (var b in SanitizedEmoteList)
                    try { await b.Value.Data.Stream.DisposeAsync(); } catch { }
            }
        });
    }
}