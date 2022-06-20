namespace ProjectIchigo.Commands.User;

internal class Social : BaseCommandModule
{
    public Bot _bot { private get; set; }


    private async Task<string> GetGif(string action)
    {
        KawaiiRequest request = JsonConvert.DeserializeObject<KawaiiRequest>(await new HttpClient().GetStringAsync($"https://kawaii.red/api/gif/{action}/token={Secrets.Secrets.KawaiiRedToken}/"));
        return request.response;
    }



    [Command("afk"),
    CommandModule("social"),
    Description("Set yourself afk: Notify users pinging you that you're currently not around")]
    public async Task Afk(CommandContext ctx, [RemainingText][Description("Text (<128 characters)")] string reason = "-")
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForModerate(ctx.Client, ctx.Message))
                return;

            if (!_bot._users.List.ContainsKey(ctx.User.Id))
                _bot._users.List.Add(ctx.User.Id, new Users.Info(_bot));

            if (reason.Length > 128)
            {
                await ctx.SendSyntaxError();
                return;
            }

            _bot._users.List[ctx.User.Id].AfkStatus.Reason = reason.Sanitize();
            _bot._users.List[ctx.User.Id].AfkStatus.TimeStamp = DateTime.UtcNow;

            var msg = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Afk Status • {ctx.Guild.Name}" },
                Color = EmbedColors.Info,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"{ctx.User.Mention} `You're now set to be afk. Next time you send a message, your afk status will be removed.`"
            });
            await Task.Delay(10000);
            _ = msg.DeleteAsync();
        }).Add(_bot._watcher, ctx);
    }



    [Command("cuddle"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Cuddle with another user")]
    public async Task Cuddle(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            string gif = await GetGif("cuddle");

            string[] phrases =
            {
                "%1 cuddles with %2! <:Dog_Blush:972881712663130222>",
            };

            string[] self_phrases =
            {
                "%1, i don't think that's how it works..",
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }));
        }).Add(_bot._watcher, ctx);
    }



    [Command("kiss"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Kiss another user")]
    public async Task Kiss(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            string gif = await GetGif("kiss");

            string[] phrases =
            {
                "%1 kisses %2! <:Dog_Blush:972881712663130222>",
            };

            string[] self_phrases =
            {
                "%1, i don't think that's how it works..",
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }));
        }).Add(_bot._watcher, ctx);
    }



    [Command("slap"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Slap another user")]
    public async Task Slap(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            string gif = await GetGif("slap");

            string[] phrases =
            {
                "%1 slaps %2! That looks like it hurt.. <:Scared:972880270069997598>",
            };

            string[] self_phrases =
            {
                "Come on, %1. There's no need to be so hard on yourself!",
                "Bad %1! I don't know what you did but bad!"
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }));
        }).Add(_bot._watcher, ctx);
    }



    [Command("kill"), Aliases("waste"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Kill another user..?")]
    public async Task Kill(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            string gif = await GetGif(new string[] { "kill", "wasted" }.OrderBy(x => Guid.NewGuid()).First());

            string[] phrases =
            {
                "%1 kills %2! That looks like it hurt.. <:Scared:972880270069997598>",
                "%1 kills %2! Ouch.. <:Scared:972880270069997598>",
            };

            string[] self_phrases =
            {
                "Come on, %1. There's no need to be so hard on yourself!",
                "Come on, %1.. This isn't a solution is it?"
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }));
        }).Add(_bot._watcher, ctx);
    }



    [Command("boop"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give another user a boop!")]
    public async Task Boop(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            string gif = await GetGif("boop");

            string[] phrases =
            {
                "%1 boops %2! Adorable..",
                "%1 boops %2! So cute!",
            };

            string[] self_phrases =
            {
                "%1, i don't think that's how it works..",
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }));
        }).Add(_bot._watcher, ctx);
    }



    [Command("highfive"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give a high five!")]
    public async Task Highfive(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            string gif = await GetGif("highfive");

            string[] phrases =
            {
                "%1 highfives %2! That's the spirit. <:AlyProud:974100194062917632>",
            };

            string[] self_phrases =
            {
                "%1, are you trying to clap..?",
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }));
        }).Add(_bot._watcher, ctx);
    }



    [Command("hug"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Hug another user!")]
    public async Task Hug(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                return;

            string gif = await GetGif("hug");

            string[] phrases =
            {
                "%1 hugs %2! How sweet! ♥",
                "%1 gives %2 a big fat hug! <:fv_woah:961993656129175592>",
                "%2, watch out! %1 is coming to squeeze you tight! <:fv_woah:961993656129175592>",
            };

            string[] self_phrases =
            {
                "There, there.. I'll hug you %1 😢",
                "Does no one else hug you, %1? There, there.. I'll hug you.. 😢",
                "There, there.. I'll hug you %1. 😢 Sorry if i'm a bit cold, i'm not human y'know.. 😓",
                "You look lonely there, %1..",
            };

            if (ctx.Member.Id == user.Id)
            {
                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
                return;
            }

            _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
            {
                Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                ImageUrl = gif,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter("kawaii.red"),
            }));
        }).Add(_bot._watcher, ctx);
    }



    [Command("pat"), Aliases("pet", "headpat", "headpet"), PreventCommandDeletion,
    CommandModule("social"),
    Description("Give someone some headpats!")]
    public async Task Pat(CommandContext ctx, DiscordUser user)
    {
        Task.Run(async () =>
        {
            Task.Run(async () =>
            {
                if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx.Message))
                    return;

                string gif = await GetGif("pat");

                string[] phrases =
                {
                    "%1 gives %2 headpats!",
                };

                string[] self_phrases =
                {
                    "There, there.. I'll give you some headpats, %1 😢",
                    "I'll give you some headpats, %1.. 😢",
                    "You look lonely there, %1..",
                };

                if (ctx.Member.Id == user.Id)
                {
                    _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                    {
                        Title = self_phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username),
                        Color = EmbedColors.HiddenSidebar,
                        Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                    }));
                    return;
                }

                _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Title = phrases.OrderBy(x => Guid.NewGuid()).First().Replace("%1", ctx.User.Username).Replace("%2", user.Username),
                    ImageUrl = gif,
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter("kawaii.red"),
                }));
            }).Add(_bot._watcher, ctx);
        }).Add(_bot._watcher, ctx);
    }
}
