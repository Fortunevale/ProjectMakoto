namespace Project_Ichigo;

internal class CustomArgumentConverter
{
    internal class BoolConverter : IArgumentConverter<bool>
    {
        public async Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(1);

            if (value.ToLower() is "true" or "y" or "enable" or "allow" or "on")
                return true;
            else if (value.ToLower() is "false" or "n" or "disable" or "disallow" or "off")
                return false;

            throw new Exception($"Invalid Argument");
        }
    }
}
