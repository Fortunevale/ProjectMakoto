namespace Project_Ichigo.InteractionSelectors;

internal class GenericSelectors
{
    internal Bot _bot { get; set; }

    internal GenericSelectors(Bot _bot)
    {
        this._bot = _bot;
    }

    internal async Task<DiscordRole> PromptRoleSelection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, DiscordMember member, DiscordMessage message, bool IncludeCreateForMe = false, string CreateForMeName = "Role", bool IncludeDisable = false, string DisableString = "Disable")
    {
        List<DiscordSelectComponentOption> roles = new();

        if (IncludeCreateForMe)
            roles.Add(new DiscordSelectComponentOption($"Create one for me..", "create_for_me", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":heavy_plus_sign:"))));

        if (IncludeDisable)
            roles.Add(new DiscordSelectComponentOption(DisableString, "disable", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":x:"))));

        var HighestRoleOnBot = (await guild.GetMemberAsync(client.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;
        var HighestRoleOnUser = member.Roles.OrderByDescending(x => x.Position).First().Position;

        foreach (var role in (await client.GetGuildAsync(guild.Id)).Roles.OrderByDescending(x => x.Value.Position))
        {
            if (HighestRoleOnBot > role.Value.Position && HighestRoleOnUser > role.Value.Position && !role.Value.IsManaged && role.Value.Id != guild.EveryoneRole.Id)
                roles.Add(new DiscordSelectComponentOption($"@{role.Value.Name} ({role.Value.Id})", role.Value.Id.ToString(), "", false, new DiscordComponentEmoji(role.Value.Color.GetClosestColorEmoji(client))));
        }

        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();
        string NextPageId = Guid.NewGuid().ToString();
        string PrevPageId = Guid.NewGuid().ToString();

        DiscordRole Role = null;

        bool FinishedSelection = false;
        bool ExceptionOccured = false;
        Exception exception = null;

        async Task RefreshRoleList()
        {
            var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":arrow_left:")));
            var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":arrow_right:")));

            var dropdown = new DiscordSelectComponent(SelectionInteractionId, "Select a role..", roles.Skip(currentPage * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(message.Embeds[0]).WithColor(ColorHelper.AwaitingInput)).AddComponents(dropdown).WithContent(message.Content);

            if (roles.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            await message.ModifyAsync(builder);
        }

        _ = RefreshRoleList();

        int TimeoutSeconds = 60;

        async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Message.Id == message.Id && e.User.Id == member.Id)
                    {
                        TimeoutSeconds = 60;
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            client.ComponentInteractionCreated -= RunDropdownInteraction;

                            if (e.Values.First() is "create_for_me")
                                Role = await guild.CreateRoleAsync(CreateForMeName);
                            else if (e.Values.First() is "disable")
                                Role = null;
                            else
                                Role = guild.GetRole(Convert.ToUInt64(e.Values.First()));


                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == PrevPageId)
                        {
                            currentPage--;
                            await RefreshRoleList();
                        }
                        else if (e.Interaction.Data.CustomId == NextPageId)
                        {
                            currentPage++;
                            await RefreshRoleList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    ExceptionOccured = true;
                    FinishedSelection = true;
                    throw;
                }
            }).Add(_bot._watcher);
        }

        client.ComponentInteractionCreated += RunDropdownInteraction;

        while (!FinishedSelection && TimeoutSeconds >= 0)
        {
            await Task.Delay(1000);
            TimeoutSeconds--;
        }

        client.ComponentInteractionCreated -= RunDropdownInteraction;

        await message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(message.Embeds[0]).WithContent(message.Content));

        if (ExceptionOccured)
            throw exception;

        if (TimeoutSeconds <= 0)
            throw new ArgumentException("No selection made");

        return Role;
    }

    internal async Task<DiscordChannel> PromptChannelSelection(DiscordClient client, DiscordGuild guild, DiscordChannel channel, DiscordMember member, DiscordMessage message, bool IncludeCreateForMe = false, string CreateForMeName = "Channel", ChannelType CreateFormeChannelType = ChannelType.Text, bool IncludeDisable = false, string DisableString = "Disable")
    {
        List<DiscordSelectComponentOption> channels = new();

        if (IncludeCreateForMe)
            channels.Add(new DiscordSelectComponentOption($"Create one for me..", "create_for_me", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":heavy_plus_sign:"))));

        if (IncludeDisable)
            channels.Add(new DiscordSelectComponentOption(DisableString, "disable", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":x:"))));

        foreach (var category in await guild.GetOrderedChannelsAsync())
        {
            foreach (var b in category.Value)
                channels.Add(new DiscordSelectComponentOption(
                    $"#{b.Name} ({b.Id})",
                    b.Id.ToString(),
                    $"{(category.Key != 0 ? $"{b.Parent.Name} " : "")}"));
        }

        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();
        string NextPageId = Guid.NewGuid().ToString();
        string PrevPageId = Guid.NewGuid().ToString();

        DiscordChannel Channel = null;

        bool FinishedSelection = false;
        bool ExceptionOccured = false;
        Exception exception = null;

        async Task RefreshRoleList()
        {
            var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":arrow_left:")));
            var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":arrow_right:")));

            var dropdown = new DiscordSelectComponent(SelectionInteractionId, "Select a channel..", channels.Skip(currentPage * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(message.Embeds[0]).WithColor(ColorHelper.AwaitingInput)).AddComponents(dropdown).WithContent(message.Content);

            if (channels.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            await message.ModifyAsync(builder);
        }

        _ = RefreshRoleList();

        int TimeoutSeconds = 60;

        async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Message.Id == message.Id && e.User.Id == member.Id)
                    {
                        TimeoutSeconds = 60;
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            client.ComponentInteractionCreated -= RunDropdownInteraction;

                            if (e.Values.First() is "create_for_me")
                                Channel = await guild.CreateChannelAsync(CreateForMeName, CreateFormeChannelType);
                            else if (e.Values.First() is "disable")
                                Channel = null;
                            else
                                Channel = guild.GetChannel(Convert.ToUInt64(e.Values.First()));

                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == PrevPageId)
                        {
                            currentPage--;
                            await RefreshRoleList();
                        }
                        else if (e.Interaction.Data.CustomId == NextPageId)
                        {
                            currentPage++;
                            await RefreshRoleList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    ExceptionOccured = true;
                    FinishedSelection = true;
                    throw;
                }
            }).Add(_bot._watcher);
        }

        client.ComponentInteractionCreated += RunDropdownInteraction;

        while (!FinishedSelection && TimeoutSeconds >= 0)
        {
            await Task.Delay(1000);
            TimeoutSeconds--;
        }

        client.ComponentInteractionCreated -= RunDropdownInteraction;

        await message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(message.Embeds[0]).WithContent(message.Content));

        if (ExceptionOccured)
            throw exception;

        if (TimeoutSeconds <= 0)
            throw new ArgumentException("No selection made");

        return Channel;
    }

    internal async Task<string> PromptCustomSelection(List<DiscordSelectComponentOption> options, DiscordClient client, DiscordGuild guild, DiscordChannel channel, DiscordMember member, DiscordMessage message, string CustomPlaceHolder = "Select an option..")
    {
        int currentPage = 0;
        string SelectionInteractionId = Guid.NewGuid().ToString();
        string NextPageId = Guid.NewGuid().ToString();
        string PrevPageId = Guid.NewGuid().ToString();

        string Selection = null;

        bool FinishedSelection = false;
        bool ExceptionOccured = false;
        Exception exception = null;

        async Task Refresh()
        {
            var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":arrow_left:")));
            var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":arrow_right:")));

            var dropdown = new DiscordSelectComponent(SelectionInteractionId, CustomPlaceHolder, options.Skip(currentPage * 25).Take(25) as IEnumerable<DiscordSelectComponentOption>);
            var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder(message.Embeds[0]).WithColor(ColorHelper.AwaitingInput)).AddComponents(dropdown).WithContent(message.Content);

            if (options.Skip(currentPage * 25).Count() > 25)
                builder.AddComponents(nextPageButton);

            if (currentPage != 0)
                builder.AddComponents(previousPageButton);

            await message.ModifyAsync(builder);
        }

        _ = Refresh();

        int TimeoutSeconds = 60;

        async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Message.Id == message.Id && e.User.Id == member.Id)
                    {
                        TimeoutSeconds = 60;
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == SelectionInteractionId)
                        {
                            client.ComponentInteractionCreated -= RunDropdownInteraction;

                            Selection = e.Values.First();

                            FinishedSelection = true;
                        }
                        else if (e.Interaction.Data.CustomId == PrevPageId)
                        {
                            currentPage--;
                            await Refresh();
                        }
                        else if (e.Interaction.Data.CustomId == NextPageId)
                        {
                            currentPage++;
                            await Refresh();
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                    ExceptionOccured = true;
                    FinishedSelection = true;
                    throw;
                }
            }).Add(_bot._watcher);
        }

        client.ComponentInteractionCreated += RunDropdownInteraction;

        while (!FinishedSelection && TimeoutSeconds >= 0)
        {
            await Task.Delay(1000);
            TimeoutSeconds--;
        }

        client.ComponentInteractionCreated -= RunDropdownInteraction;

        await message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(message.Embeds[0]).WithContent(message.Content));

        if (ExceptionOccured)
            throw exception;

        if (TimeoutSeconds <= 0)
            throw new ArgumentException("No selection made");

        return Selection;
    }
}
