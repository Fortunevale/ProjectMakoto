// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;

public sealed class Cooldown : RequiresParent<User>
{
    public Cooldown(Bot bot, User parent) : base(bot, parent)
    {
    }

    private Dictionary<string, DateTime> LastUseByCommand = new();
    private List<string> WaitingList = new();

    internal async Task<bool> Wait(SharedCommandContext ctx, int CooldownTime, bool IgnoreStaff)
    {
        if (this.Bot.status.TeamMembers.Contains(ctx.User.Id) && !IgnoreStaff)
            return false;

        if (this.WaitingList.Contains(ctx.CommandName))
        {
            var stop_warn = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} 🛑 `{ctx.BaseCommand.GetString(ctx.t.Commands.Common.Cooldown.SlowDown)}`"));
            await Task.Delay(3000);
            _ = stop_warn.DeleteAsync();
            return true;
        }

        if (!this.LastUseByCommand.ContainsKey(ctx.CommandName))
            this.LastUseByCommand.Add(ctx.CommandName, DateTime.MinValue);

        if (this.LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).GetTotalSecondsUntil() <= 0)
        {
            this.LastUseByCommand[ctx.CommandName] = DateTime.UtcNow.ToUniversalTime();
            return false;
        }

        var cancelButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), ctx.BaseCommand.GetString(ctx.t.Commands.Common.Cooldown.CancelCommand), false, EmojiTemplates.GetError(ctx.Bot).ToComponent());
        var cancellationTokenSource = new CancellationTokenSource();
        var Cancelled = false;

        var msg = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder()
            .WithContent($"{ctx.User.Mention} ⏳ {ctx.BaseCommand.GetString(ctx.t.Commands.Common.Cooldown.WaitingForCooldown, true, new TVar("Timestamp", this.LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).ToTimestamp()))}")
            .AddComponents(cancelButton));

        _ = Task.Run(async () =>
        {
            var result = await msg.WaitForButtonAsync(ctx.User);

            if (result.TimedOut || result.Result.GetCustomId() != cancelButton.CustomId)
                return;

            Cancelled = true;

            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            ctx.BaseCommand.DeleteOrInvalidate();
            cancellationTokenSource.Cancel();
        }).Add(ctx.Bot);

        var milliseconds = this.LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).GetTimespanUntil().TotalMilliseconds;
        if (milliseconds <= 0)
            milliseconds = 500;

        this.WaitingList.Add(ctx.CommandName);
        try
        {
            await Task.Delay(Convert.ToInt32(Math.Round(milliseconds, 0)), cancellationTokenSource.Token);
        }
        catch { }
        finally
        {
            _ = this.WaitingList.Remove(ctx.CommandName);
        }

        try
        {
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder()
            .WithContent($"{ctx.User.Mention} ⏳ {ctx.BaseCommand.GetString(ctx.t.Commands.Common.Cooldown.WaitingForCooldown, true, new TVar("Timestamp", this.LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).ToTimestamp()))}")
            .AddComponents(cancelButton.Disable()));
        }
        catch { }

        if (Cancelled)
            return true;

        if (ctx.CommandType == Enums.CommandType.Custom)
            ctx.BaseCommand.DeleteOrInvalidate();

        this.LastUseByCommand[ctx.CommandName] = DateTime.UtcNow.ToUniversalTime();
        return false;
    }

    public Task<bool> WaitForLight(SharedCommandContext ctx, bool IgnoreStaff = false)
        => this.Wait(ctx, 1, IgnoreStaff);

    public Task<bool> WaitForModerate(SharedCommandContext ctx, bool IgnoreStaff = false)
        => this.Wait(ctx, 6, IgnoreStaff);

    public Task<bool> WaitForHeavy(SharedCommandContext ctx, bool IgnoreStaff = false)
        => this.Wait(ctx, 20, IgnoreStaff);
}
