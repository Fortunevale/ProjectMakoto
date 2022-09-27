namespace ProjectIchigo.Commands;

internal class BatchLookupCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            List<ulong> IDs = ((string)arguments["IDs"]).Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => x.ToUInt64()).ToList();

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Looking up {IDs.Count} users..`\n`{GenerateASCIIProgressbar(0d, IDs.Count)}`").SetLoading(ctx));

            Dictionary<ulong, DiscordUser> fetched = new();

            for (int i = 0; i < IDs.Count; i++)
            {
                try
                {
                    fetched.Add(IDs[i], await ctx.Client.GetUserAsync(IDs[i]));
                }
                catch (Exception)
                {
                    fetched.Add(IDs[i], null);
                }

                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription($"`Looking up {IDs.Count} users..`\n`{GenerateASCIIProgressbar(i, IDs.Count)}`").SetLoading(ctx));
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription(string.Join("\n", fetched.Select(x => $"{(x.Value is null ? $"❌ `Failed to fetch '{x.Key}'`" : $"✅ {x.Value.Mention} `{x.Value.UsernameWithDiscriminator}` (`{x.Value.Id}`)")}"))).SetSuccess(ctx));
        });
    }
}