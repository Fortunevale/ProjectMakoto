namespace ProjectIchigo.Commands;

internal class CustomEmbedCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.EmbedLinks));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var GeneratedEmbed = new DiscordEmbedBuilder().WithDescription("New embed");

            while (true)
            {
                try
                {
                    var SetTitle = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set title", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖋")));
                    var SetAuthor = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set author", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                    var SetThumbnail = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set thumbnail", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖼")));

                    var SetDescription = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set description", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📝")));
                    var SetImage = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set image", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖼")));
                    var SetColor = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set color", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎨")));

                    var SetTimestamp = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set timestamp", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
                    var SetFooter = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set footer", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✒")));

                    var AddField = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Add field", (GeneratedEmbed.Fields.Count >= 25), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
                    var ModifyField = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Modify field", (GeneratedEmbed.Fields.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔁")));
                    var RemoveField = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Remove field", (GeneratedEmbed.Fields.Count <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➖")));

                    var FinishAndSend = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Send embed", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

                    try
                    {
                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(GeneratedEmbed)
                                        .AddComponents(new List<DiscordComponent> { SetTitle, SetAuthor, SetThumbnail })
                                        .AddComponents(new List<DiscordComponent> { SetDescription, SetImage, SetColor })
                                        .AddComponents(new List<DiscordComponent> { SetFooter, SetTimestamp })
                                        .AddComponents(new List<DiscordComponent> { AddField, ModifyField, RemoveField })
                                        .AddComponents(new List<DiscordComponent> { FinishAndSend, MessageComponents.CancelButton }));
                    }
                    catch (Exception)
                    {
                        GeneratedEmbed = new DiscordEmbedBuilder().WithDescription("New embed");
                        continue;
                    }

                    var Menu1 = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(15));

                    if (Menu1.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    if (Menu1.GetCustomId() == SetTitle.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder("Set title for embed", Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "", 0, 256, false, GeneratedEmbed.Title))
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "url", "Url", "", 0, 256, false, GeneratedEmbed.Url));

                        var ModalResult = await PromptModalWithRetry(Menu1.Result.Interaction, modal, false);

                        if (ModalResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            continue;
                        }
                        else if (ModalResult.Errored)
                        {
                            throw ModalResult.Exception;
                        }

                        InteractionCreateEventArgs Response = ModalResult.Result;

                        var url = Response.Interaction.GetModalValueByCustomId("url");

                        try
                        {
                            url = new UriBuilder(url).Uri.ToString();
                        }
                        catch (Exception)
                        { continue; }

                        if (!url.IsNullOrWhiteSpace())
                            if (!url.ToLower().StartsWith("https://") && !url.ToLower().StartsWith("http://"))
                                url = url.Insert(0, "https://");

                        GeneratedEmbed.Title = Response.Interaction.GetModalValueByCustomId("title");
                        GeneratedEmbed.Url = url;
                        continue;
                    }
                    else if (Menu1.GetCustomId() == SetAuthor.CustomId)
                    {
                        GeneratedEmbed.Author ??= new();

                        _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        var SetName = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set name", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                        var SetUrl = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set url", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("↘")));
                        var SetIcon = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set icon", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖼")));

                        var SetByUser = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set by user", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                        var SetByGuild = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set by current server", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖥")));

                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(GeneratedEmbed)
                            .AddComponents(new List<DiscordComponent> { SetName, SetUrl, SetIcon })
                            .AddComponents(new List<DiscordComponent> { SetByUser, SetByGuild })
                            .AddComponents(new List<DiscordComponent> { MessageComponents.BackButton }));

                        var Menu2 = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(15));

                        if (Menu2.TimedOut)
                        {
                            ModifyToTimedOut();
                            return;
                        }

                        if (Menu2.GetCustomId() == SetName.CustomId)
                        {
                            var modal = new DiscordInteractionModalBuilder("Set name for author field", Guid.NewGuid().ToString())
                                .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "name", "Name", "", 0, 256, false, GeneratedEmbed.Author.Name));

                            var ModalResult = await PromptModalWithRetry(Menu2.Result.Interaction, modal, false);

                            if (ModalResult.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }
                            else if (ModalResult.Cancelled)
                            {
                                continue;
                            }
                            else if (ModalResult.Errored)
                            {
                                throw ModalResult.Exception;
                            }

                            InteractionCreateEventArgs Response = ModalResult.Result;

                            GeneratedEmbed.Author.Name = Response.Interaction.GetModalValueByCustomId("name");
                            continue;
                        }
                        else if (Menu2.GetCustomId() == SetUrl.CustomId)
                        {
                            var modal = new DiscordInteractionModalBuilder("Set url for author field", Guid.NewGuid().ToString())
                                .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "url", "Url", "", 0, 256, false, GeneratedEmbed.Author.Url));

                            var ModalResult = await PromptModalWithRetry(Menu2.Result.Interaction, modal, false);

                            if (ModalResult.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }
                            else if (ModalResult.Cancelled)
                            {
                                continue;
                            }
                            else if (ModalResult.Errored)
                            {
                                throw ModalResult.Exception;
                            }

                            InteractionCreateEventArgs Response = ModalResult.Result;

                            var url = Response.Interaction.GetModalValueByCustomId("url");

                            if (!url.IsNullOrWhiteSpace())
                                if (!url.ToLower().StartsWith("https://") && !url.ToLower().StartsWith("http://"))
                                    url = url.Insert(0, "https://");

                            GeneratedEmbed.Author.Url = url;
                            continue;
                        }
                        else if (Menu2.GetCustomId() == SetIcon.CustomId)
                        {
                            _ = Menu2.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please upload an icon via '{ctx.Prefix}upload'.`\n\n" +
                            $"⚠ `Please note: Uploads are being moderated. If your upload is determined to be inappropriate or otherwise harming it will be removed and you'll lose access to the entirety of Project Ichigo. " +
                            $"This includes the bot being removed from guilds you own or manage. Please keep it safe. ♥`").AsAwaitingInput(ctx));

                            (Stream stream, int fileSize) stream;

                            try
                            {
                                stream = await PromptForFileUpload(TimeSpan.FromMinutes(1));
                            }
                            catch (AlreadyAppliedException)
                            {
                                await RespondOrEdit(new DiscordEmbedBuilder
                                {
                                    Description = $"`An upload interaction is already taking place. Please finish it beforehand.`",
                                }.AsError(ctx));
                                continue;
                            }
                            catch (ArgumentException)
                            {
                                ModifyToTimedOut();
                                continue;
                            }

                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Importing your upload..`").AsAwaitingInput(ctx));

                            if (stream.fileSize > 8000000)
                            {
                                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please attach an image below 8mb.`\nContinuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(6))}..").AsError(ctx));
                                await Task.Delay(5000);
                                continue;
                            }

                            var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.OtherAssets)).SendMessageAsync(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`").WithFile($"{Guid.NewGuid()}.png", stream.stream));

                            GeneratedEmbed.Author.IconUrl = asset.Attachments[0].Url;
                            continue;
                        }
                        else if (Menu2.GetCustomId() == SetByUser.CustomId)
                        {
                            var modal = new DiscordInteractionModalBuilder("Select user by id", Guid.NewGuid().ToString())
                                .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "userid", "User Id", "", 0, 20, false));

                            var ModalResult = await PromptModalWithRetry(Menu2.Result.Interaction, modal, false);

                            if (ModalResult.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }
                            else if (ModalResult.Cancelled)
                            {
                                continue;
                            }
                            else if (ModalResult.Errored)
                            {
                                throw ModalResult.Exception;
                            }

                            InteractionCreateEventArgs Response = ModalResult.Result;

                            try
                            {
                                var user = await ctx.Client.GetUserAsync(Convert.ToUInt64(Response.Interaction.GetModalValueByCustomId("userid")));

                                GeneratedEmbed.Author = new DiscordEmbedBuilder.EmbedAuthor
                                {
                                    Name = user.UsernameWithDiscriminator,
                                    IconUrl = user.AvatarUrl,
                                    Url = user.ProfileUrl
                                };
                            }
                            catch { }
                            continue;
                        }
                        else if (Menu2.GetCustomId() == SetByGuild.CustomId)
                        {
                            _ = Menu2.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            GeneratedEmbed.Author = new DiscordEmbedBuilder.EmbedAuthor
                            {
                                Name = ctx.Guild.Name,
                                IconUrl = ctx.Guild.IconUrl
                            };
                            continue;
                        }
                        else if (Menu2.GetCustomId() == MessageComponents.BackButton.CustomId)
                        {
                            _ = Menu2.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            continue;
                        }

                        continue;
                    }
                    else if (Menu1.GetCustomId() == SetThumbnail.CustomId)
                    {
                        GeneratedEmbed.Thumbnail ??= new();

                        _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please upload a thumbnail via '{ctx.Prefix}upload'.`\n\n" +
                            $"⚠ `Please note: Uploads are being moderated. If your upload is determined to be inappropriate or otherwise harming it will be removed and you'll lose access to the entirety of Project Ichigo. " +
                            $"This includes the bot being removed from guilds you own or manage. Please keep it safe. ♥`").AsAwaitingInput(ctx));

                        (Stream stream, int fileSize) stream;

                        try
                        {
                            stream = await PromptForFileUpload(TimeSpan.FromMinutes(1));
                        }
                        catch (AlreadyAppliedException)
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder
                            {
                                Description = $"`An upload interaction is already taking place. Please finish it beforehand.`",
                            }.AsError(ctx));
                            continue;
                        }
                        catch (ArgumentException)
                        {
                            ModifyToTimedOut();
                            continue;
                        }

                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Importing your upload..`").AsAwaitingInput(ctx));

                        if (stream.fileSize > 8000000)
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please attach an image below 8mb.`\nContinuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(6))}..").AsError(ctx));
                            await Task.Delay(5000);
                            continue;
                        }

                        var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.OtherAssets)).SendMessageAsync(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`").WithFile($"{Guid.NewGuid()}.png", stream.stream));

                        GeneratedEmbed.Thumbnail.Url = asset.Attachments[0].Url;
                        continue;
                    }
                    else if (Menu1.GetCustomId() == SetDescription.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder("Set description for embed", Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "description", "Description", "", 0, 4000, false, GeneratedEmbed.Description));

                        var ModalResult = await PromptModalWithRetry(Menu1.Result.Interaction, modal, false);

                        if (ModalResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            continue;
                        }
                        else if (ModalResult.Errored)
                        {
                            throw ModalResult.Exception;
                        }

                        InteractionCreateEventArgs Response = ModalResult.Result;

                        GeneratedEmbed.Description = Response.Interaction.GetModalValueByCustomId("description");
                        continue;
                    }
                    else if (Menu1.GetCustomId() == SetImage.CustomId)
                    {
                        _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please upload an image via '{ctx.Prefix}upload'.`\n\n" +
                            $"⚠ `Please note: Uploads are being moderated. If your upload is determined to be inappropriate or otherwise harming it will be removed and you'll lose access to the entirety of Project Ichigo. " +
                            $"This includes the bot being removed from guilds you own or manage. Please keep it safe. ♥`").AsAwaitingInput(ctx));

                        (Stream stream, int fileSize) stream;

                        try
                        {
                            stream = await PromptForFileUpload(TimeSpan.FromMinutes(1));
                        }
                        catch (AlreadyAppliedException)
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder
                            {
                                Description = $"`An upload interaction is already taking place. Please finish it beforehand.`",
                            }.AsError(ctx));
                            continue;
                        }
                        catch (ArgumentException)
                        {
                            ModifyToTimedOut();
                            continue;
                        }

                        await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Importing your upload..`").AsAwaitingInput(ctx));

                        if (stream.fileSize > 8000000)
                        {
                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please attach an image below 8mb.`\nContinuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(6))}..").AsError(ctx));
                            await Task.Delay(5000);
                            continue;
                        }

                        var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.OtherAssets)).SendMessageAsync(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`").WithFile($"{Guid.NewGuid()}.png", stream.stream));

                        GeneratedEmbed.ImageUrl = asset.Attachments[0].Url;
                        continue;
                    }
                    else if (Menu1.GetCustomId() == SetColor.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder("Set color for embed", Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "color", "Color", "#FF0000", 1, 100, false));

                        var ModalResult = await PromptModalWithRetry(Menu1.Result.Interaction, modal, false);

                        if (ModalResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            continue;
                        }
                        else if (ModalResult.Errored)
                        {
                            throw ModalResult.Exception;
                        }

                        InteractionCreateEventArgs Response = ModalResult.Result;

                        GeneratedEmbed.Color = new DiscordColor(Response.Interaction.GetModalValueByCustomId("color").Truncate(7).IsValidHexColor());
                        continue;
                    }
                    else if (Menu1.GetCustomId() == SetFooter.CustomId)
                    {
                        GeneratedEmbed.Footer ??= new();

                        _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        var SetText = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set text", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖊")));
                        var SetIcon = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set icon", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖼")));

                        var SetByUser = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set by user", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
                        var SetByGuild = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Set by current server", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖥")));

                        await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(GeneratedEmbed)
                            .AddComponents(new List<DiscordComponent> { SetText, SetIcon })
                            .AddComponents(new List<DiscordComponent> { SetByUser, SetByGuild })
                            .AddComponents(new List<DiscordComponent> { MessageComponents.BackButton }));

                        var Menu2 = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(15));

                        if (Menu2.TimedOut)
                        {
                            ModifyToTimedOut();
                            return;
                        }

                        if (Menu2.GetCustomId() == SetText.CustomId)
                        {
                            var modal = new DiscordInteractionModalBuilder("Set text for footer field", Guid.NewGuid().ToString())
                                .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "text", "Text", "", 0, 2048, false, GeneratedEmbed.Footer.Text));

                            var ModalResult = await PromptModalWithRetry(Menu2.Result.Interaction, modal, false);

                            if (ModalResult.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }
                            else if (ModalResult.Cancelled)
                            {
                                continue;
                            }
                            else if (ModalResult.Errored)
                            {
                                throw ModalResult.Exception;
                            }

                            InteractionCreateEventArgs Response = ModalResult.Result;

                            GeneratedEmbed.Footer.Text = Response.Interaction.GetModalValueByCustomId("text");
                            continue;
                        }
                        else if (Menu2.GetCustomId() == SetIcon.CustomId)
                        {
                            _ = Menu2.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please upload an icon via '{ctx.Prefix}upload'.`\n\n" +
                            $"⚠ `Please note: Uploads are being moderated. If your upload is determined to be inappropriate or otherwise harming it will be removed and you'll lose access to the entirety of Project Ichigo. " +
                            $"This includes the bot being removed from guilds you own or manage. Please keep it safe. ♥`").AsAwaitingInput(ctx));

                            (Stream stream, int fileSize) stream;

                            try
                            {
                                stream = await PromptForFileUpload(TimeSpan.FromMinutes(1));
                            }
                            catch (AlreadyAppliedException)
                            {
                                await RespondOrEdit(new DiscordEmbedBuilder
                                {
                                    Description = $"`An upload interaction is already taking place. Please finish it beforehand.`",
                                }.AsError(ctx));
                                continue;
                            }
                            catch (ArgumentException)
                            {
                                ModifyToTimedOut();
                                continue;
                            }

                            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Importing your upload..`").AsAwaitingInput(ctx));

                            if (stream.fileSize > 8000000)
                            {
                                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Please attach an image below 8mb.`\nContinuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(6))}..").AsError(ctx));
                                await Task.Delay(5000);
                                continue;
                            }

                            var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.OtherAssets)).SendMessageAsync(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`").WithFile($"{Guid.NewGuid()}.png", stream.stream));

                            GeneratedEmbed.Footer.IconUrl = asset.Attachments[0].Url;
                            continue;
                        }
                        else if (Menu2.GetCustomId() == SetByUser.CustomId)
                        {
                            var modal = new DiscordInteractionModalBuilder("Select user by id", Guid.NewGuid().ToString())
                                .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "userid", "User Id", "", 0, 20, false));

                            var ModalResult = await PromptModalWithRetry(Menu2.Result.Interaction, modal, false);

                            if (ModalResult.TimedOut)
                            {
                                ModifyToTimedOut(true);
                                return;
                            }
                            else if (ModalResult.Cancelled)
                            {
                                continue;
                            }
                            else if (ModalResult.Errored)
                            {
                                throw ModalResult.Exception;
                            }

                            InteractionCreateEventArgs Response = ModalResult.Result;

                            try
                            {
                                var user = await ctx.Client.GetUserAsync(Convert.ToUInt64(Response.Interaction.GetModalValueByCustomId("userid")));

                                GeneratedEmbed.Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = user.UsernameWithDiscriminator,
                                    IconUrl = user.AvatarUrl
                                };
                            }
                            catch { }
                            continue;
                        }
                        else if (Menu2.GetCustomId() == SetByGuild.CustomId)
                        {
                            _ = Menu2.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            GeneratedEmbed.Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = ctx.Guild.Name,
                                IconUrl = ctx.Guild.IconUrl
                            };
                            continue;
                        }
                        else if (Menu2.GetCustomId() == MessageComponents.BackButton.CustomId)
                        {
                            _ = Menu2.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            continue;
                        }

                        continue;
                    }
                    else if (Menu1.GetCustomId() == SetTimestamp.CustomId)
                    {
                        var ModalResult = await PromptModalForDateTime(Menu1.Result.Interaction, false);

                        if (ModalResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }
                        else if (ModalResult.Errored)
                        {
                            continue;
                        }

                        GeneratedEmbed.Timestamp = ModalResult.Result;
                        continue;
                    }
                    else if (Menu1.GetCustomId() == AddField.CustomId)
                    {
                        var modal = new DiscordInteractionModalBuilder("Add field for embed", Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "", 0, 256, true))
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "description", "Description", "", 0, 1024, true))
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "inline", "Inline", "", 4, 5, true, false.ToString()));

                        var ModalResult = await PromptModalWithRetry(Menu1.Result.Interaction, modal, false);

                        if (ModalResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            continue;
                        }
                        else if (ModalResult.Errored)
                        {
                            throw ModalResult.Exception;
                        }

                        InteractionCreateEventArgs Response = ModalResult.Result;

                        try
                        {
                            GeneratedEmbed.AddField(new DiscordEmbedField(Response.Interaction.GetModalValueByCustomId("title"), Response.Interaction.GetModalValueByCustomId("description"), Convert.ToBoolean(Response.Interaction.GetModalValueByCustomId("inline"))));
                        }
                        catch { }
                        continue;
                    }
                    else if (Menu1.GetCustomId() == ModifyField.CustomId)
                    {
                        _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        int Count = -1;

                        int GetInt()
                        {
                            Count++;
                            return Count;
                        }

                        var FieldResult = await PromptCustomSelection(GeneratedEmbed.Fields
                            .Select(x => new DiscordStringSelectComponentOption($"{x.Name}", GetInt().ToString(), x.Value.TruncateWithIndication(10))).ToList());

                        if (FieldResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (FieldResult.Cancelled)
                        {
                            continue;
                        }
                        else if (FieldResult.Errored)
                        {
                            throw FieldResult.Exception;
                        }

                        DiscordEmbedField FieldToEdit = GeneratedEmbed.Fields[Convert.ToInt32(FieldResult.Result)];

                        var modal = new DiscordInteractionModalBuilder("Modify field for embed", Guid.NewGuid().ToString())
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "title", "Title", "", 1, 256, true, FieldToEdit.Name))
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "description", "Description", "", 1, 1024, true, FieldToEdit.Value))
                            .AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "inline", "Inline", "", 4, 5, true, FieldToEdit.Inline.ToString()));

                        var ModalResult = await PromptModalWithRetry(Menu1.Result.Interaction, modal, false);

                        if (ModalResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ModalResult.Cancelled)
                        {
                            continue;
                        }
                        else if (ModalResult.Errored)
                        {
                            throw ModalResult.Exception;
                        }

                        InteractionCreateEventArgs Response = ModalResult.Result;

                        try
                        {
                            FieldToEdit.Name = Response.Interaction.GetModalValueByCustomId("title");
                            FieldToEdit.Value = Response.Interaction.GetModalValueByCustomId("description");
                            FieldToEdit.Inline = Convert.ToBoolean(Response.Interaction.GetModalValueByCustomId("inline"));
                        }
                        catch { }
                        continue;
                    }
                    else if (Menu1.GetCustomId() == RemoveField.CustomId)
                    {
                        _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        int Count = -1;

                        int GetInt()
                        {
                            Count++;
                            return Count;
                        }

                        var FieldResult = await PromptCustomSelection(GeneratedEmbed.Fields
                            .Select(x => new DiscordStringSelectComponentOption($"{x.Name}", GetInt().ToString(), x.Value.TruncateWithIndication(10))).ToList());

                        if (FieldResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (FieldResult.Cancelled)
                        {
                            continue;
                        }
                        else if (FieldResult.Errored)
                        {
                            throw FieldResult.Exception;
                        }

                        GeneratedEmbed.RemoveField(GeneratedEmbed.Fields[Convert.ToInt32(FieldResult.Result)]);
                        continue;
                    }
                    else if (Menu1.GetCustomId() == FinishAndSend.CustomId)
                    {
                        _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        var ChannelResult = await PromptChannelSelection(new ChannelType[] { ChannelType.Text, ChannelType.News });

                        if (ChannelResult.TimedOut)
                        {
                            ModifyToTimedOut(true);
                            return;
                        }
                        else if (ChannelResult.Cancelled)
                        {
                            await ExecuteCommand(ctx, arguments);
                            return;
                        }
                        else if (ChannelResult.Failed)
                        {
                            if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                            {
                                await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription("`Could not find any text or announcement channels in your server.`"));
                                await Task.Delay(3000);
                                continue;
                            }

                            throw ChannelResult.Exception;
                        }

                        await ChannelResult.Result.SendMessageAsync(GeneratedEmbed);
                        DeleteOrInvalidate();
                        return;
                    }
                    else if (Menu1.GetCustomId() == MessageComponents.CancelButton.CustomId)
                    {
                        _ = Menu1.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        DeleteOrInvalidate();
                        return;
                    }
                }
                catch (Exception ex) { _logger.LogError("Failed to change an embed", ex); }
            }
        });
    }
}