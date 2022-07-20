﻿namespace ProjectIchigo.Commands.BumpReminderCommand;

internal class ReviewCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var ListEmbed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Bump Reminder Settings • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = BumpReminderCommandAbstractions.GetCurrentConfiguration(ctx)
            };
            await RespondOrEdit(ListEmbed);
        });
    }
}