// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;
internal sealed class AvatarCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victim = (DiscordUser)arguments["user"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            victim ??= ctx.User;

            victim = await victim.GetFromApiAsync();

            var embed = new DiscordEmbedBuilder
            {
                ImageUrl = victim.AvatarUrl,
            }.AsInfo(ctx, this.GetString(this.t.Commands.Utility.Avatar.Avatar, false, new TVar("User", victim.GetUsernameWithIdentifier())));

            DiscordMember member = null;

            try
            { member = await victim.ConvertToMember(ctx.Guild); }
            catch { }

            var ServerProfilePictureButton = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowServer", this.GetString(this.t.Commands.Utility.Avatar.ShowServerProfile), (string.IsNullOrWhiteSpace(member?.GuildAvatarHash)), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🖥")));
            var ProfilePictureButton = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowProfile", this.GetString(this.t.Commands.Utility.Avatar.ShowUserProfile), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));

            var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ServerProfilePictureButton);

            var msg = await this.RespondOrEdit(builder);

            CancellationTokenSource cancellationTokenSource = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            _ = Task.Delay(60000, cancellationTokenSource.Token).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                    this.ModifyToTimedOut(true);
                }
            });

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    if (e.Message?.Id == msg.Id && e.User.Id == ctx.User.Id)
                    {
                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        _ = Task.Delay(60000, cancellationTokenSource.Token).ContinueWith(x =>
                        {
                            if (x.IsCompletedSuccessfully)
                            {
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                this.ModifyToTimedOut(true);
                            }
                        });

                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.GetCustomId() == ServerProfilePictureButton.CustomId)
                        {
                            embed.ImageUrl = member.GuildAvatarUrl;
                            _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ProfilePictureButton));
                        }
                        else if (e.GetCustomId() == ProfilePictureButton.CustomId)
                        {
                            embed.ImageUrl = member.AvatarUrl;
                            _ = this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(ServerProfilePictureButton));
                        }
                    }
                }).Add(ctx.Bot, ctx);
            }
        });
    }
}
