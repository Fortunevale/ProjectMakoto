# Project-Ichigo - A discord bot written in C#

## Table of Contents

* **[What is this?](#what-is-this)**
* **[Contributing or forking](#building-debugging-and-deployment)**
* **[Credits](#credits)**

## What is this?

This is a multi-purpose Discord Bot written in C# using .NET 6.

Current noteable features are:
- [ScoreSaber API](https://scoresaber.com) Integration
- Protection against Phishing Links using a huge database (Credits for huge amounts links [here](#phishing-link-repositories))
- An experience system with leaderboard and level rewards
- Social Commands like `hug`, `pat` and a few more
- An easy to use emoji and sticker stealer
- A system to backup user's roles and nickname when they leave*
- Important Moderation Features such as an Actionlog, A purge Command and a guild-purge Command**
- Reaction Roles
- No unfair premium features. (Currently no premium features at all.)
<br></br><br></br>
##### \* Roles with any significant Permissions like Administrator won't be re-applied. In addition, if the user hasn't been on the server for more than 60 days, neither the roles nor the nickname will be reapplied. Also the `clearbackup` command gives moderators ability to remove stored roles.
<br></br>
##### \** A guild-purge is similar to a purge command. However, instead of scanning just one channel for messages by the specified user, it scans all channels.

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
- [A MySQL Server](https://github.com/freyacodes/Lavalink)

## Credits

- [DisCatSharp](https://github.com/Aiko-IT-Systems/DisCatSharp) by Aiko-IT-Systems
- [Lavalink](https://github.com/freyacodes/Lavalink) by  freyacodes
#### Phishing Link Repositories
- [discord-tokenlogger-link-list](https://github.com/nikolaischunk/discord-tokenlogger-link-list/) by nikolaischunk
- [links](https://github.com/DevSpen/links/) by DevSpen
- [SteamScamSites](https://github.com/PoorPocketsMcNewHold/SteamScamSites/) by PoorPocketsMcNewHold
- [fluffy-blocklist](https://github.com/sk-cat/fluffy-blocklist/) by sk-cat
- [videogame-scam-blocklist](https://github.com/Vytrah/videogame-scam-blocklist/) by Vytrah