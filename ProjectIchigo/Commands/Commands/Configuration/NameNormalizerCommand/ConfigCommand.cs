﻿namespace ProjectIchigo.Commands.NameNormalizerCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = NameNormalizerCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, "Name Normalizer");

            var Toggle = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].NameNormalizer.NameNormalizerEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Name Normalizer", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var SearchAllNames = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Normalize Everyone's Names", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔨")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Toggle,
                SearchAllNames
            })
            .AddComponents(MessageComponents.CancelButton));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == Toggle.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].NameNormalizer.NameNormalizerEnabled = !ctx.Bot.guilds[ctx.Guild.Id].NameNormalizer.NameNormalizerEnabled;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == SearchAllNames.CustomId)
            {
                if (ctx.Bot.guilds[ctx.Guild.Id].NameNormalizer.NameNormalizerRunning)
                {
                    embed = embed.AsError(ctx, "Name Normalizer");
                    embed.Description = $"`A normalizer is already running.`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                    return;

                ctx.Bot.guilds[ctx.Guild.Id].NameNormalizer.NameNormalizerRunning = true;

                try
                {
                    embed = embed.AsLoading(ctx, "Name Normalizer");
                    embed.Description = $"`Renaming all members. This might take a while..`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                    var members = await ctx.Guild.GetAllMembersAsync();
                    int Renamed = 0;

                    for (int i = 0; i < members.Count; i++)
                    {
                        var b = members.ElementAt(i);

                        string PingableName = Regex.Replace(b.DisplayName.Normalize(NormalizationForm.FormKC), @"[^a-zA-Z0-9 _\-!.,:;#+*~´`?^°<>|""§$%&\/\\()={\[\]}²³€@_]", "");

                        if (PingableName.IsNullOrWhiteSpace())
                            PingableName = "Pingable Name";

                        if (PingableName != b.DisplayName)
                        {
                            _ = b.ModifyAsync(x => x.Nickname = PingableName);
                            Renamed++;
                            await Task.Delay(5000);
                        }
                    }

                    embed = embed.AsSuccess(ctx, "Name Normalizer");
                    embed.Description = $"`Renamed {Renamed} members.`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                    await Task.Delay(5000);
                    ctx.Bot.guilds[ctx.Guild.Id].NameNormalizer.NameNormalizerRunning = false;
                }
                catch (Exception)
                {
                    ctx.Bot.guilds[ctx.Guild.Id].NameNormalizer.NameNormalizerRunning = false;
                    throw;
                }

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}