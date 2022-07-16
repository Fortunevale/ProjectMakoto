namespace ProjectIchigo.Commands;

internal class BotnickCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx)
    {
        if (!ctx.User.IsMaintenance(ctx.Bot._status))
        {
            SendMaintenanceError();
            return false;
        }

        return true;
    }

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string newNickname = (string)arguments["newNickname"];

            try
            {
                await ctx.Guild.CurrentMember.ModifyAsync(x => x.Nickname = newNickname);

                if (newNickname.IsNullOrWhiteSpace())
                    await RespondOrEdit($"My nickname on this server has been reset.");
                else
                    await RespondOrEdit($"My nickname on this server has been changed to **{newNickname}**.");
            }
            catch (Exception)
            {
                await RespondOrEdit($"My nickname could not be changed.");
            }
        });
    }
}
