namespace ProjectIchigo.Commands;

internal class UploadCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            Stream stream = (Stream)arguments["stream"];
            int filesize = (int)arguments["filesize"];

            if (!ctx.Bot.uploadInteractions.ContainsKey(ctx.User.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`You have not yet started an upload interactions. Please only run this command when instructed to by the bot.`"
                }.AsError(ctx));
                return;
            }
            
            if (ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`You already uploaded your file.`"
                }.AsError(ctx));
                return;
            }

            if (ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.GetTotalSecondsUntil() < 0)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Your upload interactions timed out {ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.GetTimespanSince().GetHumanReadable()} ago.`"
                }.AsError(ctx));
                ctx.Bot.uploadInteractions.Remove(ctx.User.Id);
                return;
            }

            ctx.Bot.uploadInteractions[ctx.User.Id].UploadedData = stream;
            ctx.Bot.uploadInteractions[ctx.User.Id].FileSize = filesize;
            ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled = true;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Your file has been uploaded.`"
            }.AsSuccess(ctx));

            await Task.Delay(5000);
            DeleteOrInvalidate();
        });
    }
}