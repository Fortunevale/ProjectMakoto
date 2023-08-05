// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class NameNormalizerCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = t.Commands.Config.NameNormalizer;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                return $"💬 `{GetString(CommandKey.NameNormalizerEnabled)}`: {ctx.DbGuild.NameNormalizer.NameNormalizerEnabled.ToEmote(ctx.Bot)}";
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            var Toggle = new DiscordButtonComponent((ctx.DbGuild.NameNormalizer.NameNormalizerEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleNameNormalizer), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var SearchAllNames = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(CommandKey.NormalizeNow), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔨")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Toggle,
                SearchAllNames
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Toggle.CustomId)
            {
                ctx.DbGuild.NameNormalizer.NameNormalizerEnabled = !ctx.DbGuild.NameNormalizer.NameNormalizerEnabled;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == SearchAllNames.CustomId)
            {
                if (ctx.DbGuild.NameNormalizer.NameNormalizerRunning)
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, GetString(CommandKey.Title))
                                                                                   .WithDescription(GetString(CommandKey.NormalizerRunning, true))));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                    return;

                ctx.DbGuild.NameNormalizer.NameNormalizerRunning = true;

                try
                {
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsLoading(ctx, GetString(CommandKey.Title))
                                                                                   .WithDescription(GetString(CommandKey.RenamingAllMembers, true))));

                    var members = await ctx.Guild.GetAllMembersAsync();
                    int Renamed = 0;

                    for (int i = 0; i < members.Count; i++)
                    {
                        var b = members.ElementAt(i);

                        string PingableName = RegexTemplates.AllowedNickname.Replace(b.DisplayName.Normalize(NormalizationForm.FormKC), "");

                        if (PingableName.IsNullOrWhiteSpace())
                            PingableName = GetGuildString(CommandKey.DefaultName);

                        if (PingableName != b.DisplayName)
                        {
                            _ = b.ModifyAsync(x => x.Nickname = PingableName);
                            Renamed++;
                            await Task.Delay(2000);
                        }
                    }

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsSuccess(ctx, GetString(CommandKey.Title))
                                                                                   .WithDescription(GetString(CommandKey.RenamedMembers, true, new TVar("Count", Renamed)))));
                    await Task.Delay(5000);
                    ctx.DbGuild.NameNormalizer.NameNormalizerRunning = false;
                }
                catch (Exception)
                {
                    ctx.DbGuild.NameNormalizer.NameNormalizerRunning = false;
                    throw;
                }

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}