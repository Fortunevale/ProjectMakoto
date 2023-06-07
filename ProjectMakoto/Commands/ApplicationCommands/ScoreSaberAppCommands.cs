// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;

public sealed class ScoreSaberAppCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("scoresaber", "Interact with the ScoreSaber API.", dmPermission: false)]
    public sealed class ScoreSaberGroup : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("profile", "Displays you the registered profile of the mentioned user or looks up a profile by a ScoreSaber Id.", dmPermission: false)]
        public async Task ScoreSaberC(InteractionContext ctx, [Option("profile", "Id|@User")] string id = "")
            => new ScoreSaberProfileCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "id", id }
            }).Add(this._bot.watcher, ctx);

        [SlashCommand("search", "Search a user on Score Saber by name.", dmPermission: false)]
        public async Task ScoreSaberSearch(InteractionContext ctx, [Option("name", "The name to search for")] string name)
            => new ScoreSaberSearchCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "name", name }
            }).Add(this._bot.watcher, ctx);

        [SlashCommand("map-leaderboard", "Display the leaderboard off a specific map.", dmPermission: false)]
        public async Task ScoreSaberMapLeaderboard(InteractionContext ctx, [Option("LeaderboardId", "The LeaderboardId")] int boardId, [Option("Page", "The page")] int Page = 1, [Option("internal_page", "The internal page")] int Internal_Page = 0)
            => new ScoreSaberMapLeaderboardCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "boardId", boardId },
                { "Page", Page },
                { "Internal_Page", Internal_Page },
            }).Add(this._bot.watcher, ctx);

        [SlashCommand("unlink", "Allows you to remove the saved ScoreSaber profile from your Discord account.", dmPermission: false)]
        public async Task ScoreSaberUnlink(InteractionContext ctx)
            => new ScoreSaberUnlinkCommand().ExecuteCommand(ctx, this._bot).Add(this._bot.watcher, ctx);
    }
}
