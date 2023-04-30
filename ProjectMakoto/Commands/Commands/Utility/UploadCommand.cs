namespace ProjectMakoto.Commands;

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
                    Description = $"`{GetString(t.Commands.Utility.Upload.NoInteraction)}`"
                }.AsError(ctx));
                return;
            }
            
            if (ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(t.Commands.Utility.Upload.AlreadyUploaded)}`"
                }.AsError(ctx));
                return;
            }

            if (ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.GetTotalSecondsUntil() < 0)
            {
                await RespondOrEdit(new DiscordEmbedBuilder
                {
                    Description = $"`{GetString(t.Commands.Utility.Upload.TimedOut).Replace("{Timestamp}", $"`{ctx.Bot.uploadInteractions[ctx.User.Id].TimeOut.ToTimestamp()}`")}`"
                }.AsError(ctx));
                ctx.Bot.uploadInteractions.Remove(ctx.User.Id);
                return;
            }

            ctx.Bot.uploadInteractions[ctx.User.Id].UploadedData = stream;
            ctx.Bot.uploadInteractions[ctx.User.Id].FileSize = filesize;
            ctx.Bot.uploadInteractions[ctx.User.Id].InteractionHandled = true;

            await RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"`{GetString(t.Commands.Utility.Upload.Uploaded)}`"
            }.AsSuccess(ctx));

            await Task.Delay(5000);
            DeleteOrInvalidate();
        });
    }
}