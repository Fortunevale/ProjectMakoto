namespace ProjectIchigo.Commands.Commands.Configuration.InviteNotesCommand
{
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
                    Description = InviteNotesCommandAbstractions.GetCurrentConfiguration(ctx)
                }.SetInfo(ctx, "Invite Notes");

                var Toggle = new DiscordButtonComponent((ctx.Bot.guilds[ctx.Guild.Id].InviteNotes.InviteNotesEnabled ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), "Toggle Invite Notes", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

            });
        }
    }
}
