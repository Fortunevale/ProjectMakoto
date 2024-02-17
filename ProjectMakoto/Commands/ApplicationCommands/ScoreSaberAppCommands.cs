// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.ApplicationCommands;

[ModulePriority(996)]
public sealed class ScoreSaberAppCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("scoresaber", "Interact with the ScoreSaber API.", dmPermission: false)]
    public sealed class ScoreSaberGroup : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("profile", "Displays you the registered profile of the mentioned user or looks up a profile by a ScoreSaber Id.", dmPermission: false)]
        public async Task ScoreSaberC(InteractionContext ctx, [Option("profile", "Id|@User")] string id = "")
            => _ = new ScoreSaberProfileCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "id", id }
            });

        [SlashCommand("search", "Search a user on Score Saber by name.", dmPermission: false)]
        public async Task ScoreSaberSearch(InteractionContext ctx, [Option("name", "The name to search for")] string name)
            => _ = new ScoreSaberSearchCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "name", name }
            });

        [SlashCommand("map-leaderboard", "Display the leaderboard off a specific map.", dmPermission: false)]
        public async Task ScoreSaberMapLeaderboard(InteractionContext ctx, [Option("LeaderboardId", "The LeaderboardId")] int boardId, [Option("Page", "The page")] int Page = 1, [Option("internal_page", "The internal page")] int Internal_Page = 0)
            => _ = new ScoreSaberMapLeaderboardCommand().ExecuteCommand(ctx, this._bot, new Dictionary<string, object>
            {
                { "boardId", boardId },
                { "Page", Page },
                { "Internal_Page", Internal_Page },
            });

        [SlashCommand("unlink", "Allows you to remove the saved ScoreSaber profile from your Discord account.", dmPermission: false)]
        public async Task ScoreSaberUnlink(InteractionContext ctx)
            => _ = new ScoreSaberUnlinkCommand().ExecuteCommand(ctx, this._bot);
    }
}
