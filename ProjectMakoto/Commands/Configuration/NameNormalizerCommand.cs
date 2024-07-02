// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Configuration;

internal sealed class NameNormalizerCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.NameNormalizer;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                return $"💬 `{this.GetString(CommandKey.NameNormalizerEnabled)}`: {ctx.DbGuild.NameNormalizer.NameNormalizerEnabled.ToEmote(ctx.Bot)}";
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var Toggle = new DiscordButtonComponent((ctx.DbGuild.NameNormalizer.NameNormalizerEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleNameNormalizer), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var SearchAllNames = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetString(CommandKey.NormalizeNow), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔨")));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Toggle,
                SearchAllNames
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Toggle.CustomId)
            {
                ctx.DbGuild.NameNormalizer.NameNormalizerEnabled = !ctx.DbGuild.NameNormalizer.NameNormalizerEnabled;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == SearchAllNames.CustomId)
            {
                if (ctx.DbGuild.NameNormalizer.NameNormalizerRunning)
                {
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, this.GetString(CommandKey.Title))
                                                                                   .WithDescription(this.GetString(CommandKey.NormalizerRunning, true))));
                    await Task.Delay(5000);
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }

                if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                    return;

                ctx.DbGuild.NameNormalizer.NameNormalizerRunning = true;

                try
                {
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsLoading(ctx, this.GetString(CommandKey.Title))
                                                                                   .WithDescription(this.GetString(CommandKey.RenamingAllMembers, true))));

                    var members = await ctx.Guild.GetAllMembersAsync();
                    var Renamed = 0;

                    for (var i = 0; i < members.Count; i++)
                    {
                        var b = members.ElementAt(i);

                        var PingableName = RegexTemplates.AllowedNickname.Replace(b.DisplayName.Normalize(NormalizationForm.FormKC), "");

                        if (PingableName.IsNullOrWhiteSpace())
                            PingableName = this.GetGuildString(CommandKey.DefaultName);

                        if (PingableName != b.DisplayName)
                        {
                            _ = b.ModifyAsync(x => x.Nickname = PingableName);
                            Renamed++;
                            await Task.Delay(2000);
                        }
                    }

                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, this.GetString(CommandKey.Title))
                                                                                   .WithDescription(this.GetString(CommandKey.RenamedMembers, true, new TVar("Count", Renamed)))));
                    await Task.Delay(5000);
                    ctx.DbGuild.NameNormalizer.NameNormalizerRunning = false;
                }
                catch (Exception)
                {
                    ctx.DbGuild.NameNormalizer.NameNormalizerRunning = false;
                    throw;
                }

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}