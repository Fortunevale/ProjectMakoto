namespace ProjectIchigo.Commands.EmbedMessageCommand;

internal class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = EmbedMessageCommandAbstractions.GetCurrentConfiguration(ctx)
            }.SetAwaitingInput(ctx, "Embed Messages");

            var Toggle = new DiscordButtonComponent((ctx.Bot._guilds[ctx.Guild.Id].EmbedMessageSettings.UseEmbedding ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Message Embeds", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                Toggle
            })
            .AddComponents(Resources.CancelButton));

            var e = await ctx.Client.GetInteractivity().WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == Toggle.CustomId)
            {
                ctx.Bot._guilds[ctx.Guild.Id].EmbedMessageSettings.UseEmbedding = !ctx.Bot._guilds[ctx.Guild.Id].EmbedMessageSettings.UseEmbedding;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}