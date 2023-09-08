// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ProjectMakoto.Commands.DevTools;

internal sealed class EvaluationCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckBotOwner();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (ctx.CommandType is not Enums.CommandType.ApplicationCommand and not Enums.CommandType.ContextMenu)
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder().WithDescription("Evaluating CScript has the potentional of leaking confidential information. Are you sure you want to run this command as Prefix Command?").AsWarning(ctx))
                    .AddComponents(new List<DiscordComponent> { new DiscordButtonComponent(ButtonStyle.Success, "yes", "Yes"),
                                                                new DiscordButtonComponent(ButtonStyle.Danger, "no", "No")}));

                var result = await ctx.ResponseMessage.WaitForButtonAsync(ctx.User);

                if (result.TimedOut || result.GetCustomId() != "yes")
                {
                    this.DeleteOrInvalidate();
                    return;
                }
            }

            var rawCode = (string)arguments["code"];

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Evaluating..`").AsLoading(ctx));

            var code = RegexTemplates.Code.Match(rawCode).Groups[1]?.Value?.Trim() ?? "";

            if (code.IsNullOrWhiteSpace())
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`No code block was found.`").AsError(ctx));
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
                options = options.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && !x.Location.IsNullOrWhiteSpace()));

                var script = CSharpScript.Create(code, options, typeof(SharedCommandContext));
                _ = script.Compile();
                var result = await script.RunAsync(ctx).ConfigureAwait(false);

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithTitle("Successful Evaluation")
                    .WithDescription($"{(result.ReturnValue?.ToString().IsNullOrWhiteSpace() ?? true ? "`The evaluation did not return any result.`" : $"{result.ReturnValue}")}").AsSuccess(ctx));
            }
            catch (Exception ex)
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithTitle("Failed Evaluation").WithDescription($"```{ex.Message.SanitizeForCode()}```").AsError(ctx));
            }
        });
    }
}