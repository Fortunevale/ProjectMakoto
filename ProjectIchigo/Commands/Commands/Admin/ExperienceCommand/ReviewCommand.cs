﻿namespace ProjectIchigo.Commands.ExperienceCommand;

internal class ReviewCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Experience Settings • {ctx.Guild.Name}" },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = ExperienceCommandAbstractions.GetCurrentConfiguration(ctx)
            });
        });
    }
}