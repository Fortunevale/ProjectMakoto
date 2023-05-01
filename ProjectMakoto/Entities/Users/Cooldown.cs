namespace ProjectMakoto.Entities;

public class Cooldown
{
    internal Cooldown(Bot _bot)
    {
        this._bot = _bot;
    }

    private Bot _bot { get; set; }

    private Dictionary<string, DateTime> LastUseByCommand = new();
    private List<string> WaitingList = new();

    private async Task<bool> Wait(SharedCommandContext ctx, int CooldownTime, bool IgnoreStaff)
    {
        if (_bot.status.TeamMembers.Contains(ctx.User.Id) && !IgnoreStaff)
            return false;

        if (WaitingList.Contains(ctx.CommandName))
        {
            var stop_warn = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} 🛑 `{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Common.Cooldown.SlowDown)}`"));
            await Task.Delay(3000);
            _ = stop_warn.DeleteAsync();
            return true;
        }

        if (!LastUseByCommand.ContainsKey(ctx.CommandName))
            LastUseByCommand.Add(ctx.CommandName, DateTime.MinValue);

        if (LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).GetTotalSecondsUntil() <= 0)
        {
            LastUseByCommand[ctx.CommandName] = DateTime.UtcNow.ToUniversalTime();
            return false;
        }

        var cancelButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Common.Cooldown.CancelCommand), false, EmojiTemplates.GetWhiteXMark(ctx.Bot).ToComponent());
        var cancellationTokenSource = new CancellationTokenSource();
        var Cancelled = false;

        var msg = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder()
            .WithContent($"{ctx.User.Mention} ⏳ {ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Common.Cooldown.WaitingForCooldown, true, new TVar("Timestamp", LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).ToTimestamp()))}")
            .AddComponents(cancelButton));

        Task.Run(async () =>
        {
            var result = await msg.WaitForButtonAsync(ctx.User);

            if (result.TimedOut || result.Result.GetCustomId() != cancelButton.CustomId)
                return;

            Cancelled = true;

            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            ctx.BaseCommand.DeleteOrInvalidate();
            cancellationTokenSource.Cancel();
        }).Add(ctx.Bot.watcher);

        double milliseconds = LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).GetTimespanUntil().TotalMilliseconds;
        if (milliseconds <= 0)
            milliseconds = 500;

        WaitingList.Add(ctx.CommandName);
        try
        {
            await Task.Delay(Convert.ToInt32(Math.Round(milliseconds, 0)), cancellationTokenSource.Token);
        }
        catch { }
        finally
        {
            WaitingList.Remove(ctx.CommandName);
        }

        try
        {
            await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder()
            .WithContent($"{ctx.User.Mention} ⏳ {ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Common.Cooldown.WaitingForCooldown, true, new TVar("Timestamp", LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).ToTimestamp()))}")
            .AddComponents(cancelButton.Disable()));
        }
        catch { }

        if (Cancelled)
            return true;

        if (ctx.CommandType == Enums.CommandType.Custom)
            ctx.BaseCommand.DeleteOrInvalidate();

        LastUseByCommand[ctx.CommandName] = DateTime.UtcNow.ToUniversalTime();
        return false;
    }

    public async Task<bool> WaitForLight(SharedCommandContext ctx, bool IgnoreStaff = false) 
        => await Wait(ctx, 1, IgnoreStaff);

    public async Task<bool> WaitForModerate(SharedCommandContext ctx, bool IgnoreStaff = false)
        => await Wait(ctx, 6, IgnoreStaff);

    public async Task<bool> WaitForHeavy(SharedCommandContext ctx, bool IgnoreStaff = false)
        => await Wait(ctx, 20, IgnoreStaff);
}
