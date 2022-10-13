namespace ProjectIchigo.Commands.EmbedMessageCommand;

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
                Description = EmbedMessageCommandAbstractions.GetCurrentConfiguration(ctx)
            }.SetAwaitingInput(ctx, "Embed Messages");

            var ToggleMsg = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseEmbedding ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Message Link Embeds", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ToggleGithub = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseGithubEmbedding ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Github Code Embeds", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🤖")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                ToggleMsg,
                ToggleGithub
            })
            .AddComponents(MessageComponents.CancelButton));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == ToggleMsg.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseEmbedding = !ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseEmbedding;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            if (e.Result.Interaction.Data.CustomId == ToggleGithub.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseGithubEmbedding = !ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseGithubEmbedding;

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