namespace ProjectIchigo.Commands;

internal class ManualBumpCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => (await CheckPermissions(Permissions.ManageMessages));

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (!ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled)
            {
                _ = RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`The bump reminder is not set up.`").AsError(ctx));
                return;
            }

            DiscordButtonComponent YesButton = new(ButtonStyle.Success, Guid.NewGuid().ToString(), "Yes", false, DiscordEmoji.FromUnicode("✅").ToComponent());

            await RespondOrEdit(new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder().WithDescription("`Manually overwriting the last bump time will re-schedule the bump reminder as if the server was just bumped. Are you sure you want to continue?`").AsWarning(ctx))
                .AddComponents(YesButton)
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "No", false, DiscordEmoji.FromUnicode("❌").ToComponent())));

            var e = await ctx.ResponseMessage.WaitForButtonAsync(ctx.User);

            if (e.TimedOut || e.GetCustomId() != YesButton.CustomId)
            {
                DeleteOrInvalidate();
                return;
            }

            DiscordChannel channel = ctx.Guild.GetChannel(ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId);

            ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastBump = DateTime.UtcNow;
            ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastReminder = DateTime.UtcNow;
            ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.BumpsMissed = 0;
            ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastUserId = 0;
            ctx.Bot.bumpReminder.ScheduleBump(ctx.Client, ctx.Guild.Id);

            _ = channel.DeleteMessageAsync(await channel.GetMessageAsync(ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.PersistentMessageId));
            DeleteOrInvalidate();
        });
    }
}