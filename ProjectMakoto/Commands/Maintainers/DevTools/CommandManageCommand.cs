// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class CommandManageCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var EnableCommandButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Enable Command", ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Count == 0, "➕".UnicodeToEmoji().ToComponent());
            var DisableCommandButton = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), "Disable Command", false, "➖".UnicodeToEmoji().ToComponent());

            _ = await this.RespondOrEdit(new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Disabled Commands")
                    .WithDescription($"{(ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Count != 0 ? string.Join(", ", ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Select(x => $"`{x}`")) : "`No commands disabled.`")}")
                    .AsAwaitingInput(ctx))
                .AddComponents(EnableCommandButton, DisableCommandButton)
                .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Button.GetCustomId() == EnableCommandButton.CustomId)
            {
                var SelectionResult = await this.PromptCustomSelection(ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Select(x => new DiscordStringSelectComponentOption(x, x)).ToList());

                if (SelectionResult.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }
                else if (SelectionResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (SelectionResult.Errored)
                {
                    throw SelectionResult.Exception;
                }

                if (!ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Contains(SelectionResult.Result))
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription("`That command is already enabled.`")
                        .AsError(ctx));
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }

                _ = ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Remove(SelectionResult.Result);
                ctx.Bot.status.LoadedConfig.Save();

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == DisableCommandButton.CustomId)
            {
                List<string> CommandList = new();

                foreach (var cmd in ctx.Client.GetCommandList(ctx.Bot))
                {
                    if (ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Contains(cmd.Name.ToLower()))
                        continue;

                    CommandList.Add(cmd.Name.ToLower());

                    foreach (var sub in cmd.Options?.Where(x => x.Type == ApplicationCommandOptionType.SubCommand) ?? new List<DiscordApplicationCommandOption>())
                    {
                        if (ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Contains($"{cmd.Name} {sub.Name}".ToLower()))
                            continue;

                        CommandList.Add($"{cmd.Name} {sub.Name}".ToLower());
                    }
                }

                if (CommandList.Count == 0)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }

                var SelectionResult = await this.PromptCustomSelection(CommandList.Select(x =>
                    new DiscordStringSelectComponentOption(x.FirstLetterToUpper(), x,
                        (x.Contains(' ') ? "Sub Command" : (CommandList.Where(y => y.StartsWith(x)).Count() >= 2 ? "Command Group" : "Single Command")))).ToList(), "Select a command to disable..");

                if (SelectionResult.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }
                else if (SelectionResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (SelectionResult.Errored)
                {
                    throw SelectionResult.Exception;
                }

                if (ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Contains(SelectionResult.Result))
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription("`That command is already disabled.`")
                        .AsError(ctx));
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }

                ctx.Bot.status.LoadedConfig.Discord.DisabledCommands.Add(SelectionResult.Result);
                ctx.Bot.status.LoadedConfig.Save();

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}