namespace ProjectIchigo.Entities;

internal class Cooldown
{
    internal Cooldown(Bot _bot)
    {
        this._bot = _bot;
    }

    private Bot _bot { get; set; }

    private DateTime LightCommandLastUse = DateTime.MinValue;
    private DateTime ModerateCommandLastUse = DateTime.MinValue;
    private DateTime HeavyCommandLastUse = DateTime.MinValue;

    private bool WaitingLight = false;
    private bool WaitingModerate = false;
    private bool WaitingHeavy = false;

    public async Task<bool> WaitForLight(DiscordClient client, DiscordMessage message)
    {
        if (_bot._status.TeamMembers.Contains(message.Author.Id))
            return false;

        if (WaitingLight)
        {
            var stop_warn = await message.Channel.SendMessageAsync($"{message.Author.Mention} :octagonal_sign: `Please slow down. Your previous command is still queued.`");
            await Task.Delay(3000);
            _ = stop_warn.DeleteAsync();
            return true;
        }

        int cooldownTime = 1;

        if (LightCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTotalSecondsUntil() <= 0)
        {
            LightCommandLastUse = DateTime.UtcNow.ToUniversalTime();
            return false;
        }

        var msg = await message.Channel.SendMessageAsync($"{message.Author.Mention} :hourglass: `You're on cooldown for {Math.Round(LightCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTimespanUntil().TotalSeconds, 2)} second(s). As soon as your cool down is over, your command will be executed.`");

        double milliseconds = LightCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTimespanUntil().TotalMilliseconds;

        if (milliseconds <= 0)
            milliseconds = 500;

        WaitingLight = true;
        await Task.Delay(Convert.ToInt32(Math.Round(milliseconds, 0)));
        WaitingLight = false;

        _ = msg.DeleteAsync();
        LightCommandLastUse = DateTime.UtcNow.ToUniversalTime();
        return false;
    }

    public async Task<bool> WaitForModerate(DiscordClient client, DiscordMessage message)
    {
        if (_bot._status.TeamMembers.Contains(message.Author.Id))
            return false;

        if (WaitingModerate)
        {
            var stop_warn = await message.Channel.SendMessageAsync($"{message.Author.Mention} :octagonal_sign: `Please slow down. Your previous command is still queued.`");
            await Task.Delay(3000);
            _ = stop_warn.DeleteAsync();
            return true;
        }

        int cooldownTime = 8;

        if (ModerateCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTotalSecondsUntil() <= 0)
        {
            ModerateCommandLastUse = DateTime.UtcNow.ToUniversalTime();
            return false;
        }

        var msg = await message.Channel.SendMessageAsync($"{message.Author.Mention} :hourglass: `You're on cooldown for {Math.Round(ModerateCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTimespanUntil().TotalSeconds, 2)} second(s). As soon as your cool down is over, your command will be executed.`");

        double milliseconds = ModerateCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTimespanUntil().TotalMilliseconds;

        if (milliseconds <= 0)
            milliseconds = 500;

        WaitingModerate = true;
        await Task.Delay(Convert.ToInt32(Math.Round(milliseconds, 0)));
        WaitingModerate = false;

        _ = msg.DeleteAsync();
        ModerateCommandLastUse = DateTime.UtcNow.ToUniversalTime();
        return false;
    }

    public async Task<bool> WaitForHeavy(DiscordClient client, DiscordMessage message)
    {
        if (_bot._status.TeamMembers.Contains(message.Author.Id))
            return false;

        if (WaitingHeavy)
        {
            var stop_warn = await message.Channel.SendMessageAsync($"{message.Author.Mention} :octagonal_sign: `Please slow down. Your previous command is still queued.`");
            await Task.Delay(3000);
            _ = stop_warn.DeleteAsync();
            return true;
        }

        int cooldownTime = 20;

        if (HeavyCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTotalSecondsUntil() <= 0)
        {
            HeavyCommandLastUse = DateTime.UtcNow.ToUniversalTime();
            return false;
        }

        var msg = await message.Channel.SendMessageAsync($"{message.Author.Mention} :hourglass: `You're on cooldown for {Math.Round(HeavyCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTimespanUntil().TotalSeconds, 2)} second(s). As soon as your cool down is over, your command will be executed.`");

        double milliseconds = HeavyCommandLastUse.ToUniversalTime().AddSeconds(cooldownTime).GetTimespanUntil().TotalMilliseconds;

        if (milliseconds <= 0)
            milliseconds = 500;

        WaitingHeavy = true;
        await Task.Delay(Convert.ToInt32(Math.Round(milliseconds, 0)));
        WaitingHeavy = false;

        _ = msg.DeleteAsync();
        HeavyCommandLastUse = DateTime.UtcNow.ToUniversalTime();
        return false;
    }
}
