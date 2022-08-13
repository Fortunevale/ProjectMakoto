namespace ProjectIchigo.Commands;

internal class UploadCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            Stream stream = (Stream)arguments["stream"];
            int filesize = (int)arguments["filesize"];

            if (!ctx.Bot.UploadInteractions.ContainsKey(ctx.User.Id))
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`You have not yet started an upload interactions. Please only run this command when instructed to by the bot.`"
                }.SetError(ctx));
                return;
            }
            
            if (ctx.Bot.UploadInteractions[ctx.User.Id].InteractionHandled)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`You already uploaded your file.`"
                }.SetError(ctx));
                return;
            }

            if (ctx.Bot.UploadInteractions[ctx.User.Id].TimeOut.GetTotalSecondsUntil() < 0)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`Your upload interactions timed out {ctx.Bot.UploadInteractions[ctx.User.Id].TimeOut.GetTimespanSince().GetHumanReadable()} ago.`"
                }.SetError(ctx));
                ctx.Bot.UploadInteractions.Remove(ctx.User.Id);
                return;
            }

            ctx.Bot.UploadInteractions[ctx.User.Id].UploadedData = stream;
            ctx.Bot.UploadInteractions[ctx.User.Id].FileSize = filesize;
            ctx.Bot.UploadInteractions[ctx.User.Id].InteractionHandled = true;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`Your file has been uploaded.`"
            }.SetSuccess(ctx));

            await Task.Delay(5000);
            DeleteOrInvalidate();
        });
    }
}