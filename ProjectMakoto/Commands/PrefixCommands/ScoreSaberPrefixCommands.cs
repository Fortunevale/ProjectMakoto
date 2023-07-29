// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.PrefixCommands;

public sealed class ScoreSaberPrefixCommands : BaseCommandModule
{
    [Group("scoresaber"), Aliases("ss"),
    
    Description("Interact with the ScoreSaber API.")]
    internal sealed class ScoreSaberGroup : BaseCommandModule
    {
        public Bot _bot { private get; set; }

        [GroupCommand, Command("help"), Description("Sends a list of available sub-commands")]
        public async Task Help(CommandContext ctx)
            => _ = PrefixCommandUtil.SendGroupHelp(this._bot, ctx, "scoresaber");

        [Command("profile"), Description("Displays you the registered profile of the mentioned user or looks up a profile by a ScoreSaber Id.")]
        public async Task ScoreSaberC(CommandContext ctx, [Description("ScoreSaber Id or @User")] string id = "")
            => _ = new ScoreSaberProfileCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "id", id }
            });

        [Command("search"), Description("Search a user on ScoreSaber by name.")]
        public async Task ScoreSaberSearch(CommandContext ctx, [Description("Name")][RemainingText] string name)
            => _ = new ScoreSaberSearchCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "name", name }
            });

        [Command("map-leaderboard"), Description("Display the leaderboard off a specific map.")]
        public async Task ScoreSaberMapLeaderboard(CommandContext ctx, [Description("LeaderboardId")] int boardId, [Description("Page")] int Page = 1, [Description("Internal Page")] int Internal_Page = 0)
            => _ = new ScoreSaberMapLeaderboardCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "boardId", boardId },
                { "Page", Page },
                { "Internal_Page", Internal_Page },
            });

        [Command("unlink"), Description("Allows you to remove the saved ScoreSaber profile from your Discord account.")]
        public async Task ScoreSaberUnlink(CommandContext ctx)
            => _ = new ScoreSaberUnlinkCommand().ExecuteCommand(ctx, this._bot);
    }
}
