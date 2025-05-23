// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;

public sealed class Cooldown(Bot bot, User parent) : RequiresParent<User>(bot, parent)
{
    private Dictionary<string, DateTime> LastUseByCommand = new();
    private List<string> WaitingList = new();

    internal async Task<bool> Wait(SharedCommandContext ctx, int CooldownTime, bool IgnoreStaff)
    {
        if (this.Bot.status.TeamMembers.Contains(ctx.User.Id) && !IgnoreStaff)
            return false;

        bool alreadyWaiting;
        lock (this.WaitingList)
        {
            alreadyWaiting = this.WaitingList.Contains(ctx.CommandName);
        }
        if (alreadyWaiting)
        {
            var stop_warn = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithContent($"{ctx.User.Mention} 🛑 `{ctx.BaseCommand.GetString(ctx.t.Commands.Common.Cooldown.SlowDown)}`"));
            await Task.Delay(3000);
            ctx.BaseCommand.DeleteOrInvalidate();
            return true;
        }

        lock (this.LastUseByCommand)
        {
            _ = this.LastUseByCommand.TryAdd(ctx.CommandName, DateTime.MinValue);
            if (this.LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).GetTotalSecondsUntil() <= 0)
            {
                this.LastUseByCommand[ctx.CommandName] = DateTime.UtcNow.ToUniversalTime();
                return false;
            } 
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

        double milliseconds;
        lock (this.LastUseByCommand)
        {
            milliseconds = this.LastUseByCommand[ctx.CommandName].ToUniversalTime().AddSeconds(CooldownTime).GetTimespanUntil().TotalMilliseconds; 
        }
        if (milliseconds <= 0)
            milliseconds = 500;

        lock (this.WaitingList)
        {
            this.WaitingList.Add(ctx.CommandName);
        }
        try
        {
            await Task.Delay(Convert.ToInt32(Math.Round(milliseconds, 0)), cancellationTokenSource.Token);
        }
        catch { }
        finally
        {
            lock (this.WaitingList)
            {
                _ = this.WaitingList.Remove(ctx.CommandName);
            }
        }

        try
        {
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder()
                    .WithContent($"{ctx.User.Mention} ⏳ {ctx.BaseCommand.GetString(ctx.t.Commands.Common.Cooldown.WaitingForCooldown, true, new TVar("Timestamp", DateTime.UtcNow.ToTimestamp()))}")
                    .AddComponents(cancelButton.Disable()));
        }
        catch { }

        if (Cancelled)
            return true;

        if (ctx.CommandType == Enums.CommandType.Custom)
            ctx.BaseCommand.DeleteOrInvalidate();

        lock (this.LastUseByCommand)
        {
            this.LastUseByCommand[ctx.CommandName] = DateTime.UtcNow.ToUniversalTime(); 
        }
        return false;
    }

    public Task<bool> WaitForLight(SharedCommandContext ctx, bool IgnoreStaff = false)
        => this.Wait(ctx, 1, IgnoreStaff);

    public Task<bool> WaitForModerate(SharedCommandContext ctx, bool IgnoreStaff = false)
        => this.Wait(ctx, 6, IgnoreStaff);

    public Task<bool> WaitForHeavy(SharedCommandContext ctx, bool IgnoreStaff = false)
        => this.Wait(ctx, 20, IgnoreStaff);
}
