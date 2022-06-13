# Project-Ichigo - A discord bot written in C#

[![wakatime](https://wakatime.com/badge/github/Fortunevale/ProjectIchigo.svg)](https://wakatime.com/badge/github/Fortunevale/ProjectIchigo)

# **[Skip everything, i want the bot!](#getting-the-bot)**

## Table of Contents

* **[What is this?](#what-is-this)**
* **[Getting the bot](#getting-the-bot)**
* **[Contributing or forking](#building-debugging-and-deployment)**
* **[Credits](#credits)**

## What is this?

This is a multi-purpose Discord Bot written in C# using .NET 6.

Current noteable features are:
- Music Playback
- [ScoreSaber API](https://scoresaber.com) Integration
- Protection against Phishing Links using a huge database (Credits for huge amounts links [here](#phishing-link-repositories))
- An experience system with leaderboard and level rewards
- Social Commands like `hug`, `pat` and a few more
- An easy to use emoji and sticker stealer
- A system to backup user's roles and nickname when they leave*
- Important Moderation Features such as an Actionlog, A purge Command and a guild-purge Command**
- Reaction Roles
- No unfair premium features. (Currently no premium features at all.)
<br></br>
##### \* Roles with any significant Permissions like Administrator won't be re-applied. In addition, if the user hasn't been on the server for more than 60 days, neither the roles nor the nickname will be reapplied. Also the `clearbackup` command gives moderators ability to remove stored roles.

##### \** A guild-purge is similar to a purge command. However, instead of scanning just one channel for messages by the specified user, it scans all channels.

## Getting the bot

## [Click here to invite the bot](https://discord.com/api/oauth2/authorize?client_id=947716263394824213&permissions=8&scope=bot%20applications.commands)

- Phishing Protection is enabled by default, people will be banned if they send a link known to be malicous. To change this, run `;;phishing config`.
- Every new server is automatically opted into a global ban system. When someone is known to break Discord's TOS or Community Guidelines, they'll be banned on join or when the ban happens. They will not be banned when the bot is freshly added to your server. To change this behaviour you can use `;;join config`.
- You can join a support server [here](https://discord.gg/SaHT4GPGyW).

## Building, Debugging and Deployment

### Prerequisites

- [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- The mentioned dependencies [below](#required-dependencies-that-you-cant-get-on-nuget)
- Your favourite C# IDE. My personal choice is [Visual Studio Community 2022](https://visualstudio.microsoft.com/vs/community/).

### Required Dependencies that you can't get on nuget:

- [Project-Ichigo.Secrets](https://github.com/TheXorog/Project-Ichigo.Secrets.Dummy)
   - Make sure to fill in all fields in the [Secrets.cs](https://github.com/TheXorog/Project-Ichigo.Secrets.Dummy/blob/main/Secrets.cs)!
- [Xorog.ScoreSaber](https://github.com/TheXorog/Xorog.ScoreSaber)
- [Xorog.Logger](https://github.com/TheXorog/Xorog.Logger)
- [Xorog.UniversalExtensions](https://github.com/TheXorog/Xorog.UniversalExtensions)
<br></br>
- [Lavalink](https://github.com/freyacodes/Lavalink)
- [A MySQL/MariaDB Database](https://dev.mysql.com/doc/mysql-installation-excerpt/8.0/en/general-installation-issues.html)

### Required APIs, Keys and Tokens

- [Discord Team](https://discord.com/developers/teams)
- [Discord Application](https://discord.com/developers/applications/)
   - The Appliction must be in previously mentioned Discord Team. The staff-check requires this.
- [Kawaii API Key](https://kawaii.red/)
   - This is mainly for social commands like `hug`, `cuddle`, `pat` and so on.
<br></br>
- [Github Account](https://github.com/) with [this](#project-ichigo---a-discord-bot-written-in-c) Repository forked
- [Personal Access Token](https://github.com/settings/tokens) for your Github Account
   - The personal access token should have the `repo` permissions.
   - This is required for the `/github create-issue` command.

## Credits

- [DisCatSharp](https://github.com/Aiko-IT-Systems/DisCatSharp) by Aiko-IT-Systems
- [Lavalink](https://github.com/freyacodes/Lavalink) by  freyacodes
#### Phishing Link Repositories
- [discord-tokenlogger-link-list](https://github.com/nikolaischunk/discord-tokenlogger-link-list/) by nikolaischunk
- [links](https://github.com/DevSpen/links/) by DevSpen
- [SteamScamSites](https://github.com/PoorPocketsMcNewHold/SteamScamSites/) by PoorPocketsMcNewHold
- [fluffy-blocklist](https://github.com/sk-cat/fluffy-blocklist/) by sk-cat
- [videogame-scam-blocklist](https://github.com/Vytrah/videogame-scam-blocklist/) by Vytrah
