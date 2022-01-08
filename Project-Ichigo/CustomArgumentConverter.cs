namespace Project_Ichigo;

internal class CustomArgumentConverter
{
    internal class DiscordUserConverter : IArgumentConverter<DiscordUser>
    {
        public async Task<Optional<DiscordUser>> ConvertAsync(string value, CommandContext ctx)
        {
            var UserRegex = new Regex(@"^<@\!?(\d+?)>$", RegexOptions.ECMAScript | RegexOptions.Compiled);

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid))
            {
                var result = await ctx.Client.GetUserAsync(uid, true).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
                return ret;
            }

            var m = UserRegex.Match(value);
            if (m.Success && ulong.TryParse(m.Groups[ 1 ].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uid))
            {
                var result = await ctx.Client.GetUserAsync(uid, true).ConfigureAwait(false);
                var ret = result != null ? Optional.FromValue(result) : Optional.FromNoValue<DiscordUser>();
                return ret;
            }

            value = value.ToLowerInvariant();

            var di = value.IndexOf('#');
            var un = di != -1 ? value[ ..di ] : value;
            var dv = di != -1 ? value[ (di + 1).. ] : null;

            var us = ctx.Client.Guilds.Values
                .SelectMany(xkvp => xkvp.Members.Values)
                .Where(xm => xm.Username.ToLowerInvariant() == un && ((dv != null && xm.Discriminator == dv) || dv == null));

            var usr = us.FirstOrDefault();
            return usr != null ? Optional.FromValue<DiscordUser>(usr) : Optional.FromNoValue<DiscordUser>();
        }
    }

    internal class BoolConverter : IArgumentConverter<bool>
    {
        public async Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(1);

            if (value.ToLower() is "true" or "y" or "enable" or "allow")
                return true;
            else if (value.ToLower() is "false" or "n" or "disable" or "disallow")
                return false;

            throw new Exception($"Invalid Argument");
        }
    }
}
