﻿namespace ProjectIchigo.Commands.PhishingCommand;

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
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Phishing Protection Settings • {ctx.Guild.Name}", IconUrl = ctx.Guild.IconUrl },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = PhishingCommandAbstractions.GetCurrentConfiguration(ctx)
            };
            await RespondOrEdit(embed: ListEmbed);
        });
    }
}