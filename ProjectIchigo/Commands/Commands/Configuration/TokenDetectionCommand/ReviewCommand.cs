﻿namespace ProjectIchigo.Commands.TokenDetectionCommand;

internal class ReviewCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = TokenDetectionCommandAbstractions.GetCurrentConfiguration(ctx)
            }.AsInfo(ctx, "Token Detection"));
        });
    }
}