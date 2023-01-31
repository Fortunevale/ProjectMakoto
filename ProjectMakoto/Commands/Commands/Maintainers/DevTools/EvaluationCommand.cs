using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ProjectMakoto.Commands;

internal class EvaluationCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckBotOwner();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (ctx.CommandType is not Enums.CommandType.ApplicationCommand and not Enums.CommandType.ContextMenu)
            {
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder().WithDescription("Evaluating CScript has the potentional of leaking confidential information. Are you sure you want to run this command as Prefix Command?").AsBotWarning(ctx))
                    .AddComponents(new List<DiscordComponent> { new DiscordButtonComponent(ButtonStyle.Success, "yes", "Yes"),
                                                                new DiscordButtonComponent(ButtonStyle.Danger, "no", "No")}));

                var result = await ctx.ResponseMessage.WaitForButtonAsync(ctx.User);

                if (result.TimedOut || result.GetCustomId() != "yes")
                {
                    DeleteOrInvalidate();
                    return;
                }
            }

            DiscordMessage msg;

            try
            {
                msg = await ctx.Channel.GetMessageAsync((ulong)arguments["message"]);
            }
            catch (Exception)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Failed to fetch specified message.`").AsBotError(ctx));
                return;
            }

            if (msg.Author.Id != ctx.User.Id)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You cannot evaluate other people's code.`").AsBotError(ctx));
                return;
            }

            await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Evaluating..`").AsBotLoading(ctx));

            var code = RegexTemplates.Code.Match(msg.Content).Groups[1]?.Value?.Trim() ?? "";

            if (code.IsNullOrWhiteSpace())
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`No code block was found.`").AsBotError(ctx));
                return;
            }    

            try
            {
                var options = ScriptOptions.Default;
                options = options.WithImports(
                    "System", 
                    "System.Collections.Generic", 
                    "System.Linq", 
                    "System.Text", 
                    "System.Threading.Tasks", 
                    "DisCatSharp", 
                    "DisCatSharp.Entities", 
                    "DisCatSharp.Interactivity", 
                    "DisCatSharp.Interactivity.Extensions", 
                    "DisCatSharp.Interactivity.Enums", 
                    "DisCatSharp.Enums", 
                    "Newtonsoft.Json"
                    );
                options = options.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                var script = CSharpScript.Create(code, options, typeof(SharedCommandContext));
                script.Compile();
                var result = await script.RunAsync(ctx).ConfigureAwait(false);

                await RespondOrEdit(new DiscordEmbedBuilder().WithTitle("Successful Evaluation")
                    .WithDescription($"{(result.ReturnValue?.ToString().IsNullOrWhiteSpace() ?? true ? "`The evaluation did not return any result.`" : $"{result.ReturnValue}")}").AsBotSuccess(ctx));
            }
            catch (Exception ex)
            {
                await RespondOrEdit(new DiscordEmbedBuilder().WithTitle("Failed Evaluation").WithDescription($"```{ex.Message.SanitizeForCode()}```").AsBotError(ctx));
            }
        });
    }
}