﻿namespace ProjectIchigo.Commands;
internal class GlobalBanCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx)
    {
        if (!ctx.User.IsMaintenance(ctx.Bot._status))
        {
            SendMaintenanceError();
            return false;
        }

        return true;
    }

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];
            string reason = (string)arguments["reason"];

            DiscordEmbedBuilder embed = new()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Global Ban • {ctx.Guild.Name}" },
                Color = EmbedColors.Processing,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`Global banning '{victim.UsernameWithDiscriminator}' ({victim.Id})`.."
            };

            var msg = RespondOrEdit(embed);

            if (ctx.Bot._status.TeamMembers.Contains(victim.Id))
            {
                embed.Color = EmbedColors.Error;
                embed.Description = $"`'{victim.UsernameWithDiscriminator}' is registered in the staff team.`";
                msg = RespondOrEdit(embed);
                return;
            }

            ctx.Bot._globalBans.List.Add(victim.Id, new() { Reason = reason, Moderator = ctx.User.Id });

            int Success = 0;
            int Failed = 0;

            foreach (var b in ctx.Client.Guilds.OrderByDescending(x => x.Key == ctx.Guild.Id))
            {
                if (!ctx.Bot._guilds.List.ContainsKey(b.Key))
                    ctx.Bot._guilds.List.Add(b.Key, new Guilds.ServerSettings());

                if (ctx.Bot._guilds.List[b.Key].JoinSettings.AutoBanGlobalBans)
                {
                    try
                    {
                        await b.Value.BanMemberAsync(victim.Id, 7, $"Globalban: {reason}");
                        Success++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception occured while trying to ban user from {b.Key}", ex);
                        Failed++;
                    }
                }
            }

            embed.Color = EmbedColors.Success;
            embed.Description = $"`Banned '{victim.UsernameWithDiscriminator}' ({victim.Id}) from {Success}/{Success + Failed} guilds.`";
            msg = RespondOrEdit(embed);
        });
    }
}
