// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using DisCatSharp.Extensions.TwoFactorCommands.Enums;

namespace ProjectMakoto.Commands.DevTools;

internal sealed class EnrollTwoFactorCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx)
        => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            if (ctx.Client.CheckTwoFactorEnrollmentFor(ctx.User.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`You're already enrolled in Two Factor Authentication.`").AsError(ctx));
                return;
            }

            var Confirmed = false;

            var ConfirmButton = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Confirm Two Factor Authentication", false, DiscordEmoji.FromUnicode("✅").ToComponent());

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Enrolling you into Two Factor Authentication..`").AsLoading(ctx));
            var (Secret, QrCode) = ctx.Client.EnrollTwoFactor(ctx.User);
            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithContent($"Please scan this QR Code or use the Secret below to register the Two Factor in an App of your choosing." +
                $"\n\n`{Secret}`\n\n" +
                $"When you're done, please press the button below to confirm the success of the registration.")
                .WithFile("2fa.png", QrCode, false, "This is a QR Code for an Authenticator App.")
                .AddComponents(ConfirmButton, MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            _ = Task.Delay(120000).ContinueWith((_) =>
            {
                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                if (!Confirmed)
                {
                    ctx.Client.DisenrollTwoFactor(ctx.User.Id);
                    _ = this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Failed to authenticate. Enrollment reverted.`").AsError(ctx));
                }
            });

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        try
                        {
                            if (e.GetCustomId() == ConfirmButton.CustomId)
                            {
                                var tfa_result = await e.RequestTwoFactorAsync(s);

                                if (tfa_result.Result is TwoFactorResult.ValidCode or TwoFactorResult.InvalidCode)
                                    _ = tfa_result.ComponentInteraction.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                                if (tfa_result.Result == TwoFactorResult.ValidCode)
                                {
                                    Confirmed = true;
                                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Enrolled successfully.`").AsSuccess(ctx));
                                    return;
                                }

                                throw new Exception("Invalid Code");
                            }
                            else if (e.GetCustomId() == MessageComponents.CancelButtonId)
                            {
                                throw new Exception("Cancelled");
                            }
                        }
                        catch (Exception)
                        {
                            ctx.Client.DisenrollTwoFactor(ctx.User.Id);
                            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription("`Failed to authenticate. Enrollment reverted.`").AsError(ctx));
                        }
                    }
                });
            }

            ctx.Client.ComponentInteractionCreated += RunInteraction;
        });
    }
}